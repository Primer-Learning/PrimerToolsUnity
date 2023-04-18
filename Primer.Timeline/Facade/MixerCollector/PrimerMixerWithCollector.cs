using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public abstract class PrimerMixerWithCollector<TTrackBind, TData> : PrimerPlayable<TTrackBind>
        where TTrackBind : Component
    {
        private IMixerCollector<TData> collector;


        protected abstract IMixerCollector<TData> CreateCollector();

        protected abstract void Frame(IMixerCollector<TData> collector);


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            using (lifecycle.PreventInitialization()) {
                base.ProcessFrame(playable, info, playerData);
            }

            collector ??= CreateCollector();
            collector.Clear();

            if (collector is null)
                throw new Exception($"{GetType().FullName}.{nameof(CreateCollector)}() returns null!");

            collector.Collect(playable);

            if (collector.isEmpty) {
                lifecycle.Reset();
                trackTarget = default(TTrackBind);
                return;
            }

            lifecycle.Initialize();
            Frame(collector);
            collector.Clear();
        }
    }
}
