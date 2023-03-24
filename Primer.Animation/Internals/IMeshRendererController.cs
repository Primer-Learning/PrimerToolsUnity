using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Primer.Animation
{
    public interface IMeshRendererController
    {
        private static Material defaultMaterialCache;
        public static Material defaultMaterial =>
            defaultMaterialCache ??= AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

        protected internal MeshRenderer[] meshRenderers { get; }
        public Color color { get; set; }
        public Material material { get; set; }
    }

    public static class ColorControlExtensions
    {
        public static void SetMaterial(this IMeshRendererController control, Material material)
        {
            foreach (var mesh in control.meshRenderers) {
                mesh.material = material;
            }
        }

        public static void SetColor(this IMeshRendererController control, Color color)
        {
            foreach (var mesh in control.meshRenderers) {
                mesh.SetColor(color);
            }
        }

        public static Tween TweenColor(this IMeshRendererController control, Color newColor)
        {
            if (control.color == newColor)
                return Tween.noop;

            var initial = control.color;
            var children = control.meshRenderers;

            return new Tween(t => {
                var lerpedColor =  Color.Lerp(initial, newColor, t);

                foreach (var child in children)
                    child.SetColor(lerpedColor);
            });
        }
    }
}
