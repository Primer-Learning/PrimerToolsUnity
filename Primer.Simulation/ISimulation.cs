using Cysharp.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Simulation
{
    public interface ISimulation
    {
        public Rng rng { get; }
        public bool skipAnimations { get; }
        public Tween SimulateSingleCycle();
    }

    public static class ISimulationExtensions
    {
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
