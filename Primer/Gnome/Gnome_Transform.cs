using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    // This part attempts to mimic the properties of a transform so we can replace a Transform with a Gnome
    // without changing the rest of the code.
    public partial class Gnome
    {
        public Vector3 position {
            get => transform.position;
            set => transform.position = value;
        }

        public Vector3 localPosition {
            get => transform.localPosition;
            set => transform.localPosition = value;
        }

        public Quaternion rotation {
            get => transform.rotation;
            set => transform.rotation = value;
        }

        public Quaternion localRotation {
            get => transform.localRotation;
            set => transform.localRotation = value;
        }

        public float scale {
            get => transform.localScale.x;
            set => transform.localScale = Vector3.one * value;
        }

        public Vector3 localScale {
            get => transform.localScale;
            set => transform.localScale = value;
        }

        public Vector3 lossyScale => transform.lossyScale;


        public IEnumerable<Transform> children => usedChildren;

        public T GetComponent<T>(bool forceCreate = false) where T : Component
        {
            if (forceCreate)
                return transform.gameObject.AddComponent<T>();

            return transform.GetComponent<T>() ?? transform.gameObject.AddComponent<T>();
        }

        public T[] ChildComponents<T>() where T : Component
        {
            return transform.GetComponentsInChildren<T>()
                .Where(x => x.GetComponent<GnomeEvents.IsRemoving>() is null)
                .ToArray();
        }

        public Gnome SetDefaults()
        {
            transform.SetDefaults();
            return this;
        }
    }
}
