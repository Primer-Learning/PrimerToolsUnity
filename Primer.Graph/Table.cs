using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    [RequireComponent(typeof(MultipleAxesController))]
    public class Table : MonoBehaviour
    {
        private MultipleAxesController axes;

        [InlineEditor]
        [ChildGameObjectsOnly]
        public GridGenerator grid;

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

        private void OnEnable() => UpdateAxes();

        private void OnValidate() => UpdateAxes();


        private void UpdateAxes()
        {
            axes ??= GetComponent<MultipleAxesController>();
            axes.SetAxes(UpdateDomain, x, y);
        }

        private void UpdateDomain()
        {
            // noop
        }
    }
}
