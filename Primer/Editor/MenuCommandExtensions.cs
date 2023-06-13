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
            if (Selection.activeGameObject is {} selected) {
                return selected;
            }

            if (command.context is GameObject context) {
                return context;
            }

            throw new Exception("No game object selected");
        }

        public static GameObject[] GetSelectedElements(this MenuCommand command)
        {
            if (Selection.gameObjects is {} selected) {
                return selected;
            }

            if (command.context is GameObject context) {
                return new [] {context};
            }

            throw new Exception("No game objects selected");
        }
    }
}
