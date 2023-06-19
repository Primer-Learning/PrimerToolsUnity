using System;
using System.Collections;
using System.Collections.Generic;
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

        [ShowInInspector, ReadOnly] private int framesWatched = 0;
        [ShowInInspector, ReadOnly] private int framesSaved = 0;

        [Title("Output quality settings")]
        public bool transparentBackground = true;
        [OnValueChanged(nameof(ApplyPreset))]
        public QualityPreset qualityPreset;
        [ShowInInspector, ReadOnly] private int resolutionHeight = 1080;
        [ShowInInspector, ReadOnly] private int resolutionWidth = 1920;
        [ShowInInspector, ReadOnly] private int frameRate = 60;

        [Title("Start and end handling")]
        public float recordingStartTimeInSeconds = 0;
        [FormerlySerializedAs("maxSeconds")]
        [FormerlySerializedAs("maxTime")] public float endTimeInSeconds = 0;
        public AudioClip endSound;
        
        [Title("Saving options")]
        [FormerlySerializedAs("avoidDuplicatedFrames")]
        public bool omitRepeatingFrames = false;
        public bool fillGapsInPreviousTake = false;
        public string frameOutDir;

        private bool waitingForEndProcess = false;
        private static string defaultOutDir => Path.Combine(Directory.GetCurrentDirectory(), "..");
        private string destinationDirectory;
        private byte[] lastFrame;
        
        // Speed tracking
        private System.Diagnostics.Stopwatch renderIntervalStopwatch = new();
        private long lastElapsedTime = 0;
        private List<long> renderIntervals = new();
        private List<long> readPixelsTimes = new();
        private List<long> renderTimes = new();
        private List<long> encodeToPNGTimes = new();
        private List<long> writeAllBytesTimes = new();
        private List<long> textureCreationTimes = new();
        private List<long> renderTextureCreationTimes = new();

        [Button(ButtonSizes.Large)]
        public void OpenDestinationDirectory()
        {
            Process.Start(GetScenePath());
        }

        private void Update()
        {
            if (waitingForEndProcess || !PrimerTimeline.isPlaying)
                return;

            framesWatched++;

            if (framesWatched < recordingStartTimeInSeconds * frameRate)
                return;

            
            var path = Path.Combine(destinationDirectory, $"{framesWatched:000000}.png");
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
            ApplyPreset();
            
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
            
            PrintTimeLogs();

            UnityEditor.EditorApplication.ExitPlaymode();
        }

        [Button]
        public void PrintTimeLogs()
        {
            double avgRenderInterval = renderIntervals.Average();
            double stdDevRenderInterval = Math.Sqrt(renderIntervals.Average(v => Math.Pow(v - avgRenderInterval, 2)));

            

            double avgTextureCreationTime = textureCreationTimes.Average();
            double stdDevTextureCreationTime = Math.Sqrt(textureCreationTimes.Average(v => Math.Pow(v - avgTextureCreationTime, 2)));

            double avgRenderTextureCreationTime = renderTextureCreationTimes.Average();
            double stdDevRenderTextureCreationTime = Math.Sqrt(renderTextureCreationTimes.Average(v => Math.Pow(v - avgRenderTextureCreationTime, 2)));

            double avgRenderTime = renderTimes.Average();
            double stdDevRenderTime = Math.Sqrt(renderTimes.Average(v => Math.Pow(v - avgRenderTime, 2)));

            double avgReadPixelsTime = readPixelsTimes.Average();
            double stdDevReadPixelsTime = Math.Sqrt(readPixelsTimes.Average(v => Math.Pow(v - avgReadPixelsTime, 2)));
            
            double avgEncodeTime = encodeToPNGTimes.Average();
            double stdDevEncodeTime = Math.Sqrt(encodeToPNGTimes.Average(v => Math.Pow(v - avgEncodeTime, 2)));
        
            double avgWriteTime = writeAllBytesTimes.Average();
            double stdDevWriteTime = Math.Sqrt(writeAllBytesTimes.Average(v => Math.Pow(v - avgWriteTime, 2)));
            
            var toPrint = $"Average Time between RenderToPNG calls: {avgRenderInterval}ms,\nStandard Deviation: {stdDevRenderInterval}ms\n\n";
            toPrint += $"Average Texture Creation time: {avgTextureCreationTime}ms,\nStandard Deviation: {stdDevTextureCreationTime}ms\n\n";
            toPrint += $"Average RenderTexture Creation time: {avgRenderTextureCreationTime}ms,\nStandard Deviation: {stdDevRenderTextureCreationTime}ms\n\n";
            toPrint += $"Average Render time: {avgRenderTime}ms,\nStandard Deviation: {stdDevRenderTime}ms\n\n";
            toPrint += $"Average ReadPixels time: {avgReadPixelsTime}ms,\nStandard Deviation: {stdDevReadPixelsTime}ms\n\n";
            toPrint += $"Average EncodeToPNG time: {avgEncodeTime}ms,\nStandard Deviation: {stdDevEncodeTime}ms\n\n";
            toPrint += $"Average WriteAllBytes time: {avgWriteTime}ms,\nStandard Deviation: {stdDevWriteTime}ms";
            
            Debug.Log(toPrint);
        }

        // private readonly List<string> createdDirs = new();
        private string GetContainerDirectory()
        {
            var scenePath = GetScenePath();
            var presetName = qualityPreset.ToString();

            var currentDirectoryPath = Path.Combine(scenePath, $"Current {presetName} take");

            if (Directory.Exists(currentDirectoryPath) && !fillGapsInPreviousTake)
            {
                // Find the next available numbered directory name
                var takeCount = 0;
                var takesString = $"{presetName} take 1";
                var takePath = Path.Combine(scenePath, takesString);
                while (Directory.Exists(takePath)) {
                    takeCount++;
                    takesString = $"{presetName} take {takeCount + 1}";
                    takePath = Path.Combine(scenePath, takesString);
                }
                
                // Move the previous take to the numbered directory name
                Directory.Move(currentDirectoryPath, takePath);
            }
            
            Directory.CreateDirectory(currentDirectoryPath);

            return currentDirectoryPath;
        }

        private string GetScenePath()
        {
            var outDir = string.IsNullOrWhiteSpace(frameOutDir)
                ? defaultOutDir
                : frameOutDir;

            var dirname = $"{SceneManager.GetActiveScene().name}_recordings";
            var scenePath = Path.Combine(outDir, "png", dirname);

            if (!Directory.Exists(scenePath))
            {
                Directory.CreateDirectory(scenePath);
            }
            
            return scenePath;
        }

        internal void RenderToPNG(string path, int resWidth, int resHeight)
        {
            if (renderIntervalStopwatch.IsRunning)
            {
                long currentElapsedTime = renderIntervalStopwatch.ElapsedMilliseconds;
                long interval = currentElapsedTime - lastElapsedTime;
                renderIntervals.Add(interval);
                lastElapsedTime = currentElapsedTime;
            }
            else
            {
                renderIntervalStopwatch.Start();
            }
            
            
            // If it's not new take, and the png exists already, skip
            if (File.Exists(path) && fillGapsInPreviousTake) return;
            
            // Set up camera
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var image = new Texture2D(resWidth, resHeight, TextureFormat.RGBA32, false);
            stopwatch.Stop();
            textureCreationTimes.Add(stopwatch.ElapsedMilliseconds);
            
            stopwatch.Restart();
            var rt = new RenderTexture(resWidth, resHeight, 24);
            stopwatch.Stop();
            renderTextureCreationTimes.Add(stopwatch.ElapsedMilliseconds);

            cam.targetTexture = rt;
            RenderTexture.active = rt;

            // Render to image texture
            stopwatch.Restart();
            cam.Render();
            stopwatch.Stop();
            renderTimes.Add(stopwatch.ElapsedMilliseconds);
            
            stopwatch.Restart();
            image.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            stopwatch.Stop();
            readPixelsTimes.Add(stopwatch.ElapsedMilliseconds);

            // Save png
            stopwatch.Restart();
            var bytes = image.EncodeToPNG();
            stopwatch.Stop();
            encodeToPNGTimes.Add(stopwatch.ElapsedMilliseconds);

            if (!omitRepeatingFrames || (lastFrame is null || !bytes.SequenceEqual(lastFrame))) {
                lastFrame = bytes;
                
                stopwatch.Restart();
                File.WriteAllBytes(path, bytes);
                stopwatch.Stop();
                writeAllBytesTimes.Add(stopwatch.ElapsedMilliseconds);
            }

            // Clean up
            cam.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            Destroy(image);
        }

        private bool ShouldEnd()
        {
            // If endTimeInSeconds is set, and we're past endTimeInSeconds 
            if (endTimeInSeconds > recordingStartTimeInSeconds && framesWatched > endTimeInSeconds * frameRate)
            {
                Debug.Log("Reached max frames. Exiting play mode.");
                return true;
            }
            
            var director = FindObjectOfType<PlayableDirector>();
            // If endTimeInSeconds is not set, and
            // If PlayableDirector is done (and exists)
            // +1 second of frames as a just-in-case buffer
            if (endTimeInSeconds <= recordingStartTimeInSeconds && director is not null && framesWatched > (director.duration + 1) * frameRate)
            {
                Debug.Log("Reached end of playable director. Exiting play mode.");
                return true;
            }

            // If > 1hr has been recorded
            // And endTimeInSeconds is not set ( to something > 1hr, for example )
            if (endTimeInSeconds <= recordingStartTimeInSeconds && framesWatched > frameRate * 60 * 60) {
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
                    frameRate = 60;
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
        Low,      // 720p, 60 fps
        High,     // 1080p, 60 fps
        Publish   // 4k, 60 fps
    }
}
