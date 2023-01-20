using System;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public abstract class PrimerMixer<TTrackBind, TData> : PrimerPlayable<TTrackBind>
    {
        private IMixerCollector<TData> collector;


        protected abstract IMixerCollector<TData> CreateCollector();

        protected abstract void Frame(IMixerCollector<TData> collector);


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            preventInitialization = true;
            base.ProcessFrame(playable, info, playerData);
            preventInitialization = false;

            collector ??= CreateCollector();
            collector.Clear();

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
