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

        private static Texture[] GetPreviewsFor(LatexExpression expression) => (
            from character in expression
            select character.mesh into mesh
            where mesh is not null
            select AssetPreview.GetAssetPreview(mesh) into texture
            select ResizeAndFlip(texture, charPreviewSize, charPreviewSize)
        ).ToArray();

        private static Texture ResizeAndFlip(Texture source, int newWidth, int newHeight)
        {
            if (!source)
                return source;

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

        public static int ShowGroup(LatexExpression expression, (int start, int end) range)
            => ShowGroup(expression.Slice(range.start, range.end));

        public static int ShowGroup(LatexExpression expression)
        {
            var width = Screen.width;
            var cols = width / (charPreviewSize + 10);

            var textures = GetPreviewsFor(expression);
            return GUILayout.SelectionGrid(0, textures, cols);
        }
    }
}
