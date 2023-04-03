using UnityEngine;

namespace Primer.Shapes
{
    public partial class PrimerArrow2
    {
        public void Recalculate()
        {
            if (shaftObject == null || headObject == null || tailObject == null || gameObject.IsPreset())
                return;

            startArrowLength = tailPointer ? realArrowLength : 0;
            endArrowLength = headPointer ? realArrowLength : 0;
            shaftLength = length - startArrowLength - endArrowLength;

            this.GetPrimer().FindIntrinsicScale();

            if (shaftLength <= 0)
            {
                transform.localScale = Vector3.zero;
                return;
            }

            CalculatePosition();
            CalculateChildrenPosition();
        }

        private void CalculatePosition()
        {
            var arrow = transform;
            var diff = head - tail;

            arrow.SetGlobalScale(Vector3.one);
            arrow.rotation = Quaternion.FromToRotation(Vector3.right, diff);
            arrow.position = diff / 2 + tail;
        }

        private void CalculateChildrenPosition()
        {
            var childRotation = Quaternion.Euler(axisRotation, 0, 0);
            var shaftMiddle = (tailSpace + startArrowLength) / 2 - (headSpace + endArrowLength) / 2;

            shaftObject.localPosition = new Vector3(shaftMiddle, 0, 0);
            shaftObject.localScale = new Vector3(shaftLength, thickness, thickness);
            shaftObject.localRotation = childRotation;

            var edge = (head - tail).magnitude / 2;

            tailObject.gameObject.SetActive(tailPointer);
            headObject.gameObject.SetActive(headPointer);

            if (tailPointer)
                CalculatePointer(tailObject, childRotation, -(edge - tailSpace - startArrowLength));

            if (headPointer)
                CalculatePointer(headObject, childRotation, edge - headSpace - endArrowLength);
        }

        private void CalculatePointer(Transform pointer, Quaternion childRotation, float x)
        {
            pointer.localScale = pointer.GetPrimer().FindIntrinsicScale() * thickness;
            pointer.localPosition = new Vector3(x, 0, 0);
            pointer.localRotation = childRotation;
        }
    }
}
