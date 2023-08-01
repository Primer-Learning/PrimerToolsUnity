using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using Primer.Graph;
using Primer.Simulation;
using Simulation.GameTheory;
using UnityEngine;

namespace Primer.SpecificSimulations
{
    [RequireComponent(typeof(DHRBRewardEditorComponent))]
    public class DHRBAgentBasedSimController : AgentBasedEvoGameTheorySimController<DHRB>
    {
        public Graph.Graph graph;
        public TernaryPlot ternaryPlot;
        
        protected override void SetStrategyRule()
        {
            strategyRule = new DHRBStrategyRule();
            strategyRule.rewardMatrix = GetComponent<RewardEditorComponent<DHRB>>().rewardMatrix;
        }

        protected override async UniTask OnSimStart()
        {
            var numDoves = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Dove));
            var numHawks = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Hawk));
            var numRetaliators = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Retaliator));
            var numBullies = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Bully));
            var total = numDoves + numHawks + numRetaliators + numBullies;
            
            if (graph is not null)
            {
                
                using var stackedArea = graph.AddStackedArea("Allele frequencies");
                stackedArea.enabled = true;
                stackedArea.colors =
                    new List<Color>() {PrimerColor.blue, PrimerColor.red, PrimerColor.green, PrimerColor.yellow};
                stackedArea.SetData(
                    new float [] {numDoves},
                    new float [] {numHawks}, 
                    new float [] {numRetaliators},
                    new float [] {numBullies}
                );
                
                await stackedArea.GrowFromStart();
            }

            if (ternaryPlot is not null)
            {
                var gnome = ternaryPlot.GetContentGnome();

                var blob = gnome.AddPrefab<PrimerBlob>("blob_skinned", "blob");
                blob.transform.localScale = Vector3.zero;
                blob.transform.localPosition = TernaryPlot.CoordinatesToPosition(
                    (float) numDoves / total,
                    (float) numHawks / total,
                    (float) numRetaliators / total
                );
                await blob.transform.ScaleTo(Vector3.one / ternaryPlot.transform.localScale.x);
            }
            Debug.Log("Sim started");
        }

        protected override async UniTask OnCycleCompleted()
        {
            var numDoves = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Dove));
            var numHawks = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Hawk));
            var numRetaliators = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Retaliator));
            var numBullies = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Bully));
            var total = numDoves + numHawks + numRetaliators + numBullies;

            if (graph is not null)
            {
                using var stackedArea = graph.AddStackedArea("Allele frequencies");
                stackedArea.colors =
                    new List<Color>() {PrimerColor.blue, PrimerColor.red, PrimerColor.green, PrimerColor.yellow};
                stackedArea.AddData(numDoves, numHawks, numRetaliators, numBullies);

                await stackedArea.Transition();
            }
            
            if (ternaryPlot is not null)
            {
                var gnome = ternaryPlot.GetContentGnome();

                var blob = gnome.AddPrefab<PrimerBlob>("blob_skinned", "blob");
                blob.transform.localScale = Vector3.one / ternaryPlot.transform.localScale.x;
                
                await blob.transform.MoveTo(
                    TernaryPlot.CoordinatesToPosition(
                        (float) numDoves / total,
                        (float) numHawks / total,
                        (float) numRetaliators / total
                    )
                );
            }
            Debug.Log("Cycle completed");
        }
    }
}