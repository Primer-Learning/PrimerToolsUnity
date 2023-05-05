using Primer.Animation;
using UnityEngine;

namespace Primer.Shapes
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
        public Tween GrowFromStart()
        {
            var requiresAdjustmentTweening = tailPoint.adjustment != headPoint.adjustment;
            var originalHeadPosition = (Vector3Provider)headPoint;
            var originalHeadAdjustment = headPoint.adjustment;

            tailPoint.CopyTo(headPoint);

            var growTween = Animate(headEnd: originalHeadPosition)
                .Observe(onComplete: () => originalHeadPosition.ApplyTo(headPoint));

            return requiresAdjustmentTweening
                ? Tween.Parallel(growTween, Tween.Value(() => headPoint.adjustment, originalHeadAdjustment))
                    .Observe(afterUpdate: t => {
                            if (t is 0 or 1)
                                return;

                            PrimerLogger.Log(
                                $"Arrow [{name}]:",
                                t,
                                headPoint.vector,
                                headPoint.adjustment,
                                originalHeadAdjustment
                            );
                        }
                    )
                : growTween;
        }

        public Tween ShrinkToEnd()
        {
            var shrinkTween = Animate(tailEnd: head);

            return tailPoint.adjustment == headPoint.adjustment
                ? shrinkTween
                : Tween.Parallel( shrinkTween, Tween.Value(() => tailPoint.adjustment, headPoint.adjustment));
        }

        public Tween Animate(
            Vector3Provider headEnd = null,
            Vector3Provider tailEnd = null,
            Vector3Provider headStart = null,
            Vector3Provider tailStart = null)
        {
            var tailTracking = tailPoint.isTracking ? (Vector3Provider)tailPoint : null;
            var headTracking = headPoint.isTracking ? (Vector3Provider)headPoint : null;
            var tailTween = tailPoint.Tween(tailEnd, tailStart);
            var headTween = headPoint.Tween(headEnd, headStart);

            var tween = new Tween(t => {
                tailPoint.vector = tailTween(t);
                headPoint.vector = headTween(t);
                Recalculate();
            });

            if (tailTracking is null && headTracking is null)
                return tween;

            return tween.Observe(
                onComplete: () => {
                    tailTracking?.ApplyTo(tailPoint);
                    headTracking?.ApplyTo(headPoint);
                }
            );
        }
        #endregion
    }
}
