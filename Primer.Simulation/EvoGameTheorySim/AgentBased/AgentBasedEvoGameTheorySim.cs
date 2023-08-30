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
        public FruitTree[] trees => transform.GetComponentsInChildren<FruitTree>();
        
        public readonly Gnome creatureGnome;

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
        public IEnumerable<Creature> agents => creatureGnome.ChildComponents<Creature>();

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
            creatureGnome = container.AddGnome("Blobs").ScaleChildrenInPlayMode(1);

            this.skipAnimations = skipAnimations;

            PlaceInitialBlobs(initialBlobs);

            foreach (var tree in trees)
            {
                tree.rng = rng;
            }
        }

        private void PlaceInitialBlobs(Dictionary<T, int> initialBlobs)
        {
            creatureGnome.Reset();
            
            
            // Add up the ints in the dictionary to get the total number of initial blobs
            var blobCount = initialBlobs.Sum(x => x.Value);
            if (blobCount == 0) return;

            var positions = GetBlobsRestingPosition(blobCount)
                .Select(x => terrain.GetGroundAtLocal(x))
                .Shuffle(rng)
                .ToList();

            var center = positions.Average();

            if (transform is null) Debug.Log("transform is null");
            
            foreach (var (strategy, count) in initialBlobs) {
                for (var i = 0; i < count; i++) {
                    var blob = creatureGnome.AddPrefab<Transform>("blob_skinned", $"Initial blob {strategy}");
                    var agent = blob.GetOrAddComponent<Creature>();
                    agent.strategy = strategy;
                    agent.landscape = terrain;
                    agent.rng = rng;
                    _strategyRule.OnAgentCreated(agent);
                    blob.position = positions[i];
                    blob.LookAt(center);
                }
            }

            creatureGnome.Purge(defer: true);
        }

        public async UniTask SimulateSingleCycle()
        {
            Debug.Log("Simulating single cycle");
            turn++;

            await CreateFood();
            await AgentsGoToTrees();
            await AgentsEatFood();
            await AgentsReturnHome();
            await AgentsReproduceOrDie();
            CleanUp();
        }

        public Tween CreateFood()
        {
            foreach (var agent in agents)
            {
                agent.PurgeStomach();
            }
            
            return trees.Select(x => x.GrowRandomFruitsUpToTotal(total: 2, delayRange: 1)).RunInParallel();
        }

        public Tween AgentsGoToTrees()
        {
            // Make agents each go to a random tree, but a maximum of two per tree
            var treeSlots = trees.Concat(trees).Shuffle(rng);
            return agents
                .Shuffle(rng)
                .Take(treeSlots.Count)
                .Zip(treeSlots, (agent, tree) => agent.GoToEat(tree))
                .RunInParallel();
        }

        public Tween AgentsEatFood()
        {
            // Make agents eat food, but only agents where goingToEat is not null
            return agents
                .Where(agent => agent.goingToEat != null)
                .GroupBy(x => x.goingToEat)
                .Select(x => Eat(competitors: x.ToList(), tree: x.Key))
                .RunInParallel();
        }

        public Tween AgentsReturnHome()
        {
            return agents
                .Zip(GetBlobsRestingPosition(), (agent, position) => agent.ReturnHome(position))
                .RunInParallel();
        }

        public Tween AgentsReproduceOrDie()
        {
            creatureGnome.Reset();

            var newAgents = new List<Creature>();
            foreach (var agent in agents) {
                agent.PurgeStomach();
                
                if (agent.canSurvive)
                    creatureGnome.Insert(agent);

                if (agent.canReproduce)
                {
                    var child = creatureGnome.Add(agent, $"Blob born in {turn}");
                    child.ConsumeEnergy();
                    child.rng = rng;
                    child.strategy = agent.strategy;
                    child.transform.localScale = Vector3.zero;
                    newAgents.Add(child);
                }
                
                agent.ConsumeEnergy();
            }

            creatureGnome.Purge();
            
            return newAgents.Select(x => x.ScaleTo(1)).RunInParallel();
            // return Tween.noop;
        }
        
        public void CleanUp()
        {
        }

        private Tween Eat(List<Creature> competitors, FruitTree tree)
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
            var agentCount = blobCount ?? creatureGnome.childCount;
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
            creatureGnome?.RemoveAllChildren();
        }
    }
}
