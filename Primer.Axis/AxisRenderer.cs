using System;
using Primer.Timeline;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Axis
{
    [ExecuteAlways]
    public class AxisRenderer : MonoBehaviour
    {
        [SerializeField]
        public AxisDomain domain = new();

        [SerializeField]
        [HideInInlineEditors]
        internal AxisRod rod = new();

        [SerializeField]
        internal AxisLabel label = new();

        [SerializeField]
        [HideInInlineEditors]
        internal AxisArrows arrows = new();

        [SerializeField]
        [HideInInlineEditors]
        public AxisTicks ticks = new();


        public float min {
            get => domain.min;
            set => Meta.ReactiveProp(() => domain.min, value, UpdateChildren);
        }

        public float max {
            get => domain.max;
            set => Meta.ReactiveProp(() => domain.max, value, UpdateChildren);
        }


        public float DomainToPosition(float domainValue) => domainValue * domain.scale;


        internal bool ListenDomainChange(Action onDomainChange)
        {
            if (domain.onChange == onDomainChange)
                return false;

            domain.onChange = onDomainChange;
            return true;
        }

        public void OnValidate()
        {
            UpdateChildren();
        }


        [Button(ButtonSizes.Large)]
        public void UpdateChildren()
        {
            var container = new Container(transform).ScaleChildrenInPlayMode();

            // Rod is not a generated object so we keep it as child even if disabled
            container.Insert(rod.transform);

            if (enabled && isActiveAndEnabled) {
                rod.Update(domain);
                label.Update(container, domain, 0.25f);
                arrows.Update(container, domain);
                ticks.Update(container, domain);
            }

            container.Purge(defer: true);
        }
    }
}
