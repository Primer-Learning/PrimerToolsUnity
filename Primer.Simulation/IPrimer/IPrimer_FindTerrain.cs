using UnityEngine;

namespace Primer.Simulation
{
    public static class IPrimer_FindTerrainExtensions
    {
        public static Landscape FindTerrain(this IPrimer self)
        {
            return self.transform.GetComponentInParent<Landscape>() ??
                self.transform.GetComponentInParent<ISimulation>()?.terrain;
        }

        public static Landscape FindTerrain(this IPrimer self, ref Landscape cache)
        {
            if (cache != null)
                return cache;

            return self.transform.GetComponentInParent<Landscape>() ??
                self.transform.GetComponentInParent<ISimulation>()?.terrain;
        }

        public static Landscape FindTerrain(this Component self)
        {
            return self.transform.GetComponentInParent<Landscape>() ??
                self.transform.GetComponentInParent<ISimulation>()?.terrain;
        }

        public static Landscape FindTerrain(this Component self, ref Landscape cache)
        {
            if (cache != null)
                return cache;

            return self.transform.GetComponentInParent<Landscape>() ??
                self.transform.GetComponentInParent<ISimulation>()?.terrain;
        }
    }
}
