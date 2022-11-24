using System;
using UnityEngine;

namespace Primer
{

    public static class UnityEventHook
    {
        const string GAME_OBJECT_TAG = "[Unity event hook]";

        [ExecuteAlways]
        class EventCatcher : MonoBehaviour
        {
            void Update() => UpdateEvent?.Invoke();
        }

        public static event Action UpdateEvent;
        public static event Action OnUpdate
        {
            add {
                EnsureMonoBehaviourIsInScene();
                UpdateEvent += value;
            }
            remove {
                UpdateEvent -= value;
                RemoveMonoBehaviourIfNoListeners();
            }
        }

        #region GameObject with Component creation / removal

        static UnityEventHook() {
            // Create tag to identify them later
            UnityTagManager.CreateTag(GAME_OBJECT_TAG);

            foreach (var go in GameObject.FindGameObjectsWithTag(GAME_OBJECT_TAG)) {
                go.Dispose();
            }
        }

        static EventCatcher catcher;

        static void EnsureMonoBehaviourIsInScene() {
            if (catcher != null) return;

            var gameObject = new GameObject(GAME_OBJECT_TAG);

            gameObject.tag = GAME_OBJECT_TAG;
            gameObject.hideFlags = HideFlags.DontSave;

            catcher = gameObject.AddComponent<EventCatcher>();
        }

        static void RemoveMonoBehaviourIfNoListeners() {
            if (UpdateEvent != null) return;
            catcher.gameObject.Dispose();
        }
        #endregion
    }
}
