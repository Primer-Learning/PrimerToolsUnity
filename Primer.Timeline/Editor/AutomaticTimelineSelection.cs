using System.Threading.Tasks;
using UnityEditor;
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
            var timeline = GameObject.Find("Timeline");
            var director = timeline?.GetComponent<PlayableDirector>();

            if (director is null)
                return;

            Lock(director);
        }

        private static async void Lock(PlayableDirector director)
        {
            for (var i = 0; i < 100; i++) {
                var window = TimelineEditor.GetWindow();

                if (window is null) {
                    await Task.Delay(250);
                    continue;
                }

                if (!window.locked) {
                    window.SetTimeline(director);
                    window.locked = true;
                }

                break;
            }
        }
    }
}
