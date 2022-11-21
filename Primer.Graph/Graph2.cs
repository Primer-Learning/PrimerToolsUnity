using System.Collections.Generic;
using Shapes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Graph
{
    [ExecuteInEditMode]
    public class Graph2 : PrimerBehaviour
    {
        [FormerlySerializedAs("ticLabelDistanceVertical")]
        public float ticLabelDistance = 0.25f;
        // public float ticLabelDistanceHorizontal = 0.65f;
        [Range(0, 0.5f)]
        public float paddingFraction = 0.05f;
        public bool isRightHanded = true;
        public bool enableZAxis = true;
        public Transform domain;

        [Header("Axes")]
        public Axis2 _x;
        Axis2 x => _x is not null && !_x.hidden && _x.enabled ? _x : null;
        public Axis2 _y;
        Axis2 y => _y is not null && !_y.hidden && _y.enabled ? _y : null;
        public Axis2 _z;
        Axis2 z => _z is not null && !_z.hidden && _z.enabled ? _z : null;

        [Header("Prefabs")]
        public PrimerBehaviour arrowPrefab;
        public PrimerText2 primerTextPrefab;
        public Tic2 ticPrefab;
        public Polyline linePrefab;

        void Update() {
            EnsureDomainDimensions();
        }

        void OnEnable() {
            OnValidate();
        }

        void OnValidate() {
            EnsureRightHanded();
            _z.hidden = !enableZAxis;
        }

        public void Regenerate() {
            // we use internal (_) fields because we want them to
            // update their children even if they are hidden
            // as this is when they delete all unused objects
            if (_x is not null) _x.UpdateChildren();
            if (_y is not null) _y.UpdateChildren();
            if (_z is not null) _z.UpdateChildren();
        }

        public Vector3 DomainToPosition(Vector3 domain) => new Vector3(
            x ? x.DomainToPosition(domain.x) : 0,
            y ? y.DomainToPosition(domain.y) : 0,
            z ? z.DomainToPosition(domain.z) : 0
        );

        void EnsureDomainDimensions() {
            var scale = new Vector3(
                _x ? _x.DomainToPosition(1, true) : 1,
                _y ? _y.DomainToPosition(1, true) : 1,
                _z ? _z.DomainToPosition(1, true) : 1
            );

            if (isRightHanded) scale.z *= -1;

            if (domain is null || domain.localScale == scale) return;

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

        bool lastRightHanded = true;
        void EnsureRightHanded() {
            if (z is null) return;
            if (isRightHanded == lastRightHanded) return;

            lastRightHanded = isRightHanded;

            z.transform.rotation = isRightHanded
                ? Quaternion.Euler(0, 90, 0)
                : Quaternion.Euler(0, -90, 0);
        }
    }
}
