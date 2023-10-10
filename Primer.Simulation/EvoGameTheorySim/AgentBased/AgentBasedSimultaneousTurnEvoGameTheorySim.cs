using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using Primer.Simulation.Genome.Strategy;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using SimpleGnome = Primer.SimpleGnome;

namespace Simulation.GameTheory
{
    public enum HomeOptions
    {
        Random,
        ChooseNearestEveryDay,
        Keep
    }

    public enum TreeSelectionOptions
    {
        Random,
        PreferNearest,
    }
    
    public enum ReproductionType
    {
        Sexual,
        Asexual
    }
    
    public class AgentBasedSimultaneousTurnEvoGameTheorySim : ISimulation, IPrimer, IDisposable
    {
        private HomeOptions _homeOptions;
        private TreeSelectionOptions _treeSelectionOptions;
        private ReproductionType _reproductionType;
        
        public int turn = 0;
        
        private Landscape terrain => transform.GetComponentInChildren<Landscape>();
        public FruitTree[] trees => transform.GetComponentsInChildren<FruitTree>();
        public Home[] homes => transform.GetComponentsInChildren<Home>();
        
        public readonly Pool<SimultaneousTurnCreature> creaturePool;

        private readonly SimultaneousTurnGameAgentHandler _simultaneousTurnGameAgentHandler;

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

        public IEnumerable<SimultaneousTurnCreature> creatures =>
            creaturePool.ChildComponents<SimultaneousTurnCreature>().Where(x => x.gameObject.activeSelf);
        public IEnumerable<SimultaneousTurnStrategyGene> alleles => creatures.SelectMany(x => x.strategyGenes.GetAlleles());
        public int currentCreatureCount => creaturePool.activeChildCount;
        
        // Constructor that accepts a list of creatures instead of a dictionary
        public AgentBasedSimultaneousTurnEvoGameTheorySim(
            Transform transform,
            List<SimultaneousTurnCreature> initialBlobs,
            SimultaneousTurnGameAgentHandler simultaneousTurnGameAgentHandler,
            int rngSeed,
            Rng rng = null,
            bool skipAnimations = false,
            HomeOptions homeOptions = HomeOptions.Random,
            TreeSelectionOptions treeSelectionOptions = TreeSelectionOptions.Random,
            ReproductionType reproductionType = ReproductionType.Asexual
            )
        {
            this.transform = transform;
            _simultaneousTurnGameAgentHandler = simultaneousTurnGameAgentHandler;
            _homeOptions = homeOptions;
            _treeSelectionOptions = treeSelectionOptions;
            _reproductionType = reproductionType;

            if (rng != null)
            {
                this.rng = rng;
            }
            else
            {
                this.rng = new Rng(rngSeed);
            }

            creaturePool = new Pool<SimultaneousTurnCreature>("Blobs", parent: transform);
            creaturePool.GetChildren().Where(x => !initialBlobs.Contains(x.GetComponent<SimultaneousTurnCreature>())).ForEach(x => x.gameObject.Dispose());
            creaturePool.prefab = Resources.Load<GameObject>("blob_skinned");
            
            this.skipAnimations = skipAnimations;

            ConfigureInitialBlobs(initialBlobs);

            foreach (var tree in trees)
            {
                tree.rng = rng;
            }
            foreach (var home in homes)
            {
                home.OrderTreesByDistance();
            }
        }

