using System;
using Primer.Animation;
using Primer.Timeline;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    [ExecuteAlways]
    public partial class Axis : MonoBehaviour
    {
        private Action onDomainChange;

        private void DomainChanged()
        {
            onDomainChange?.Invoke();
            UpdateChildren();
        }

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


        public Tween GrowFromOrigin(float newMax) => GrowFromOrigin(min, newMax);
        public Tween GrowFromOrigin(float newMin, float newMax)
        {
            min = 0;
            max = 0;

            this.SetActive(true);

            return Tween.Parallel(
                delayBetweenStarts: 0.1f,
                this.ScaleTo(1, initialScale: 0),
                Tween.Parallel(
                    Tween.Value(() => min, from: 0, to: newMin),
                    Tween.Value(() => max, from: 0, to: newMax)
                )
            );
        }

        public Tween ShrinkToOrigin()
        {
            return Tween.Parallel(
                delayBetweenStarts: 0.1f,
                Tween.Parallel(
                    Tween.Value(() => min, from: min, to: 0),
                    Tween.Value(() => max, from: max, to: 0)
                ),
                this.ScaleTo(0, initialScale: 1)
            );
        }


        [Button(ButtonSizes.Large)]
        public void UpdateChildren()
        {
            // We don't want to run this on prefabs
            if (gameObject.IsPreset())
                return;

            var gnome = new Gnome(transform)
                .ScaleChildrenInPlayMode();

            if (enabled && isActiveAndEnabled) {
                UpdateRod(gnome);
                UpdateLabel(gnome);
                UpdateArrows(gnome);
                UpdateTicks(gnome);
            }

            gnome.Purge(defer: true);
        }
    }
}
