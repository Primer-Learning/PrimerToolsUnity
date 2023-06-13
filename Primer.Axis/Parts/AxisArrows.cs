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
        public ArrowPresence presence = ArrowPresence.Both;

        [RequiredIn(PrefabKind.PrefabAsset)]
        public PrefabProvider prefab;


        public void Update(Container container, AxisDomain domain)
        {
            if (presence == ArrowPresence.Neither)
                return;

            var endArrow = container.Add(prefab, "End Arrow");
            endArrow.localRotation = Quaternion.Euler(0f, 90f, 0f);
            endArrow.localPosition = new Vector3(domain.rodEnd, 0f, 0f);

            if (presence != ArrowPresence.Both)
                return;

            var originArrow = container.Add(prefab, "Origin Arrow");
            originArrow.localRotation = Quaternion.Euler(0f, -90f, 0f);
            originArrow.localPosition = new Vector3(domain.rodStart, 0f, 0f);
        }
    }
}
