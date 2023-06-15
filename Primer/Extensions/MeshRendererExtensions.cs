using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class MeshRendererExtensions
    {
        private static Material defaultMaterialCache;
        public static Material defaultMaterial
            => defaultMaterialCache ??= Resources.Load<Material>("BasicDiffuseWhite");

        private static readonly Dictionary<WeakReference<MeshRenderer>, Material> memory = new();
        private static readonly Dictionary<WeakReference<SkinnedMeshRenderer>, Material> skinnedMemory = new();

        public static Color GetColor(this MeshRenderer renderer)
        {
            return memory.TryGetValue(new WeakReference<MeshRenderer>(renderer), out var material)
                ? material.color
                : renderer.sharedMaterial?.color
                ?? defaultMaterial.color;
        }

        public static Color GetColor(this SkinnedMeshRenderer renderer)
        {
            return skinnedMemory.TryGetValue(new WeakReference<SkinnedMeshRenderer>(renderer), out var material)
                ? material.color
                : renderer.sharedMaterial?.color
                ?? defaultMaterial.color;
        }

        public static void SetColor(this MeshRenderer renderer, Color color)
        {
            GetMaterial(renderer).color = color;
        }

        public static void SetColor(this SkinnedMeshRenderer renderer, Color color)
        {
            GetMaterial(renderer).color = color;
        }

        public static Material GetMaterial(this MeshRenderer renderer)
        {
            var weak = new WeakReference<MeshRenderer>(renderer);

            if (!memory.TryGetValue(weak, out var material)) {
                material = renderer.sharedMaterial is null
                    ? new Material(defaultMaterial)
                    : new Material(renderer.sharedMaterial);

                memory.Add(weak, material);
            }

            renderer.material = material;
            return material;
        }

        public static Material GetMaterial(this SkinnedMeshRenderer renderer)
        {
            if (renderer.sharedMaterial == null)
                throw new InvalidOperationException("SkinnedMeshRenderer doesn't have a material");

            var weak = new WeakReference<SkinnedMeshRenderer>(renderer);

            if (!skinnedMemory.TryGetValue(weak, out var material)) {
                material = new Material(renderer.sharedMaterial);
                skinnedMemory.Add(weak, material);
            }

            renderer.material = material;
            return material;
        }
    }
}
