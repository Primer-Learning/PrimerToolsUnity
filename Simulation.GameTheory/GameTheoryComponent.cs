using Cysharp.Threading.Tasks;
using Primer;
using Primer.Simulation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Simulation.GameTheory
{
    [ExecuteAlways]
    public class GameTheoryComponent : MonoBehaviour
    {
        // [Min(1)] public int foodPerTurn = 10;
        [Min(1)] public int initialBlobs = 2;
        public int seed = 0;
        public bool skipAnimations = false;

        [SerializeReference]
        [Required]
        [PropertyOrder(10)]
        [HideLabel, Title("Conflict Resolution Rule")]
        public ConflictResolutionRule conflictResolutionRule = new SimpleConflictResolution();

        private AgentBasedEvoGameTheorySim _sim;
        private int turn;

        private void OnValidate() => OnEnable();

        public void OnEnable()
        {
            turn = 0;

            _sim = new AgentBasedEvoGameTheorySim(
                transform: transform,
                seed: seed,
                // foodPerTurn: foodPerTurn,
                initialBlobs: initialBlobs,
                conflictResolutionRule
            ) {
                skipAnimations = skipAnimations,
            };
        }

        public void OnDisable()
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
            OnDisable();
            OnEnable();
        }

    }
}
