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

        public float DomainToPosition(float domainValue)
        {
            return isActiveAndEnabled
                ? domainValue * scale
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


        public Tween GrowFromOrigin() => GrowFromOrigin(min, max);
        public Tween GrowFromOrigin(float newMax) => GrowFromOrigin(min, newMax);
        public Tween GrowFromOrigin(float newMin, float newMax)
        {
            range = new MinMax { min = 0, max = 0 };
            UpdateChildren();

            range = new MinMax { min = newMin, max = newMax };
            this.SetActive(true);

            return Tween.Parallel(
                delayBetweenStarts: 0.1f,
                this.ScaleTo(1, initialScale: 0),
                Transition()
            );
        }

        public Tween ShrinkToOrigin()
        {
            range = new MinMax { min = 0, max = 0 };

            return Tween.Parallel(
                delayBetweenStarts: 0.1f,
                Transition(),
                this.ScaleTo(0, initialScale: 1)
            );
        }

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
                update = update.Observe(afterUpdate: t => onDomainChange?.Invoke());
                hasDomainChanged = false;
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
