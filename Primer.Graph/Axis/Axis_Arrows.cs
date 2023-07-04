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

        private void UpdateArrows(Container container)
        {
            var domain = this;

            if (arrowPresence == ArrowPresence.Neither)
                return;

            var endArrow = container.Add(arrowPrefab, "End Arrow");
            endArrow.localRotation = Quaternion.Euler(0f, 90f, 0f);
            endArrow.localPosition = new Vector3(domain.rodEnd, 0f, 0f);

            if (arrowPresence != ArrowPresence.Both)
                return;

            var originArrow = container.Add(arrowPrefab, "Origin Arrow");
            originArrow.localRotation = Quaternion.Euler(0f, -90f, 0f);
            originArrow.localPosition = new Vector3(domain.rodStart, 0f, 0f);
        }
    }
}
