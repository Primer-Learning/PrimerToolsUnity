using System.Collections.Generic;
using Primer.Axis;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    [ExecuteInEditMode]
    public class Graph2 : PrimerBehaviour
    {
        [Title("Graph controls")]
        public bool enableZAxis = true;
        public bool isRightHanded = true;
        public Transform domain;

        [ShowInInspector]
        private MultipleAxesController axes = new();

        [Title("Axes references")]
        [SerializeField]
        [InlineEditor]
        [ChildGameObjectsOnly]
        private AxisRenderer x;

        [SerializeField]
        [InlineEditor]
        [ChildGameObjectsOnly]
        private AxisRenderer y;

        [SerializeField]
        [InlineEditor]
        [ChildGameObjectsOnly]
        private AxisRenderer z;


        // This warning is show where these properties are used, we checked for != null here
        // ReSharper disable Unity.NoNullPropagation
        private AxisRenderer enabledXAxis => (x != null) && x.enabled && x.isActiveAndEnabled ? x : null;
        private AxisRenderer enabledYAxis => (y != null) && y.enabled && y.isActiveAndEnabled ? y : null;
        private AxisRenderer enabledZAxis => (z != null) && z.enabled && z.isActiveAndEnabled ? z : null;


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

            axes.SetAxes(EnsureDomainDimensions, x, y, z);
        }

        public Vector3 DomainToPosition(Vector3 domain) => new(
            enabledXAxis?.DomainToPosition(domain.x) ?? 0,
            enabledYAxis?.DomainToPosition(domain.y) ?? 0,
            enabledZAxis?.DomainToPosition(domain.z) ?? 0
        );

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

            var childCount = domain.childCount;
            var children = new List<(Transform, Vector3)>();

            for (var i = childCount - 1; i >= 0; i--) {
                var child = domain.GetChild(i);
                children.Add((child, child.localPosition));
                child.parent = null;
            }

            domain.localScale = scale;

            for (var i = childCount - 1; i >= 0; i--) {
                var (child, pos) = children[i];
                child.parent = domain;
                child.localPosition = pos;
            }
        }
    }
}
