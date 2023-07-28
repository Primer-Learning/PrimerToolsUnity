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
            var (first, second) = agents.Shuffle().Take(2).ToList();
            var firstBehaviour = first.strategy;
            var secondBehaviour = second.strategy;

            await UniTask.WhenAll(
                first.Eat(tree),
                second.Eat(tree)
            );

            switch (firstBehaviour, secondBehaviour) {
                case (DHRB.Hawk, DHRB.Hawk):
                    // energy wasted on fighting
                    first.energy += rewardMatrix.Get(DHRB.Hawk, DHRB.Hawk);
                    second.energy += rewardMatrix.Get(DHRB.Hawk, DHRB.Hawk);
                    return;
            
                case (DHRB.Hawk, DHRB.Dove):
                    first.energy += rewardMatrix.Get(DHRB.Hawk, DHRB.Dove);
                    second.energy += rewardMatrix.Get(DHRB.Dove, DHRB.Hawk);
                    return;
            
                case (DHRB.Dove, DHRB.Hawk):
                    // second steals from first
                    first.energy += rewardMatrix.Get(DHRB.Dove, DHRB.Hawk);
                    second.energy += rewardMatrix.Get(DHRB.Hawk, DHRB.Dove);
                    return;
            
                case (DHRB.Dove, DHRB.Dove):
                    first.energy = rewardMatrix.Get(DHRB.Dove, DHRB.Dove);
                    second.energy = rewardMatrix.Get(DHRB.Dove, DHRB.Dove);
                    return;
            }
        }
    }
}
