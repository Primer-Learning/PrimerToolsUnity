using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class MethodicalPlayableEvaluator : MonoBehaviour
    {
        private PlayableDirector playableDirector;

        private void Awake()
        {
            playableDirector = GetComponent<PlayableDirector>();
        }

        private void Start()
        {
            playableDirector.Pause();
            Time.captureFramerate = 60;
        }

        private void Update()
        {
            if (Time.time <= playableDirector.duration)
            { 
                EvaluatePlayable(Time.time);
            }
            else
            {
                UnityEditor.EditorApplication.ExitPlaymode();
            }
        }

        private void EvaluatePlayable(float time)
        {
            playableDirector.time = time;
            playableDirector.Evaluate();
        }
    }
}