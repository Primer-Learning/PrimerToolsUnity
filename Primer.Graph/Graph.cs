using System;
using Sirenix.OdinInspector;
using UnityEngine;

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

        [OnValueChanged(nameof(EnsureDomainDimensions))]
        public Transform domain;

        private MultipleAxesController axes;

        [Title("Axes references")]
        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        private Axis x;

        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        private Axis y;

        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        private Axis z;


        // This warning is show where these properties are used, we checked for != null here
        // ReSharper disable Unity.NoNullPropagation
        public Axis enabledXAxis => (x != null) && x.enabled && x.isActiveAndEnabled ? x : null;
        public Axis enabledYAxis => (y != null) && y.enabled && y.isActiveAndEnabled ? y : null;
        public Axis enabledZAxis => (z != null) && z.enabled && z.isActiveAndEnabled ? z : null;

        public Vector3 positionMultiplier => new(
            enabledXAxis ? 1 : 0,
            enabledYAxis ? 1 : 0,
            enabledZAxis ? 1 : 0
        );

        public Action onDomainChanged;


        private void OnEnable() => UpdateAxes();

        private void OnValidate() => UpdateAxes();


        private void UpdateAxes()
        {
            if (z != null) {
                z.gameObject.SetActive(enableZAxis);

                if (enableZAxis) {
                    z.transform.rotation = isRightHanded
                        ? Quaternion.Euler(0, 90, 0)
                        : Quaternion.Euler(0, -90, 0);
                }
            }

            axes ??= GetComponent<MultipleAxesController>();
            axes.SetAxes(EnsureDomainDimensions, x, y, z);
        }

        public Vector3 DomainToPosition(Vector3 value) => new(
            enabledXAxis?.DomainToPosition(value.x) ?? 0,
            enabledYAxis?.DomainToPosition(value.y) ?? 0,
            enabledZAxis?.DomainToPosition(value.z) ?? 0
        );

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
                x?.DomainToPosition(1) ?? 1,
                y?.DomainToPosition(1) ?? 1,
                z?.DomainToPosition(1) ?? 1
            );

            if (isRightHanded)
                scale.z *= -1;

            if (domain is null || domain.localScale == scale)
                return;

            // TODO: this causes problems when we don't want to change the scale of domain childrens
            // this should be converted into a Component "InvertParentScale" that listens to onDomainChange
            // beforeDomainChange can be added to track the previous scale value

            // var childCount = domain.childCount;
            // var children = new List<(Transform child, Vector3 pos, Vector3 scale)>();

            // Reverse iteration because we remove them as we go
            // for (var i = childCount - 1; i >= 0; i--) {
            //     var child = domain.GetChild(i);
            //     children.Add((child, child.localPosition, child.localScale));
            //     child.parent = null;
            // }

            domain.localScale = scale;

            // foreach (var (child, pos, prevScale) in children) {
            //     child.parent = domain;
            //     child.localPosition = pos;
            //     child.localScale = prevScale;
            // }

            onDomainChanged?.Invoke();
        }
    }
}
