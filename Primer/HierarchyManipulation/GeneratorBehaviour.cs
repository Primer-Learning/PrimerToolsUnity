using UnityEditor;
using UnityEngine;

namespace Primer
{
    public abstract class GeneratorBehaviour : MonoBehaviour
    {
        private bool isUpdateCancelled;
        private ChildrenDeclaration declaration;


        protected abstract void UpdateChildren(bool isEnabled, ChildrenDeclaration declaration);

        protected virtual ChildrenDeclaration CreateChildrenDeclaration() => new(transform);


        protected void OnEnable() => UpdateChildren();

        protected void OnValidate()
        {
            if (!EditorApplication.isPlaying)
                UpdateChildren();
        }


        protected void CancelCurrentUpdate()
            => isUpdateCancelled = true;

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
