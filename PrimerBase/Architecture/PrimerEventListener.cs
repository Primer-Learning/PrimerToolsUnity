using UnityEngine;
using UnityEngine.Events;

namespace Primer
{
    public class PrimerEventListener : MonoBehaviour
    {
        public PrimerEvent _event;
        public UnityEvent response;

        void OnEnable() => _event.RegisterListener(this);
        void OnDisable() => _event.UnregisterListener(this);

        public void OnEventRaised() => response.Invoke();
    }
}
