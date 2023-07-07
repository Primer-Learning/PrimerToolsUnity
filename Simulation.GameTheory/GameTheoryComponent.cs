using Cysharp.Threading.Tasks;
using Primer;
using Primer.Simulation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Simulation.GameTheory
{
    [ExecuteAlways]
    public class GameTheoryComponent : MonoBehaviour, ISimulation
    {
        [Min(1)] public int foodPerTurn = 10;
        [Min(1)] public int initialBlobs = 2;
        public int seed = 0;
        public bool _skipAnimations = false;

        [ShowInInspector]
        [MinValue(4)]
        public int size {
            get => terrain.size.x;
            set => terrain.size = new Vector3Int(value, 4, value);
        }

        [SerializeReference]
        [Required]
        [PropertyOrder(10)]
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
                seed: seed,
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


        #region ISimulation implementation
        public Rng rng => simulation.rng;
        public Landscape terrain => simulation.terrain;
        public bool skipAnimations => _skipAnimations;
        public UniTask SimulateSingleCycle() => simulation.SimulateSingleCycle();
        #endregion
    }
}
