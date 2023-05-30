using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Primer.Timeline;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace Primer.Scene
{
    [RequireComponent(typeof(Camera))]
    public class RenderToPng : MonoBehaviour
    {
        private Camera cameraCache;
        public AudioClip endSound;
        internal Camera cam => cameraCache ??= GetComponent<Camera>();

        [FormerlySerializedAs("avoidDuplicatedFrames")] public bool omitRepeatingFrames = false;
        public bool transparentBackground = true;
        public bool fillGapsInPreviousTake = false;
        public string frameOutDir;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public int frameRate = 60;
        public int maxFrames = 0;

        [ShowInInspector, ReadOnly]
        private int framesSaved = 0;

        private bool waitingForEndProcess = false;

        private static string defaultOutDir => Path.Combine(Directory.GetCurrentDirectory(), "..", "..");
        private string destinationDirectory;
        private byte[] lastFrame;

        [Button(ButtonSizes.Large)]
        public void OpenDestinationDirectory()
        {
            Process.Start(GetScenePath());
        }

        private void Update()
        {
            if (waitingForEndProcess || !PrimerTimeline.isPlaying)
                return;

            if (Time.frameCount > 999999) {
                Debug.LogWarning("y tho");
                return;
            }

            var path = Path.Combine(destinationDirectory, $"{framesSaved:000000}.png");

            RenderToPNG(path, resolutionWidth, resolutionHeight);
            framesSaved++;
            
            if (maxFrames > 0 && framesSaved >= maxFrames)
            {
                waitingForEndProcess = true;
                StartCoroutine(EndRecording());
            }
        }

        private void Start()
        {
            Time.captureFramerate = frameRate;
            destinationDirectory = GetContainerDirectory();

            if (transparentBackground)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0, 0, 0, 0);
            }
        }

        IEnumerator EndRecording()
        {
            if (endSound is not null)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.clip = endSound;
                source.Play();
                while (source.isPlaying)
                {
                    yield return null;
                }
            }

            Debug.Log("Reached max frames. Exiting play mode.");
            UnityEditor.EditorApplication.ExitPlaymode();
        }


        // private readonly List<string> createdDirs = new();
        private string GetContainerDirectory()
        {
            var scenePath = GetScenePath();

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

            if (!fillGapsInPreviousTake)
            {
                Directory.CreateDirectory(takePath);
            }
            else
            {
                takesString = $"take {takeCount}";
                takePath = Path.Combine(scenePath, takesString);
            }

            return takePath;
        }

        private string GetScenePath()
        {
            var outDir = string.IsNullOrWhiteSpace(frameOutDir)
                ? defaultOutDir
                : frameOutDir;

            var dirname = $"{SceneManager.GetActiveScene().name}_recordings";
            var scenePath = Path.Combine(outDir, "png", dirname);
            return scenePath;
        }

        internal void RenderToPNG(string path, int resWidth, int resHeight)
        {
            // If it's not new take, and the png exists already, skip
            if (File.Exists(path) && fillGapsInPreviousTake) return;
            
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

            if (!omitRepeatingFrames || (lastFrame is null || !bytes.SequenceEqual(lastFrame))) {
                lastFrame = bytes;
                File.WriteAllBytes(path, bytes);
            }

            // Clean up
            cam.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            Destroy(image);
        }
    }
}
