using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Graph
{
    [ExecuteAlways]
    [RequireComponent(typeof(MultipleAxesController))]
    public class Graph : MonoBehaviour
    {
        [Title("Graph controls")]
        [OnValueChanged(nameof(EnsureDomainDimensions))]
        public bool enableZAxis = true;

        [EnableIf(nameof(enableZAxis))]
        [OnValueChanged(nameof(EnsureDomainDimensions))]
        public bool isRightHanded = true;

        private MultipleAxesController axes;

        [Title("Axes references")]
        [FormerlySerializedAs("x")]
        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        private Axis xAxis;

        [FormerlySerializedAs("y")]
        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        private Axis yAxis;

        [FormerlySerializedAs("z")]
        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        private Axis zAxis;

        public Axis x => (xAxis != null) && xAxis.enabled && xAxis.isActiveAndEnabled ? xAxis : null;
        public Axis y => (yAxis != null) && yAxis.enabled && yAxis.isActiveAndEnabled ? yAxis : null;
        public Axis z => (zAxis != null) && zAxis.enabled && zAxis.isActiveAndEnabled ? zAxis : null;

        [OnValueChanged(nameof(EnsureDomainDimensions))]
        public Transform domain;

        private Container contentCache;
        public Container content => contentCache ??= new Container(domain);

        public Action onDomainChanged;


        private void OnEnable() => UpdateAxes();

        private void OnValidate() => UpdateAxes();


        private void UpdateAxes()
        {
            if (zAxis != null) {
                zAxis.SetActive(enableZAxis);

                if (enableZAxis) {
                    zAxis.transform.rotation = isRightHanded
                        ? Quaternion.Euler(0, 90, 0)
                        : Quaternion.Euler(0, -90, 0);
                }
            }

            axes ??= GetComponent<MultipleAxesController>();
            axes.SetAxes(EnsureDomainDimensions, xAxis, yAxis, zAxis);
        }

        public Vector3 DomainToPosition(Vector3 value)
        {
            return new Vector3(
                x?.DomainToPosition(value.x) ?? 0,
                y?.DomainToPosition(value.y) ?? 0,
                z?.DomainToPosition(value.z) ?? 0
            );
        }

        public Vector3 GetScaleNeutralizer(Vector3 originalScale)
        {
            var domainScale = domain.localScale;

            return new Vector3(
                originalScale.x / domainScale.x,
                originalScale.y / domainScale.y,
                originalScale.z / domainScale.z
            );
        }

        private void EnsureDomainDimensions()
        {
            var scale = new Vector3(
                xAxis?.DomainToPosition(1) ?? 1,
                yAxis?.DomainToPosition(1) ?? 1,
                zAxis?.DomainToPosition(1) ?? 1
            );

            if (isRightHanded)
                scale.z *= -1;

            if (domain is null || domain.localScale == scale)
                return;

            domain.localScale = scale;
            onDomainChanged?.Invoke();
        }

        public PrimerLine AddLine(string name)
        {
            return content.Add<PrimerLine>(name);
        }

        public PrimerSurface AddSurface(string name)
        {
            return content.Add<PrimerSurface>(name);
        }

        public StackedArea AddStackedArea(string name)
        {
            return content.Add<StackedArea>(name);
        }

        public NewBarPlot AddBarPlot(string name)
        {
            return content.Add<NewBarPlot>(name);
        }
    }
}
