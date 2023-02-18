using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;
using UnityEngine.Timeline;

namespace Primer.Timeline.Editor
{
    public abstract class ManipulateTime : TimelineAction
    {
        protected static float inspectedTime => (float)TimelineEditor.inspectedDirector.time;
        protected static IEnumerable<TrackAsset> allTracks => TimelineEditor.inspectedDirector.GetAllTracks();

        public override ActionValidity Validate(ActionContext context) => ActionValidity.Valid;
    }
}

