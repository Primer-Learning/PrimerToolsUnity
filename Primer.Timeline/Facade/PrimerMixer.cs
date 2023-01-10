using System;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public abstract class PrimerMixer<TTrackBind, TData> : PrimerPlayable
    {
        private IMixerCollector<TData> collector;
        protected TTrackBind trackTarget { get; private set; }


        protected abstract IMixerCollector<TData> CreateCollector();

        protected abstract void Frame(IMixerCollector<TData> collector);


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);

            if (playerData is null)
                return;

            if (playerData is not TTrackBind boundObject) {
                throw new Exception(
                    $"Expected track target to be {typeof(TTrackBind).Name} but {playerData?.GetType().Name} found"
                );
            }

            trackTarget = boundObject;
            collector ??= CreateCollector();

            if (collector is null)
                throw new Exception($"{GetType().FullName}.{nameof(CreateCollector)}() returns null!");

            collector.Collect(playable);

            if (collector.isEmpty) {
                RunStop();
                trackTarget = default(TTrackBind);
                return;
            }

            RunStart();

            Frame(collector);

            collector.Clear();
        }
    }
}
