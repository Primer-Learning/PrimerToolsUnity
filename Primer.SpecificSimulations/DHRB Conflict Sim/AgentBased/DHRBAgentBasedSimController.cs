using Primer.Simulation;
using Simulation.GameTheory;
using UnityEngine;

namespace Primer.SpecificSimulations
{
    [RequireComponent(typeof(DHRBRewardEditorComponent))]
    public class DHRBAgentBasedSimController : AgentBasedEvoGameTheorySimController<DHRB>
    {
        protected override void SetStrategyRule()
        {
            strategyRule = new DHRBStrategyRule();
            strategyRule.rewardMatrix = GetComponent<RewardEditorComponent<DHRB>>().rewardMatrix;
        }
    }
}