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

        public static Tween TweenColor(this IMeshController self, Color newColor)
        {
            var meshes = self.GetMeshRenderers();

            void SetColorToAll(Color x)
            {
                foreach (var mesh in meshes) {
                    mesh.SetColor(x);
                }
            }

            return Tween.Value(SetColorToAll, self.GetColor, () => newColor);
        }
    }
}
