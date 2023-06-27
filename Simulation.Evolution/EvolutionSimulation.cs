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
    public class EvolutionSimulation : ISimulation, IPrimer
    {
        private readonly List<Vector2> foodPosition;
        private readonly Landscape terrain;
        private readonly Container foodContainer;
        private readonly Container agentContainer;

        public Transform transform { get; }
        public Component component => transform;
        public Vector2 size => new(terrain.size.x, terrain.size.z);
        public bool skipAnimations { get; init; }

        public EvolutionSimulation(Transform transform, int foodPerTurn, int initialBlobs)
        {
            this.transform = transform;

            terrain = transform.GetOrAddComponent<Landscape>();
            foodPosition = PoissonDiscSampler.Rectangular(foodPerTurn, size).ToList();

            var container = new Container(transform);
            foodContainer = container.AddContainer("Food").ScaleChildrenInPlayMode();
            agentContainer = container.AddContainer("Blobs").ScaleChildrenInPlayMode();

            PlaceInitialBlobs(initialBlobs);
        }

        private void PlaceInitialBlobs(int initialBlobs)
        {
            agentContainer.Reset();

            foreach (var position in GetAgentsRestingPosition(initialBlobs)) {
                var blob = agentContainer.AddPrefab<Transform>("blob_skinned");
                blob.GetOrAddComponent<Agent>();
                blob.position = terrain.GetGroundAt(position);
                blob.LookAt(blob.position + Vector3.forward);
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

            foreach (var point in foodPosition) {
                var item = foodContainer.Add<Food>();
                item.transform.position = terrain.GetGroundAt(point.x, point.y) + Vector3.up * 0.5f;
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
                .Select(
                    group => {
                        var food = group.Key;
                        var agents = group.ToList();
                        return agents.Count == 1 ? agents[0].Eat() : ResolveConflict(agents, food);
                    }
                )
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

        private UniTask ResolveConflict(List<Agent> agents, Food food)
        {
            throw new System.NotImplementedException();
        }

        private IEnumerable<Vector2> GetAgentsRestingPosition()
        {
            return GetAgentsRestingPosition(agentContainer.ChildComponents<Agent>().Count());
        }

        private IEnumerable<Vector2> GetAgentsRestingPosition(int agentCount)
        {
            const int margin = 1;
            var positions = (size.x - margin * 2) / agentCount;
            var offset = positions / 2 + margin;

            for (var i = 0; i < agentCount; i++)
                yield return new Vector2(positions * i + offset, margin);
        }
    }
}
