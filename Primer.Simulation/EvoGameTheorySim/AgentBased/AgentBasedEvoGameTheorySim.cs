using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using UnityEngine;

namespace Simulation.GameTheory
{
    public class AgentBasedEvoGameTheorySim<T> : ISimulation, IPrimer, IDisposable where T : Enum
    {
        public int turn = 0;
        private readonly int foodPerTurn;
        
        private Landscape terrain => transform.GetComponentInChildren<Landscape>();
        public FruitTree[] trees => transform.GetComponentsInChildren<FruitTree>();
        
        public readonly SimpleGnome creatureGnome;

        private readonly StrategyRule<T> _strategyRule;

        public Rng rng { get; }
        
        private bool _skipAnimations; 
        public bool skipAnimations { 
            get => _skipAnimations;
            set
            {
                _skipAnimations = value;
                foreach (var tree in trees) tree.skipAnimations = skipAnimations;
                foreach (var agent in creatures) agent.skipAnimations = skipAnimations;
            }
        }
        public Transform transform { get; }
        public Component component => transform;
        public IEnumerable<Creature> creatures => creatureGnome.ChildComponents<Creature>();
        public int currentCreatureCount => creatureGnome.activeChildCount;

        
        // Constructor that creates blobs from a dictionary of strategies and counts
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

            creatureGnome = new SimpleGnome("Blobs", parent: transform);

            this.skipAnimations = skipAnimations;

            PlaceInitialBlobs(initialBlobs);

            foreach (var tree in trees)
            {
                tree.rng = rng;
            }
        }
        
        // Constructor that accepts a list of creatures instead of a dictionary
        public AgentBasedEvoGameTheorySim(
            Transform transform,
            int seed,
            List<Creature> initialBlobs,
            StrategyRule<T> strategyRule,
            bool skipAnimations = false)
        {
            this.transform = transform;
            this._strategyRule = strategyRule;

            rng = new Rng(seed);

            creatureGnome = new SimpleGnome("Blobs", parent: transform);

            this.skipAnimations = skipAnimations;

            ConfigureInitialBlobs(initialBlobs);

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
                    var creature = creatureGnome.Add<Creature>("blob_skinned", $"Initial {strategy} {i + 1}");
                    creature.strategy = strategy;
                    creature.landscape = terrain;
                    creature.rng = rng;
                    _strategyRule.OnAgentCreated(creature);
                    creature.transform.position = positions[i];
                    creature.transform.LookAt(center);
                }
            }
        }

        private void ConfigureInitialBlobs(List<Creature> initialBlobs)
        {
            creatureGnome.Reset();
            
            foreach (var creature in initialBlobs)
            {
                creature.transform.SetParent(creatureGnome);
                creature.gameObject.SetActive(true);
                creature.landscape = terrain;
                creature.rng = rng;
                _strategyRule.OnAgentCreated(creature);
            }
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
            foreach (var creature in creatures)
            {
                creature.PurgeStomach();
            }
            
            return trees.Select(x => x.GrowRandomFruitsUpToTotal(total: 2, delayRange: 1)).RunInParallel();
        }

        public Tween AgentsGoToTrees()
        {
            // Make creatures each go to a random tree, but a maximum of two per tree
            var treeSlots = trees.Concat(trees).Shuffle(rng);
            return creatures
                .Shuffle(rng)
                .Take(treeSlots.Count)
                .Zip(treeSlots, (creature, tree) => creature.GoToEat(tree))
                .RunInParallel();
        }

        public Tween AgentsEatFood()
        {
            // Make creatures eat food, but only creatures where goingToEat is not null
            return creatures
                .Where(creature => creature.goingToEat != null)
                .GroupBy(x => x.goingToEat)
                .Select(x => Eat(competitors: x.ToList(), tree: x.Key))
                .RunInParallel();
        }

        public Tween AgentsReturnHome()
        {
            return creatures
                .Zip(GetBlobsRestingPosition(), (creature, position) => creature.ReturnHome(position))
                .RunInParallel();
        }

        public Tween AgentsReproduceOrDie()
        {
            var newAgents = new List<Creature>();
            
            foreach (var creature in creatures) {
                creature.PurgeStomach();

                if (!creature.canSurvive)
                {
                    creature.ShrinkAndDispose();
                    creature.ConsumeEnergy();
                    continue;
                }

                if (creature.canReproduce)
                {
                    var child = creatureGnome.Add<Creature>("blob_skinned", $"{creature.strategy} {newAgents.Count(x => x.strategy.Equals(creature.strategy)) + 1} born turn {turn}");
                    child.strategy = creature.strategy;
                    child.landscape = terrain;
                    child.transform.localPosition = creature.transform.localPosition;
                    _strategyRule.OnAgentCreated(child);
                    child.rng = rng;
                    child.transform.localScale = Vector3.zero;
                    newAgents.Add(child);
                }
                
                creature.ConsumeEnergy();
            }
            
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
                    throw new ArgumentException("Cannot eat without creatures", nameof(competitors));
            }
        }

        private IEnumerable<Vector2> GetBlobsRestingPosition(int? blobCount = null)
        {
            float margin = terrain.roundingRadius;
            var offset = Vector2.one * margin;
            var perimeter = terrain.size - offset * 2;
            var edgeLength = perimeter.x * 2 + perimeter.y * 2;
            var creatureCount = blobCount ?? creatureGnome.activeChildCount;
            var positions = edgeLength / creatureCount;
            var centerInSlot = positions / 2;
            var centerInTerrain = terrain.size / -2;
        
            
            for (var i = 0; i < creatureCount; i++) {
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
        
        private int StrategyCount(Enum strategy)
        {
            return creatures.Count(x => x.strategy.Equals(strategy));
        }

        public void Dispose()
        {
            creatureGnome?.Reset(hard: true);
        }
    }
}
