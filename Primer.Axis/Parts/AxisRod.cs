using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Axis
{
    [HideLabel]
    [Serializable]
    [InlineProperty]
    [DisableContextMenu]
    [HideReferenceObjectPicker]
    [Title("Rod")]
    internal class AxisRod
    {
        [ChildGameObjectsOnly]
        public Transform transform;

        public float thickness = 1;


        public void AddTo(ChildrenDeclaration declaration)
            => declaration.NextIs(transform);

        public void Update(AxisDomain domain)
        {
            transform.localPosition = new Vector3(domain.offset, 0f, 0f);
            transform.localScale = new Vector3(domain.length, thickness, thickness);
        }
    }
}
