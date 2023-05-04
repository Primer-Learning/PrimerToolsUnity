using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace Primer.Timeline.Editor
{
    [InitializeOnLoad]
    public class AutomaticTimelineSelection
    {
        static AutomaticTimelineSelection()
        {
            EditorSceneManager.sceneOpened += (_, _) => StartProcess();
            StartProcess();
        }

        private static async void StartProcess()
        {
            await Task.Delay(100);

            for (var i = 0; i < 100; i++) {
                var window = TimelineEditor.GetWindow();

                if (window is null) {
                    await Task.Delay(100);
                    continue;
                }

                if (window.locked)
                    EnsureTimelineIsUpdated();
                else
                    SelectFirstDirector(window);

                return;
            }
        }

        private static async void EnsureTimelineIsUpdated()
        {
            var director = TimelineEditor.inspectedDirector;

            if (director != null && director.time is not 0)
                return;

            // Unity destroys the timeline before 2s for some reason
            await Task.Delay(2000);
            TimelineEditor.Refresh(RefreshReason.SceneNeedsUpdate);
        }

        private static void SelectFirstDirector(TimelineEditorWindow window)
        {
            var director = Object.FindObjectOfType<PlayableDirector>();
            if (director == null) return;

            window.SetTimeline(director);
            window.locked = true;
        }
    }
}
