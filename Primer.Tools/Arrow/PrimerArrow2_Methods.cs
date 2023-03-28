using Primer.Animation;
using UnityEngine;

namespace Primer.Tools
{
    public partial class PrimerArrow2
    {
        #region Setters
        public PrimerArrow2 Follow(GameObject from, GameObject to) => Follow(from.transform, to.transform);
        public PrimerArrow2 Follow(Component from, Component to) => Follow(from.transform, to.transform);

        public PrimerArrow2 Follow(Transform from, Transform to)
        {
            tailPoint.follow = from;
            headPoint.follow = to;
            return this;
        }

        public void SetFromTo(Vector3 from, Vector3 to, bool global)
        {
            globalPositioning = global;
            SetFromTo(from, to);
        }

        public void SetFromTo(Vector3 from, Vector3 to)
        {
            tailPoint.vector = from;
            headPoint.vector = to;
        }

        private void SetLength(float value)
        {
            // If the length is too small, just prevent the change
            if (value < (tailPointer ? realArrowLength : 0) + (headPointer ? realArrowLength : 0))
                return;

            var diff = head - tail;
            headPoint.vector += (value - diff.magnitude) * Vector3.Normalize(diff);
        }
        #endregion


        #region Animations
        public Tween GrowFromStart(Vector3Provider headPos, Vector3Provider tailPos)
        {
            return Animate(
                tailStart: tailPos,
                headStart: tailPos,
                tailEnd: tailPos,
                headEnd: headPos
            );
        }

        public Tween ShrinkToEnd()
        {
            tailPoint.StopTracking();
            headPoint.StopTracking();
            return Animate(tailEnd: head);
        }

        public Tween Animate(
            Vector3Provider headEnd = null,
            Vector3Provider tailEnd = null,
            Vector3Provider headStart = null,
            Vector3Provider tailStart = null
        )
        {
            var tailTween = tailPoint.Tween(tailEnd, tailStart);
            var headTween = headPoint.Tween(headEnd, headStart);

            return new Tween(
                t => {
                    if (tailTween is not null)
                        tailPoint.vector = tailTween(t);

                    if (headTween is not null)
                        headPoint.vector = headTween(t);

                    Recalculate();
                }
            );
        }
        #endregion
    }
}
