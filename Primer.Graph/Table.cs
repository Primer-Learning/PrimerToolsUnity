using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    public class Table : MonoBehaviour, IAxisController
    {
        [Title("Axes references")]
        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        public Axis x;

        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        public Axis y;

        public Axis[] axes => new[] { x, y };
    }
}
