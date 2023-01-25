using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Primer.Timeline.Editor
{
    [CustomEditor(typeof(GenericClip))]
    public class GenericClipEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var behaviour = (target as GenericClip)?.template;
            var animation = behaviour?.animation;

            if (animation is null)
                return;

            behaviour.method = MethodSelector.Render(
                behaviour.method,
                MethodSelector.GetMethodsWithParamsOfType(animation, typeof(float)),
                hideIfEmpty: true
            );
        }
    }
}
