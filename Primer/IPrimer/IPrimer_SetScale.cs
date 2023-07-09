using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_SetScaleExtensions
    {
        #region Overloads for IPrimer and Component with float and Vector3
        public static void SetScale(this IPrimer self, int scale, bool isGlobal = false)
        {
            self.transform.SetScale(Vector3.one * scale, isGlobal);
        }

        public static void SetScale(this IPrimer self, float scale, bool isGlobal = false)
        {
            self.transform.SetScale(Vector3.one * scale, isGlobal);
        }

        public static void SetScale(this IPrimer self, double scale, bool isGlobal = false)
        {
            self.transform.SetScale(Vector3.one * (float)scale, isGlobal);
        }

        public static void SetScale(this IPrimer self, Vector3 scale, bool isGlobal = false)
        {
            self.transform.SetScale(scale, isGlobal);
        }

        public static void SetScale(this Component self, int scale, bool isGlobal = false)
        {
            self.transform.SetScale(Vector3.one * scale, isGlobal);
        }

        public static void SetScale(this Component self, float scale, bool isGlobal = false)
        {
            self.transform.SetScale(Vector3.one * scale, isGlobal);
        }

        public static void SetScale(this Component self, double scale, bool isGlobal = false)
        {
            self.transform.SetScale(Vector3.one * (float)scale, isGlobal);
        }

        public static void SetScale(this Component self, Vector3 scale, bool isGlobal = false)
        {
            self.transform.SetScale(scale, isGlobal);
        }
        #endregion


        #region Overloads for IEnumerable<IPrimer> and IEnumerable<Component>
        public static void SetScale(this IEnumerable<IPrimer> self, float scale, bool isGlobal = false)
        {
            foreach (var item in self)
                item.transform.SetScale(Vector3.one * scale, isGlobal);
        }

        public static void SetScale(this IEnumerable<IPrimer> self, Vector3 scale, bool isGlobal = false)
        {
            foreach (var item in self)
                item.transform.SetScale(scale, isGlobal);
        }

        public static void SetScale(this IEnumerable<Component> self, float scale, bool isGlobal = false)
        {
            foreach (var item in self)
                item.transform.SetScale(Vector3.one * scale, isGlobal);
        }

        public static void SetScale(this IEnumerable<Component> self, Vector3 scale, bool isGlobal = false)
        {
            foreach (var item in self)
                item.transform.SetScale(scale, isGlobal);
        }

        public static void SetScale(this IEnumerable<Transform> self, float scale, bool isGlobal = false)
        {
            foreach (var item in self)
                item.SetScale(Vector3.one * scale, isGlobal);
        }

        public static void SetScale(this IEnumerable<Transform> self, Vector3 scale, bool isGlobal = false)
        {
            foreach (var item in self)
                item.SetScale(scale, isGlobal);
        }
        #endregion


        // Actual implementation
        // All overloads redirect here
        public static void SetScale(this Transform self, Vector3 scale, bool isGlobal = false)
        {
            if (isGlobal)
                SetGlobalScale(self, scale);
            else
                self.localScale = scale;
        }


        #region Internals
        private static void SetGlobalScale(Transform transform, Vector3 scale)
        {
            var parentScale = transform.parent is null
                ? Vector3.zero
                : transform.parent.lossyScale;

            transform.localScale = parentScale == Vector3.zero
                ? scale
                : scale.ElementWiseMultiply(InvertScale(parentScale));
        }

        private static Vector3 InvertScale(Vector3 v)
        {
            return new Vector3(1 / v.x, 1 / v.y, 1 / v.z);
        }
        #endregion
    }
}
