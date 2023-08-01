using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Primer;
using Simulation.GameTheory;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Simulation
{
    [ExecuteAlways]
    public class AgentBasedEvoGameTheorySimController<T> : MonoBehaviour where T : Enum
    {
        public int seed = 0;
        public bool skipAnimations = false;

        [Required]
        [PropertyOrder(10)]
        [HideLabel, Title("Conflict Resolution Rule")]
        protected StrategyRule<T> strategyRule;

        protected AgentBasedEvoGameTheorySim<T> _sim;
        private int turn;
        
        #region Initial population handling 
        [SerializeField, HideInInspector]
        private List<T> initialStrategyList;
        [SerializeField, HideInInspector]
        private List<int> initialStrategyCountList;

        [FormerlySerializedAs("initialStrategyCount")] [ShowInInspector, DictionaryDrawerSettings(KeyLabel = "Strategy", ValueLabel = "Count")]
        public Dictionary<T, int> initialAlleleCounts = new() {
        };

        [Button]
        private void SaveInitialPopulation()
        {
            initialStrategyList = new List<T>(initialAlleleCounts.Keys);
            initialStrategyCountList = new List<int>(initialAlleleCounts.Values);
            InitializeSim();
        }
        
        private Dictionary<T, int> ConstructInitialStrategiesDictionary()
        {
            var dict = new Dictionary<T, int>();
            for (var i = 0; i < initialStrategyList.Count; i++)
            {
                dict.Add(initialStrategyList[i], initialStrategyCountList[i]);
            }
            initialAlleleCounts = dict;
            return dict;
        }
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

        private void OnValueChange() => InitializeSim();

        public void OnEnable() => InitializeSim();

        public async void OnDisable() => DisposeSim();
        #endregion

        #region Sim lifecycle
        
        protected virtual void SetStrategyRule() {}
        protected virtual async UniTask OnSimStart() {}
        protected virtual async UniTask OnCycleCompleted() {}
        protected virtual async UniTask OnReset() {}
        
        private void InitializeSim()
        {
            turn = 0;
            SetStrategyRule();

            _sim = new AgentBasedEvoGameTheorySim<T>(
                transform: transform,
                seed: seed,
                initialBlobs: ConstructInitialStrategiesDictionary(),
                strategyRule
            ) {
                skipAnimations = skipAnimations,
            };
        }

        private void DisposeSim()
        {
            _sim?.Dispose();
        }

        public async void Start()
        {
            if (!Application.isPlaying)
                return;

            await OnSimStart();
            
            while (true) {
                await _sim.SimulateSingleCycle();
                await UniTask.Delay(1000);
                await OnCycleCompleted();
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
            await _sim.SimulateSingleCycle();
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
