using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer.Simulation.Genome.Strategy;
using Simulation.GameTheory;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Simulation
{
    [ExecuteAlways]
    public class AgentBasedSimultaneousTurnEvoGameTheorySimController : MonoBehaviour
    {
        public int seed = 0;
        public bool runWhenEnteringPlayMode;
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

        public SimultaneousTurnGameAgentHandler simultaneousTurnGameAgentHandler;

        public AgentBasedSimultaneousTurnEvoGameTheorySim sim;
        private int turn;
        
        #region Initial population handling 
        [SerializeField, HideInInspector]
        private List<Type> initialStrategyList;
        [SerializeField, HideInInspector]
        private List<int> initialStrategyCountList;

        [ShowInInspector, DictionaryDrawerSettings(KeyLabel = "Strategy", ValueLabel = "Count")]
        public Dictionary<Type, int> initialAlleleCounts = new();

        [Button]
        private void SaveInitialPopulation()
        {
            initialStrategyList = new List<Type>(initialAlleleCounts.Keys);
            initialStrategyCountList = new List<int>(initialAlleleCounts.Values);
            InitializeSim();
        }
        
        // private Dictionary<Type, int> ConstructInitialStrategiesDictionary()
        // {
        //     var dict = new Dictionary<SimultaneousTurnStrategyGene, int>();
        //     for (var i = 0; i < initialStrategyList.Count; i++)
        //     {
        //         dict.Add(initialStrategyList[i], initialStrategyCountList[i]);
        //     }
        //     initialAlleleCounts = dict;
        //     return dict;
        // }

        // private List<SimultaneousTurnCreature> CreateInitialCreatures()
        // {
        //     var initialCreaturesDict = ConstructInitialStrategiesDictionary();
        //     var initialCreatures = new List<SimultaneousTurnCreature>();
        //     var creatureGnome = new SimpleGnome("Blobs", parent: transform);
        //     foreach (var (strategy, count) in initialCreaturesDict) {
        //         for (var i = 0; i < count; i++) {
        //             var creature = creatureGnome.Add<SimultaneousTurnCreature>("blob_skinned", $"Initial {strategy} {i + 1}");
        //             initialCreatures.Add(creature);
        //             creature.strategyGenes = (SimultaneousTurnGenome)new Genome(strategy);
        //             creature.home = sim.homes.RandomItem();
        //             creature.transform.position = creature.home.transform.position;
        //             simultaneousTurnGameAgentHandler.OnAgentCreated(creature);
        //         }
        //     }
        //
        //     return initialCreatures;
        // }
        
        #endregion

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

        public void OnEnable() => InitializeSim();

        public async void OnDisable() => DisposeSim();
        #endregion

        #region Sim lifecycle
        
        protected virtual void SetStrategyRule() {}
        protected virtual async UniTask OnSimStart() {}
        protected virtual async UniTask OnCycleCompleted() {}
        protected virtual async UniTask OnReset() {}
        
        public void InitializeSim(List<SimultaneousTurnCreature> creatures = null)
        {
            turn = 0;
            SetStrategyRule();
            // creatures ??= CreateInitialCreatures();
            sim = new AgentBasedSimultaneousTurnEvoGameTheorySim(
                transform: transform,
                seed: seed,
                initialBlobs: creatures,
                simultaneousTurnGameAgentHandler,
                skipAnimations: skipAnimations,
                homeOptions: homeOptions,
                treeSelectionOptions: treeSelectionOptions,
                reproductionType: reproductionType
            );
        }

        private void DisposeSim()
        {
            sim?.Dispose();
        }

        public async void Start()
        {
            if (!Application.isPlaying || !runWhenEnteringPlayMode)
                return;
            
            await OnSimStart();
            
            while (true) {
                await sim.SimulateSingleCycle();
                await OnCycleCompleted();
                await UniTask.Delay(1);
            }
        }

        [Title("Controls", HorizontalLine = false)]
        [Button]
        public async void RunTurn()
        {
            if (turn == 0)
                await OnSimStart();
            
            turn++;
            this.Log($"Running turn {turn}");
            await sim.SimulateSingleCycle();
            await OnCycleCompleted();
            this.Log($"Completed turn {turn}");
        }
        
        [Button("Reset")]
        public async void Reset()
        {
            await OnReset();
            DisposeSim();
            InitializeSim();
        }
        #endregion
    }
}
