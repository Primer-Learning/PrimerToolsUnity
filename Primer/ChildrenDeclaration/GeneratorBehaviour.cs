using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Primer
{
    public abstract class GeneratorBehaviour : MonoBehaviour
    {
        private bool isUpdateCancelled;
        private ChildrenDeclaration declaration;


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
        [PropertySpace(SpaceBefore = 32)]
        [Button(ButtonSizes.Medium, Stretch = false, Icon = SdfIconType.ArrowRepeat)]
        [ContextMenu("PRIMER > Update children")]
        protected void UpdateChildren()
        {
            if (gameObject.IsPreset())
                return;

            isUpdateCancelled = false;
            declaration ??= CreateChildrenDeclaration();
            declaration.Reset();

            UpdateChildren(enabled && isActiveAndEnabled, declaration);

            if (!isUpdateCancelled)
                declaration.Apply();
        }
    }
}
