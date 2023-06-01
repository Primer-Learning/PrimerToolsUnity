using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Primer.Timeline;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace Primer.Scene
{
    [RequireComponent(typeof(Camera))]
    public class RenderToPng : MonoBehaviour
    {
        private Camera cameraCache = null;
        internal Camera cam => cameraCache != null ? cameraCache : cameraCache = GetComponent<Camera>();
        
        [ShowInInspector, ReadOnly]
        private int framesSaved = 0;

        [Title("Output quality settings")]
        [OnValueChanged(nameof(ApplyPreset))]
        public bool transparentBackground = true;
        public QualityPreset qualityPreset;
        [ShowInInspector, ReadOnly] private int resolutionWidth = 1920;
        [ShowInInspector, ReadOnly] private int resolutionHeight = 1080;
        [ShowInInspector, ReadOnly] private int frameRate = 60;
        
        [Title("End handling")]
        [FormerlySerializedAs("maxTime")] public float maxSeconds = 0;
        public AudioClip endSound;
        
        [Title("Saving options")]
        [FormerlySerializedAs("avoidDuplicatedFrames")]
        public bool omitRepeatingFrames = false;
        public bool fillGapsInPreviousTake = false;
        public string frameOutDir;

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

            var path = Path.Combine(destinationDirectory, $"{framesSaved:000000}.png");

            RenderToPNG(path, resolutionWidth, resolutionHeight);
            framesSaved++;
            
            if (ShouldEnd())
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

            UnityEditor.EditorApplication.ExitPlaymode();
        }


        // private readonly List<string> createdDirs = new();
        private string GetContainerDirectory()
        {
            var scenePath = GetScenePath();
            var presetName = qualityPreset.ToString();

            // Make a folder specifically for this take
            var takeCount = 0;
            var takesString = $"{presetName} take 1";
            var takePath = Path.Combine(scenePath, takesString);
            // Increment the folder number until one doesn't exist, 
            while (Directory.Exists(takePath)) {
                takeCount++;
                takesString = $"{presetName} take {takeCount + 1}";
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

        private bool ShouldEnd()
        {
            // If maxSeconds is set, and we're past maxSeconds 
            if (maxSeconds > 0 && framesSaved > maxSeconds * frameRate)
            {
                Debug.Log("Reached max frames. Exiting play mode.");
                return true;
            }
            
            var director = FindObjectOfType<PlayableDirector>();
            // If maxSeconds is not set, and
            // If PlayableDirector is done (and exists)
            // +1 second of frames as a just-in-case buffer
            if (maxSeconds <= 0 && director is not null && framesSaved > (director.duration + 1) * frameRate)
            {
                Debug.Log("Reached end of playable director. Exiting play mode.");
                return true;
            }

            // If > 1hr has been recorded
            // And maxSeconds is not set ( to something > 1hr, for example )
            if (maxSeconds <= 0 && framesSaved > frameRate * 60 * 60) {
                Debug.Log("Reached 1hr recorded. Probably an error, but congrats to your hard drive on being big!");
                return true;
            }

            return false;
        }
        
        private void ApplyPreset()
        {
            switch (qualityPreset)
            {
                case QualityPreset.Minimal:
                    resolutionWidth = 854;
                    resolutionHeight = 480;
                    frameRate = 30;
                    break;
                case QualityPreset.Low:
                    resolutionWidth = 1280;
                    resolutionHeight = 720;
                    frameRate = 30;
                    break;
                case QualityPreset.High:
                    resolutionWidth = 1920;
                    resolutionHeight = 1080;
                    frameRate = 60;
                    break;
                case QualityPreset.Publish:
                    resolutionWidth = 3840;
                    resolutionHeight = 2160;
                    frameRate = 60;
                    break;
            }
        }
    }
    public enum QualityPreset
    {
        Minimal,  // 480p, 30 fps
        Low,      // 720p, 30 fps
        High,     // 1080p, 60 fps
        Publish   // 4k, 60 fps
    }
}
