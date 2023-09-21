using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Animation
{
    public static class IMeshController_TweenColorExtensions
    {
        public static Tween TweenColor(this Renderer self, Color newColor)
        {
            return Tween.Value(self.SetColor, self.GetColor, () => newColor);
        }

        public static Tween TweenColor(this IEnumerable<Renderer> self, Color newColor)
        {
            var meshes = self.ToArray();
            return Tween.Value(meshes.SetColor, meshes.GetColor, () => newColor);
        }

        public static Tween TweenColor(this IMeshController self, Color newColor)
        {
            var meshes = self.GetMeshRenderers();
            return Tween.Value(meshes.SetColor, meshes.GetColor, () => newColor);
        }
        public static Tween FadeToAlpha(this Renderer self, float alpha)
        {
            return alpha >= 1 ? self.FadeIn() : self.FadeOut(alpha);
        }
        public static Tween FadeOut(this Renderer self, float alpha)
        {
            var mat = self.sharedMaterial;
            StandardShaderUtils.ChangeRenderMode(mat, StandardShaderUtils.BlendMode.Transparent);
            return self.TweenAlpha(alpha);
        }
        public static Tween FadeIn(this Renderer self)
        {
            var mat = self.sharedMaterial;
            return self.TweenAlpha(1).Observe(onComplete: () => StandardShaderUtils.ChangeRenderMode(mat, StandardShaderUtils.BlendMode.Opaque));
        }

        public static Tween TweenAlpha(this Renderer self, float alpha)
        {
            return Tween.Value(
                self.SetColor,
                self.GetColor,
                () =>
                {
                    var oldColor = self.GetColor();
                    return new Color(oldColor.r, oldColor.g, oldColor.b, alpha);
                }
            );
        }
    }
}
