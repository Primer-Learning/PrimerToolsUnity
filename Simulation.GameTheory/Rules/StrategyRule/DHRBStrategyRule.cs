using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Simulation.GameTheory
{
    public enum DHRB
    {
        Dove,
        Hawk,
        Retaliator,
        Bully
    }
    public class DHRBStrategyRule : StrategyRule<DHRB>
    {
        // These classes can be moved outside if they are used in more than one place
        // to /Concepts/ folder maybe
        // as long as it's only used inside this class, let's keep it here
        public abstract class ConflictBehaviour : MonoBehaviour {}

        // public float hawkDoveRatio = 0.5f;
        // public float hawkDoveBenefit = 0.5f;
        // public float hawkHawkCost = 1f;

        public override void OnAgentCreated(Agent agent)
        {
            switch (agent.strategy) {
                case DHRB.Dove:
                    Debug.Log("Dove created");
                    agent.gameObject.GetComponent<PrimerBlob>().SetColor(PrimerColor.blue);
                    break;
                case DHRB.Hawk:
                    Debug.Log("Hawk created");
                    agent.gameObject.GetComponent<PrimerBlob>().SetColor(PrimerColor.red);
                    break;
                case DHRB.Retaliator:
                    Debug.Log("Retaliator created");
                    agent.gameObject.GetComponent<PrimerBlob>().SetColor(PrimerColor.green);
                    break;
                case DHRB.Bully:
                    agent.gameObject.GetComponent<PrimerBlob>().SetColor(PrimerColor.yellow);
                    break;
            }
        }

        public override async UniTask Resolve(IEnumerable<Agent> agents, FruitTree tree)
        {
            var (first, second) = agents.Shuffle().Take(2).ToList();
            var firstBehaviour = first.strategy;
            var secondBehaviour = second.strategy;

            await UniTask.WhenAll(
                first.Eat(tree),
                second.Eat(tree)
            );
            
            first.energy+= 1;
            second.energy+= 1;

            // switch (firstBehaviour, secondBehaviour) {
            //     case (DHRB.Hawk, DHRB.Hawk):
            //         // energy wasted on fighting
            //         first.energy -= hawkHawkCost.hawkVsHawk;
            //         second.energy -= hawkHawkCost;
            //         return;
            //
            //     case (DHRB.Hawk, DHRB.Dove):
            //         // first steals from second
            //         first.energy += hawkDoveBenefit;
            //         second.energy -= hawkDoveBenefit;
            //         return;
            //
            //     case (DHRB.Dove, DHRB.Hawk):
            //         // second steals from first
            //         first.energy -= hawkDoveBenefit;
            //         second.energy += hawkDoveBenefit;
            //         return;
            //
            //     case (DHRB.Dove, DHRB.Dove):
            //         return;
            // }
        }
    }
}
