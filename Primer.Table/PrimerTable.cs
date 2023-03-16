using Primer.Axis;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Table
{
    [RequireComponent(typeof(MultipleAxesController))]
    public class PrimerTable : MonoBehaviour
    {
        private MultipleAxesController axes;

        [InlineEditor]
        [ChildGameObjectsOnly]
        // [RequiredIn(PrefabKind.PrefabAsset)]
        public GridGenerator grid;

        [Title("Axes references")]
        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        public AxisRenderer x;

        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        public AxisRenderer y;

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
