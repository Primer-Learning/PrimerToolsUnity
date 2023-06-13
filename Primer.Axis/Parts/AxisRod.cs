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

        public void Update(AxisDomain domain)
        {
            transform.localPosition = new Vector3(domain.rodStart, 0f, 0f);
            transform.localScale = new Vector3(domain.rodEnd - domain.rodStart, thickness, thickness);
        }
    }
}
