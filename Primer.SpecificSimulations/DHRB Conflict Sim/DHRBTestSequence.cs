using System.Collections.Generic;
using System.Linq;
using Primer;
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

        public List<Home> homes;

        private List<Home> _homes => homes.Count > 0
                    ? homes
                    : new List<Home>(simController.transform.Find("Terrain").Find("Trees")
                        .GetComponentsInChildren<Home>());
        
        public override void Cleanup()
        {
            base.Cleanup();
            
            var initialStrategyCountsByHome = new List<Dictionary<DHRB, int>>()
            {
                new()
                {
                    {DHRB.Dove, 1},
                    {DHRB.Hawk, 1}
                },
                new()
                {
                    {DHRB.Dove, 1},
                    {DHRB.Hawk, 1}
                },
                new()
                {
                    {DHRB.Dove, 1},
                    {DHRB.Hawk, 1}
                },
                new()
                {
                    {DHRB.Dove, 1},
                    {DHRB.Hawk, 1}
                }
            };

            var creatureGnome = new SimpleGnome("Blobs", parent: simController.transform);

            var initialCreatures = new List<Creature>();
            for (var i = 0; i < initialStrategyCountsByHome.Count; i++)
            {
                var initialCreaturesDict = initialStrategyCountsByHome[i];
                foreach (var (strategy, count) in initialCreaturesDict) {
                    for (var j = 0; j < count; j++) {
                        var creature = creatureGnome.Add<Creature>("blob_skinned", $"Initial {strategy} {j + 1} on home {i}");
                        initialCreatures.Add(creature);
                        creature.strategy = strategy;
                        creature.home = _homes[i];
                        simController.strategyRule.OnAgentCreated(creature);
                    }
                }
            }
            
            simController.InitializeSim(initialCreatures);
            foreach (var tree in sim.trees)
            {
                tree.Reset();
            }
        }

        public override async IAsyncEnumerator<Tween> Define()
        {
            var numDays = 5;
            for (var i = 0; i < numDays; i++)
            {
                yield return sim.CreateFood();
                yield return sim.AgentsGoToTrees();
                yield return sim.AgentsEatFood();
                yield return sim.AgentsReturnHome();
                yield return sim.AgentsReproduceOrDie();
            }
        }
    }
}