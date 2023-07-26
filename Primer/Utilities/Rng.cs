using System;

namespace Primer
{
    public class Rng
    {
        private static Random staticRandom;

        public static void Initialize(int? seed = null)
        {
            if (seed is null)
            {
                // If seed is null, we don't care what the seed is.
                // So if the staticRandom object already exists, we don't need to do anything.
                // But if it doesn't, creat it and set the seed to the current time.
                if (staticRandom is not null) return;
                staticRandom = new Random(Environment.TickCount);
            }
            else
            {
                // If we did give a seed, we should remake the staticRandom object with that seed.
                staticRandom = new Random(seed.Value);
            }
        }
        
        public static int Range(int maxExclusive) => Range(0, maxExclusive);

        public static int Range(int minInclusive, int maxExclusive)
        {
            Initialize();
            return staticRandom.Next(minInclusive, maxExclusive);
        }

        public static float Range(float maxInclusive) => Range(0, maxInclusive);

        public static float Range(float minInclusive, float maxExclusive)
        {
            Initialize();
            return (float) staticRandom.NextDouble() * (maxExclusive - minInclusive) + minInclusive;
        }

        // instance

        public Random rand { get; }

        public Rng(Random rand)
        {
            this.rand = rand;
        }

        public Rng(int seed) : this(new Random(seed))
        {
        }
    }

    // Methods are declared as extension methods so that they can be used even on `null` Rngs.
    public static class RngExtensions
    {
        public static int Range(this Rng rng, int maxExclusive) => Range(rng, 0, maxExclusive);

        public static int Range(this Rng rng, int minInclusive, int maxExclusive)
        {
            var rand = rng?.rand;

            return rand is null
                ? Rng.Range(minInclusive, maxExclusive)
                : rand.Next(minInclusive, maxExclusive);
        }

        public static float Range(this Rng rng, float maxExclusive) => Range(rng, 0, maxExclusive);

        public static float Range(this Rng rng, float minInclusive, float maxExclusive)
        {
            var rand = rng?.rand;

            return rand is null
                ? Rng.Range(minInclusive, maxExclusive)
                : (float)(rand.NextDouble() * (maxExclusive - minInclusive) + minInclusive);
        }
    }
}
