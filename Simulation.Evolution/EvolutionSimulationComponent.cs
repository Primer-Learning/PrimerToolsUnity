using System;
using Primer;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Simulation.Evolution
{
    [ExecuteAlways]
    public class EvolutionSimulationComponent : MonoBehaviour
    {
        public Vector2 size = Vector2.one * 10;
        public int foodPerTurn = 10;
        public int initialBlobs = 2;
        public bool skipAnimations = false;

        private EvolutionSimulation simulation;
        private int turn;

        private void OnValidate() => Awake();

        public void Awake()
        {
            turn = 0;

            simulation = new EvolutionSimulation(
                transform: transform,
                size: size,
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
