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

        public override void OnAgentCreated(Agent agent)
        {
            switch (agent.strategy) {
                case DHRB.Dove:
                    agent.gameObject.GetComponent<PrimerBlob>().SetColor(PrimerColor.blue);
                    break;
                case DHRB.Hawk:
                    agent.gameObject.GetComponent<PrimerBlob>().SetColor(PrimerColor.red);
                    break;
                case DHRB.Retaliator:
                    agent.gameObject.GetComponent<PrimerBlob>().SetColor(PrimerColor.green);
                    break;
                case DHRB.Bully:
                    agent.gameObject.GetComponent<PrimerBlob>().SetColor(PrimerColor.yellow);
                    break;
            }
        }

        public override async UniTask Resolve(IEnumerable<Agent> agents, FruitTree tree)
        {
            var (first, second) = agents.Shuffle().ToList();

            await UniTask.WhenAll(
                first.EatAnimation(tree),
                second.EatAnimation(tree)
            );
            
            first.energy += rewardMatrix.Get((DHRB) first.strategy, (DHRB) second.strategy);
            second.energy += rewardMatrix.Get((DHRB) second.strategy, (DHRB) first.strategy);
        }
    }
}
