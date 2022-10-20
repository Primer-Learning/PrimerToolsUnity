using UnityEngine;
public static class MonoBehaviourExtensions
{
    public static void Dispose(this MonoBehaviour behaviour) {
        behaviour.gameObject.Dispose();
    }
}
