using Cysharp.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Simulation
{
    public interface ISimulation
    {
        public Rng rng { get; }
        public bool skipAnimations { get; }
        // public UniTask SimulateSingleCycle();
    }
}
