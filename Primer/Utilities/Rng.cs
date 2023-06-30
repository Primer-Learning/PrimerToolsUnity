namespace Primer
{
    public class Rng
    {
        public static int Range(int maxExclusive) => Range(0, maxExclusive);

        public static int Range(int minInclusive, int maxExclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }

        public static float Range(float maxInclusive) => Range(0, maxInclusive);

        public static float Range(float minInclusive, float maxInclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxInclusive);
        }

        // instance

        public System.Random rand { get; }

        public Rng(System.Random rand)
        {
            this.rand = rand;
        }

        public Rng(int seed) : this(new System.Random(seed))
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

        public static float Range(this Rng rng, float maxInclusive) => Range(rng, 0, maxInclusive);

        public static float Range(this Rng rng, float minInclusive, float maxInclusive)
        {
            var rand = rng?.rand;

            return rand is null
                ? Rng.Range(minInclusive, maxInclusive)
                : (float)(rand.NextDouble() * (maxInclusive - minInclusive) + minInclusive);
        }
    }
}
