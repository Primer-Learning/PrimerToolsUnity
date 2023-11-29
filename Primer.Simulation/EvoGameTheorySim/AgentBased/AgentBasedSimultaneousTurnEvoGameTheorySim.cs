using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using Primer.Simulation.Genome.Strategy;
using Sirenix.Utilities;
using UnityEngine;

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
        private bool _lowRes;
        
        public int turn = 0;
        
        private Landscape terrain => transform.GetComponentInChildren<Landscape>();
        public FruitTree[] trees => transform.GetComponentsInChildren<FruitTree>();
        public Home[] homes => transform.GetComponentsInChildren<Home>();
        
        private Transform _creatureParent;
        private static Pool creaturePool => Pool.GetPool(CommonPrefabs.Blob);

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
            _creatureParent.ChildComponents<SimultaneousTurnCreature>().Where(x => x.gameObject.activeSelf);
        public IEnumerable<SimultaneousTurnStrategyGene> alleles => creatures.SelectMany(x => x.strategyGenes.GetAlleles());
        public float GetFrequency(Type allele) => alleles.Count(x => x.GetType() == allele) / (float)alleles.Count();
        public int currentCreatureCount => _creatureParent.GetActiveChildren().Count();
        
        // Constructor that accepts a list of creatures instead of a dictionary
        public AgentBasedSimultaneousTurnEvoGameTheorySim(
            Transform transform,
            List<SimultaneousTurnCreature> initialBlobs,
            SimultaneousTurnGameAgentHandler simultaneousTurnGameAgentHandler,
            Transform creatureParent,
            Rng rng = null,
            bool skipAnimations = false,
            HomeOptions homeOptions = HomeOptions.Random,
            TreeSelectionOptions treeSelectionOptions = TreeSelectionOptions.Random,
            ReproductionType reproductionType = ReproductionType.Asexual,
            bool lowRes = false
            )
        {
            this.transform = transform;
            _creatureParent = creatureParent;
            _simultaneousTurnGameAgentHandler = simultaneousTurnGameAgentHandler;
            _homeOptions = homeOptions;
            _treeSelectionOptions = treeSelectionOptions;
            _reproductionType = reproductionType;
            _lowRes = lowRes;

            this.rng = rng;
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
            _creatureParent.GetChildren().ForEach(x => creaturePool.Return(x));
            
            foreach (var creature in initialBlobs)
            {
                creature.transform.SetParent(_creatureParent.transform);
                creature.gameObject.SetActive(true);
                creature.landscape = terrain;
                creature.rng = rng;
                _simultaneousTurnGameAgentHandler.OnAgentCreated(creature);
            }
            ChooseHomes();
        }

        public Tween CreateFood()
        {
            // foreach (var creature in creatures)
            // {
            //     creature.PurgeStomach();
            // }

            return Tween.Parallel(
                trees.Select(x => x.GrowHighFruits(delayRange: 1)).RunInParallel(),
                trees.Select(x => x.GrowSpecificFruits(new[] { 0, 1 }, delayRange: 1)).RunInParallel()
            );
        }

        public Tween AgentsGoToTreesInOneTween()
        {
            Debug.LogWarning("Using deprecated method AgentsGoToTreesInOneTween");
            
            // Make creatures each go to a random tree, but a maximum of two per tree
            var trees1 = trees.ToList();
            var trees2 = trees.ToList();

            // var goToTreesTweens = Tween.noop;
            // var deathTweens = Tween.noop;
            
            var goToTreesTweenList = new List<Tween>();
            var deathTweenList = new List<Tween>();
            var currentHomes = new List<Home>();
            
            switch (_treeSelectionOptions)
            {
                case TreeSelectionOptions.Random:
                    // Make creatures each go to a random tree, but a maximum of two per tree
                    var treeSlots = trees.Shuffle(rng).Concat(trees.Shuffle(rng)).ToList();
                    var shuffledCreatures = creatures.Shuffle(rng);
                    goToTreesTweenList = new List<Tween>();
                    
                    for (var i = 0; i < shuffledCreatures.Count; i++)
                    {
                        var creature = shuffledCreatures[i];
                        if (i < treeSlots.Count)
                        {
                            var numFruitsClaimed = i / trees.Length;
                            var fruitIndex = creature.OrderFruitByDistance(treeSlots[i])[numFruitsClaimed];
                            goToTreesTweenList.Add(creature.GoToEat(treeSlots[i], fruitIndex: fruitIndex));
                            if (!currentHomes.Contains(creature.home))
                            {
                                currentHomes.Add(creature.home);
                            }
                        }
                        else
                        {
                            deathTweenList.Add(creature.ScaleTo(0).Observe(onComplete: () => creaturePool.Return(creature.transform)));
                        }
                    }
                    break;
                case TreeSelectionOptions.PreferNearest:
                    foreach (var creature in creatures.Shuffle(rng))
                    {
                        if (!currentHomes.Contains(creature.home))
                        {
                            currentHomes.Add(creature.home);
                        }
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
                        deathTweenList.Add(creature.ScaleTo(0).Observe(onComplete: () => creaturePool.Return(creature.transform)));
                    }
                    break;
                default:
                    Debug.LogError("Tree selection option not implemented");
                    return null;
            }
            return Tween.Parallel(
                currentHomes.Select(x => x.Open()).RunInParallel(),
                goToTreesTweenList.RunInParallel() with {delay = 0.25f},
                currentHomes.Select(x => x.Close() with {delay = 0.5f}).RunInParallel(),
                deathTweenList.RunInParallel() with { delay = 0.25f }
            );
        }
        public IEnumerable<Tween> AgentsGoToTrees()
        {
            // Make creatures each go to a random tree, but a maximum of two per tree
            var trees1 = trees.ToList();
            var trees2 = trees.ToList();
            
            var goToTreesTweenList = new List<Tween>();
            var deathTweenList = new List<Tween>();
            var currentHomes = new List<Home>();
            
            switch (_treeSelectionOptions)
            {
                case TreeSelectionOptions.Random:
                    // Make creatures each go to a random tree, but a maximum of two per tree
                    var treeSlots = trees.Shuffle(rng).Concat(trees.Shuffle(rng)).ToList();
                    var shuffledCreatures = creatures.Shuffle(rng);
                    goToTreesTweenList = new List<Tween>();
                    
                    for (var i = 0; i < shuffledCreatures.Count; i++)
                    {
                        var creature = shuffledCreatures[i];
                        creature.TouchGround();
                        if (i < treeSlots.Count)
                        {
                            var numFruitsClaimed = i / trees.Length;
                            var fruitIndex = creature.OrderFruitByDistance(treeSlots[i])[numFruitsClaimed];
                            goToTreesTweenList.Add(creature.GoToEat(treeSlots[i], fruitIndex: fruitIndex));
                            if (!currentHomes.Contains(creature.home))
                            {
                                currentHomes.Add(creature.home);
                            }
                        }
                        else
                        {
                            deathTweenList.Add(creature.ScaleTo(0).Observe(onComplete: () => creaturePool.Return(creature.transform)));
                        }
                    }
                    break;
                case TreeSelectionOptions.PreferNearest:
                    foreach (var creature in creatures.Shuffle(rng))
                    {
                        if (!currentHomes.Contains(creature.home))
                        {
                            currentHomes.Add(creature.home);
                        }
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
                        deathTweenList.Add(creature.ScaleTo(0).Observe(onComplete: () => creaturePool.Return(creature.transform)));
                    }
                    break;
                default:
                    Debug.LogError("Tree selection option not implemented");
                    yield return null;
                    break;
            }
            yield return Tween.Parallel(
                currentHomes.Select(x => x.Open()).RunInParallel(),
                goToTreesTweenList.RunInParallel() with {delay = 0.25f},
                currentHomes.Select(x => x.Close() with {delay = 0.5f}).RunInParallel()
            );
            if (deathTweenList.Any()) yield return deathTweenList.RunInParallel();
        }

        public IEnumerable<Tween> AgentsEatFood()
        {
            // Create a list of enumerators, one for each tree that has creatures going to eat it
            var enumerables = creatures
                .Where(creature => creature.goingToEat != null)
                .GroupBy(x => x.goingToEat)
                .Select(x => Eat(competitors: x.ToList(), tree: x.Key));
            return enumerables.ParallelizeTweenLists();
        }

        public Tween AgentsEatFoodInOneTween()
        {
            return AgentsEatFood().RunInParallel();
        }

        public Tween AgentsReturnHome()
        {
            ChooseHomes();
            
            // Hax
            var offsets = new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(1, 0, 1),
                new Vector3(-1, 0, 1),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, -1),
                new Vector3(-1, 0, -1),
            };
            offsets.Shuffle(rng);
            
            var returnHomeTweens = new List<Tween>();
            var homesList = new List<Home>();
            var i = 0;
            foreach (var creature in creatures)
            {
                if (!homesList.Contains(creature.home))
                {
                    homesList.Add(creature.home);
                }
                returnHomeTweens.Add(creature.ReturnHome(offset: offsets[i % offsets.Count]));
                i++;
            }

            return Tween.Parallel(
                homesList.Select(x => x.Open()).RunInParallel(),
                returnHomeTweens.RunInParallel(),
                homesList.Select(x => x.Close() with { delay = 0.75f }).RunInParallel()
            );
        }

        // Only exists to avoid causing compile errors in old scenes
        public Tween AgentsReproduceOrDieInOneTween()
        {
            var willReproduce = new List<SimultaneousTurnCreature>();

            foreach (var creature in creatures)
            {
                if (creature.canReproduce) willReproduce.Add(creature);
                else if (!creature.canSurvive) creaturePool.Return(creature.transform);
                
                creature.energy = 0;
                creature.PurgeStomach();
            }

            
            // Get creatures that can reproduce, then group them by home.
            // Then for each group of creatures, choose two creatures at random and make them reproduce.
            // We loop this way even if reproduction is asexual
            var newAgents = new List<SimultaneousTurnCreature>();
            var creaturesByHome = willReproduce.GroupBy(x => x.home);
            foreach (var home in creaturesByHome)
            {
                var offsets = new List<Vector3>()
                {
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(-1, 0, 0),
                    new Vector3(0, 0, 1),
                    new Vector3(1, 0, 1),
                    new Vector3(-1, 0, 1),
                    new Vector3(0, 0, -1),
                    new Vector3(1, 0, -1),
                    new Vector3(-1, 0, -1),
                };

                // var filledSlots = Array.CreateInstance(true
                
                var creaturesInHome = home.Shuffle(rng).ToList();

                var invalidOffsets = new List<int>();
                foreach (var creature in creaturesInHome)
                {
                    foreach (var offset in offsets)
                    {
                        if (Vector3.SqrMagnitude(creature.transform.position - offset) < 0.001f)
                        {
                            invalidOffsets.Add(offsets.IndexOf(offset));
                        }
                    }
                }
                for (var index = invalidOffsets.Count - 1; index >= 0; index--)
                {
                    offsets.RemoveAt(index);
                }
                Debug.Log(offsets);
                
                while (creaturesInHome.Count > 1)
                {
                    var (first, second) = creaturesInHome.Take(2).ToList();
                    
                    // Pass the parents in opposite orders so it works in the asexual case
                    Vector3 childPosition = default;
                    if (offsets.Any())
                    {
                        var index = Rng.RangeInt(offsets.Count);
                        childPosition = offsets[index];
                        offsets.RemoveAt(index);
                    }
                    newAgents.Add(CreateChild(first, second, localPosition: childPosition));
                    newAgents.Add(CreateChild(second, first, localPosition: childPosition));
                    
                    creaturesInHome.Remove(first);
                    creaturesInHome.Remove(second);
                }
                
                // Check if anyone is left over
                if (creaturesInHome.Count == 1)
                {
                    var creature = creaturesInHome.First();
                    newAgents.Add(CreateChild(creature, null));
                }
            }
            
            return newAgents.Select(x => x.ScaleTo(1)).RunInParallel();
        }

        public IEnumerable<Tween> AgentsReproduceOrDie(bool log = false)
        {
            var deathTweens =
                creatures.Select(x => x.ScaleTo(0).Observe(onComplete: () => creaturePool.Return(x.transform)));
            
            var willReproduce = new List<SimultaneousTurnCreature>();

            foreach (var creature in creatures)
            {
                var survive = creature.canSurvive;
                if (log) Debug.Log($"{creature.name} can survive: {survive}");
                if (survive) willReproduce.Add(creature);
                var reproduce = creature.canReproduce;
                if (log) Debug.Log($"{creature.name} can reproduce: {reproduce}");
                if (reproduce) willReproduce.Add(creature);

                creature.energy = 0;
                creature.PurgeStomach();
            }

            // Get creatures that can reproduce, then group them by home.
            // Then for each group of creatures, choose two creatures at random and make them reproduce.
            // We loop this way even if reproduction is asexual
            var newAgents = new List<SimultaneousTurnCreature>();
            var creaturesByHome = willReproduce.GroupBy(x => x.home);
            foreach (var home in creaturesByHome)
            {
                var offsetsBank = new List<Vector3>()
                {
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(-1, 0, 0),
                    new Vector3(0, 0, 1),
                    new Vector3(1, 0, 1),
                    new Vector3(-1, 0, 1),
                    new Vector3(0, 0, -1),
                    new Vector3(1, 0, -1),
                    new Vector3(-1, 0, -1),
                };
                var offsets = new List<Vector3>(offsetsBank);
                var creaturesInHome = home.Shuffle(rng).ToList();

                var invalidOffsets = new List<int>();
                foreach (var creature in creaturesInHome)
                {
                    foreach (var offset in offsets)
                    {
                        if (Vector3.SqrMagnitude(creature.transform.position - creature.home.transform.position - offset) < 0.0001f)
                        {
                            invalidOffsets.Add(offsets.IndexOf(offset));
                        }
                    }
                }
                for (var index = invalidOffsets.Count - 1; index >= 0; index--)
                {
                    if (offsets.Count == 0) break;
                    offsets.RemoveAt(invalidOffsets[index]);
                }
                if (offsets.Count == 0)
                {
                    offsets = new List<Vector3>(offsetsBank);
                }
                
                while (creaturesInHome.Count > 1)
                {
                    var (first, second) = creaturesInHome.Take(2).ToList();
                    
                    // Pass the parents in opposite orders so it works in the asexual case
                    Vector3 childPosition = default;
                    if (offsets.Any())
                    {
                        var index = rng.RangeInt(offsets.Count);
                        childPosition = offsets[index];
                        offsets.RemoveAt(index);
                    }
                    newAgents.Add(CreateChild(first, second, localPosition: childPosition));
                    if (offsets.Count == 0)
                    {
                        offsets = new List<Vector3>(offsetsBank);
                    }
                    childPosition = default;
                    if (offsets.Any())
                    {
                        var index = rng.RangeInt(offsets.Count);
                        childPosition = offsets[index];
                        offsets.RemoveAt(index);
                    }
                    newAgents.Add(CreateChild(second, first, localPosition: childPosition));
                    if (offsets.Count == 0)
                    {
                        offsets = new List<Vector3>(offsetsBank);
                    }
                    
                    creaturesInHome.Remove(first);
                    creaturesInHome.Remove(second);
                }

                // Check if anyone is left over
                if (creaturesInHome.Count == 1)
                {
                    var creature = creaturesInHome.First();
                    newAgents.Add(CreateChild(creature, null));
                }
            }
            
            yield return newAgents.Select(x => x.ScaleTo(1)).RunInParallel();
            yield return deathTweens.RunInParallel();
        }

        private SimultaneousTurnCreature CreateChild(SimultaneousTurnCreature firstParent, SimultaneousTurnCreature secondParent, Vector3 localPosition = default)
        {
            var child = _creatureParent.GetPrefabInstance(CommonPrefabs.Blob).GetOrAddComponent<SimultaneousTurnCreature>();
            if (_lowRes) child.blob.SwapMesh();
            else child.blob.SwapMesh(PrimerBlob.MeshType.HighPolySkinned);
            
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
            child.transform.localPosition = firstParent.home.transform.localPosition + localPosition;
            child.transform.localRotation = Quaternion.Euler(0, Rng.RangeInt(60, 300), 0);
            _simultaneousTurnGameAgentHandler.OnAgentCreated(child);
            child.rng = rng;
            child.transform.localScale = Vector3.zero;
            child.energy = 0;
            child.TouchGround();

            return child;
        }

        private IEnumerable<Tween> Eat(List<SimultaneousTurnCreature> competitors, FruitTree tree)
        {
            switch (competitors.Count) {
                case 1:
                {
                    var eatTweens = new List<Tween>();
                    competitors[0].energy++;
                    var fruits = competitors[0].OrderFruitByDistance(tree);
                    yield return competitors[0].EatAnimation(tree, fruits[0]);

                    if (!tree.hasFruit) break;
                    competitors[0].energy++;
                    fruits = competitors[0].OrderFruitByDistance(tree);
                    yield return competitors[0].EatAnimation(tree, fruits[0]);
                    break;
                }
                
                case 2:

                    foreach (var tween in _simultaneousTurnGameAgentHandler.Resolve(competitors, tree))
                    {
                        yield return tween;
                    }
                    break;
                
                default:
                    throw new ArgumentException("Cannot eat without creatures", nameof(competitors));
            }
        }

        private void ChooseHomes()
        {
            switch (_homeOptions)
            {
                case HomeOptions.Random:
                    creatures.ForEach(x => x.home = homes.RandomItem(rng));
                    break;
                case HomeOptions.ChooseNearestEveryDay:
                    creatures.ForEach(x => x.home = homes.OrderBy(y => (x.transform.position - y.transform.position).sqrMagnitude).First());
                    break;
                case HomeOptions.Keep:
                    creatures.ForEach(x => x.home ??= homes.RandomItem(rng));
                    break;
                default:
                    Debug.LogError("Home option not implemented");
                    break;
            }
        }

        public void Dispose()
        {
            _creatureParent.GetChildren().ForEach(x => creaturePool.Return(x));
            transform.gameObject.SetActive(false);
        }
    }
}
