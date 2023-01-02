using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class TransformExtensions
    {
        public static List<Transform> GetChildren(this Transform transform)
        {
            var list = new List<Transform>();

            for (var i = 0; i < transform.childCount; i++)
                list.Add(transform.GetChild(i));

            return list;
        }
    }
}
