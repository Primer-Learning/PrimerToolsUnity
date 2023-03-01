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

        internal static string defaultOutDir => Path.Combine(Directory.GetCurrentDirectory(), "..\\..");
        private string destinationDirectory;

        private void Update()
        {
            if (Time.frameCount > 999999) {
                Debug.LogWarning("y tho");
                return;
            }

            framesSaved++;
            var path = Path.Combine(destinationDirectory, $"{framesSaved:000000}.png");

            RenderToPNG(path, resolutionWidth, resolutionHeight);
        }

        private void Start()
        {
            destinationDirectory = GetContainerDirectory();
        }


        // private readonly List<string> createdDirs = new();
        private string GetContainerDirectory()
        {
            var outDir = string.IsNullOrWhiteSpace(frameOutDir)
                ? defaultOutDir
                : frameOutDir;

            var dirname = $"{SceneManager.GetActiveScene().name}_recordings";
            var scenePath = Path.Combine(outDir, "png", dirname);

            // Make a folder specifically for this take
            var takeCount = 0;
            var takesString = "take 1";
            var takePath = Path.Combine(scenePath, takesString);
            // Increment the folder number until one doesn't exist, 
            while (Directory.Exists(takePath)) {
                takeCount++;
                takesString = $"take {takeCount + 1}";
                takePath = Path.Combine(scenePath, takesString);
            }
            Directory.CreateDirectory(takePath);

            return takePath;
        }

        internal void RenderToPNG(string path, int resWidth, int resHeight)
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
