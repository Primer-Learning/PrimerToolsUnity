using System.Collections.Generic;
using UnityEngine;

namespace PrimerBase
{
    public abstract class ObjectGenerator : MonoBehaviour
    {
        public static string GENERATED_GAME_OBJECT_PREFIX = "*";

        /// <summary>
        ///     Generates all required children and configures them.
        ///     Generation shhould only happen once, if they haven't been generated yet.
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

        public T Create<T>(T template, Quaternion rotation, Vector3 position) where T : MonoBehaviour {
            var child = Create(template);
            child.transform.localRotation = rotation;
            child.transform.localPosition = position;
            return child;
        }

        public T Create<T>(T template, Vector3 position) where T : MonoBehaviour {
            var child = Create(template);
            child.transform.localPosition = position;
            return child;
        }

        public T Create<T>(T template, Quaternion rotation) where T : MonoBehaviour {
            var child = Create(template);
            child.transform.localRotation = rotation;
            return child;
        }

        public T Create<T>(T template) where T : MonoBehaviour {
            var child = Instantiate(template, transform);
            child.gameObject.hideFlags = HideFlags.DontSave;
            child.name = $"{GENERATED_GAME_OBJECT_PREFIX}{template.name}";
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
        #endregion
    }
}
