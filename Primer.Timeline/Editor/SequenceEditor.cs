using System.Threading.Tasks;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

namespace Primer.Timeline.Editor
{
    [CustomEditor(typeof(Sequence), editorForChildClasses: true)]
    public class SequenceEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Evaluate"))
                Reevaluate();
        }

        private async void Reevaluate()
        {
            var director = TimelineEditor.inspectedDirector;
            if (director == null) return;

            var player = SequenceOrchestrator.GetPlayerFor((Sequence)target);
            await player.Reset();
            director.Evaluate();
        }
    }
}
