using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Animation;
using Primer.Simulation;

namespace Simulation.GameTheory
{
    public class DHRBSimultaneousTurnGameAgentHandler : SimultaneousTurnGameAgentHandler
    {
        public override void OnAgentCreated(SimultaneousTurnCreature creature)
        {
            var alleles = creature.strategyGenes.GetAlleles();

            if (alleles.Count == 0)
            {
                creature.gameObject.GetComponent<PrimerBlob>().SetColor(alleles[0].color);
            }
            else
            {
                creature.gameObject.GetComponent<PrimerBlob>()
                    .SetColor(
                        PrimerColor.JuicyInterpolate(
                            PrimerColor.blue,
                            PrimerColor.red,
                            alleles.Count(x => 
                                x.GetType() == typeof(SimultaneousTurnStrategyGenes.Hawk)) / (float)alleles.Count)
                    );

            }
        }

        public override Tween Resolve(IEnumerable<SimultaneousTurnCreature> creature, FruitTree tree)
        {
            var (first, second) = creature.Shuffle().ToList();

            first.energy += rewardMatrix.Get( first.action,  second.action);
            second.energy += rewardMatrix.Get( second.action,  first.action);
            
            return Tween.Parallel(
                first.EatAnimation(tree),
                second.EatAnimation(tree)
            );
        }
    }
}
