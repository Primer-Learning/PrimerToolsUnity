using System.Collections.Generic;
using Primer.Axis;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Graph
{
    [ExecuteInEditMode]
    public class Graph2 : PrimerBehaviour, IAxisConfig
    {
        [Header("Prefabs")]
        [SerializeField]
        [FormerlySerializedAs("arrowPrefab")]
        private Transform _arrowPrefab;

        [SerializeField]
        [FormerlySerializedAs("paddingFraction")]
        [Range(0, 0.5f)]
        private float _paddingFraction = 0.05f;

        [SerializeField]
        [FormerlySerializedAs("primerTextPrefab")]
        private PrimerText2 _primerTextPrefab;

        [SerializeField]
        [FormerlySerializedAs("ticLabelDistance")]
        private float _ticLabelDistance = 0.25f;

        [SerializeField]
        [FormerlySerializedAs("ticPrefab")]
        private Tic2 _ticPrefab;

        [Header("Axes")]
        public AxisRenderer _x;
        public AxisRenderer _y;
        public AxisRenderer _z;
        public Transform domain;
        public bool enableZAxis = true;
        public bool isRightHanded = true;

        private bool lastRightHanded = true;
        private AxisRenderer x => _x is not null && !_x.hidden && _x.enabled ? _x : null;
        private AxisRenderer y => _y is not null && !_y.hidden && _y.enabled ? _y : null;
        private AxisRenderer z => _z is not null && !_z.hidden && _z.enabled ? _z : null;

        public float ticLabelDistance => _ticLabelDistance;
        public float paddingFraction => _paddingFraction;
        public Transform arrowPrefab => _arrowPrefab;
        public PrimerText2 primerTextPrefab => _primerTextPrefab;
        public Tic2 ticPrefab => _ticPrefab;

        private void Update()
        {
            EnsureDomainDimensions();
        }

        private void OnEnable()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            EnsureRightHanded();
            _z.hidden = !enableZAxis;
        }

        public void Regenerate()
        {
            // we use internal (_) fields because we want them to
            // update their children even if they are hidden
            // as this is when they delete all unused objects
            if (_x is not null)
                _x.UpdateChildren();

            if (_y is not null)
                _y.UpdateChildren();

            if (_z is not null)
                _z.UpdateChildren();
        }

        public Vector3 DomainToPosition(Vector3 domain) => new(
            x ? x.DomainToPosition(domain.x) : 0,
            y ? y.DomainToPosition(domain.y) : 0,
            z ? z.DomainToPosition(domain.z) : 0
        );

        private void EnsureDomainDimensions()
        {
            var scale = new Vector3(
                _x ? _x.DomainToPosition(1, true) : 1,
                _y ? _y.DomainToPosition(1, true) : 1,
                _z ? _z.DomainToPosition(1, true) : 1
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
