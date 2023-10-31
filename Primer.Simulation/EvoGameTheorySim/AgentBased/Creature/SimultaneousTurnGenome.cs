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
        public SimultaneousTurnGenome(Gene gene, Rng rng = null) : base(gene, rng) {}
        public SimultaneousTurnGenome(Rng rng = null, params SimultaneousTurnStrategyGene[] genes) : base(rng, genes) {}
        public SimultaneousTurnGenome(SimultaneousTurnStrategyGene[,] genes) : base(genes) {}
        
        public SimultaneousTurnAction GetAction(Rng rng)
        {
            var homolog = homologs.RandomItem(rng);
            var chromosome = homolog.chromosomes.RandomItem(rng);
            var gene = (SimultaneousTurnStrategyGene) chromosome.genes.RandomItem(rng);
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