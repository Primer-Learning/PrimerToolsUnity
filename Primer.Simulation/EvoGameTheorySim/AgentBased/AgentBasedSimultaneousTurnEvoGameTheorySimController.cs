using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Simulation.Genome.Strategy;
using Simulation.GameTheory;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Primer.Simulation
{
    [ExecuteAlways]
    public class AgentBasedSimultaneousTurnEvoGameTheorySimController : MonoBehaviour
    {
        public HomeOptions homeOptions;
        public TreeSelectionOptions treeSelectionOptions;
        public ReproductionType reproductionType;
        
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
        
        private Pool<SimultaneousTurnCreature> _creaturePool;
        public Pool<SimultaneousTurnCreature> creaturePool
        {
            get
            {
                if (_creaturePool != null && _creaturePool.transform) return _creaturePool;

                new SimpleGnome("Blobs", parent: transform);
                _creaturePool = new Pool<SimultaneousTurnCreature>("Blobs", parent: transform);
                _creaturePool.prefab = Resources.Load<GameObject>("blob_skinned");
                return _creaturePool;
            }
        } 

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
        
        public AgentBasedSimultaneousTurnEvoGameTheorySim sim;
        public IEnumerable<Home> homes => placer.transform.GetComponentsInChildren<Home>();
        public IEnumerable<FruitTree> trees => placer.transform.GetComponentsInChildren<FruitTree>();
        public IEnumerable<SimultaneousTurnCreature> creatures =>
            transform.Find("Blobs").GetComponentsInChildren<SimultaneousTurnCreature>();
        public PoissonPrefabPlacer placer => transform.GetComponentInChildren<PoissonPrefabPlacer>();
        public Landscape terrain => transform.GetComponentInChildren<Landscape>();
        private int turn;
        
        public SimultaneousTurnCreature AddSimultaneousTurnCreature(string creatureName, SimultaneousTurnStrategyGene gene, bool initialEnergy = false, Home home = null)
        {
            var t = creaturePool.Add();
            var simultaneousTurnCreature = t.GetOrAddComponent<SimultaneousTurnCreature>();
            simultaneousTurnCreature.energy = initialEnergy ? 1 : 0;
            simultaneousTurnCreature.home = home ? home : homes.RandomItem();
            simultaneousTurnCreature.transform.position = simultaneousTurnCreature.home.transform.position;
            simultaneousTurnCreature.transform.localScale = Vector3.one;
            simultaneousTurnCreature.strategyGenes = new SimultaneousTurnGenome(gene);
            simultaneousTurnGameAgentHandler.OnAgentCreated(simultaneousTurnCreature);
            return simultaneousTurnCreature;
        }

        #region MonoBehaviour method implementations
        private void Update()
        {
            if (Application.isPlaying && Input.GetKeyDown(KeyCode.RightArrow))
                Time.timeScale = Math.Min(100, Time.timeScale * 2);
            if (Application.isPlaying && Input.GetKeyDown(KeyCode.LeftArrow))
                Time.timeScale /= 2;
            if (Application.isPlaying && Input.GetKeyDown(KeyCode.Space))
                Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        }

        public void Awake()
        {
            var terrainAndObjectGnome = new SimpleGnome(transform);
            var newPlacer = terrainAndObjectGnome.Add<PoissonPrefabPlacer>("Trees");
            newPlacer.prefab1 = Resources.Load<Transform>("mango tree medium");
            newPlacer.prefab2 = Resources.Load<Transform>("home");
            newPlacer.landscape = terrainAndObjectGnome.Add<Landscape>("Terrain");
        }

        #endregion

        #region Sim lifecycle
        
        protected virtual void SetStrategyRule() {}
        
        public void InitializeSim(List<SimultaneousTurnCreature> creatures, Rng rng)
        {
            turn = 0;
            SetStrategyRule();
            sim = new AgentBasedSimultaneousTurnEvoGameTheorySim(
                transform,
                creatures,
                simultaneousTurnGameAgentHandler,
                 _creaturePool,
                rng: rng,
                skipAnimations: skipAnimations,
                homeOptions: homeOptions,
                treeSelectionOptions: treeSelectionOptions,
                reproductionType: reproductionType
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
                trees.Select(x => x.ScaleTo(1) with { delay = Rng.RangeFloat(0.2f) }).RunInParallel(),
                homes.Select(x => x.ScaleTo(1) with { delay = Rng.RangeFloat(0.2f) }).RunInParallel()
            );
        }
    }
}
