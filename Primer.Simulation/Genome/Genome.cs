using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using Primer.Simulation.Genome.Strategy;

namespace Primer.Simulation.EvoGameTheorySim.AgentBased
{
    public class Genome<T> where T : Genome<T>
    {
        private int ploidy => homologs[0].chromosomes.Length;
        protected Homolog[] homologs;

        public Genome() {}

        // Simple constructor for a haploid genome with single-gene chromosomes.
        public Genome(params Gene[] genes)
        {
            var length = genes.Length;
            homologs = new Homolog[length];
            for (var i = 0; i < length; i++)
            {
                homologs[i] = new Homolog();
                homologs[i].chromosomes = new Chromosome[1];
                homologs[i].chromosomes[0] = new Chromosome();
                homologs[i].chromosomes[0].genes = new Gene[1];
                homologs[i].chromosomes[0].genes[0] = genes[i];
            }
        }
        
        // Slightly more complex constructor for an n-ploid genome with single-gene chromosomes.
        public Genome(Gene[,] genes)
        {
            var numHomologs = genes.GetLength(0);
            var numChromosomes = genes.GetLength(1);
            
            homologs = new Homolog[numHomologs];
            for (var i = 0; i < numHomologs; i++)
            {
                homologs[i] = new Homolog();
                for (var j = 0; j < numChromosomes; j++)
                {
                    homologs[i].chromosomes[j] = new Chromosome();
                    homologs[i].chromosomes[j].genes = new Gene[1];
                    homologs[i].chromosomes[j].genes[0] = genes[i,j];
                }
            }
        }
        
        // Most general constructor with specifying homologs.
        public Genome(Homolog[] homologs)
        {
            this.homologs = homologs;
        }
        
        private Chromosome[] CreateGamete()
        {
            var gamete = new Chromosome[homologs.Length];
            for (var i = 0; i < homologs.Length; i++)
            {
                gamete[i] = homologs[i].chromosomes[Rng.RangeInt(0, ploidy)];
            }
            return gamete;
        }
        
        public T SexuallyReproduce(T otherParent)
        {
            var gamete1 = CreateGamete();
            var gamete2 = otherParent.CreateGamete();
            
            var newHomologs = new Homolog[homologs.Length];
            for (var i = 0; i < homologs.Length; i++)
            {
                // Put the gametes together into a new homolog with ploidy chromosomes.
                newHomologs[i] = new Homolog
                {
                    chromosomes =  new Chromosome[] {gamete1[i], gamete2[i]}
                };
            }
            
            var offspring = Activator.CreateInstance<T>();
            offspring.homologs = newHomologs;
            return offspring;
        }

        
        // Current thinking is that classes inheriting from Genome
        // will have methods for expressing a trait from the relevant genes.
    }
    
    // A homolog is a set of corresponding chromosomes
    // of size n for an n-ploid organism.
    public class Homolog
    {
        internal Chromosome[] chromosomes;
    }
    public class Chromosome
    {
        internal Gene[] genes;
    }
    public abstract class Gene
    {
        private static DominanceType dominanceType;
    }
    public enum DominanceType
    {
        Dominant,
        Recessive,
        CoDominant
    }
}