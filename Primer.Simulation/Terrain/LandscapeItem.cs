using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Simulation
{
    public class LandscapeItem : MonoBehaviour
    {
        [SerializeField]
        private Landscape _landscape;
        public Landscape landscape => this.FindTerrain(ref _landscape);

        [Button]
        public void TouchGround()
        {
            transform.position = landscape.GetGroundAt(transform.position);
        }
    }
}
