using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Debug = UnityEngine.Debug;

namespace Primer.Timeline
{
    public abstract class PrimerTrack : TrackAsset
    {
        public virtual float defaultDuration { get; } = 1;

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            if (clip.asset is PrimerClip asset)
                asset.resolver ??= graph.GetResolver();

            return base.CreatePlayable(graph, gameObject, clip);
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var trackTarget = graph.GetResolver() is PlayableDirector director
                ? director.GetGenericBinding(this) as Component
                : null;

            if (trackTarget == null)
                LogNoTrackTargetWarning();

            var clipCount = 0;

            foreach (var clip in GetClips()) {
                if (clip.asset is not PrimerClip asset)
                    continue;

                asset.resolver ??= graph.GetResolver();
                asset.trackTransform = trackTarget?.transform;
                asset.start = (float)clip.start;
                asset.duration = (float)clip.duration;
                asset.clipIndex = clipCount++;

                var canReplaceClipName = string.IsNullOrWhiteSpace(clip.displayName)
                    || clip.displayName == nameof(SequenceClip)
                    || clip.displayName[0] != '[';

                if (canReplaceClipName && !string.IsNullOrWhiteSpace(asset.clipName))
                    clip.displayName = asset.clipName;
            }

            var mixer = (ScriptPlayable<PrimerMixer>)CreateMixer(graph, inputCount);
            mixer.GetBehaviour().isMuted = muted;
            return mixer;
        }

        protected virtual Playable CreateMixer(PlayableGraph graph, int inputCount)
        {
            return ScriptPlayable<PrimerMixer>.Create(graph, inputCount);
        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);

            clip.duration = defaultDuration;

            // clip.postExtrapolationMode = TimelineClip.ClipExtrapolation.Hold;
            //
            // HACK: Property's setter above is internal (why? ask Unity), so we use reflection to set it.
            const BindingFlags privateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
            clip.GetType()
                .GetMethod("set_postExtrapolationMode", privateInstance)
                ?.Invoke(clip, new object[] { TimelineClip.ClipExtrapolation.Hold });
        }

        internal static void LogNoTrackTargetWarning()
        {
            Debug.LogWarning($"{nameof(PrimerTrack)} has no target");
        }
    }
}
