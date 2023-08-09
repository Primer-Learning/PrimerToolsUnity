using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    public abstract class PrimerClip : PlayableAsset, ITimelineClipAsset
    {
        public float? expectedDuration = null;

        protected abstract PrimerPlayable template { get; }
        protected PrimerPlayable lastPlayable;

        public ClipCaps clipCaps => ClipCaps.Extrapolation;

        public virtual string clipName => "";


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<PrimerPlayable>.Create(graph, template);

            lastPlayable = playable.GetBehaviour();
            lastPlayable.onDurationReported = x => expectedDuration = x;

            return playable;
        }


        #region Properties propagated to PrimerPlayable
        public Transform trackTransform {
            get => template.trackTransform;
            set {
                template.trackTransform = value;

                if (lastPlayable is not null)
                    lastPlayable.trackTransform = value;
            }
        }

        public IExposedPropertyTable resolver {
            get => template.resolver;
            set {
                template.resolver = value;

                if (lastPlayable is not null)
                    lastPlayable.resolver = value;
            }
        }

        public int clipIndex {
            get => template.clipIndex;
            set {
                template.clipIndex = value;

                if (lastPlayable is not null)
                    lastPlayable.clipIndex = value;
            }
        }

        public float start {
            get => lastPlayable?.start ?? template.start;
            set {
                template.start = value;

                if (lastPlayable is not null)
                    lastPlayable.start = value;
            }
        }

        public new float duration {
            get => template.duration;
            set {
                template.duration = value;

                if (lastPlayable is not null)
                    lastPlayable.duration = value;
            }
        }
        #endregion
    }
}
