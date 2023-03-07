using System.Threading;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Tools
{
    [ExecuteAlways]
    public class PrimerBracket2 : MonoBehaviour, IMeshRendererController
    {
        #region Children objects
        [SerializeField, PrefabChild]
        private Transform leftTip;
        [SerializeField, PrefabChild]
        private Transform leftBar;
        [SerializeField, PrefabChild]
        private Transform leftCenter;
        [SerializeField, PrefabChild]
        private Transform rightCenter;
        [SerializeField, PrefabChild]
        private Transform rightBar;
        [SerializeField, PrefabChild]
        private Transform rightTip;
        #endregion

        #region public float width;
        [SerializeField, HideInInspector]
        private float _width = 1f;

        [ShowInInspector]
        [PropertyOrder(-1)]
        [PropertyRange(0.01f, 10f)]
        public float width {
            get => _width;
            set {
                _width = value;
                Refresh();
            }
        }
        #endregion

        #region public Color color;
        [SerializeField, HideInInspector]
        private Color _color = Color.white;

        [ShowInInspector]
        [PropertyOrder(-1)]
        public Color color {
            get => _color;
            set {
                _color = value;
                this.SetColor(value);
            }
        }
        #endregion

        #region public Material material;
        [SerializeField, HideInInspector]
        private Material _material;

        [ShowInInspector]
        [PropertyOrder(-1)]
        public Material material {
            get => _material ??= IMeshRendererController.defaultMaterial;
            set {
                _material = value;
                this.SetMaterial(value);
            }
        }
        #endregion

        [Title("Details")]
        [PropertyOrder(-1)]
        public float tipWidth = 0.39f;

        #region public Vector3 anchor;
        [Title("Anchor")]
        public ScenePoint anchorPoint = Vector3.zero;

        public Vector3 anchor {
            get => anchorPoint.value;
            set => anchorPoint.value = value;
        }
        #endregion

        #region public Vector3 left;
        [Title("Left")]
        public ScenePoint leftPoint = new Vector3(-1, 0, 1);

        public Vector3 left {
            get => leftPoint.value;
            set => leftPoint.value = value;
        }
        #endregion

        #region public Vector3 right;
        [Title("Right")]
        public ScenePoint rightPoint = new Vector3(1, 0, 1);

        public Vector3 right {
            get => rightPoint.value;
            set => rightPoint.value = value;
        }
        #endregion

        MeshRenderer[] IMeshRendererController.meshRenderers => GetComponentsInChildren<MeshRenderer>();


        #region Unity events
        private void OnEnable()
        {
            anchorPoint.onChange = Refresh;
            leftPoint.onChange = Refresh;
            rightPoint.onChange = Refresh;
        }

        private void OnDisable()
        {
            anchorPoint.onChange = null;
            leftPoint.onChange = null;
            rightPoint.onChange = null;
        }

        private void OnValidate()
        {
            Refresh();
        }

        private void Update()
        {
            if (ScenePoint.CheckTrackedObject(anchorPoint, leftPoint, rightPoint)) {
                Refresh();
            }
        }
        #endregion


        public void SetPoints(Vector3? anchor = null, Vector3? left = null, Vector3? right = null, bool? isGlobal = null)
        {
            if (anchor.HasValue) this.anchor = anchor.Value;
            if (left.HasValue) this.left = left.Value;
            if (right.HasValue) this.right = right.Value;

            if (!isGlobal.HasValue)
                return;

            anchorPoint.isWorldPosition = isGlobal.Value;
            leftPoint.isWorldPosition = isGlobal.Value;
            rightPoint.isWorldPosition = isGlobal.Value;
        }

        public async UniTask Animate(Vector3? anchor = null, Vector3? left = null, Vector3? right = null,
            Tweener animation = null, CancellationToken ct = default)
        {
            var anchorStart = this.anchor;
            var leftStart = this.left;
            var rightStart = this.right;
            var anchorEnd = anchor ?? anchorStart;
            var leftEnd = left ?? leftStart;
            var rightEnd = right ?? rightStart;

            if (this.anchor == anchorEnd && this.left == leftEnd && this.right == rightEnd)
                return;

            if (!Application.isPlaying) {
                this.anchor = anchorEnd;
                this.left = leftEnd;
                this.right = rightEnd;
                return;
            }

            await foreach (var t in animation.Tween(0, 1f, ct)) {
                if (ct.IsCancellationRequested) return;

                var updatedAnchor = anchorPoint.isTracking ? anchorPoint.value : anchorStart;
                var updatedLeft = leftPoint.isTracking ? leftPoint.value : leftStart;
                var updatedRight = rightPoint.isTracking ? rightPoint.value : rightStart;

                anchorPoint.value = Vector3.Lerp(updatedAnchor, anchor ?? updatedAnchor, t);
                leftPoint.value = Vector3.Lerp(updatedLeft, left ?? updatedLeft, t);
                rightPoint.value = Vector3.Lerp(updatedRight, right ?? updatedRight, t);
            }
        }

        // This method is marked as performance intensive because it logs a warning 🤦
        // ReSharper disable Unity.PerformanceAnalysis
        [Title("Controls", horizontalLine: false)]
        [Button("Refresh")]
        public void Refresh()
        {
            if (leftTip == null || leftBar == null || rightBar == null || rightTip == null || gameObject.IsPreset())
                return;

            var self = transform;
            var parent = self.parent;

            var anchorLocal = anchorPoint.GetLocalPosition(parent);
            var leftLocal = leftPoint.GetLocalPosition(parent);
            var rightLocal = rightPoint.GetLocalPosition(parent);

            // mouth is the open side of the bracket
            var mouth = leftLocal - rightLocal;
            var toLeft = leftLocal - anchorLocal;
            var toRight = rightLocal - anchorLocal;

            // Cross returns a vector that is orthogonal (perpendicular) to both input parameters
            var upwards = Vector3.Cross(toLeft, toRight);
            var forward = Vector3.Cross(upwards, mouth);
            var center = Vector3.Project(toLeft, forward);

            var leftLength = BarLength(toLeft, center);
            var rightLength = BarLength(toRight, center);

            if (leftLength < 0.01f || rightLength < 0.01f || Mathf.Abs(leftLength + rightLength) > mouth.magnitude) {
                Debug.LogWarning("Refusing to render a broken-looking bracket");
                return;
            }

            leftBar.localScale = new Vector3(leftLength, 1, 1);
            rightBar.localScale = new Vector3(rightLength, 1, 1);

            self.rotation = Quaternion.LookRotation(forward,  upwards);
            self.localScale = new Vector3(width, width, center.magnitude);

            self.position = anchorPoint.GetWorldPosition(parent);
            leftTip.position = leftPoint.GetWorldPosition(parent);
            rightTip.position = rightPoint.GetWorldPosition(parent);
        }

        private float BarLength(Vector3 toSide, Vector3 center)
        {
            var diff = toSide - center;
            var distance = diff.magnitude;
            var tipsWidth = tipWidth * width * 2;
            return (distance - tipsWidth) / width;
        }
    }
}
