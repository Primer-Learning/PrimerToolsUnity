using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Simulation;
using Primer.Timeline;
using UnityEngine;

namespace Simulation.GameTheory
{
    public class AgentBasedEvoGameTheorySim<T> : ISimulation, IPrimer, IDisposable where T : Enum
    {
        private int turn = 0;
        private readonly int foodPerTurn;
        
        private Landscape terrain => transform.GetComponentInChildren<Landscape>();
        private FruitTree[] trees => transform.GetComponentsInChildren<FruitTree>();

        // private readonly Gnome foodContainer;
        private readonly Gnome _agentGnome;

        private readonly StrategyRule<T> _strategyRule;

        public Rng rng { get; }
        // public Landscape terrain { get; }
        public bool skipAnimations { get; init; }
        public Transform transform { get; }
        public Component component => transform;
        private IEnumerable<Agent> agents => _agentGnome.ChildComponents<Agent>();

        public AgentBasedEvoGameTheorySim(
            Transform transform,
            int seed,
            // int foodPerTurn,
            Dictionary<T, int> initialBlobs,
            StrategyRule<T> strategyRule)
        {
            // this.foodPerTurn = foodPerTurn;
            this.transform = transform;
            this._strategyRule = strategyRule;

            rng = new Rng(seed);

            var container = new Gnome(transform);
            // foodContainer = gnome.AddContainer("Food").ScaleGrandchildrenInPlayMode(1);
            _agentGnome = container.AddContainer("Blobs").ScaleChildrenInPlayMode(1);

            PlaceInitialBlobs(initialBlobs);
        }

        private void PlaceInitialBlobs(Dictionary<T, int> initialBlobs)
        {
            _agentGnome.Reset();
            
            // Add up the ints in the dictionary to get the total number of initial blobs
            var blobCount = initialBlobs.Sum(x => x.Value);

            var positions = GetBlobsRestingPosition(blobCount)
                .Select(x => terrain.GetGroundAtLocal(x))
                .Shuffle()
                .ToList();

            var center = positions.Average();

            if (transform is null) Debug.Log("transform is null");
            
            foreach (var (strategy, count) in initialBlobs) {
                for (var i = 0; i < count; i++) {
                    var blob = _agentGnome.AddPrefab<Transform>("blob_skinned", $"Initial blob {strategy}");
                    var agent = blob.GetOrAddComponent<Agent>();
                    agent.strategy = strategy;
                    agent.landscape = terrain;
                    _strategyRule.OnAgentCreated(agent);
                    blob.position = positions[i];
                    blob.LookAt(center);
                }
            }

            _agentGnome.Purge(defer: true);
        }

        public async UniTask SimulateSingleCycle()
        {
            turn++;
            await CreateFood();
            await AgentsGoToTrees();
            await AgentsEatFood();
            await AgentsReturnHome();
            await AgentsReproduceOrDie();
        }

        private async UniTask CreateFood()
        {
            foreach (var tree in trees)
            {
                tree.GrowRandomFruitsUpToTotal(total: 2, delayRange: 1).PlayAndForget();
            }
        
            // Give time for the food to be scale up
            await UniTask.Delay(500);
        }

        private UniTask AgentsGoToTrees()
        {
            // return agents
            //     .Select(agent => agent.GoToEat(trees.RandomItem()))
            //     .RunInParallel();
            
            // Make agents go to trees, but a maximum of two per tree
            var agentsPerTree = 2;
            return agents
                .Shuffle()
                .Take(agentsPerTree * trees.Length)
                .Select((agent, index) => (agent, index))
                .GroupBy(x => x.index / agentsPerTree)
                .Select(x => x.Select(y => y.agent).ToList())
                .Zip(trees, (agentsAtTree, tree) => agentsAtTree.Select(agent => agent.GoToEat(tree)))
                .SelectMany(x => x)
                .RunInParallel();
        }

        private async UniTask AgentsEatFood()
        {
            // Make agents eat food, but only agents where gointToEat is not null
            await agents
                .Where(agent => agent.goingToEat != null)
                .GroupBy(x => x.goingToEat)
                .Select(x => Eat(competitors: x.ToList(), tree: x.Key))
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
            // foodContainer.RemoveAllChildren();
            _agentGnome.Reset();

            foreach (var agent in agents) {
                if (agent.canSurvive)
                    _agentGnome.Insert(agent);

                if (agent.canReproduce)
                {
                    var child = _agentGnome.Add(agent, $"Blob born in {turn}");
                    child.ConsumeEnergy();
                }

                agent.ConsumeEnergy();
            }

            _agentGnome.Purge();

            // Make room for new blobs and fill gaps left by dead blobs
            // await AgentsReturnHome();
        }

        private async UniTask Eat(List<Agent> competitors, FruitTree tree)
        {
            switch (competitors.Count) {
                case 0:
                    throw new ArgumentException("Cannot eat without agents", nameof(competitors));

                case 1:
                    await competitors[0].Eat(tree);

                    if (tree.hasFruit)
                        await competitors[0].Eat(tree);

                    return;

                case > 1:
                    await _strategyRule.Resolve(competitors, tree);
                    return;
            }
        }

        private IEnumerable<Vector2> GetBlobsRestingPosition(int? blobCount = null)
        {
            float margin = terrain.roundingRadius;
            var offset = Vector2.one * margin;
            var perimeter = terrain.size - offset * 2;
            var edgeLength = perimeter.x * 2 + perimeter.y * 2;
            var agentCount = blobCount ?? _agentGnome.childCount;
            var positions = edgeLength / agentCount;
            var centerInSlot = positions / 2;
            var centerInTerrain = terrain.size / -2;
        
            for (var i = 0; i < agentCount; i++) {
                var linearPosition = positions * i + centerInSlot;
                var perimeterPosition = PositionToPerimeter(perimeter, linearPosition);
                yield return perimeterPosition + offset + centerInTerrain;
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
            _agentGnome?.RemoveAllChildren();
        }
    }
}
