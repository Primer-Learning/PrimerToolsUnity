using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Axis
{
    [ExecuteAlways]
    public class AxisRenderer : MonoBehaviour
    {
        [Header("Prefabs")]
        public Transform arrowPrefab;
        public AxisTick tickPrefab;

        [Header("Domain")]
        public float min;
        public float max = 10;
        [Min(0.1f)] public float length = 1;

        [Header("Rod")]
        public Transform rod;
        public float thickness = 1;

        [Header("Label")]
        public string label = "Label";
        public Vector3 labelOffset = Vector3.zero;
        public AxisLabelPosition labelPosition = AxisLabelPosition.End;

        [Header("Arrows")]
        public ArrowPresence arrowPresence = ArrowPresence.Both;
        public float paddingFraction = 0.05f;

        [Header("Tics")]
        public bool showTics = true;
        [Min(0)] public float ticStep = 2;
        [Range(1, 100)] public int maxTics = 50;
        public float ticLabelDistance = 0.25f;
        public List<TicData> manualTics = new();


        internal float positionMultiplier => length * (1 - 2 * paddingFraction) / (max - min);
        internal float offset => -length * paddingFraction + min * positionMultiplier;


        public float DomainToPosition(float domainValue) => domainValue * positionMultiplier;


        private void OnEnable() => UpdateChildren();

        private void OnValidate() => UpdateChildren();


        #region Children management
        private AxisLabel labelController;
        private AxisArrows arrowsController;
        private AxisTicks ticksController;

        public void UpdateChildren()
        {
            if (gameObject.IsPreset())
                return;

            labelController ??= new AxisLabel(this);
            arrowsController ??= new AxisArrows(this);
            ticksController ??= new AxisTicks(this);

            var children = new ChildrenModifier(transform) {
                onCreate = x => x.GetPrimer().ScaleUpFromZero().Forget(),
                onRemove = x => x.GetPrimer().ShrinkAndDispose(),
            };

            // Rod is not a generated object
            children.NextMustBe(rod);

            if (enabled && gameObject.activeSelf) {
                rod.localPosition = new Vector3(offset, 0f, 0f);
                rod.localScale = new Vector3(length, thickness, thickness);

                labelController.Update(children);
                arrowsController.Update(children);
                ticksController.Update(children);
            }

            children.Apply();
        }
        #endregion
    }
}
