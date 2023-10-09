using System.Collections.Generic;
using Primer;
using Primer.Simulation;
using Primer.Simulation.EvoGameTheorySim.AgentBased;
using Primer.Simulation.Genome.Strategy;

namespace Simulation.GameTheory
{
    public class SimultaneousTurnGenome : Genome<SimultaneousTurnGenome>
    {
        public SimultaneousTurnGenome() {}
        public SimultaneousTurnGenome(Homolog[] homologs) : base(homologs) {}
        
        public SimultaneousTurnGenome(params SimultaneousTurnStrategyGene[] genes) : base(genes) {}
        public SimultaneousTurnGenome(SimultaneousTurnStrategyGene[,] genes) : base(genes) {}
        
        public SimultaneousTurnAction GetAction()
        {
            var homolog = homologs.RandomItem();
            var chromosome = homolog.chromosomes.RandomItem();
            var gene = (SimultaneousTurnStrategyGene) chromosome.genes.RandomItem();
            return gene.action;
        }
        public List<SimultaneousTurnStrategyGene> GetAlleles()
        {
            var alleles = new List<SimultaneousTurnStrategyGene>();
            foreach (var homolog in homologs)
            {
                foreach (var choromosome in homolog.chromosomes)
                {
                    foreach (var gene in choromosome.genes)
                    {
                        alleles.Add((SimultaneousTurnStrategyGene) gene);
                    }
                }
            }
            return alleles;
        }
    }
}