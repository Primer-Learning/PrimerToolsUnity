using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Simulation
{

    public class EvoGameTheorySim<T> where T : Enum
    {
        private static readonly Cache<SimParams, AlleleFrequency<T>[]> cache = new();

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
            var simParams = new SimParams(rewardMatrix, baseFitness, runStepSize, initial, maxIterations, minDelta);

            if (cache.Peek(simParams, out var cached)) {
                Debug.Log("Cache hit!");
                return cached.ToList();
            }

            var result = SimulateInternal(initial, maxIterations, minDelta).ToArray();
            cache.Save(simParams, result);
            return result;
        }


        private IEnumerable<AlleleFrequency<T>> SimulateInternal(AlleleFrequency<T> initial, int maxIterations = 1000, float minDelta = 0.01f)
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
                    strategy: x.Item1,
                    frequency: x.Item2,
                    fitness: CalculateFitness(x.Item1, currentState)
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
            return previous.Sum(x => x.Item2 * rewardMatrix[strategy, x.Item1]) + baseFitness;
        }


        [Serializable]
        private class SimParams
        {
            public RewardMatrix<T> rewardMatrix;
            public float baseFitness;
            public float runStepSize;
            public AlleleFrequency<T> initial;
            public int maxIterations;
            public float minDelta;

            public SimParams(
                RewardMatrix<T> rewardMatrix, float baseFitness, float runStepSize,
                AlleleFrequency<T> initial, int maxIterations, float minDelta
            ) {
                this.rewardMatrix = rewardMatrix;
                this.baseFitness = baseFitness;
                this.runStepSize = runStepSize;
                this.initial = initial;
                this.maxIterations = maxIterations;
                this.minDelta = minDelta;
            }
        }
    }
}
