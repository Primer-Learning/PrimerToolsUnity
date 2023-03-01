using System;
using Sirenix.OdinInspector;

namespace Primer.Tools
{
    [IncludeMyAttributes]
    [Required]
    [ChildGameObjectsOnly]
    [FoldoutGroup("Child objects")]
    public class PrefabChildAttribute : Attribute  {}
}
