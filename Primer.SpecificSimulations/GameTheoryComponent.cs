using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Primer;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Simulation.GameTheory
{
    [ExecuteAlways]
    [RequireComponent(typeof(DHRBRewardEditorComponent))]
    public class GameTheoryComponent : MonoBehaviour
    {
        public int seed = 0;
        public bool skipAnimations = false;

        [Required]
        [PropertyOrder(10)]
        [HideLabel, Title("Conflict Resolution Rule")]
        private StrategyRule<DHRB> strategyRule = new DHRBStrategyRule();

        private AgentBasedEvoGameTheorySim<DHRB> _sim;
        private int turn;
        
        #region Initial population handling 
        [SerializeField, HideInInspector]
        private List<DHRB> initialStrategyList;
        [SerializeField, HideInInspector]
        private List<int> initialStrategyCountList;

        [ShowInInspector, DictionaryDrawerSettings(KeyLabel = "Strategy", ValueLabel = "Count")]
        public Dictionary<DHRB, int> initialStrategyCount = new() {
            { DHRB.Dove, 1},
            { DHRB.Hawk, 1},
            { DHRB.Retaliator, 1},
        };

        private void UpdateInitialLists()
        {
            initialStrategyList = new List<DHRB>(initialStrategyCount.Keys);
            initialStrategyCountList = new List<int>(initialStrategyCount.Values);
        }
        
        private Dictionary<DHRB, int> ConstructInitialStrategiesDictionary()
        {
            var dict = new Dictionary<DHRB, int>();
            for (var i = 0; i < initialStrategyList.Count; i++)
            {
                dict.Add(initialStrategyList[i], initialStrategyCountList[i]);
            }
            return dict;
        }
        #endregion

        #region MonoBehaviour method implementations
        private void Update()
        {
            if (Application.isPlaying && Input.GetKeyDown(KeyCode.RightArrow))
                Time.timeScale *= 2;
            if (Application.isPlaying && Input.GetKeyDown(KeyCode.LeftArrow))
                Time.timeScale /= 2;
            if (Application.isPlaying && Input.GetKeyDown(KeyCode.Space))
                Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        }
        
        private void OnValidate() => InitializeSim();

        public void OnEnable()
        {
            InitializeSim();
        }

        public void OnDisable()
        {
            DisposeSim();
        }
        #endregion

        #region Sim lifecycle
        private void InitializeSim()
        {
            turn = 0;
            strategyRule.rewardMatrix = GetComponent<DHRBRewardEditorComponent>().rewardMatrix;
            if (!Application.isPlaying) UpdateInitialLists();

            _sim = new AgentBasedEvoGameTheorySim<DHRB>(
                transform: transform,
                seed: seed,
                // foodPerTurn: foodPerTurn,
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

            while (true) {
                await _sim.SimulateSingleCycle();
                await UniTask.Delay(1000);
            }
        }

        [Title("Controls", HorizontalLine = false)]
        [Button]
        public async void RunTurn()
        {
            turn++;
            this.Log($"Running turn {turn}");
            await _sim.SimulateSingleCycle();
            this.Log($"Completed turn {turn}");
        }
        
        [Button("Reset")]
        public void Reset()
        {
            DisposeSim();
            InitializeSim();
        }
        #endregion

    }
}
