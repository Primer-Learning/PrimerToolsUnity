using UnityEditor;
using UnityEngine;

namespace Primer
{
    public static class UnityTagManager
    {
        // from
        // https://bladecast.pro/unity-tutorial/create-tags-by-script

        static Object tagManagerAsset;
        static Object TagManagerAsset => tagManagerAsset ??= AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");

        public static void CreateTag(string tag) {
            var asset = TagManagerAsset;
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
