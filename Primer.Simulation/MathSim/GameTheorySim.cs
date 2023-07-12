using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Simulation
{
    public class GameTheorySim<T> where T : Enum
    {
        private readonly RewardMatrix<T> rewardMatrix;
        private readonly float baseFitness;
        private readonly float stepSize;

        public GameTheorySim(RewardMatrix<T> rewardMatrix, float baseFitness = 1, float stepSize = 0.1f)
        {
            this.rewardMatrix = rewardMatrix;
            this.baseFitness = baseFitness;
            this.stepSize = stepSize;
        }

        public IEnumerable<AlleleFrequency<T>> Simulate(AlleleFrequency<T> initial, int maxIterations = 1000, float minDelta = 0.01f)
        {
            var last = initial;
            yield return initial;

            for (var i = 0; i < maxIterations; i++) {
                var current = SingleIteration(last);

                yield return current;

                if (last.Delta(current) < minDelta) {
                    Debug.Log($"Delta is below threshold after {i} iterations: {last.Delta(current)} < {minDelta}");
                    break;
                }

                last = current;
            }
        }

        private AlleleFrequency<T> SingleIteration(AlleleFrequency<T> previous)
        {
            var list = previous
                .Select(x => (
                    strategy: x.Key,
                    frequency: x.Value,
                    fitness: CalculateFitness(x.Key, previous)
                ))
                .ToList();

            var avgFitness = list.Average(x => x.fitness);
            var result = new AlleleFrequency<T>();

            foreach (var (strategy, frequency, fitness) in list) {
                var difference = frequency * (fitness - avgFitness) / avgFitness;
                result[strategy] = frequency + stepSize * difference;
            }

            result.Normalize();
            return result;
        }

        private float CalculateFitness(T strategy, AlleleFrequency<T> previous)
        {
            return previous.Sum(x => x.Value * rewardMatrix[strategy, x.Key]) + baseFitness;
        }
    }
}
