using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Axis
{
    [Serializable]
    [Title("Arrows")]
    [HideLabel]
    [InlineProperty]
    [DisableContextMenu]
    [HideReferenceObjectPicker]
    internal class AxisArrows
    {
        private Transform originArrow;
        private Transform endArrow;

        [Required]
        [RequiredIn(PrefabKind.PrefabAsset)]
        public Transform prefab;

        public ArrowPresence presence = ArrowPresence.Both;


        public void Update(ChildrenDeclaration declaration, AxisDomain domain)
        {
            if (presence == ArrowPresence.Neither) {
                endArrow = null;
                originArrow = null;
                return;
            }

            declaration.NextIsInstanceOf(
                prefab: prefab,
                cache: ref endArrow,
                name: "End Arrow",
                init: x => x.localRotation = Quaternion.Euler(0f, 90f, 0f)
            );

            endArrow.localPosition = new Vector3(domain.length + domain.offset, 0f, 0f);

            if (presence != ArrowPresence.Both) {
                originArrow = null;
                return;
            }

            declaration.NextIsInstanceOf(
                prefab: prefab,
                cache: ref originArrow,
                name: "Origin Arrow",
                init: x => x.localRotation = Quaternion.Euler(0f, -90f, 0f)
            );

            originArrow.localPosition = new Vector3(domain.offset, 0f, 0f);
        }
    }
}
