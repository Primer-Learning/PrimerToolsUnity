using UnityEngine;
using UnityEngine.Events;

namespace Primer
{
    public class PrimerEventListener : MonoBehaviour, IPrimerEventListener
    {
        public PrimerEvent @event;
        public UnityEvent response;

        void OnEnable() => @event.RegisterListener(this);
        void OnDisable() => @event.UnregisterListener(this);

        public void OnEventRaised() => response.Invoke();
    }
}
