using System;
using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using Primer.Simulation.Strategy.DHRB_Strategies;
using Primer.Timeline;
using Simulation.GameTheory;
using UnityEngine;

namespace Scenes.Intro_Scene_Sources
{
    public class DHRBTestSequence : Sequence
    {
        public AgentBasedSimultaneousTurnEvoGameTheorySimController simController;
        public AgentBasedSimultaneousTurnEvoGameTheorySim sim => simController.sim;

        public List<Home> homes;

        private List<Home> _homes => homes.Count > 0
                    ? homes
                    : new List<Home>(simController.transform.Find("Terrain").Find("Trees")
                        .GetComponentsInChildren<Home>());
        
        public override void Cleanup()
        {
            base.Cleanup();
            
            var initialStrategyCountsByHome = new List<Dictionary<Type, int>>()
            {
                new()
                {
                    {typeof(Dove), 3},
                    {typeof(Hawk), 0}
                },
                new()
                {
                    {typeof(Dove), 0},
                    {typeof(Hawk), 3}
                }
                // new()
                // {
                //     {DHRB.Retaliator, 3}
                // },
                // new()
                // {
                //     {DHRB.Bully, 3}
                // }
            };

            var creatureGnome = new SimpleGnome("Blobs", parent: simController.transform);

            var initialCreatures = new List<SimultaneousTurnCreature>();
            for (var i = 0; i < initialStrategyCountsByHome.Count; i++)
            {
                var initialCreaturesDict = initialStrategyCountsByHome[i];
                foreach (var (strategy, count) in initialCreaturesDict) {
                    for (var j = 0; j < count; j++) {
                        var creature = creatureGnome.Add<SimultaneousTurnCreature>("blob_skinned", $"Initial {strategy} {j + 1} on home {i}");
                        initialCreatures.Add(creature);
                        creature.strategyGenes = new Type[] {strategy}; 
                        creature.home = _homes[i];
                        simController.simultaneousTurnGameAgentHandler.OnAgentCreated(creature);
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
            var numDays = 20;
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