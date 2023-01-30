using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Primer
{
    public abstract class GeneratorBehaviour : MonoBehaviour
    {
        private bool isUpdateCancelled;
        private ChildrenDeclaration declaration;
        public readonly UnityEvent afterUpdate = new();


        protected abstract void UpdateChildren(bool isEnabled, ChildrenDeclaration children);

        protected virtual ChildrenDeclaration CreateChildrenDeclaration() => new(transform);


        protected void OnEnable() => UpdateChildren();

        protected void OnValidate()
        {
            if (!EditorApplication.isPlaying)
                UpdateChildren();
        }

        protected void CancelCurrentUpdate()
            => isUpdateCancelled = true;


        [PropertyOrder(100)]
        [ButtonGroup("Generator group")]
        [Button(ButtonSizes.Medium, Icon = SdfIconType.ArrowRepeat)]
        [ContextMenu("PRIMER > Update children")]
        protected void UpdateChildren()
        {
            if (this == null || gameObject.IsPreset())
                return;

            isUpdateCancelled = false;
            declaration ??= CreateChildrenDeclaration();
            declaration.Reset();

            UpdateChildren(enabled && isActiveAndEnabled, declaration);

            if (isUpdateCancelled)
                return;

            declaration.Apply();
            afterUpdate.Invoke();
        }

        [PropertyOrder(100)]
        [ButtonGroup("Generator group")]
        [Button(ButtonSizes.Medium, Icon = SdfIconType.Trash)]
        [ContextMenu("PRIMER > Regenerate children")]
        protected void RegenerateChildren()
        {
            if (gameObject.IsPreset())
                return;

            ChildrenDeclaration.Clear(transform);
            UpdateChildren();
        }
    }
}
