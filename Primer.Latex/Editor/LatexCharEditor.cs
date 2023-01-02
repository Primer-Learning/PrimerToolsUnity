using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    public static class LatexCharEditor
    {
        private static int charPreviewSize = 50;

        public static void CharPreviewSize()
        {
            charPreviewSize = EditorGUILayout.IntSlider(charPreviewSize, 10, 100);
        }

        private static Texture[] GetPreviewsFor(LatexChar[] chars, int start, int end)
        {
            return (
                from character in chars.Skip(start).Take(end - start)
                select character.symbol?.mesh into mesh
                where mesh is not null
                select AssetPreview.GetAssetPreview(mesh) into texture
                select ResizeAndFlip(texture, charPreviewSize, charPreviewSize)
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

        public static int ShowGroup(LatexChar[] chars, int start, int end)
        {
            var width = Screen.width;
            var cols = width / (charPreviewSize + 10);

            var textures = GetPreviewsFor(chars, start, end);
            return GUILayout.SelectionGrid(0, textures, cols);
        }
    }
}
