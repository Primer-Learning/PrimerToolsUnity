using System;
using System.Collections.Generic;
using System.Linq;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor;
using UnityEngine;

namespace Primer
{
    public class Pool<T> : IDisposable, IPrimer where T : Component
    {
        public GameObject prefab;
        
        public void Dispose()
        {
            transform.gameObject.SetActive(false);
        }
        
        public void Reset(bool hard = false)
        {
            foreach (var child in transform.GetChildren())
            {
                if (hard)
                    child.gameObject.Dispose();
                else
                    child.gameObject.SetActive(false);
            }
        }

        public Transform transform { get; }
        public Component component { get; }
        
        public IEnumerable<Transform> activeChildren => transform.GetChildren().Where(x => x.gameObject.activeSelf);
        public int activeChildCount => transform.GetChildren().Count(x => x.gameObject.activeSelf);

        #region Add

        public T AddOrActivate()
        {
            var inactiveChildren = transform.GetChildren().Where(x => x.gameObject.activeSelf == false && x.HasComponent<T>()).ToArray();
            
            GameObject newObject;
            // Return unused pooled object if one exists
            if (inactiveChildren.Length > 0)
            { newObject = inactiveChildren.ToArray()[0].gameObject; }
            // Instantiate a prefab if one is defined
            else if (prefab != null)
            {
                newObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }
            // Otherwise, just a new gameObject
            else { newObject = new GameObject(); }

            var t = newObject.transform;
            t.SetParent(transform);
            newObject.gameObject.SetActive(true);
            // These will likely be overridden, but it's a nice default
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            return newObject.GetOrAddComponent<T>();
        }

        #endregion
        
        #region Constructors
        public Pool(string name, Component parent = null)
        {
            transform = parent is null
                ? ObjectManagementUtilities.GetRootTransform(name)
                : ObjectManagementUtilities.GetDirectChild(parent.transform, name);

            component = transform;
            transform.gameObject.SetActive(true);
        }

        public Pool(Transform component)
        {
            this.component = component;
            transform = component;
            transform.gameObject.SetActive(true);
        }

        protected Pool(Component component)
        {
            this.component = component;
            transform = component.transform;
            transform.gameObject.SetActive(true);
        }
        #endregion
    }
}