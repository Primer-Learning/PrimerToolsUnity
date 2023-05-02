using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline.Editor
{
    [InitializeOnLoad]
    public class AutomaticTimelineSelection
    {
        static AutomaticTimelineSelection()
        {
            EditorSceneManager.sceneOpened += async (_, _) => {
                await Task.Delay(100);
                SelectTimeline();
            };

            SelectTimeline();
        }

        private static async void SelectTimeline()
        {
            for (var i = 0; i < 100; i++) {
                var window = TimelineEditor.GetWindow();

                if (window is null) {
                    await Task.Delay(100);
                    continue;
                }

                if (window.locked)
                    return;

                var director = GameObject.FindObjectOfType<PlayableDirector>();
                if (director == null) return;

                window.SetTimeline(director);
                window.locked = true;
                return;
            }
        }
    }
}
