using System.Collections.Generic;
using UnityEngine;

namespace PrimerBase
{
    public class ObjectGenerator : MonoBehaviour
    {
        public static string GENERATED_GAME_OBJECT_PREFIX = "*";
        public Transform generatedObjectsContainer;

        void OnValidate() {
            if (generatedObjectsContainer == (object)null) {
                generatedObjectsContainer = transform;
            }
        }

        public T GenerateChild<T>(T template) where T : MonoBehaviour {
            var child = Instantiate(template, generatedObjectsContainer);
            child.gameObject.hideFlags = HideFlags.DontSave;
            child.name = $"{GENERATED_GAME_OBJECT_PREFIX}{template.name}";
            return child;
        }

        public void RemoveGeneratedChildren() {
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
        }
    }
}
