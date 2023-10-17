using System;
using Random = System.Random;

namespace Primer
{
    public class Rng
    {
        public static Random staticRandom;

        public static void Initialize(int? seed = null)
        {
            if (seed is null)
            {
                // If seed is null, we don't care what the seed is.
                // So if the staticRandom object already exists, we don't need to do anything.
                // But if it doesn't, create it and set the seed to the current time.
                if (staticRandom is not null) return;
                staticRandom = new Random(Environment.TickCount);
            }
            else
            {
                // If we did give a seed, we should remake the staticRandom object with that seed.
                staticRandom = new Random(seed.Value);
            }
        }
        
        public static int RangeInt(int maxExclusive) => RangeInt(0, maxExclusive);

        public static int RangeInt(int minInclusive, int maxExclusive)
        {
            Initialize();
            return staticRandom.Next(minInclusive, maxExclusive);
        }

        public static float RangeFloat(float maxInclusive) => RangeFloat(0, maxInclusive);

        public static float RangeFloat(float minInclusive, float maxExclusive)
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
        public static int RangeInt(this Rng rng, int maxExclusive) => RangeInt(rng, 0, maxExclusive);

        public static int RangeInt(this Rng rng, int minInclusive, int maxExclusive)
        {
            var rand = rng?.rand;

            return rand is null
                ? Rng.RangeInt(minInclusive, maxExclusive)
                : rand.Next(minInclusive, maxExclusive);
        }

        public static float RangeFloat(this Rng rng, float maxExclusive) => RangeFloat(rng, 0, maxExclusive);

        public static float RangeFloat(this Rng rng, float minInclusive, float maxExclusive)
        {
            var rand = rng?.rand;

            return rand is null
                ? Rng.RangeFloat(minInclusive, maxExclusive)
                : (float)(rand.NextDouble() * (maxExclusive - minInclusive) + minInclusive);
        }
    }
}
