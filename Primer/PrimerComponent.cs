using UnityEngine;

namespace Primer
{
    public class PrimerComponent : MonoBehaviour
    {
        private Container containerCache;
        public Container container => containerCache ??= new Container(transform);
    }
}
