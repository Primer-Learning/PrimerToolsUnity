using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Simulation;
using Primer.Timeline;
using Simulation.Evolution.Extensions;
using UnityEngine;

namespace Simulation.Evolution
{
    public class EvolutionSimulation : ISimulation, IPrimer, IDisposable
    {
        private readonly List<Vector2> foodPosition;
        private readonly Landscape terrain;
        private readonly Container foodContainer;
        private readonly Container agentContainer;
        private readonly int foodPerTurn;

        public Transform transform { get; }
        public Component component => transform;
        public Vector2 size => new(terrain.size.x, terrain.size.z);
        public bool skipAnimations { get; init; }

        public EvolutionSimulation(Transform transform, int foodPerTurn, int initialBlobs)
        {
            this.foodPerTurn = foodPerTurn;
            this.transform = transform;

            terrain = transform.GetOrAddComponent<Landscape>();

            var container = new Container(transform);
            foodContainer = container.AddContainer("Food").ScaleChildrenInPlayMode();
            agentContainer = container.AddContainer("Blobs").ScaleChildrenInPlayMode();

            PlaceInitialBlobs(initialBlobs);
        }

        private void PlaceInitialBlobs(int blobCount)
        {
            agentContainer.Reset();

            var positions = GetAgentsRestingPosition(blobCount)
                .Select(x => terrain.GetGroundAtLocal(x))
                .ToList();

            var center = positions.Average();

            foreach (var position in positions) {
                var blob = agentContainer.AddPrefab<Transform>("blob_skinned");
                blob.GetOrAddComponent<Agent>();
                blob.position = position;
                blob.LookAt(center);
            }

            agentContainer.Purge(defer: true);
        }

        public async UniTask RunTurn()
        {
            await CreateFood();
            await AgentsGatherFood();
            await AgentsEatFood();
            await AgentsReturnHome();
            await AgentsReproduceOrDie();
        }

        private UniTask CreateFood()
        {
            foodContainer.Reset();

            foreach (var point in PoissonDiscSampler.Rectangular(foodPerTurn, size)) {
                var item = foodContainer.Add<Food>();
                item.transform.position = terrain.GetGroundAt(point - size / 2);
                item.Initialize();
            }

            foodContainer.Purge();

            // Give time for the food to be scale up
            return UniTask.Delay(500);
        }

        private UniTask AgentsGatherFood()
        {
            var food = foodContainer.ChildComponents<Food>().ToList();

            return agentContainer
                .ChildComponents<Agent>()
                .Select(agent => agent.GoToEat(food.RandomItem()))
                .RunInParallel();
        }

        private UniTask AgentsEatFood()
        {
            return agentContainer
                .ChildComponents<Agent>()
                .GroupBy(x => x.goingToEat)
                .Select(x => Eat(x.ToList(), x.Key))
                .RunInParallel();
        }

        private UniTask AgentsReturnHome()
        {
            return agentContainer.ChildComponents<Agent>()
                .Zip(GetAgentsRestingPosition(), (agent, position) => agent.ReturnHome(position))
                .RunInParallel();
        }

        private async UniTask AgentsReproduceOrDie()
        {
            agentContainer.Reset();

            foreach (var agent in agentContainer.ChildComponents<Agent>()) {
                if (agent.canSurvive)
                    agentContainer.Insert(agent);

                if (agent.canReproduce)
                    agentContainer.Add(agent);

                agent.ConsumeEnergy();
            }

            agentContainer.Purge();

            // Make room for new blobs and fill gaps left by dead blobs
            await AgentsReturnHome();
        }

        private static async UniTask Eat(List<Agent> agents, Food food)
        {
            if (agents.Count is 0)
                throw new ArgumentException("Cannot eat without agents", nameof(agents));

            if (agents.Count == 1) {
                await agents[0].Eat(food);

                if (food.hasMore)
                    await agents[0].Eat(food);

                return;
            }

            // only two are going to eat, no matter how many are in the group
            var first = agents.RandomItem();
            var second = agents.RandomItem();

            while (first == second)
                second = agents.RandomItem();

            await UniTask.WhenAll(
                first.Eat(food),
                second.Eat(food)
            );
        }

        private IEnumerable<Vector2> GetAgentsRestingPosition()
        {
            return GetAgentsRestingPosition(agentContainer.ChildComponents<Agent>().Length);
        }

        private IEnumerable<Vector2> GetAgentsRestingPosition(int agentCount)
        {
            const int margin = 2;
            var offset = Vector2.one * margin;
            var perimeter = size - offset * 2;
            var edgeLength = perimeter.x * 2 + perimeter.y * 2;
            var positions = edgeLength / agentCount;
            var slotCenter = positions / 2;

            for (var i = 0; i < agentCount; i++) {
                var linearPosition = positions * i + slotCenter;
                yield return PositionToPerimeter(perimeter, linearPosition) + offset;
            }
        }

        private static Vector2 PositionToPerimeter(Vector2 perimeter, float position)
        {
            if (position < perimeter.x)
                return new Vector2(position, 0);

            position -= perimeter.x;

            if (position < perimeter.y)
                return new Vector2(perimeter.x, position);

            position -= perimeter.y;

            if (position < perimeter.x)
                return new Vector2(perimeter.x - position, perimeter.y);

            position -= perimeter.x;

            return new Vector2(0, perimeter.y - position);
        }

        public void Dispose()
        {
            foodContainer?.RemoveAllChildren();
            agentContainer?.RemoveAllChildren();
        }
    }
}
