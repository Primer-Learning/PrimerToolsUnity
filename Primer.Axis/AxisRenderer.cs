using System;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Axis
{
    [ExecuteAlways]
    public class AxisRenderer : MonoBehaviour
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


        public float DomainToPosition(float domainValue) => domainValue * domain.positionMultiplier;

        private void OnEnable() => UpdateChildren();

        internal void OnValidate() => UpdateChildren();


        internal bool ListenDomainChange(Action onDomainChange)
        {
            if (domain.onChange == onDomainChange)
                return false;

            domain.onChange = onDomainChange;
            return true;
        }

        public void UpdateChildren()
        {
            if (gameObject.IsPreset())
                return;

            var children = new ChildrenDeclaration(
                transform,
                onCreate: x => x.GetPrimer().ScaleUpFromZero().Forget(),
                onRemove: x => x.GetPrimer().ShrinkAndDispose()
            );

            // Rod is not a generated object so we keep it as child even if disabled
            rod.AddTo(children);

            if (enabled && isActiveAndEnabled) {
                rod.Update(domain);
                label.Update(children, domain, 0.25f);
                arrows.Update(children, domain);
                ticks.Update(children, domain);
            }

            children.Apply();
        }
    }
}
