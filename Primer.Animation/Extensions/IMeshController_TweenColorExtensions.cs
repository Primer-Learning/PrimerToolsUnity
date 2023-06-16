using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Animation
{
    public static class IMeshController_TweenColorExtensions
    {
        public static Tween TweenColor(this MeshRenderer self, Color newColor)
        {
            return Tween.Value(self.SetColor, self.GetColor, () => newColor);
        }

        public static Tween TweenColor(this SkinnedMeshRenderer self, Color newColor)
        {
            return Tween.Value(self.SetColor, self.GetColor, () => newColor);
        }

        public static Tween TweenColor(this IEnumerable<MeshRenderer> self, Color newColor)
        {
            var meshes = self.ToArray();
            return Tween.Value(meshes.SetColor, meshes.GetColor, () => newColor);
        }

        public static Tween TweenColor(this IMeshController self, Color newColor)
        {
            var meshes = self.GetMeshRenderers();
            return Tween.Value(meshes.SetColor, meshes.GetColor, () => newColor);
        }
    }
}
