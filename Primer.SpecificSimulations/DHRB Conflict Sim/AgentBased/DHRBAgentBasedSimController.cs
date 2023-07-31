using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer.Simulation;
using Simulation.GameTheory;
using UnityEngine;

namespace Primer.SpecificSimulations
{
    [RequireComponent(typeof(DHRBRewardEditorComponent))]
    public class DHRBAgentBasedSimController : AgentBasedEvoGameTheorySimController<DHRB>
    {
        public Graph.Graph graph;
        protected override void SetStrategyRule()
        {
            strategyRule = new DHRBStrategyRule();
            strategyRule.rewardMatrix = GetComponent<RewardEditorComponent<DHRB>>().rewardMatrix;
        }

        protected override async UniTask OnSimStart()
        {
            if (graph is null) return;
            
            var numDoves = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Dove));
            var numHawks = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Hawk));
            var numRetaliators = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Retaliator));
            var numBullies = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Bully));
            
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

        protected override async UniTask OnCycleCompleted()
        {
            if (graph is null) return;
            
            var numDoves = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Dove));
            var numHawks = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Hawk));
            var numRetaliators = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Retaliator));
            var numBullies = _sim.agents.Count(agent => agent.strategy.Equals(DHRB.Bully));
            
            using var stackedArea = graph.AddStackedArea("Allele frequencies");
            stackedArea.colors =
                new List<Color>() {PrimerColor.blue, PrimerColor.red, PrimerColor.green, PrimerColor.yellow};
            stackedArea.AddData(numDoves, numHawks, numRetaliators, numBullies);

            await stackedArea.Transition();
        }
    }
}