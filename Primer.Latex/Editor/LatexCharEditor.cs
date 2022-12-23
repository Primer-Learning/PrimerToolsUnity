using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    public static class LatexCharEditor
    {
        public static Texture[] GetPreviewsFor(LatexChar[] chars, int start, int end, int size)
        {
            return (
                from character in chars.Skip(start).Take(end - start)
                select character.symbol?.mesh into mesh
                where mesh is not null
                select AssetPreview.GetAssetPreview(mesh) into texture
                select ResizeAndFlip(texture, size, size)
            ).ToArray();
        }

        private static Texture ResizeAndFlip(Texture source, int newWidth, int newHeight)
        {
            var temporary = RenderTexture.GetTemporary(newWidth, newHeight);
            var newTexture = new Texture2D(newWidth, newHeight);

            source.filterMode = FilterMode.Point;
            temporary.filterMode = FilterMode.Point;
            RenderTexture.active = temporary;

            Graphics.Blit(source, temporary, new Vector2(-1, 1), Vector2.zero);
            newTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            newTexture.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(temporary);
            return newTexture;
        }
    }
}
