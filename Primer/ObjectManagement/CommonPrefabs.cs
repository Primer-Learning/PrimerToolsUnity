using System.Collections.Generic;

namespace Primer
{
    public enum CommonPrefabs
    {
        Blob,
        RockHome,
        MangoTree,
        Mango
    }

    public class PrefabFinder
    {
        public static Dictionary<CommonPrefabs, string> prefabNames = new()
        {
            { CommonPrefabs.Blob , "blob_skinned"},
            { CommonPrefabs.RockHome , "home"},
            { CommonPrefabs.MangoTree , "mango tree medium"},
            { CommonPrefabs.Mango , "mango"}
        };
    } 
}