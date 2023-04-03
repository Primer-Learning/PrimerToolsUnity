using System;
using Sirenix.OdinInspector;

namespace Primer.Shapes
{
    [IncludeMyAttributes]
    [Required]
    [PropertyOrder(-100)]
    [ChildGameObjectsOnly]
    [FoldoutGroup("Child objects")]
    public class PrefabChildAttribute : Attribute { }
}
