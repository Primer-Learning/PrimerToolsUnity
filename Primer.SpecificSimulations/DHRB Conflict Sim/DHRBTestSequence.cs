using System.Collections.Generic;
using Primer.Animation;
using Primer.Simulation;
using Primer.Timeline;
using Simulation.GameTheory;
using UnityEngine;

namespace Scenes.Intro_Scene_Sources
{
    public class DHRBTestSequence : Sequence
    {
        public AgentBasedEvoGameTheorySimController<DHRB> simController;
        public AgentBasedEvoGameTheorySim<DHRB> sim => simController.sim;
        
        public override void Cleanup()
        {
            base.Cleanup();
            simController.InitializeSim();
            foreach (var tree in sim.trees)
            {
                tree.Reset();
            }
        }

        public override async IAsyncEnumerator<Tween> Define()
        {
            yield return sim.CreateFood();
            yield return sim.AgentsGoToTrees();
            yield return sim.AgentsEatFood();
            yield return sim.AgentsReturnHome();
            yield return sim.AgentsReproduceOrDie();
            
            sim.CleanUp();
            yield return Tween.noop;
        }
    }
}