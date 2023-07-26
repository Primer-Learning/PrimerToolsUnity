using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Simulation
{
    public class LandscapeItem : MonoBehaviour
    {
        [SerializeField]
        private Landscape _landscape;

        public Landscape landscape
        {
            get => _landscape ? _landscape : transform.GetComponentInParent<Landscape>();
            set => _landscape = value;
        } 

        [Button]
        public void TouchGround()
        {
            transform.position = landscape.GetGroundAt(transform.position);
        }
    }
}
