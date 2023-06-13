using System;
using Primer.Axis;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Table
{
    // When this is moved we should rename Primer.Graph to Primer.Dataviz
    [Obsolete("This should be moved to Primer.Graph")]

    [RequireComponent(typeof(MultipleAxesController))]
    public class PrimerTable : MonoBehaviour
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
