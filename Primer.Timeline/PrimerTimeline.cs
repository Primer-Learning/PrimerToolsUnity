using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Primer.Timeline
{
    public static class PrimerTimeline
    {
        private const string COLLECTABLE_OBJECT_TAG = "EphemeralTimelineObject";
        private static bool haveEphemeralObjectsBeenCollected = false;

        static PrimerTimeline()
        {
            UnityTagManager.CreateTag(COLLECTABLE_OBJECT_TAG);
        }

        public static void MarkAsEphemeral(Component component) => MarkAsEphemeral(component.gameObject);
        public static void MarkAsEphemeral(GameObject gameObject)
        {
            haveEphemeralObjectsBeenCollected = false;
            gameObject.tag = COLLECTABLE_OBJECT_TAG;
        }

        public static void DisposeEphemeralObjects()
        {
            if (haveEphemeralObjectsBeenCollected)
                return;

            haveEphemeralObjectsBeenCollected = true;
            CollectAndDisposeEphemerals();
        }

        private static void CollectAndDisposeEphemerals()
        {
            IterateAllObjects(transform => {
                if (!transform.gameObject.CompareTag(COLLECTABLE_OBJECT_TAG))
                    return false;

                Debug.Log("DisposeEphemeralObject: " + transform.gameObject.name);
                transform.gameObject.Dispose();
                return true;
            });
        }


        // The following methods are candidate to be moved to a separate utility class.
        // Primer.Scene/SceneTools was the first option but that would make Primer.Timeline depend on Primer.Scene
        // which is not desirable.
        // It'll stay here for now as it's the only place where it's used.

        /// <summary>
        ///     Recursively iterates all objects in the scene and calls the iterator function on each one.
        ///     If the iterator returns true, the children of the object will not be iterated.
        /// </summary>
        /// <param name="iterator">The function to call on each GameObject.</param>
        public static void IterateAllObjects(Func<Transform, bool> iterator)
        {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            for (var i = 0; i < rootObjects.Length; i++)
                IterateChildren(rootObjects[i].transform, iterator);
        }

        private static void IterateChildren(Transform transform, Func<Transform, bool> iterator)
        {
            if (iterator(transform))
                return;

            for (var i = 0; i < transform.childCount; i++)
                IterateChildren(transform.GetChild(i), iterator);
        }
    }
}