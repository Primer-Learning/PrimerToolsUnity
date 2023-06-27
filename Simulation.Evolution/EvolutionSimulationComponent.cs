using System;
using Primer;
using Primer.Simulation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Simulation.Evolution
{
    [ExecuteAlways]
    [RequireComponent(typeof(Landscape))]
    public class EvolutionSimulationComponent : MonoBehaviour
    {
        [Min(1)] public int foodPerTurn = 10;
        [Min(1)] public int initialBlobs = 2;
        public bool skipAnimations = false;

        private EvolutionSimulation simulation;
        private int turn;

        private void OnValidate() => Awake();

        public void Awake()
        {
            turn = 0;

            simulation = new EvolutionSimulation(
                transform: transform,
                foodPerTurn: foodPerTurn,
                initialBlobs: initialBlobs
            ) {
                skipAnimations = skipAnimations,
            };
        }

        [Button]
        public async void RunTurn()
        {
            turn++;
            this.Log($"Running turn {turn}");
            await simulation.RunTurn();
            this.Log($"Completed turn {turn}");
        }
    }
}
