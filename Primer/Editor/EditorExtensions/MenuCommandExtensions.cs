using System;
using UnityEditor;
using UnityEngine;

namespace Primer.Editor
{
    public static class MenuCommandExtensions
    {
        public static bool HasSelectedElement(this MenuCommand command)
        {
            return Selection.activeGameObject != null;
        }

        public static GameObject GetSelectedElement(this MenuCommand command)
        {
            // Get the selected element and it's animator
            if (command.context is not GameObject selected) {
                throw new Exception("No game object selected");
            }

            return selected;
        }
    }
}
