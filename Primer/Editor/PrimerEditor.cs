using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Primer.Editor
{
    public class PrimerEditor<T> : OdinEditor where T : class
    {
        protected T component => target as T;

        protected float spacingPixels = 16;

        protected void Space() => GUILayout.Space(spacingPixels);

        protected void PropertyField(string propertyName)
            => Tree.GetPropertyAtPath(propertyName).Draw();
    }
}
