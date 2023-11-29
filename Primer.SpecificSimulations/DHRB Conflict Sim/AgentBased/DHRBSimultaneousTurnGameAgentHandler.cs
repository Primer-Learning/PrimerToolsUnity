using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using UnityEngine;

namespace Simulation.GameTheory
{
    public class DHRBSimultaneousTurnGameAgentHandler : SimultaneousTurnGameAgentHandler
    {
        public override void OnAgentCreated(SimultaneousTurnCreature creature)
        {
            var alleles = creature.strategyGenes.GetAlleles();

            if (alleles.Count == 1)
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

        public override IEnumerable<Tween> Resolve(IEnumerable<SimultaneousTurnCreature> creature, FruitTree tree)
        {
            var (first, second) = creature.ToList();
            
            // These are determined randomly from the genome, so we get each once
            var firstAction = first.action;
            
            // Give the rewards
            var secondAction = second.action;
            first.energy += rewardMatrix.Get( firstAction,  secondAction);
            second.energy += rewardMatrix.Get( secondAction,  firstAction);
            
            switch (firstAction)
            {
                case SimultaneousTurnAction.Dove:
                    switch (secondAction)
                    {
                        case SimultaneousTurnAction.Dove:
                            yield return Tween.Series(
                                Tween.Parallel(
                                    first.WalkTo(tree, stopDistance: 1).Observe(onComplete: () =>
                                    {
                                        first.blob.animator.SetTrigger("Shake object");
                                        first.blob.EffortEyes();
                                    }),
                                    second.WalkTo(tree, stopDistance: 1).Observe(onComplete: () =>
                                    {
                                        second.blob.animator.SetTrigger("Shake object");
                                        second.blob.EffortEyes();
                                    })
                                ),
                                (Tween.noop with { duration = 0.5f }).Observe(onComplete: () =>
                                {
                                    foreach (var fruit in tree.highFruits.Concat(tree.fruits))
                                    {
                                        tree.DetachFruit(fruit);
                                    }
                                    first.blob.animator.ResetTrigger("Shake object");
                                    first.blob.animator.SetTrigger("Wiggles");
                                    first.blob.NeutralEyes();
                                    second.blob.animator.ResetTrigger("Shake object");
                                    second.blob.animator.SetTrigger("Wiggles");
                                    second.blob.NeutralEyes();
                                })
                            );
                            // first.name = "Dove";
                            // first.name = "blob_skinned";
                            yield return Tween.Parallel(
                                first.EatAnimationForGroundMango(tree, first.OrderFruitByDistance(tree)[0]),
                                second.EatAnimationForGroundMango(tree, first.OrderFruitByDistance(tree)[1])
                            );
                            yield return Tween.Parallel(
                                first.EatAnimationForGroundMango(tree, first.OrderHighFruitByDistance(tree)[0], high: true),
                                second.EatAnimationForGroundMango(tree, first.OrderHighFruitByDistance(tree)[1], high: true)
                            );

                    break;
                        case SimultaneousTurnAction.Hawk:
                            foreach (var tween in CooperatorAndDefector(first, second, tree))
                            {
                                yield return tween;
                            }
                            break;
                    }
                    break;
                case SimultaneousTurnAction.Hawk:
                    switch (secondAction)
                    {
                        case SimultaneousTurnAction.Dove:
                            foreach (var tween in CooperatorAndDefector(second, first, tree))
                            {
                                yield return tween;
                            }
                            break;
                        case SimultaneousTurnAction.Hawk:
                            var differenceVector =
                                (first.transform.position - second.transform.position).ElementWiseMultiply(
                                    new Vector3(1, 0, 1));
                            var meetingPoint = (first.transform.position + second.transform.position) / 2
                                + Vector3.Cross( differenceVector, Vector3.up).normalized * first.transform.parent.localScale.x;

                            yield return Tween.Series(
                                Tween.Parallel(
                                    first.WalkTo(meetingPoint, offset: differenceVector.normalized / 2 * first.transform.parent.localScale.x),
                                    second.WalkTo(meetingPoint, offset: -differenceVector.normalized / 2 * first.transform.parent.localScale.x)
                                ).Observe(afterComplete: () =>
                                    {
                                        first.blob.Panic(duration: 1);
                                        second.blob.Panic(duration: 1);
                                    }
                                ),
                                Tween.noop with { duration = 1 }
                            );
                            yield return Tween.Parallel(
                                first.EatAnimation(tree, first.OrderFruitByDistance(tree)[0]),
                                second.EatAnimation(tree, second.OrderFruitByDistance(tree)[0])
                            );
                            break;
                    }
                    break;
                // None of the above (shouldn't happen)
                case SimultaneousTurnAction.Bully:
                case SimultaneousTurnAction.Retaliator:
                default:
                {
                    if (Application.isPlaying) Debug.LogWarning($"Unexpected action combination {firstAction}, {secondAction}");
                    else Debug.LogError($"Unexpected action combination {firstAction}, {secondAction}");
                
                    yield return Tween.Parallel(
                        first.EatAnimation(tree, first.OrderFruitByDistance(tree)[0]),
                        second.EatAnimation(tree, second.OrderFruitByDistance(tree)[0])
                    );
                    yield break;
                }
            }
        }
        
        private IEnumerable<Tween> CooperatorAndDefector(SimultaneousTurnCreature cooperator, SimultaneousTurnCreature defector, FruitTree tree)
        {
            yield return cooperator.WalkTo(tree, stopDistance: 1).Observe(afterComplete: () =>
            {
                cooperator.blob.animator.SetTrigger("Shake object");
                cooperator.blob.EffortEyes();
            });
            // Defector eats a fruit
            yield return Tween.Parallel(
                defector.EatAnimation(tree, defector.OrderFruitByDistance(tree)[0]),
                (Tween.noop with { duration = 0.5f }).Observe(afterComplete: () =>
                {
                    cooperator.blob.animator.SetTrigger("Wiggles");
                    cooperator.blob.NeutralEyes();
                })
            );
            var fruitIndex = cooperator.OrderFruitByDistance(tree)[0];
            var fruit = tree.fruits[fruitIndex];
            var actualFruit = fruit.GetChild(0);
            yield return Tween.Parallel(
                cooperator.EatAnimation(tree, fruitIndex),
                Tween.Series(
                    defector.WalkTo(fruit, stopDistance: 1)
                        .Observe(afterComplete: () =>
                        {
                            defector.blob.StartLookingAt(actualFruit);
                        }),
                    (Tween.noop with { duration = 1 })
                    .Observe(afterComplete: () =>
                    {
                        defector.blob.Chew(1);
                        defector.blob.animator.SetBool("ArmsInFront", true);
                    }),
                    (Tween.noop with { duration = 1 })
                    .Observe(afterComplete: () =>
                    {
                        defector.blob.animator.SetBool("ArmsInFront", false);
                        defector.blob.StopLooking(duration: 0.2f);
                    })
                )
            );
        }
    }
}
