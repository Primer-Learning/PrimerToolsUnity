using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    public partial class Axis
    {
        #region public ArrowPresence arrowPresence;
        [SerializeField, HideInInspector]
        private ArrowPresence _arrowPresence = ArrowPresence.Both;

        [Title("Arrows")]
        [ShowInInspector]
        public ArrowPresence arrowPresence {
            get => _arrowPresence;
            set {
                _arrowPresence = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public PrefabProvider arrowPrefab;
        [RequiredIn(PrefabKind.PrefabAsset)]
        [SerializeField, HideInInspector]
        private PrefabProvider _arrowPrefab;

        [ShowInInspector]
        public PrefabProvider arrowPrefab {
            get => _arrowPrefab;
            set {
                _arrowPrefab = value;
                UpdateChildren();
            }
        }
        #endregion

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
