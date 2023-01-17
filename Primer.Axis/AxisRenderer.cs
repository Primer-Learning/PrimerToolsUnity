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
        internal AxisDomain domain = new();

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
        internal AxisTicks ticks = new();


        public float DomainToPosition(float domainValue) => domainValue * domain.scale;


        new internal void OnValidate() => base.OnValidate();


        internal bool ListenDomainChange(Action onDomainChange)
        {
            if (domain.onChange == onDomainChange)
                return false;

            domain.onChange = onDomainChange;
            return true;
        }


        protected override ChildrenDeclaration CreateChildrenDeclaration() => new(
            transform,
            onCreate: x => x.GetPrimer().ScaleUpFromZero().Forget(),
            onRemove: x => x.GetPrimer().ShrinkAndDispose()
        );

        protected override void UpdateChildren(bool isEnabled, ChildrenDeclaration declaration)
        {
            // Rod is not a generated object so we keep it as child even if disabled
            rod.AddTo(declaration);

            if (!isEnabled)
                return;

            rod.Update(domain);
            label.Update(declaration, domain, 0.25f);
            arrows.Update(declaration, domain);
            ticks.Update(declaration, domain);
        }
    }
}
