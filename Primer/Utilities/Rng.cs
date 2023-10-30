using System;
using UnityEngine;
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
        
        public static int RangeIntStatic(int maxExclusive) => RangeIntStatic(0, maxExclusive);

        public static int RangeIntStatic(int minInclusive, int maxExclusive)
        {
            Debug.Log("static rng int");
            Initialize();
            return staticRandom.Next(minInclusive, maxExclusive);
        }

        public static float RangeFloatStatic(float maxInclusive) => RangeFloatStatic(0, maxInclusive);

        public static float RangeFloatStatic(float minInclusive, float maxExclusive)
        {
            Debug.Log("static rng float");
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
        public int RangeInt(int maxExclusive) => RangeInt(0, maxExclusive);

        public int RangeInt(int minInclusive, int maxExclusive)
        {
            return rand.Next(minInclusive, maxExclusive);
        }

        public float RangeFloat(float maxExclusive) => RangeFloat(0, maxExclusive);

        public float RangeFloat(float minInclusive, float maxExclusive)
        {
            return (float)(rand.NextDouble() * (maxExclusive - minInclusive) + minInclusive);
        }
    }

    // Methods are declared as extension methods so that they can be used even on `null` Rngs.
    public static class RngExtensions
    {
    }
}
