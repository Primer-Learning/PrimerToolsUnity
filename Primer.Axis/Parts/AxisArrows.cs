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

        public ArrowPresence presence = ArrowPresence.Both;

        [RequiredIn(PrefabKind.PrefabAsset)]
        public PrefabProvider prefab;


        public void Update(ChildrenDeclaration declaration, AxisDomain domain)
        {
            if (presence == ArrowPresence.Neither) {
                endArrow = null;
                originArrow = null;
                return;
            }

            declaration.NextIsInstanceOf(
                prefab,
                cache: ref endArrow,
                name: "End Arrow",
                init: x => x.localRotation = Quaternion.Euler(0f, 90f, 0f)
            );

            endArrow.localPosition = new Vector3(domain.rodEnd, 0f, 0f);

            if (presence != ArrowPresence.Both) {
                originArrow = null;
                return;
            }

            declaration.NextIsInstanceOf(
                prefab,
                cache: ref originArrow,
                name: "Origin Arrow",
                init: x => x.localRotation = Quaternion.Euler(0f, -90f, 0f)
            );

            originArrow.localPosition = new Vector3(domain.rodStart, 0f, 0f);
        }
    }
}
