using System;
using System.Linq;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    [ExecuteAlways]
    public partial class Axis : MonoBehaviour
    {
        private bool hasDomainChanged = false;
        private Action onDomainChange;
        private float? lastReportedScale = null;

        public float DomainToPosition(float domainValue)
        {
            return isActiveAndEnabled
                ? domainValue * (lastReportedScale ?? scale)
                : 0;
        }

        public void OnEnable() => UpdateChildren();
        public void OnValidate() => UpdateChildren(defer: true);


        public Tween Transition(bool defer = false)
        {
            // We don't want to run this on prefabs
            if (gameObject.IsPreset())
                return Tween.noop;

            var gnome = new Primer.SimpleGnome(transform);
            var updateParts = Tween.Parallel(
                TransitionRod(gnome),
                TransitionLabel(gnome),
                TransitionArrows(gnome)
            );
            
            // var addParts = gnome.newChildren
            //     .Select(x => x.ScaleUpFromZero())
            //     .RunInParallel();

            var (addTicks, updateTicks, removeTicks) = TransitionTicks(gnome, defer);

            // var removeParts = gnome.oldChildren
            //     .Select(x => x.ScaleDownToZero().Observe(onDispose: x.Dispose))
            //     .RunInParallel();

            var update = Tween.Parallel(
                updateTicks,
                // removeParts,
                updateParts
                // addParts
            );

            if (hasDomainChanged) {
                hasDomainChanged = false;
                lastReportedScale ??= scale;
                var prevScale = lastReportedScale.Value;

                update = update.Observe(
                    afterUpdate: t => {
                        lastReportedScale = Mathf.Lerp(prevScale, scale, t);
                        onDomainChange?.Invoke();
                    }
                );
            }

            return Tween.Parallel(
                delayBetweenStarts: 0.1f,
                // This may be null which will immediately start the tween below
                removeTicks,
                update,
                addTicks
            )
            .Observe(
                afterUpdate: EnsureOnlyOneTransitionAtATime()
            );
        }

        private int lastTransitionId = 0;
        private Action<float> EnsureOnlyOneTransitionAtATime()
        {
            lastTransitionId++;
            var currentTransition = lastTransitionId;

            return t => {
                if (currentTransition == lastTransitionId)
                    return;

                throw new Exception(
                    @"Two axis.Transition() running at the same time.
there are two axis.Transition() trying to modify axis {name} as the same time
this can happen if you combine two graph.Something() operations in a single tween
you won't like what will happen if we continue this path

If you need to modify multiple properties of a axis (or graph) change them directly
and call to .Transition() only once after that

    graph.min = 0;
    graph.max = 10;
    graph.scale = 1;
    graph.Transition();

instead of

    // BAD! 👎
    Tween.Parallel(
      graph.SetDomain(10, 0)
      graph.SetScale(1)
    );

END OF PRIMER MESSAGES
"
                );
            };
        }

        [Button(ButtonSizes.Large)]
        public void UpdateChildren() => UpdateChildren(defer: false);

        public void UpdateChildren(bool defer)
        {
            using var tween = Transition(defer);
            tween?.Apply();
        }
    }
}
