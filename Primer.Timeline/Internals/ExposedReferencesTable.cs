using System.Collections.Generic;
using UnityEngine;

namespace Primer.Timeline
{
    public class ExposedReferencesTable : MonoBehaviour, IExposedPropertyTable
    {
        public List<PropertyName> properties = new();
        public List<Object> references = new();

        private void Awake()
        {
            hideFlags = HideFlags.HideInInspector;
        }

        public T Get<T>(ExposedReference<T> reference) where T : Object
        {
            var result = GetReferenceValue(reference.exposedName, out var idValid);
            return idValid ? result as T : null;
        }

        public void Set<T>(ExposedReference<T> reference, T value)
            where T : Object
        {
            SetReferenceValue(reference.exposedName, value);
        }

        public Object GetReferenceValue(PropertyName id, out bool idValid)
        {
            var index = properties.IndexOf(id);

            if (index == -1) {
                idValid = false;
                return null;
            }

            idValid = true;
            return references[index];
        }

        public void SetReferenceValue(PropertyName id, Object value)
        {
            var index = properties.IndexOf(id);

            if (index == -1) {
                properties.Add(id);
                references.Add(value);
            } else {
                references[index] = value;
            }
        }

        public void ClearReferenceValue(PropertyName id)
        {
            var index = properties.IndexOf(id);

            if (index == -1)
                return;

            properties.RemoveAt(index);
            references.RemoveAt(index);
        }
    }
}
