using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer
{
    public abstract class ObjectGenerator : MonoBehaviour
    {
        static readonly string generatedGameObjectPrefix = "*";

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

        public T Create<T>(T template, Quaternion rotation) where T : Component => Create(template, null, rotation);
        public T Create<T>(T template, Vector3? position = null, Quaternion? rotation = null) where T : Component {
            var child = Instantiate(template, transform);

            child.gameObject.hideFlags = HideFlags.DontSave;
            child.name = $"{generatedGameObjectPrefix}{template.name}";

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
                if (child.gameObject.name.StartsWith(generatedGameObjectPrefix)) {
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
                Func<T, U, bool> compare,
                Func<U, bool> getGameObject = null
            ) where U : Object {
            if (getGameObject is null) {
                getGameObject = x => !!x;
            }

            var garbageCollected = existing.Where(x => !getGameObject(x)).ToArray();
            var remove = existing.Where(getGameObject).ToList();
            var add = wanted.ToList();
            var update = new List<(T, U)>();

            foreach (var entry in wanted) {
                var found = remove.Find(obj => compare(entry, obj));

                if (found is not null) {
                    remove.Remove(found);
                    add.Remove(entry);
                    update.Add((entry, found));
                }
            }

            foreach (var obj in garbageCollected) {
                if (Application.isPlaying)
                    Destroy(obj);
                else
                    DestroyImmediate(obj);
            }

            return (add, remove.Concat(garbageCollected), update);
        }

        protected IEnumerable<T> RemoveGarbageCollected<T>(IEnumerable<T> source) where T : Object {
            return source.Where(x => !!x);
        }
        #endregion
    }
}
