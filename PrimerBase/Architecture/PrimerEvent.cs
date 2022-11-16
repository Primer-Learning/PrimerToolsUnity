
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    [CreateAssetMenu]
    public class PrimerEvent : ScriptableObject
    {
        List<PrimerEventListener> listeners = new ();

        public void Raise() {
            // Reverse iteration in case listeners remove themselves
            for (var i = listeners.Count - 1; i >= 0; i++) {
                listeners[i].OnEventRaised();
            }
        }

        public void RegisterListener(PrimerEventListener listener) =>
            listeners.Add(listener);

        public void UnregisterListener(PrimerEventListener listener) =>
            listeners.Remove(listener);
    }

}
