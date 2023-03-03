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
        private Transform rightBar;
        [SerializeField, PrefabChild]
        private Transform rightTip;

        [Title("Anchor")]
        public ScenePoint anchorPoint = Vector3.zero;

        [Title("Left")]
        public ScenePoint leftPoint = new Vector3(-1, 0, 1);

        [Title("Right")]
        public ScenePoint rightPoint = new Vector3(1, 0, 1);

        private void OnEnable()
        {
            // TODO: Scheduler doesn't work
            // var scheduler = new Scheduler(Refresh);
            // anchorPoint.onChange = scheduler.Schedule;
            // leftPoint.onChange = scheduler.Schedule;
            // rightPoint.onChange = scheduler.Schedule;
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

            var leftTipSize = leftBar.localPosition.x;
            var rightTipSize = rightBar.localPosition.x;
            var leftLength = (toLeft - center).magnitude - Mathf.Abs(leftTipSize * 2);
            var rightLength = (toRight - center).magnitude - Mathf.Abs(rightTipSize * 2);

            if (leftLength < 0.01f || rightLength < 0.01f || Mathf.Abs(leftLength + rightLength) > mouth.magnitude) {
                Debug.LogWarning("Refusing to render a broken-looking bracket");
                return;
            }

            leftBar.localScale = new Vector3(leftLength, 1, 1);
            rightBar.localScale = new Vector3(rightLength, 1, 1);

            self.rotation = Quaternion.LookRotation(forward,  upwards);
            self.localScale = new Vector3(1, 1, center.magnitude);

            self.position = anchorPoint.GetWorldPosition(parent);
            leftTip.position = leftPoint.GetWorldPosition(parent);
            rightTip.position = rightPoint.GetWorldPosition(parent);
        }
    }
}
