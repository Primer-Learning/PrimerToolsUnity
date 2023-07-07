using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Simulation
{
    public interface ISimulation
    {
        public Rng rng { get; }
        public Landscape terrain { get; }
        public bool skipAnimations { get; }
        public UniTask SimulateSingleCycle();
    }

    public static class ISimulationExtensions
    {
        public static Vector3 GetGroundAtLocal(this ISimulation sim, Vector3 localPosition)
            => sim.terrain.GetGroundAtLocal(localPosition);

        public static Vector3 GetGroundAtLocal(this ISimulation sim, Vector2 localPosition)
            => sim.terrain.GetGroundAtLocal(localPosition);

        public static Vector3 GetGroundAt(this ISimulation sim, Vector3 worldPosition)
            => sim.terrain.GetGroundAt(worldPosition);

        public static Vector3 GetGroundAt(this ISimulation sim, Vector2 worldPosition)
            => sim.terrain.GetGroundAt(worldPosition);

        public static async UniTask MainLoop(this ISimulation simulation, uint msBetweenCycles)
        {
            // Iif we're skipping animations, we probably want to speed up the animations too.
            // This fixes the frame rate per game time, speeding up the simulation in real time
            // (assuming the frames are quick to calculate).
            // It also removes variability that might be caused by frame rate variation on async functions.
            if (simulation.skipAnimations)
                Time.captureFramerate = 60;

            Time.timeScale = 1;

            while (true) {
                await simulation.SimulateSingleCycle();

                if (!simulation.skipAnimations && msBetweenCycles > 0)
                    await UniTask.Delay((int)msBetweenCycles);
            }
        }
    }
}
