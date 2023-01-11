using System;
using UnityEngine;

namespace Primer
{
    public class DontCallMeOnPrefabException : Exception
    {
        public static void ThrowIfIsPrefab(Component component, string name)
        {
            if (component.gameObject.IsPreset())
                throw new DontCallMeOnPrefabException(name);
        }

        public DontCallMeOnPrefabException(string name)
            : base(
                $@"


{name} is instantiated over a prefab! Don't generate children on a prefab or {name} won't be able to remove them!

Add this before instantiating {name}:

```
if (transform.gameObject.IsPreset()) return;
```

                   ".Trim()
            ) {}
    }
}
