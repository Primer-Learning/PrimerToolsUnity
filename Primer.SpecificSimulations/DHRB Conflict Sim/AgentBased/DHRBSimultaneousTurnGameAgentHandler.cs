using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Animation;
using Primer.Simulation.Strategy.DHRB_Strategies;

namespace Simulation.GameTheory
{
    // public enum DHRB
    // {
    //     Dove,
    //     Hawk,
    //     Retaliator,
    //     Bully
    // }
    public class DHRBSimultaneousTurnGameAgentHandler : SimultaneousTurnGameAgentHandler
    {
        public override void OnAgentCreated(SimultaneousTurnCreature creature)
        {
            creature.gameObject.GetComponent<PrimerBlob>()
                .SetColor(
                    PrimerColor.JuicyInterpolate(
                        PrimerColor.blue,
                        PrimerColor.red,
                        creature.strategyGenes.Count(x => x.Equals(typeof(Hawk))) / (float)creature.strategyGenes.Length)
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

        public override Tween Resolve(IEnumerable<SimultaneousTurnCreature> creature, FruitTree tree)
        {
            var (first, second) = creature.Shuffle().ToList();

            first.energy += rewardMatrix.Get( first.strategy,  second.strategy);
            second.energy += rewardMatrix.Get( second.strategy,  first.strategy);
            
            return Tween.Parallel(
                first.EatAnimation(tree),
                second.EatAnimation(tree)
            );
        }
    }
}
