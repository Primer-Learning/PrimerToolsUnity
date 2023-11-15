using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Simulation.Genome.Strategy;
using Simulation.GameTheory;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Simulation
{
    [ExecuteAlways]
    public class AgentBasedSimultaneousTurnEvoGameTheorySimController : MonoBehaviour
    {
        public HomeOptions homeOptions;
        public TreeSelectionOptions treeSelectionOptions;
        public ReproductionType reproductionType;
        public bool lowRes = false;

        public Rng rng;
        
        [SerializeField, HideInInspector]
        private bool _skipAnimations = false;
        [PropertyOrder(-1)]
        [ShowInInspector]
        public bool skipAnimations
        {
            get => _skipAnimations;
            set
            {
                _skipAnimations = value;
                if (sim != null)
                    sim.skipAnimations = value;
            }
        }
        
        public Transform creatureParent => new SimpleGnome("Blobs", parent: transform).transform;

        private SimultaneousTurnGameAgentHandler _simultaneousTurnGameAgentHandler;
        public SimultaneousTurnGameAgentHandler simultaneousTurnGameAgentHandler
        {
            get
            {
                if (_simultaneousTurnGameAgentHandler != null) return _simultaneousTurnGameAgentHandler;

                SetStrategyRule();
                return _simultaneousTurnGameAgentHandler;
            }
            set
            {
                _simultaneousTurnGameAgentHandler = value;
            }
        }

        private RewardMatrix _rewardMatrix;
        
        public RewardMatrix rewardMatrix
        {
            get => simultaneousTurnGameAgentHandler.rewardMatrix;
            
        }
        
        public AgentBasedSimultaneousTurnEvoGameTheorySim sim;
        public IEnumerable<Home> homes => placer.transform.Find("Homes").GetComponentsInChildren<Home>(includeInactive: true);
        public IEnumerable<FruitTree> trees => placer.transform.Find("Trees").GetComponentsInChildren<FruitTree>(includeInactive: true);
        public IEnumerable<SimultaneousTurnCreature> creatures =>
            transform.Find("Blobs").GetComponentsInChildren<SimultaneousTurnCreature>();
        public PoissonPrefabPlacer placer => transform.GetComponentInChildren<PoissonPrefabPlacer>();
        public Landscape terrain => transform.GetComponentInChildren<Landscape>();
        private int turn;
        
        public SimultaneousTurnCreature AddSimultaneousTurnCreature(string creatureName, SimultaneousTurnStrategyGene gene, bool initialEnergy = false, Home home = null)
        {
            var t = creatureParent.GetPrefabInstance(CommonPrefabs.Blob);
            var simultaneousTurnCreature = t.GetOrAddComponent<SimultaneousTurnCreature>();
            simultaneousTurnCreature.energy = initialEnergy ? 1 : 0;
            simultaneousTurnCreature.home = home ? home : homes.RandomItem(rng: rng);
            simultaneousTurnCreature.transform.position = simultaneousTurnCreature.home.transform.position;
            simultaneousTurnCreature.transform.localScale = Vector3.one;
            simultaneousTurnCreature.strategyGenes = new SimultaneousTurnGenome(rng, gene);
            simultaneousTurnGameAgentHandler.OnAgentCreated(simultaneousTurnCreature);
            if (lowRes) simultaneousTurnCreature.blob.SwapMesh();
            else simultaneousTurnCreature.blob.SwapMesh(PrimerBlob.MeshType.HighPolySkinned);
            
            return simultaneousTurnCreature;
        }

        #region MonoBehaviour method implementations

        public void Awake()
        {
            var terrainAndObjectGnome = new SimpleGnome(transform);
            var newPlacer = terrainAndObjectGnome.Add<PoissonPrefabPlacer>("Trees");
            newPlacer.prefab1 = CommonPrefabs.MangoTree;
            newPlacer.prefab2 = CommonPrefabs.RockHome;
            newPlacer.landscape = terrainAndObjectGnome.Add<Landscape>("Terrain");
        }

        #endregion

        #region Sim lifecycle
        protected virtual void SetStrategyRule() {}
        
        public void InitializeSim(List<SimultaneousTurnCreature> creatures, Rng rng = null)
        {
            rng ??= this.rng;
            turn = 0;
            SetStrategyRule();
            sim = new AgentBasedSimultaneousTurnEvoGameTheorySim(
                transform,
                creatures,
                simultaneousTurnGameAgentHandler,
                creatureParent,
                rng: rng,
                skipAnimations: skipAnimations,
                homeOptions: homeOptions,
                treeSelectionOptions: treeSelectionOptions,
                reproductionType: reproductionType,
                lowRes: lowRes
            );
        }
        #endregion
        
        public Tween PlaceAndScaleTreesAndHomes(int numTrees, int numHomes)
        {
            placer.numberToPlace1 = numTrees;
            placer.numberToPlace2 = numHomes;
            return PlaceAndScaleTreesAndHomes();
        }

        public Tween PlaceAndScaleTreesAndHomes()
        {
            placer.Place();
            trees.ForEach(x => x.transform.localScale = Vector3.zero);
            homes.ForEach(x => x.transform.localScale = Vector3.zero);

            return Tween.Parallel(
                trees.Select(x => x.ScaleTo(1) with { delay = rng.RangeFloat(0.2f) }).RunInParallel(),
                homes.Select(x => x.ScaleTo(1) with { delay = rng.RangeFloat(0.2f) }).RunInParallel()
            );
        }
        
        public SimultaneousTurnCreature AddCreature(float energy = 0, SimultaneousTurnStrategyGene gene = null, Home home = null)
        {
            var creature = creatureParent.GetPrefabInstance(CommonPrefabs.Blob).GetOrAddComponent<SimultaneousTurnCreature>();
            var t = creature.transform;
            t.localScale = Vector3.one;
            t.localRotation = Quaternion.Euler(0, 180, 0);
                
            creature.energy = energy;
            creature.PurgeStomach();
                
            gene ??= new SimultaneousTurnStrategyGenes.Dove();
            creature.strategyGenes = new SimultaneousTurnGenome(rng, gene);
            simultaneousTurnGameAgentHandler.OnAgentCreated(creature);

            if (home != null)
            {
                creature.home = home;
                creature.transform.position = home.transform.position;
            }
            else
            {
                creature.home = homes.RandomItem(rng: rng);
            }
            
            return creature;
        }
    }
}
