using System;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Axis
{
    [ExecuteAlways]
    public class AxisRenderer : GeneratorBehaviour
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


        internal new void OnValidate() => base.OnValidate();


        internal bool ListenDomainChange(Action onDomainChange)
        {
            if (domain.onChange == onDomainChange)
                return false;

            domain.onChange = onDomainChange;
            return true;
        }


        protected override ChildrenDeclaration CreateChildrenDeclaration() => new(
            transform,
            onCreate: x => x.GetPrimer().ScaleUpFromZero().PlayAndForget(),
            onRemove: x => x.GetPrimer().ShrinkAndDispose().Forget()
        );

        protected override void UpdateChildren(bool isEnabled, ChildrenDeclaration children)
        {
            // Rod is not a generated object so we keep it as child even if disabled
            rod.AddTo(children);

            if (!isEnabled)
                return;

            rod.Update(domain);
            label.Update(children, domain, 0.25f);
            arrows.Update(children, domain);
            ticks.Update(children, domain);
        }

        [PropertyOrder(100)]
        [ButtonGroup("Generator group")]
        [Button(ButtonSizes.Medium, Icon = SdfIconType.Trash)]
        [ContextMenu("PRIMER > Regenerate children")]
        protected override void RegenerateChildren()
        {
            if (gameObject.IsPreset())
                return;

            ChildrenDeclaration.Clear(transform, skip: new []{ rod.transform });
            UpdateChildren();
        }
    }
}
