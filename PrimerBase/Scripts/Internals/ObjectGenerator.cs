using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer
{

    public abstract class ObjectGenerator : MonoBehaviour
    {
        static string GENERATED_GAME_OBJECT_PREFIX = "*";

        /// <summary>
        ///     Generates all required children and configures them.
        ///     Generation should only happen once, if they haven't been generated yet.
        /// </summary>
        public abstract void UpdateChildren();

        /// <summary>
        ///     Invoked when children have been deleted, remove all references to them.
        /// </summary>
        protected abstract void OnChildrenRemoved();


        #region Lifecycle
        // Update will take ensure animation works in play mode
        // TODO: this will be replaced with proper Timeline integration
        void Update() {
            if (Application.isPlaying) {
                UpdateChildren();
            }
        }

        // OnEnable will reset the component when
        // - It's created
        // - Script changes
        // - Has been disabled and re-enabled
        void OnEnable() {
            RemoveGeneratedChildren();
            UpdateChildren();
        }

        void OnDisable() {
            RemoveGeneratedChildren();
        }
        #endregion


        #region Child generation methods
        bool hasGeneratedChildren;

        public T Create<T>(T template, Quaternion rotation) where T : MonoBehaviour => Create(template, null, rotation);
        public T Create<T>(T template, Vector3? position = null, Quaternion? rotation = null) where T : MonoBehaviour {
            var child = Instantiate(template, transform);

            child.gameObject.hideFlags = HideFlags.DontSave;
            child.name = $"{GENERATED_GAME_OBJECT_PREFIX}{template.name}";

            if (rotation is not null)
                child.transform.localRotation = (Quaternion)rotation;

            if (position is not null)
                child.transform.localPosition = (Vector3)position;

            hasGeneratedChildren = true;
            return child;
        }

        public void RemoveGeneratedChildren(bool force = false) {
            if (!hasGeneratedChildren && !force) {
                return;
            }

            var toDispose = new List<GameObject>();

            foreach (Transform child in transform) {
                // Maybe there is a better way to detect generated objects
                if (child.gameObject.name.StartsWith(GENERATED_GAME_OBJECT_PREFIX)) {
                    toDispose.Add(child.gameObject);
                }
            }

            foreach (var child in toDispose) {
                child.Dispose();
            }

            hasGeneratedChildren = false;
            OnChildrenRemoved();

        }

        protected (
            IEnumerable<T> toAdd,
            IEnumerable<U> toRemove,
            IEnumerable<(T, U)> toUpdate
        ) SynchronizeLists<T, U>(
            IEnumerable<T> wanted,
            IEnumerable<U> existing,
            Func<T, U, bool> compare
        ) where U : Object {
            var garbageCollected = existing.Where(x => !x).ToArray();
            var remove = existing.Where(x => x).ToList();
            var add = wanted.ToList();
            var update = new List<(T, U)>();

            foreach (var entry in wanted) {
                var found = remove.Find(obj => compare(entry, obj));

                if (found) {
                    remove.Remove(found);
                    add.Remove(entry);
                    update.Add((entry, found));
                }
            }

            return (add, remove.Concat(garbageCollected), update);
        }

        protected IEnumerable<T> RemoveGarbageCollected<T>(IEnumerable<T> source) where T : Object {
            return source.Where(x => !!x);
        }
        #endregion
    }
}
