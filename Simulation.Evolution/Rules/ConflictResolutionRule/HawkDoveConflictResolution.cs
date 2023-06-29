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
        // These classes can be moved outside if they are used in more than one place
        // to /Concepts/ folder maybe
        // as long as it's only used inside this class, let's keep it here
        public abstract class ConflictBehaviour : MonoBehaviour {}
        public class Hawk : ConflictBehaviour {}
        public class Dove : ConflictBehaviour {}

        public float hawkDoveRatio = 0.5f;
        public float hawkDoveBenefit = 0.5f;
        public float hawkHawkCost = 1f;

        public override void OnAgentCreated(Agent agent)
        {
            if (Random.value < hawkDoveRatio)
                agent.GetOrAddComponent<Hawk>();
            else
                agent.GetOrAddComponent<Dove>();
        }

        public override async UniTask Resolve(IEnumerable<Agent> agents, Food food)
        {
            var (first, second) = agents.Shuffle().Take(2).ToList();
            var firstBehaviour = first.GetComponent<ConflictBehaviour>();
            var secondBehaviour = second.GetComponent<ConflictBehaviour>();

            await UniTask.WhenAll(
                first.Eat(food),
                second.Eat(food)
            );

            switch (firstBehaviour, secondBehaviour) {
                case (Hawk, Hawk):
                    // energy wasted on fighting
                    first.energy -= hawkHawkCost;
                    second.energy -= hawkHawkCost;
                    return;

                case (Hawk, Dove):
                    // first steals from second
                    first.energy += hawkDoveBenefit;
                    second.energy -= hawkDoveBenefit;
                    return;

                case (Dove, Hawk):
                    // second steals from first
                    first.energy -= hawkDoveBenefit;
                    second.energy += hawkDoveBenefit;
                    return;

                case (Dove, Dove):
                    return;
            }
        }
    }
}
