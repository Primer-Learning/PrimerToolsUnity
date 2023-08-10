using System;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Primer.Timeline
{
    [Serializable]
    internal abstract class ViewInPlayModeBehaviour
    {
        public abstract void Execute();

        public static PlayableDirector FindDirector()
        {
            return Object.FindObjectOfType<PlayableDirector>();
        }
    }
}
