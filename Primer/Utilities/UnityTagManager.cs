using UnityEditor;
using UnityEngine;

namespace Primer
{
    public static class UnityTagManager
    {
        // from
        // https://bladecast.pro/unity-tutorial/create-tags-by-script

        private static Object tagManagerAssetCache;
        private static Object tagManagerAsset => tagManagerAssetCache ??= AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");

        public static void CreateTag(string tag) {
            var asset = tagManagerAsset;
            if (asset == null) return;

            var so = new SerializedObject(asset);
            var tags = so.FindProperty("tags");

            var numTags = tags.arraySize;

            // do not create duplicates
            for (var i = 0; i < numTags; i++) {
                var existingTag = tags.GetArrayElementAtIndex(i);
                if (existingTag.stringValue == tag) return;
            }

            tags.InsertArrayElementAtIndex(numTags);
            tags.GetArrayElementAtIndex(numTags).stringValue = tag;
            so.ApplyModifiedProperties();
            so.Update();
        }
    }
}
