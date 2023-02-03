using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    public abstract class PrimerTrack : TrackAsset
    {
        // public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        // {
        //     base.GatherProperties(director, driver);
        //
        //     foreach (var clip in GetClips()) {
        //         if (clip.asset is not PrimerClip<PrimerPlayable> asset)
        //             continue;
        //
        //         var behaviour = asset.template;
        //
        //         if (behaviour is not IPropertyModifier modifier)
        //             continue;
        //
        //         try {
        //             modifier.RegisterProperties(driver);
        //         }
        //         catch (Exception err) {
        //             // Prevents it from playing later, which helps ensure we don't let changes
        //             // to the scene get saved accidentally.
        //             behaviour.isFailed = true;
        //             Debug.LogException(err);
        //         }
        //     }
        // }
    }
}
