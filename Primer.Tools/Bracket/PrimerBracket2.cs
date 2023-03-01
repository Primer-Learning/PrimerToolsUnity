using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Tools
{
    [ExecuteAlways]
    public class PrimerBracket2 : MonoBehaviour
    {
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

        [HideLabel, Title("Anchor")]
        public ScenePoint anchorPoint = Vector3.zero;

        [HideLabel, Title("Left")]
        public ScenePoint leftPoint = new Vector3(-1, 0, 1);

        [HideLabel, Title("Right")]
        public ScenePoint rightPoint = new Vector3(1, 0, 1);

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

        private void Update()
        {
            if (ScenePoint.CheckTrackedObject(anchorPoint, leftPoint, rightPoint)) {
                Refresh();
            }
        }

        [Title("Controls", horizontalLine: false)]
        [Button("Refresh")]
        private void Refresh()
        {
            var self = transform;
            var parent = self.parent;

            var anchor = anchorPoint.GetLocalPosition(parent);
            var left = leftPoint.GetLocalPosition(parent);
            var right = rightPoint.GetLocalPosition(parent);

            // mouth is the open side of the bracket
            var mouth = left - right;
            var toLeft = left - anchor;
            var toRight = right - anchor;

            // Cross returns a vector that is orthogonal (perpendicular) to both input parameters
            var upwards = Vector3.Cross(toLeft, toRight);
            var forward = Vector3.Cross(upwards, mouth);
            var center = Vector3.Project(toLeft, forward);

            var leftLength = (toLeft - center).magnitude;
            var rightLength = (toRight - center).magnitude;

            if (leftLength < 0.01f || rightLength < 0.01f) {
                Debug.LogWarning("Refusing to render a broken-looking bracket");
                return;
            }

            leftBar.localScale = new Vector3(leftLength - Mathf.Abs(leftBar.localPosition.x * 2), 1, 1);
            rightBar.localScale = new Vector3(rightLength - Mathf.Abs(rightBar.localPosition.x * 2), 1, 1);

            self.rotation = Quaternion.LookRotation(forward,  upwards);
            self.localScale = new Vector3(1, 1, center.magnitude);

            self.position = anchorPoint.GetWorldPosition(parent);
            leftTip.position = leftPoint.GetWorldPosition(parent);
            rightTip.position = rightPoint.GetWorldPosition(parent);
        }


        private void SetBarLength(Transform bar, float length)
        {
            var scale = bar.localScale;
            bar.localScale = new Vector3(length, scale.y, scale.z);
        }
    }
}
