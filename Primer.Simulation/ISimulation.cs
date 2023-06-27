using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Simulation
{
    public interface ISimulation
    {
        // public int secondsBetweenTurns { get; }
        public bool skipAnimations { get; }
        public UniTask RunTurn();
    }

    public static class ISimulationExtensions
    {
        public static async UniTask MainLoop(this ISimulation simulation)
        {
            // Iif we're skipping animations, we probably want to speed up the animations too.
            // This fixes the frame rate per game time, speeding up the simulation in real time
            // (assuming the frames are quick to calculate).
            // It also removes variability that might be caused by frame rate variation on async functions.
            if (simulation.skipAnimations)
                Time.captureFramerate = 60;

            Time.timeScale = 1;

            while (true) {
                // var startTime = Time.time;

                await simulation.RunTurn();

                // var elapsedTime = Time.time - startTime;
                // var remainingTime = simulation.secondsBetweenTurns - elapsedTime;
                //
                // if (!simulation.skipAnimations && remainingTime > 0)
                //     await UniTask.Delay((int)(remainingTime * 1000));
            }
        }
    }
}
