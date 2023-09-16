using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Animation;

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

        public override void OnAgentCreated(Creature creature)
        {
            creature.gameObject.GetComponent<PrimerBlob>()
                .SetColor(
                    PrimerColor.JuicyInterpolate(
                        PrimerColor.blue,
                        PrimerColor.red,
                        creature.strategyGenes.Count(x => x.Equals(DHRB.Hawk)) / (float)creature.strategyGenes.Length)
                );

            // switch (creature.strategy) {
            //     case DHRB.Dove:
            //         creature.gameObject.GetComponent<PrimerBlob>().SetColor(PrimerColor.blue);
            //         break;
            //     case DHRB.Hawk:
            //         creature.gameObject.GetComponent<PrimerBlob>().SetColor(PrimerColor.red);
            //         break;
            //     case DHRB.Retaliator:
            //         creature.gameObject.GetComponent<PrimerBlob>().SetColor(PrimerColor.green);
            //         break;
            //     case DHRB.Bully:
            //         creature.gameObject.GetComponent<PrimerBlob>().SetColor(PrimerColor.yellow);
            //         break;
            // }
        }

        public override Tween Resolve(IEnumerable<Creature> creature, FruitTree tree)
        {
            var (first, second) = creature.Shuffle().ToList();

            first.energy += rewardMatrix.Get((DHRB) first.strategy, (DHRB) second.strategy);
            second.energy += rewardMatrix.Get((DHRB) second.strategy, (DHRB) first.strategy);
            
            return Tween.Parallel(
                first.EatAnimation(tree),
                second.EatAnimation(tree)
            );
        }
    }
}
