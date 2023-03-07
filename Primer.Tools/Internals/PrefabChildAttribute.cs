using System;
using Sirenix.OdinInspector;

namespace Primer.Tools
{
    [IncludeMyAttributes]
    [Required]
    [PropertyOrder(-100)]
    [ChildGameObjectsOnly]
    [FoldoutGroup("Child objects")]
    public class PrefabChildAttribute : Attribute  {}
}
