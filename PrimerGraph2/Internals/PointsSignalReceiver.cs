using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(PointData2))]
public class PointsSignalReceiver : SignalReceiver
{
    public Vector3[] points;
    // public SignalAssetEventPair[] signalAssetEventPairs;
    int lastPushed = -1;
    //
    // [Serializable]
    // public class SignalAssetEventPair
    // {
        public SignalAsset signalAsset;
    //     public ParameterizedEvent events;
    //
    //     [Serializable]
    //     public class ParameterizedEvent : UnityEvent<bool> { }
    // }

    // void Awake() {
    //     this.
    // }

    // public new int Count()
    // {
    //     return 1;
    // }
    //
    // public new IEnumerable<SignalAsset> GetRegisteredSignals()
    // {
    //     return new SignalAsset[] { signalAsset };
    // }
    //
    // public new UnityEvent GetReaction(SignalAsset key)
    // {
    //     UnityEvent potato = new UnityEvent();
    //
    //     potato.AddListener(AddNext);
    //
    //     return potato;
    // }

    public void AddNext() {
        Debug.Log("POTATO'");
        lastPushed++;

        if (lastPushed < points.Length) {
            GetComponent<PointData2>().AddPoint(points[lastPushed]);
        }
    }

    // public new void OnNotify(Playable origin, INotification notification, object context)
    // {
    //     Debug.Log("CAPTURED");
    //     // if(notification is bool boolEmitter)
    //     // {
    //     //     var matches = signalAssetEventPairs.Where(x => ReferenceEquals(x.signalAsset, boolEmitter.asset));
    //     //     foreach (var m in matches)
    //     //     {
    //     //         m.events.Invoke(boolEmitter.parameter);
    //     //     }
    //     // }
    // }
}
