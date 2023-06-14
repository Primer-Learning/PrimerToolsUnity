using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_GetChildrenExtensions
    {
        public static IEnumerable<Transform[]> GetChildren(this IEnumerable<IPrimer> self)
        {
            return self.Select(x => x.transform.transform.GetChildren());
        }

        public static IEnumerable<Transform[]> GetChildren(this IEnumerable<Component> self)
        {
            return self.Select(x => x.transform.GetChildren());
        }

        public static Transform[] GetChildren(this IPrimer self)
        {
            return self.transform.transform.GetChildren();
        }

        public static Transform[] GetChildren(this Component self)
        {
            return self.transform.GetChildren();
        }

        public static Transform[] GetChildren(this Transform transform)
        {
            var children = new Transform[transform.childCount];

            for (var i = 0; i < transform.childCount; i++)
                children[i] = transform.GetChild(i);

            return children;
        }

    }
}
