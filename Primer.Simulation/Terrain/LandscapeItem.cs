using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Simulation
{
    public class LandscapeItem : MonoBehaviour
    {
        public Landscape landscape;

        [Button]
        public void TouchGround()
        {
            landscape ??= GetComponentInParent<Landscape>() ?? GetComponentInParent<ISimulation>()?.terrain;
            transform.position = landscape.GetGroundAt(transform.position);
        }
    }
}
