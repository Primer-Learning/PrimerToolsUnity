using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Simulation.Evolution
{
    public class HawkDoveConflictResolution : ConflictResolutionRule
    {
        public class Hawk : MonoBehaviour {}

        public float hawkDoveRatio = 0.5f;
        public float hawkDoveBenefit = 0.5f;
        public float hawkHawkCost = 1f;

        public override void OnAgentCreated(Agent agent)
        {
            if (Random.value < hawkDoveRatio)
                agent.GetOrAddComponent<Hawk>();
        }

        public override async UniTask Resolve(IEnumerable<Agent> agents, Food food)
        {
            var (first, second) = agents.Shuffle().Take(2).ToList();
            var firstIsHawk = first.GetComponent<Hawk>() is not null;
            var secondIsHawk = second.GetComponent<Hawk>() is not null;

            await UniTask.WhenAll(
                first.Eat(food),
                second.Eat(food)
            );

            switch (firstIsHawk, secondIsHawk) {
                case (true, true):
                    // they eat but waste the energy fighting
                    first.energy -= hawkHawkCost;
                    second.energy -= hawkHawkCost;
                    return;

                case (true, false):
                    first.energy += hawkDoveBenefit;
                    second.energy -= hawkDoveBenefit;
                    return;

                case (false, true):
                    first.energy -= hawkDoveBenefit;
                    second.energy += hawkDoveBenefit;
                    return;

                case (false, false):
                    return;
            }
        }
    }
}
