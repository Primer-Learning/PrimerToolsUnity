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

        internal bool ListenDomainChange(Action listener)
        {
            if (onDomainChange == listener)
                return false;

            onDomainChange = listener;
            return true;
        }


        public void OnEnable() => UpdateChildren();
        public void OnValidate() => UpdateChildren();


        public Tween Transition()
        {
            // We don't want to run this on prefabs
            if (gameObject.IsPreset())
                return Tween.noop;

            var gnome = Gnome.For(this);
            var updateParts = Tween.Parallel(
                TransitionRod(gnome),
                TransitionLabel(gnome),
                TransitionArrows(gnome)
            );

            var addParts = gnome.GetCreatedChildren()
                .Select(x => x.ScaleUpFromZero())
                .RunInParallel();

            var (addTicks, updateTicks, removeTicks) = TransitionTicks(gnome);

            var removeParts = gnome.ManualPurge(defer: true)
                .Select(x => x.ScaleDownToZero().Observe(onDispose: x.Dispose))
                .RunInParallel();

            var update = Tween.Parallel(
                updateTicks,
                removeParts,
                updateParts,
                addParts
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
                delayBetweenStarts: 0.25f,
                // This may be null which will immediately start the tween below
                removeTicks,
                update,
                addTicks
            );
        }


        [Button(ButtonSizes.Large)]
        public void UpdateChildren()
        {
            using var tween = Transition();
            tween?.Apply();
        }
    }
}
