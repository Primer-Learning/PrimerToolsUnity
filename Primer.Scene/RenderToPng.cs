using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Primer
{
    public class RenderToPng : MonoBehaviour
    {
        private Camera cameraCache;
        internal Camera cam => cameraCache == null ? cameraCache = GetComponent<Camera>() : cameraCache;


        public string frameOutDir;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        [ReadOnly]
        private int framesSaved = 0;


        private void OnEnable()
        {
            if (!string.IsNullOrWhiteSpace(frameOutDir)) return;
            frameOutDir = Directory.GetCurrentDirectory();
            Debug.LogWarning($"Frame capture directory not set. Setting to {frameOutDir}.");
        }

        private void Update()
        {
            if (Time.frameCount > 999999) {
                Debug.LogWarning("y tho");
                return;
            }

            framesSaved++;

            var dirname = $"{SceneManager.GetActiveScene().name}_recordings";
            var path = Path.Combine(frameOutDir, "png", dirname, $"{framesSaved:000000}.png");

            RenderToPNG(path, resolutionWidth, resolutionHeight);
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
