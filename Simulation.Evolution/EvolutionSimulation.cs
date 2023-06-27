using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using Primer.Timeline;
using Simulation.Evolution.Extensions;
using UnityEngine;

namespace Simulation.Evolution
{
    public class EvolutionSimulation : ISimulation, IPrimer
    {
        private readonly int initialBlobs;

        private readonly List<Vector2> foodPosition;
        private readonly RandomElevationTerrain terrain;
        private readonly Container foodContainer;
        private readonly Container agentContainer;
        private readonly Vector2 size;

        public Transform transform { get; }
        public Component component => transform;
        public bool skipAnimations { get; init; }

        public EvolutionSimulation(Transform transform, Vector2 size, int foodPerTurn, int initialBlobs)
        {
            this.transform = transform;
            this.size = size;
            this.initialBlobs = initialBlobs;

            foodPosition = PoissonDiscSampler.Rectangular(foodPerTurn, size).ToList();
            terrain = transform.GetOrAddComponent<RandomElevationTerrain>();

            var container = new Container(transform);
            foodContainer = container.AddContainer("Food").ScaleChildrenInPlayMode();
            agentContainer = container.AddContainer("Blobs").ScaleChildrenInPlayMode();
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
            var allAgents = agentContainer.ChildComponents<Agent>().ToList();
            var positions = size.x / allAgents.Count;

            return allAgents.Select((agent, index) => agent.ReturnHome(new Vector2(positions * index, 0)))
                .RunInParallel();
        }

        private UniTask AgentsReproduceOrDie()
        {
            agentContainer.Reset();

            foreach (var agent in agentContainer.ChildComponents<Agent>()) {
                if (agent.canSurvive)
                    agentContainer.Insert(agent);

                if (agent.canReproduce) {
                    var copy = agentContainer.Add(agent);
                }

                agent.ConsumeEnergy();
            }

            agentContainer.Purge();

            // Give time for the new blobs to be scale up and the dead ones to be scale down
            return UniTask.Delay(500);
        }

        private UniTask ResolveConflict(List<Agent> agents, Food food)
        {
            throw new System.NotImplementedException();
        }
    }
}
