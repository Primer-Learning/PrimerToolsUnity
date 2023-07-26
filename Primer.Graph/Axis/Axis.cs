using System;
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
            return domainValue * scale;
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


        [Button(ButtonSizes.Large)]
        public void UpdateChildren()
        {
            // We don't want to run this on prefabs
            if (gameObject.IsPreset())
                return;

            var container = new Container(transform)
                .ScaleChildrenInPlayMode();

            if (enabled && isActiveAndEnabled) {
                UpdateRod(container);
                UpdateLabel(container);
                UpdateArrows(container);
                UpdateTicks(container);
            }

            container.Purge(defer: true);
        }
    }
}
