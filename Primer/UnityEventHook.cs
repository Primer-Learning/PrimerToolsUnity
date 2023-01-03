using System;
using UnityEngine;

namespace Primer
{
    public static class UnityEventHook
    {
        private const string GAME_OBJECT_TAG = "[Unity event hook]";
        public static event Action OnUpdate {
            add {
                EnsureMonoBehaviourIsInScene();
                UpdateEvent += value;
            }
            remove {
                UpdateEvent -= value;
                RemoveMonoBehaviourIfNoListeners();
            }
        }

        public static event Action UpdateEvent;

        [ExecuteAlways]
        private class EventCatcher : MonoBehaviour
        {
            private void Update() => UpdateEvent?.Invoke();
        }


        #region GameObject with Component creation / removal
        static UnityEventHook()
        {
            // Create tag to identify them later
            UnityTagManager.CreateTag(GAME_OBJECT_TAG);

            foreach (var go in GameObject.FindGameObjectsWithTag(GAME_OBJECT_TAG)) {
                go.Dispose();
            }
        }

        private static EventCatcher catcher;

        private static void EnsureMonoBehaviourIsInScene()
        {
            if (catcher != null)
                return;

            var gameObject = new GameObject(GAME_OBJECT_TAG) {
                tag = GAME_OBJECT_TAG,
                hideFlags = HideFlags.DontSave,
            };

            catcher = gameObject.AddComponent<EventCatcher>();
        }

        private static void RemoveMonoBehaviourIfNoListeners()
        {
            if (UpdateEvent != null)
                return;

            catcher.Dispose();
            catcher = null;
        }
        #endregion
    }
}
