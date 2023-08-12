using System;
using System.Linq;
using System.Reflection;
using Primer;
using Primer.Scene;
using Primer.Timeline;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
///   Implement this interface to specify which scenes to record.
/// </summary>
public interface RecordAllScenes
{
    public string[] scenes { get; }

    [MenuItem("Primer/Record all scenes/Minimal Quality")]
    private static void RecordMinimalQuality() => Init(QualityPreset.Minimal);

    [MenuItem("Primer/Record all scenes/Low Quality")]
    private static void RecordLowQuality() => Init(QualityPreset.Low);

    [MenuItem("Primer/Record all scenes/High Quality")]
    private static void RecordHighQuality() => Init(QualityPreset.High);

    [MenuItem("Primer/Record all scenes/Publish Quality")]
    private static void RecordPublishQuality() => Init(QualityPreset.Publish);

    private static void Init(QualityPreset qualityPreset)
    {
        var recorder = new GameObject("Record all scenes")
            .AddComponent<RecordAllScenesComponent>();

        recorder.quality = qualityPreset;
        recorder.scenes = GetScenesToRecord();
        recorder.LoadNextScene();
    }

    private static string[] GetScenesToRecord()
    {
        var implementations = TypeCache.GetTypesDerivedFrom<RecordAllScenes>();

        if (implementations.Count is 0)
            throw new MissingMemberException($"Implement {nameof(RecordAllScenes)} to specify scenes to record.");

        if (implementations.Count > 1) {
            throw new AmbiguousMatchException(
                $"Multiple implementations of {nameof(RecordAllScenes)} found: "
                + string.Join(", ", implementations.Select(t => t.Name))
            );
        }

        var implementation = implementations.First();
        var scenesProp = implementation.GetProperty("scenes");
        var instance = Activator.CreateInstance(implementation);
        var scenes = (string[])scenesProp.GetValue(instance);

        if (scenes is null)
            throw new MissingFieldException("No scenes found");

        return scenes;
    }

    [ExecuteAlways]
    private class RecordAllScenesComponent : MonoBehaviour
    {
        public string[] scenes;
        public int sceneIndex;
        public QualityPreset quality;

        public void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        public void LoadNextScene()
        {
            if (sceneIndex >= scenes.Length) {
                this.Log("No more scenes to load.");
                this.Dispose();
                return;
            }

            var scene = scenes[sceneIndex];
            this.Log("Loading scene ", scene);
            sceneIndex++;

            // after we load the scene `this` is destroyed, the returned recorder is the new `this`
            var autoPlayer = LoadSceneAndRecreateMyself(scene);

            if (!PrimerTimeline.director) {
                autoPlayer.Log("No timeline in scene {scene} - skipping.");
                autoPlayer.LoadNextScene();
                return;
            }

            PrimerTimeline.EnterPlayMode();
        }

        private RecordAllScenesComponent LoadSceneAndRecreateMyself(string scene)
        {
            // ReSharper disable LocalVariableHidesMember
            var name = this.name;
            var scenes = this.scenes;
            var sceneIndex = this.sceneIndex;
            var quality = this.quality;
            // ReSharper restore LocalVariableHidesMember

            EditorSceneManager.OpenScene("Assets/" + scene + ".unity");

            var recorder = new GameObject("Record all scenes")
                .AddComponent<RecordAllScenesComponent>();

            recorder.name = name;
            recorder.scenes = scenes;
            recorder.sceneIndex = sceneIndex;
            recorder.quality = quality;

            return recorder;
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                LoadNextScene();
            else if (state == PlayModeStateChange.EnteredPlayMode)
                ConfigureRenderer();
        }

        private void ConfigureRenderer()
        {
            var pngRenderer = FindObjectOfType<RenderToPng>() ?? Camera.main.GetOrAddComponent<RenderToPng>();

            pngRenderer.enabled = true;
            pngRenderer.qualityPreset = quality;

            // These are all very likely for non-test renders
            pngRenderer.recordingStartTimeInSeconds = 0;
            pngRenderer.endTimeInSeconds = 0;
            pngRenderer.transparentBackground = true;
            pngRenderer.omitRepeatingFrames = false;
            pngRenderer.fillGapsInPreviousTake = false;

            if (sceneIndex != scenes.Length - 1)
                pngRenderer.endSound = null;
        }
    }
}
