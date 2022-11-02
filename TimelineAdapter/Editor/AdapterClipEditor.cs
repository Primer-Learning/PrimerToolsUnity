using UnityEditor;

namespace Primer.Timeline.Editor
{
    [CustomEditor(typeof(AdapterClip))]
    internal class AdapterClipEditor : PrimerEditor<AdapterClip>
    {
        public override void OnInspectorGUI() {
            serializedObject.Update();

            DerivedTypeSelectorWithProps<ScrubbableAdapter>(nameof(AdapterClip.adapter));

            if (serializedObject.ApplyModifiedProperties()) {
                // Clear the errors and allow the clip to try and play again. Where best to do this
                // was found mostly via trial-and-error, so this may not be an ideal spot either.
                var adapterClip = (AdapterClip)target;
                adapterClip.adapter?.errors.Clear();
            }
        }
    }
}
