using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Primer
{
    public static class RendererExtensions
    {
        private static Material defaultMaterialCache;
        public static Material defaultMaterial
            => defaultMaterialCache ??= Resources.Load<Material>("BasicDiffuse.mat");

        private static readonly Dictionary<WeakReference<Renderer>, Material> memory = new();


        #region Get/Set color
        public static Color GetColor(this Renderer renderer)
        {
            return memory.TryGetValue(new WeakReference<Renderer>(renderer), out var material)
                ? material.color
                : renderer.sharedMaterial?.color
                ?? defaultMaterial.color;
        }

        public static void SetColor(this Renderer renderer, Color color)
        {
            GetMaterial(renderer).color = color;
        }

        public static Color GetColor(this IEnumerable<Renderer> self)
        {
            return self.GetMaterial()?.color ?? Color.white;
        }

        public static void SetColor(this IEnumerable<Renderer> self, Color color)
        {
            foreach (var mesh in self) {
                mesh.SetColor(color);
            }
        }
        #endregion

        #region Get/Set material
        public static Material GetMaterial(this Renderer renderer)
        {
            var weak = new WeakReference<Renderer>(renderer);

            if (!memory.TryGetValue(weak, out var material)) {
                material = renderer.sharedMaterial is null
                    ? new Material(defaultMaterial)
                    : new Material(renderer.sharedMaterial);

                memory.Add(weak, material);
            }

            renderer.material = material;
            return material;
        }
        

        public static Material GetMaterial(this IEnumerable<Renderer> self)
        {
            var renderers = self.ToArray();

            return renderers.Length is 0
                ? defaultMaterial
                : renderers[0].sharedMaterial;
        }

        public static void SetMaterial(this IEnumerable<Renderer> self, Material material)
        {
            foreach (var mesh in self) {
                mesh.material = material;
            }
        }
        #endregion
    }
}
