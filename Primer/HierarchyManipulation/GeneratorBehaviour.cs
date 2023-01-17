using UnityEditor;
using UnityEngine;

namespace Primer
{
    public abstract class GeneratorBehaviour : MonoBehaviour
    {
        private ChildrenDeclaration declaration;


        protected abstract void UpdateChildren(bool isEnabled, ChildrenDeclaration declaration);

        protected virtual ChildrenDeclaration CreateChildrenDeclaration() => new(transform);


        protected void OnEnable() => UpdateChildren();

        protected void OnValidate()
        {
            if (!EditorApplication.isPlaying)
                UpdateChildren();
        }


        protected void UpdateChildren()
        {
            if (gameObject.IsPreset())
                return;

            declaration ??= CreateChildrenDeclaration();
            declaration.Reset();

            UpdateChildren(enabled && isActiveAndEnabled, declaration);

            declaration.Apply();
        }
    }
}
