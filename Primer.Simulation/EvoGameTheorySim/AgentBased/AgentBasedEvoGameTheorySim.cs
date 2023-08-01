using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;
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
        
        private readonly Gnome _agentGnome;

        private readonly StrategyRule<T> _strategyRule;

        public Rng rng { get; }
        
        private bool _skipAnimations; 
        public bool skipAnimations { 
            get => _skipAnimations;
            set
            {
                _skipAnimations = value;
                foreach (var tree in trees) tree.skipAnimations = skipAnimations;
                foreach (var agent in agents) agent.skipAnimations = skipAnimations;
            }
        }
        public Transform transform { get; }
        public Component component => transform;
        public IEnumerable<Agent> agents => _agentGnome.ChildComponents<Agent>();

        public AgentBasedEvoGameTheorySim(
            Transform transform,
            int seed,
            Dictionary<T, int> initialBlobs,
            StrategyRule<T> strategyRule,
            bool skipAnimations = false)
        {
            this.transform = transform;
            this._strategyRule = strategyRule;

            rng = new Rng(seed);

            var container = new Gnome(transform);
            _agentGnome = container.AddContainer("Blobs").ScaleChildrenInPlayMode(1);

            this.skipAnimations = skipAnimations;

            PlaceInitialBlobs(initialBlobs);
        }

        private void PlaceInitialBlobs(Dictionary<T, int> initialBlobs)
        {
            _agentGnome.Reset();
            
            // Add up the ints in the dictionary to get the total number of initial blobs
            var blobCount = initialBlobs.Sum(x => x.Value);

            var positions = GetBlobsRestingPosition(blobCount)
                .Select(x => terrain.GetGroundAtLocal(x))
                .Shuffle(rng)
                .ToList();

            var center = positions.Average();

            if (transform is null) Debug.Log("transform is null");
            
            foreach (var (strategy, count) in initialBlobs) {
                for (var i = 0; i < count; i++) {
                    var blob = _agentGnome.AddPrefab<Transform>("blob_skinned", $"Initial blob {strategy}");
                    var agent = blob.GetOrAddComponent<Agent>();
                    agent.strategy = strategy;
                    agent.landscape = terrain;
                    agent.rng = rng;
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
            
            CleanUp();
        }

        private Tween CreateFood()
        {
            return trees.Select(x => x.GrowRandomFruitsUpToTotal(total: 2, delayRange: 1)).RunInParallel();
        }

        private Tween AgentsGoToTrees()
        {
            // Make agents each go to a random tree, but a maximum of two per tree
            var treeSlots = trees.Concat(trees).Shuffle(rng);
            return agents
                .Shuffle(rng)
                .Take(treeSlots.Count)
                .Zip(treeSlots, (agent, tree) => agent.GoToEat(tree))
                .RunInParallel();
        }

        private Tween AgentsEatFood()
        {
            // Make agents eat food, but only agents where goingToEat is not null
            return agents
                .Where(agent => agent.goingToEat != null)
                .GroupBy(x => x.goingToEat)
                .Select(x => Eat(competitors: x.ToList(), tree: x.Key))
                .RunInParallel();
        }

        private Tween AgentsReturnHome()
        {
            return agents
                .Zip(GetBlobsRestingPosition(), (agent, position) => agent.ReturnHome(position))
                .RunInParallel();
        }

        private Tween AgentsReproduceOrDie()
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
                    child.rng = rng;
                    child.strategy = agent.strategy;
                }

                agent.ConsumeEnergy();
            }

            _agentGnome.Purge();

            // Make room for new blobs and fill gaps left by dead blobs
            // await AgentsReturnHome();
            return Tween.noop;
        }
        
        private void CleanUp()
        {
            foreach (var agent in agents) agent.DisposeDetachedFruit();
        }

        private Tween Eat(List<Agent> competitors, FruitTree tree)
        {
            switch (competitors.Count) {
                case 1:
                {
                    var eatTweens = new List<Tween>();
                    competitors[0].energy++;
                    eatTweens.Add(competitors[0].EatAnimation(tree));

                    if (!tree.hasFruit) return eatTweens.RunInParallel();
                    competitors[0].energy++;
                    eatTweens.Add(competitors[0].EatAnimation(tree));
                    return Tween.Series(eatTweens);
                }
                
                case > 1:
                    return _strategyRule.Resolve(competitors, tree);
                
                default:
                    throw new ArgumentException("Cannot eat without agents", nameof(competitors));
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
