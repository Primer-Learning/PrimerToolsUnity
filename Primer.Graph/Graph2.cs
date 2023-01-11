using System.Collections.Generic;
using Primer.Axis;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Graph
{
    [ExecuteInEditMode]
    public class Graph2 : PrimerBehaviour
    {
        private bool lastRightHanded = true;


        public bool enableZAxis = true;
        public bool isRightHanded = true;
        public Transform domain;

        [FormerlySerializedAs("_x")]
        [InlineEditor] public AxisRenderer xAxis;
        [FormerlySerializedAs("_y")]
        [InlineEditor] public AxisRenderer yAxis;
        [FormerlySerializedAs("_z")]
        [InlineEditor] public AxisRenderer zAxis;


        // This warning is show everywhere this properties are used even if we checked for null here
        // ReSharper disable Unity.NoNullPropagation
        private AxisRenderer x => (xAxis != null) && xAxis.isActiveAndEnabled ? xAxis : null;
        private AxisRenderer y => (yAxis != null) && yAxis.isActiveAndEnabled ? yAxis : null;
        private AxisRenderer z => (zAxis != null) && zAxis.isActiveAndEnabled ? zAxis : null;


        private void Update() => EnsureDomainDimensions();

        private void OnEnable() => UpdateAxes();

        private void OnValidate() => UpdateAxes();


        private void UpdateAxes()
        {
            EnsureRightHanded();
            zAxis.gameObject.SetActive(enableZAxis);
        }

        public Vector3 DomainToPosition(Vector3 domain) => new(
            x?.DomainToPosition(domain.x) ?? 0,
            y?.DomainToPosition(domain.y) ?? 0,
            z?.DomainToPosition(domain.z) ?? 0
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

        private void EnsureRightHanded()
        {
            if (z is null)
                return;

            if (isRightHanded == lastRightHanded)
                return;

            lastRightHanded = isRightHanded;

            z.transform.rotation = isRightHanded
                ? Quaternion.Euler(0, 90, 0)
                : Quaternion.Euler(0, -90, 0);
        }
    }
}
