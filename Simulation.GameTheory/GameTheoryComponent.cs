using Cysharp.Threading.Tasks;
using Primer;
using Primer.Simulation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Simulation.GameTheory
{
    [ExecuteAlways]
    [RequireComponent(typeof(Landscape))]
    public class GameTheoryComponent : MonoBehaviour
    {
        [Min(1)] public int foodPerTurn = 10;
        [Min(1)] public int initialBlobs = 2;
        public bool skipAnimations = false;

        [SerializeReference, Required]
        [HideLabel, Title("Conflict Resolution Rule")]
        public ConflictResolutionRule conflictResolutionRule = new SimpleConflictResolution();

        private GameTheorySimulation simulation;
        private int turn;

        private void OnValidate() => OnEnable();

        public void OnEnable()
        {
            turn = 0;

            simulation = new GameTheorySimulation(
                transform: transform,
                foodPerTurn: foodPerTurn,
                initialBlobs: initialBlobs,
                conflictResolutionRule
            ) {
                skipAnimations = skipAnimations,
            };
        }

        public void OnDisable()
        {
            simulation?.Dispose();
        }

        public async void Start()
        {
            if (!Application.isPlaying)
                return;

            while (true) {
                await simulation.SimulateSingleCycle();
                await UniTask.Delay(1000);
            }
        }

        [Title("Controls", HorizontalLine = false)]
        [Button]
        public async void RunTurn()
        {
            turn++;
            this.Log($"Running turn {turn}");
            await simulation.SimulateSingleCycle();
            this.Log($"Completed turn {turn}");
        }
    }
}