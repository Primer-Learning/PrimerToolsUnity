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
        private readonly int foodPerTurn;

        private readonly Landscape terrain;
        private readonly Container foodContainer;
        private readonly Container agentContainer;

        private readonly ConflictResolutionRule conflictResolutionRule;

        public Transform transform { get; }
        public Component component => transform;
        public Vector2 size => new(terrain.size.x, terrain.size.z);
        public bool skipAnimations { get; init; }
        private IEnumerable<Agent> agents => agentContainer.ChildComponents<Agent>();

        public EvolutionSimulation(
            Transform transform,
            int foodPerTurn,
            int initialBlobs,
            ConflictResolutionRule conflictResolutionRule)
        {
            this.foodPerTurn = foodPerTurn;
            this.transform = transform;
            this.conflictResolutionRule = conflictResolutionRule;

            terrain = transform.GetOrAddComponent<Landscape>();

            var container = new Container(transform);
            foodContainer = container.AddContainer("Food").ScaleChildrenInPlayMode();
            agentContainer = container.AddContainer("Blobs").ScaleChildrenInPlayMode();

            PlaceInitialBlobs(initialBlobs);
        }

        private void PlaceInitialBlobs(int blobCount)
        {
            agentContainer.Reset();

            var positions = GetBlobsRestingPosition(blobCount)
                .Select(x => terrain.GetGroundAtLocal(x))
                .ToList();

            var center = positions.Average();

            foreach (var position in positions) {
                var blob = agentContainer.AddPrefab<Transform>("blob_skinned");
                var agent = blob.GetOrAddComponent<Agent>();
                conflictResolutionRule.OnAgentCreated(agent);
                blob.position = position;
                blob.LookAt(center);
            }

            agentContainer.Purge(defer: true);
        }

        public async UniTask SimulateSingleCycle()
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

            return agents
                .Select(agent => agent.GoToEat(food.RandomItem()))
                .RunInParallel();
        }

        private UniTask AgentsEatFood()
        {
            return agents
                .GroupBy(x => x.goingToEat)
                .Select(x => Eat(competitors: x.ToList(), food: x.Key))
                .RunInParallel();
        }

        private UniTask AgentsReturnHome()
        {
            return agents
                .Zip(GetBlobsRestingPosition(), (agent, position) => agent.ReturnHome(position))
                .RunInParallel();
        }

        private async UniTask AgentsReproduceOrDie()
        {
            agentContainer.Reset();

            foreach (var agent in agents) {
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

        private async UniTask Eat(List<Agent> competitors, Food food)
        {
            switch (competitors.Count) {
                case 0:
                    throw new ArgumentException("Cannot eat without agents", nameof(competitors));

                case 1:
                    await competitors[0].Eat(food);

                    if (food.hasMore)
                        await competitors[0].Eat(food);

                    return;

                case > 1:
                    conflictResolutionRule.Resolve(competitors, food);
                    return;
            }
        }

        private IEnumerable<Vector2> GetBlobsRestingPosition(int? blobCount = null)
        {
            const int margin = 2;
            var offset = Vector2.one * margin;
            var perimeter = size - offset * 2;
            var edgeLength = perimeter.x * 2 + perimeter.y * 2;
            var agentCount = blobCount ?? agents.Count();
            var positions = edgeLength / agentCount;
            var centerInSlot = positions / 2;

            for (var i = 0; i < agentCount; i++) {
                var linearPosition = positions * i + centerInSlot;
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
