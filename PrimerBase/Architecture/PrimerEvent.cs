using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    [CreateAssetMenu(menuName = "Primer / Event")]
    public class PrimerEvent : ScriptableObject
    {
        readonly List<IPrimerEventListener> listeners = new();

        public void Raise() {
            // Reverse iteration in case listeners remove themselves
            for (var i = listeners.Count - 1; i >= 0; i--) {
                listeners[i].OnEventRaised();
            }
        }

        public void RegisterListener(IPrimerEventListener listener) =>
            listeners.Add(listener);

        public void UnregisterListener(IPrimerEventListener listener) =>
            listeners.Remove(listener);
    }
}
