using System.Collections.Generic;
using UnityEngine;

namespace PrimerBase
{
    public class ObjectGenerator : MonoBehaviour
    {
        public static string GENERATED_GAME_OBJECT_PREFIX = "*";
        public Transform generatedObjectsContainer;
        bool hasGeneratedChildren;


        #region Lifecycle
        // Update will take ensure animation works in play mode
        void Update() {
            if (Application.isPlaying) {
                UpdateGeneratedChildren();
            }
        }

        void OnValidate() {
            if (generatedObjectsContainer == (object)null) {
                generatedObjectsContainer = transform;
            }
        }

        // OnEnable will reset the component when
        // - It's created
        // - Script changes
        // - Has been disabled and re-enabled
        void OnEnable() {
            OnValidate();
            RemoveGeneratedChildren();
            UpdateGeneratedChildren();
        }

        void OnDisable() {
            RemoveGeneratedChildren();
        }
        #endregion


        #region Methods to override
        /// <summary>
        ///     Generates all required children and configures them.
        ///     Generation shhould only happen once, if they haven't been generated yet.
        /// </summary>
        public virtual void UpdateGeneratedChildren() { }

        /// <summary>
        ///     Invoked when children have been deleted, remove all references to them.
        /// </summary>
        protected virtual void RemoveGeneratedChildrenReferences() { }
        #endregion


        #region Child generation tools
        protected T GenerateChild<T>(T template, Quaternion rotation, Vector3 position) where T : MonoBehaviour {
            var child = GenerateChild(template);
            child.transform.localRotation = rotation;
            child.transform.localPosition = position;
            return child;
        }

        protected T GenerateChild<T>(T template, Vector3 position) where T : MonoBehaviour {
            var child = GenerateChild(template);
            child.transform.localPosition = position;
            return child;
        }

        protected T GenerateChild<T>(T template, Quaternion rotation) where T : MonoBehaviour {
            var child = GenerateChild(template);
            child.transform.localRotation = rotation;
            return child;
        }

        protected T GenerateChild<T>(T template) where T : MonoBehaviour {
            var child = Instantiate(template, generatedObjectsContainer);
            child.gameObject.hideFlags = HideFlags.DontSave;
            child.name = $"{GENERATED_GAME_OBJECT_PREFIX}{template.name}";
            hasGeneratedChildren = true;
            return child;
        }

        protected void RemoveGeneratedChildren(bool force = false) {
            if (!hasGeneratedChildren && !force) {
                return;
            }

            var toDispose = new List<GameObject>();

            foreach (Transform child in generatedObjectsContainer) {
                // Maybe there is a better way to detect generated objects
                if (child.gameObject.name.StartsWith(GENERATED_GAME_OBJECT_PREFIX)) {
                    toDispose.Add(child.gameObject);
                }
            }

            foreach (var child in toDispose) {
                child.Dispose();
            }

            hasGeneratedChildren = false;
            RemoveGeneratedChildrenReferences();
        }
        #endregion
    }
}
