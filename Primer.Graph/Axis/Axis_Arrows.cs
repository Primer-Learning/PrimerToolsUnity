using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Graph
{
    public partial class Axis
    {
        [Title("Arrows")]
        [FormerlySerializedAs("_arrowPresence")]
        public ArrowPresence arrowPresence = ArrowPresence.Both;

        [FormerlySerializedAs("_arrowPrefab")] [RequiredIn(PrefabKind.PrefabAsset)]
        public PrefabProvider arrowPrefab;

        private Tween TransitionArrows(Gnome gnome)
        {
            if (arrowPresence == ArrowPresence.Neither)
                return null;

            var endArrow = gnome.Add(arrowPrefab, "End Arrow");
            endArrow.localRotation = Quaternion.Euler(0f, 90f, 0f);
            var endArrowPos = new Vector3(rodEnd, 0f, 0f);
            var endArrowTween = endArrowPos == endArrow.localPosition ? null : endArrow.MoveTo(endArrowPos);

            if (arrowPresence != ArrowPresence.Both)
                return endArrowTween;

            var originArrow = gnome.Add(arrowPrefab, "Origin Arrow");
            originArrow.localRotation = Quaternion.Euler(0f, -90f, 0f);
            var originArrowPos = new Vector3(rodStart, 0f, 0f);
            var originArrowTween = originArrowPos == originArrow.localPosition ? null : originArrow.MoveTo(originArrowPos);

            return Tween.Parallel(endArrowTween, originArrowTween);
        }
    }
}
