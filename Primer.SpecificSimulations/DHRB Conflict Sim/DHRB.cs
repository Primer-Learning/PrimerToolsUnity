using Primer.Simulation;
using Tick = System.Tuple<float, float, float>;

public enum DHRB
{
    Dove,
    Hawk,
    Retaliator,
    Bully
}

public static class AlleleFrequencyExtensions
{
    public static void Deconstruct(this AlleleFrequency<DHRB> a, out float d, out float h, out float r, out float b)
    {
        d = a[DHRB.Dove];
        h = a[DHRB.Hawk];
        r = a[DHRB.Retaliator];
        b = a[DHRB.Bully];
    }
}
