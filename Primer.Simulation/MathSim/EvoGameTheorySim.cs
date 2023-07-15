using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Simulation
{
    public class EvoGameTheorySim<T> where T : Enum
    {
        private readonly RewardMatrix<T> rewardMatrix;
        private readonly float baseFitness;
        private readonly float runStepSize;
        public bool logWhenEquilibrium = false;

        public EvoGameTheorySim(RewardMatrix<T> rewardMatrix, float baseFitness = 1, float runStepSize = 0.1f)
        {
            this.rewardMatrix = rewardMatrix;
            this.baseFitness = baseFitness;
            this.runStepSize = runStepSize;
        }

        public IEnumerable<AlleleFrequency<T>> Simulate(AlleleFrequency<T> initial, int maxIterations = 1000, float minDelta = 0.01f)
        {
            var last = initial;
            yield return initial;

            for (var i = 0; i < maxIterations; i++) {
                var current = SingleIteration(last, runStepSize);

                yield return current;

                if (last.DeltaMagnitude(current) < minDelta) {
                    if (logWhenEquilibrium) Debug.Log($"DeltaMagnitude is below threshold after {i} iterations: {last.DeltaMagnitude(current)} < {minDelta}");
                    break;
                }

                last = current;
            }
        }

        // The Vector Field plotter uses this but wants to set its own step size.
        public AlleleFrequency<T> SingleIteration(AlleleFrequency<T> previous, float stepSize)
        {
            var difference = CalculateDifference(previous);
            var result = new AlleleFrequency<T>();
            
            foreach (var (strategy, _) in previous) {
                result[strategy] = previous[strategy] + stepSize * difference[strategy];
            }

            result.Normalize();
            return result;
        }

        // Pulled this out to make it easier to make a vector field visualization
        public AlleleFrequency<T> CalculateDifference(AlleleFrequency<T> currentState)
        {
            var list = currentState
                .Select(x => (
                    strategy: x.Key,
                    frequency: x.Value,
                    fitness: CalculateFitness(x.Key, currentState)
                ))
                .ToList();
            
            var avgFitness = list.Average(x => x.fitness);

            var difference = new AlleleFrequency<T>();
            
            foreach (var (strategy, frequency, fitness) in list) {
                difference[strategy] = frequency * (fitness - avgFitness) / avgFitness;
            }

            return difference;
        }

        private float CalculateFitness(T strategy, AlleleFrequency<T> previous)
        {
            return previous.Sum(x => x.Value * rewardMatrix[strategy, x.Key]) + baseFitness;
        }
    }
}