        private void ConfigureInitialBlobs(List<SimultaneousTurnCreature> initialBlobs)
        {
            creaturePool.Reset();
            
            foreach (var creature in initialBlobs)
            {
                creature.transform.SetParent(creaturePool.transform);
                creature.gameObject.SetActive(true);
                creature.landscape = terrain;
                creature.rng = rng;
                _simultaneousTurnGameAgentHandler.OnAgentCreated(creature);
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
            var trees1 = trees.ToList();
            var trees2 = trees.ToList();

            switch (_treeSelectionOptions)
            {
                case TreeSelectionOptions.Random:
                    // Make creatures each go to a random tree, but a maximum of two per tree
                    var treeSlots = trees.Concat(trees).Shuffle(rng);
                    var shuffledCreatures = creatures.Shuffle(rng);
                    var goToTreesTweens = creatures
                        .Take(treeSlots.Count)
                        .Zip(treeSlots, (creature, tree) => creature.GoToEat(tree))
                        .RunInParallel();
                    var deathTweens= shuffledCreatures
                        .Skip(treeSlots.Count)
                        .Select(creature => creature.ScaleTo(0).Observe(onComplete: () => creature.gameObject.SetActive(false)))
                        .RunInParallel();
                    return Tween.Parallel(
                        goToTreesTweens,
                        deathTweens
                    );
                case TreeSelectionOptions.PreferNearest:
                    var goToTreesTweenList = new List<Tween>();
                    var deathTweenList = new List<Tween>();
                    foreach (var creature in creatures.Shuffle(rng))
                    {
                        var foundTree = false;
                        foreach (var tree in creature.home.treesByDistance)
                        {
                            if (!trees1.Contains(tree)) continue;
                            goToTreesTweenList.Add(creature.GoToEat(tree, fruitIndex: 0));
                            trees1.Remove(tree);
                            foundTree = true;
                            break;
                        }
                        if (foundTree) continue;
                        foreach (var tree in creature.home.treesByDistance)
                        {
                            if (!trees2.Contains(tree)) continue;
                            goToTreesTweenList.Add(creature.GoToEat(tree, fruitIndex: 1));
                            trees2.Remove(tree);
                            foundTree = true;
                            break;
                        }
                        if (foundTree) continue;
                        deathTweenList.Add(creature.ScaleTo(0).Observe(onComplete: () => creature.gameObject.SetActive(false)));
                    }
                    return Tween.Parallel(
                        goToTreesTweenList.RunInParallel(),
                        deathTweenList.RunInParallel()
                    );
                default:
                    Debug.LogError("Tree selection option not implemented");
                    return null;
            }
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
                .Zip(ChooseHomes(), (creature, home) => creature.ReturnHome(home))
                .RunInParallel();
        }

        public Tween AgentsReproduceOrDie()
        {
            var newAgents = new List<SimultaneousTurnCreature>();

            foreach (var creature in creatures)
            {
                creature.PurgeStomach();
                if (!creature.canSurvive)
                {
                    // TODO: Add death animation
                    creature.gameObject.SetActive(false);
                    creature.ConsumeEnergy();
                }
            }
            
            // Get creatures that can reproduce, then group them by home.
            // Then for each group of creatures, choose two creatures at random and make them reproduce.
            // We loop this way even if reproduction is asexual
            var creaturesByHome = creatures
                .Where(x => x.canReproduce)
                .GroupBy(x => x.home);

            foreach (var home in creaturesByHome)
            {
                var creaturesInHome = home.Shuffle(rng).ToList();
                while (creaturesInHome.Count > 1)
                {
                    var (first, second) = creaturesInHome.Take(2).ToList();
                    first.PurgeStomach();
                    second.PurgeStomach();
                    
                    // Pass the parents in opposite orders so it works in the asexual case
                    newAgents.Add(CreateChild(first, second));
                    newAgents.Add(CreateChild(second, first));
                    
                    creaturesInHome.Remove(first);
                    creaturesInHome.Remove(second);
                }
                
                // Check if anyone is left over
                if (creaturesInHome.Count == 1)
                {
                    var creature = creaturesInHome.First();
                    creature.PurgeStomach();
                    newAgents.Add(CreateChild(creature, null));
                }
            }
            
            return newAgents.Select(x => x.ScaleTo(1)).RunInParallel();
        }

        private SimultaneousTurnCreature CreateChild(SimultaneousTurnCreature firstParent, SimultaneousTurnCreature secondParent)
        {
            // var childGO = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("blob_skinned")) as GameObject;
            // childGO.transform.parent = creaturePool.transform;

            var child = creaturePool.AddOrActivate();
            child.home = firstParent.home;
            
            // Inheritance depends on reproduction type
            if (_reproductionType == ReproductionType.Asexual || secondParent == null)
            {
                child.strategyGenes = firstParent.strategyGenes;
            }
            else
            {
                child.strategyGenes = firstParent.strategyGenes.SexuallyReproduce(secondParent.strategyGenes);
                // case ReproductionType.SexualHaploid:
                // {
                //     var strategyGenes = firstParent.strategyGenes
                //         .Zip(secondParent.strategyGenes, (a, b) => rng.rand.NextDouble() < 0.5 ? a : b).ToArray();
                //
                //     child.strategyGenes = strategyGenes;
                //     break;
                // }
                // case ReproductionType.SexualDiploid:
                // {
                //     var numGenes = firstParent.strategyGenes.Length;
                //     if (numGenes % 2 != 0)
                //     {
                //         Debug.LogError("Number of genes must be even for diploid reproduction");
                //         return null;
                //     }
                //     var strategyGenes = new Type[numGenes];
                //     for (var i = 0; i < numGenes / 2; i++)
                //     {
                //         strategyGenes[i] = rng.rand.NextDouble() < 0.5 ? firstParent.strategyGenes[i]
                //             : firstParent.strategyGenes[i + numGenes / 2];
                //     }
                //     for (var i = numGenes / 2; i < numGenes; i++)
                //     {
                //         strategyGenes[i] = rng.rand.NextDouble() < 0.5 ? secondParent.strategyGenes[i]
                //             : secondParent.strategyGenes[i - secondParent.strategyGenes.Length / 2];
                //     }
                //     child.strategyGenes = strategyGenes;
                //     break;
                // }
            }

            child.landscape = terrain;
            child.transform.localPosition = firstParent.transform.localPosition;
            _simultaneousTurnGameAgentHandler.OnAgentCreated(child);
            child.rng = rng;
            child.transform.localScale = Vector3.zero;

            return child;
        }

        public void CleanUp()
        {
        }

        private Tween Eat(List<SimultaneousTurnCreature> competitors, FruitTree tree)
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
                    return _simultaneousTurnGameAgentHandler.Resolve(competitors, tree);
                
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
            var creatureCount = blobCount ?? creaturePool.activeChildCount;
            var positions = edgeLength / creatureCount;
            var centerInSlot = positions / 2;
            var centerInTerrain = terrain.size / -2;
            
            for (var i = 0; i < creatureCount; i++) {
                var linearPosition = positions * i + centerInSlot;
                var perimeterPosition = PositionToPerimeter(perimeter, linearPosition);
                yield return perimeterPosition + offset + centerInTerrain;
            }
        }
        private IEnumerable<Home> ChooseHomes()
        {
            switch (_homeOptions)
            {
                case HomeOptions.Random:
                    return creatures.Select(x => homes.RandomItem(rng));
                case HomeOptions.ChooseNearestEveryDay:
                    return creatures.Select(x => homes.OrderBy(y => (x.transform.position - y.transform.position).sqrMagnitude).First());
                case HomeOptions.Keep:
                    return creatures.Select(x => x.home ?? homes.RandomItem(rng));
                default:
                    Debug.LogError("Home option not implemented");
                    return null;
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
            return creatures.Count(x => x.action.Equals(strategy));
        }

        public void Dispose()
        {
            creaturePool?.Reset(hard: true);
        }
        
        // private string StrategyGenesString(Enum[] strategyGenes)
        // {
        //     // Iterate through the enum of type T and count the number of times each strategy appears in the creature's strategy genes
        //     var strategyCounts = new Dictionary<T, int>();
        //     foreach (var strategy in Enum.GetValues(typeof(T)).Cast<T>())
        //     {
        //         strategyCounts.Add(strategy, strategyGenes.Count(x => x.Equals(strategy)));
        //     }
        //     // Return a string of the strategy counts
        //     return string.Join(", ", strategyCounts.Select(x => $"{x.Key}: {x.Value}"));
        // }
    }
}
