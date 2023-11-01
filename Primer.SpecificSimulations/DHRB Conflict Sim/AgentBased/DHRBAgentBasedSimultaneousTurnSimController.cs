using Primer.Graph;
using Primer.Simulation;
using Simulation.GameTheory;
using UnityEngine;

namespace Primer.SpecificSimulations
{
    [RequireComponent(typeof(DHRBRewardEditorComponent))]
    public class DHRBAgentBasedSimultaneousTurnSimController : AgentBasedSimultaneousTurnEvoGameTheorySimController
    {
        public Graph.Graph graph;
        public TernaryPlot ternaryPlot;
        
        protected override void SetStrategyRule()
        {
            simultaneousTurnGameAgentHandler = new DHRBSimultaneousTurnGameAgentHandler();
            simultaneousTurnGameAgentHandler.rewardMatrix = GetComponent<DHRBRewardEditorComponent>().rewardMatrix;
        }
    }
}