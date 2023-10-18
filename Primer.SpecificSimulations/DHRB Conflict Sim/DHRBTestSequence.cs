using System;
using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using Primer.Simulation.EvoGameTheorySim.AgentBased;
using Primer.Simulation.Genome.Strategy;
using Primer.Timeline;
using Simulation.GameTheory;
using UnityEngine;
using SimpleGnome = Primer.SimpleGnome;

namespace Scenes.Intro_Scene_Sources
{
    public class DHRBTestSequence : Sequence
    {
        public AgentBasedSimultaneousTurnEvoGameTheorySimController simController;
        public AgentBasedSimultaneousTurnEvoGameTheorySim sim => simController.sim;

        public List<Home> homes;

        private List<Home> _homes => homes.Count > 0
                    ? homes
                    : new List<Home>(simController.transform.Find("Trees")
                        .GetComponentsInChildren<Home>());

        public override async IAsyncEnumerator<Tween> Define()
        {
            var initialStrategyCountsByHome = new List<Dictionary<SimultaneousTurnStrategyGene, int>>()
            {
                new()
                {
                    {new SimultaneousTurnStrategyGenes.Dove(), 1},
                    {new SimultaneousTurnStrategyGenes.Hawk(), 0}
                },
                new()
                {
                    {new SimultaneousTurnStrategyGenes.Dove(), 0},
                    {new SimultaneousTurnStrategyGenes.Hawk(), 1}
                }
            };

            var creaturePool = new Pool<SimultaneousTurnCreature>("Blobs", parent: simController.transform);
            creaturePool.prefab = Resources.Load<GameObject>("blob_skinned");

            var initialCreatures = new List<SimultaneousTurnCreature>();
            for (var i = 0; i < initialStrategyCountsByHome.Count; i++)
            {
                var initialCreaturesDict = initialStrategyCountsByHome[i];
                foreach (var (strategy, count) in initialCreaturesDict) {
                    for (var j = 0; j < count; j++) {
                        var creature = creaturePool.Add();
                        initialCreatures.Add(creature);
                        creature.strategyGenes = new SimultaneousTurnGenome(strategy); 
                        creature.home = _homes[i % _homes.Count];
                        simController.simultaneousTurnGameAgentHandler.OnAgentCreated(creature);
                    }
                }
            }
            
            simController.InitializeSim(initialCreatures, new Rng(0));
            foreach (var tree in sim.trees)
            {
                tree.Reset();
            }

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