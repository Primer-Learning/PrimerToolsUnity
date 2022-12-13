using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Primer.Scene
{
    [RequireComponent(typeof(Camera))]
    public class RenderToPng : MonoBehaviour
    {
        private Camera cameraCache;
        internal Camera cam => cameraCache == null ? cameraCache = GetComponent<Camera>() : cameraCache;


        public string frameOutDir;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        internal int framesSaved = 0;


        internal static string defaultOutDir => Directory.GetCurrentDirectory();


        private void Update()
        {
            if (Time.frameCount > 999999) {
                Debug.LogWarning("y tho");
                return;
            }

            framesSaved++;

            var dirname = GetContainerDirectory();
            var path = Path.Combine(dirname, $"{framesSaved:000000}.png");

            RenderToPNG(path, resolutionWidth, resolutionHeight);
        }


        private readonly List<string> createdDirs = new();
        private string GetContainerDirectory()
        {
            var outDir = string.IsNullOrWhiteSpace(frameOutDir)
                ? defaultOutDir
                : frameOutDir;

            var dirname = $"{SceneManager.GetActiveScene().name}_recordings";
            var path = Path.Combine(outDir, "png", dirname);

            if (!createdDirs.Contains(path)) {
                Directory.CreateDirectory(path);
                createdDirs.Add(path);
            }

            return path;
        }

        private void RenderToPNG(string path, int resWidth, int resHeight)
        {
            // Set up camera
            var image = new Texture2D(resWidth, resHeight, TextureFormat.RGBA32, false);
            var rt = new RenderTexture(resWidth, resHeight, 24);

            cam.targetTexture = rt;
            RenderTexture.active = rt;

            // Render to image texture
            cam.Render();
            image.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);

            // Save png
            var bytes = image.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            // Clean up
            cam.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            Destroy(image);
        }
    }
}
