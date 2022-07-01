using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("Primer.LatexRenderer.Editor")]

namespace LatexRenderer
{
    [ExecuteInEditMode]
    [SelectionBase]
    public class LatexRendererComponent : MonoBehaviour
    {
        private const float SvgPixelsPerUnit = 10f;

        private const HideFlags SvgPartsHideFlags = HideFlags.NotEditable;

        [TextArea] public string latex;

        [Tooltip(@"These will be inserted into the LaTeX template before \begin{document}.")]
        public List<string> headers = new()
        {
            @"\documentclass[preview]{standalone}",
            @"\usepackage[english]{babel}",
            @"\usepackage[utf8]{inputenc}",
            @"\usepackage[T1]{fontenc}",
            @"\usepackage{amsmath}",
            @"\usepackage{amssymb}",
            @"\usepackage{dsfont}",
            @"\usepackage{setspace}",
            @"\usepackage{tipa}",
            @"\usepackage{relsize}",
            @"\usepackage{textcomp}",
            @"\usepackage{mathrsfs}",
            @"\usepackage{calligra}",
            @"\usepackage{wasysym}",
            @"\usepackage{ragged2e}",
            @"\usepackage{physics}",
            @"\usepackage{xcolor}",
            @"\usepackage{microtype}",
            @"\usepackage{pifont}",
            @"\linespread{1}"
        };

        [SerializeField] [HideInInspector] private Build _currentBuild;

        public Material material;

        private readonly LatexToSvgConverter _converter = LatexToSvgConverter.Create();

        private readonly SpriteDirectRenderer _renderer = new();

#if UNITY_EDITOR
        private void Reset()
        {
            material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        }
#endif

        public async void Update()
        {
            var currentKey = new Build.Key(this);
            if (_currentBuild?.Source != currentKey) _currentBuild = new Build(currentKey);

            if (_currentBuild.LatexToSvgTask is null)
            {
                _currentBuild.LatexToSvgTask = _converter.RenderLatexToSvg(latex, headers);
                _currentBuild.Svg = await _currentBuild.LatexToSvgTask;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    EditorApplication.QueuePlayerLoopUpdate();
#endif
            }
            else if (_currentBuild.Svg is not null && !_currentBuild.DidCreateSvgParts)
            {
                // This must be done within the player update loop, so it's important that this isn't run after any
                // await calls in this   function. If it's done outside of it, there will be an error when creating the
                // sprite.
                var sprites = BuildSprites(_currentBuild.Svg);
                _renderer.SetSprites(sprites.Select(i => (i.Item2, (Vector3)i.Item1)), material,
                    false);
                _currentBuild.DidCreateSvgParts = true;
            }

            _renderer.Draw(transform);
        }

        public (bool isRunning, AggregateException exception) GetTaskStatus()
        {
            if (_currentBuild?.LatexToSvgTask is null) return (false, null);

            return (!_currentBuild.LatexToSvgTask.IsCompleted,
                _currentBuild.LatexToSvgTask.Exception);
        }

        private static List<(Vector2, Sprite)> BuildSprites(string svgText)
        {
            SVGParser.SceneInfo sceneInfo;
            try
            {
                sceneInfo = SVGParser.ImportSVG(new StringReader(svgText));
            }
            catch (Exception e)
            {
                Debug.LogError($"Invalid SVG, got error: {e}");
                return null;
            }

            var allGeometry = VectorUtils.TessellateScene(sceneInfo.Scene,
                new VectorUtils.TessellationOptions
                {
                    StepDistance = 100.0f,
                    MaxCordDeviation = 0.5f,
                    MaxTanAngleDeviation = 0.1f,
                    SamplingStepSize = 0.01f
                });

            var scaledBounds = VectorUtils.Bounds(from geometry in allGeometry
                from vertex in geometry.Vertices
                select geometry.WorldTransform * vertex / SvgPixelsPerUnit);

            // Holds an (offset, sprite) for each shape in the SVG
            var sprites = new List<(Vector2, Sprite)>(allGeometry.Count);

            foreach (var geometry in allGeometry)
            {
                var offset = VectorUtils.Bounds(from vertex in geometry.Vertices
                    select geometry.WorldTransform * vertex / SvgPixelsPerUnit).min;

                // This matches the way flipYAxis would work in BuildSprite if we gave it all of the geometry in the SVG
                // rather than just one at a time.
                offset.y = scaledBounds.height - offset.y;

                sprites.Add((offset,
                    VectorUtils.BuildSprite(new List<VectorUtils.Geometry> { geometry },
                        SvgPixelsPerUnit, VectorUtils.Alignment.TopLeft, Vector2.zero, 128, true)));
            }

            return sprites;
        }

        // private void CreateSvgParts(List<(Vector2, Sprite)> sprites)
        // {
        //     var partNumber = 0;
        //     foreach (var (offset, sprite) in sprites)
        //     {
        //         var obj = new GameObject($"SvgPart {partNumber++}");
        //
        //         var renderer = obj.AddComponent<SpriteRenderer>();
        //         renderer.sprite = sprite;
        //         if (Material)
        //             renderer.material = Material;
        //
        //         obj.transform.parent = gameObject.transform;
        //         obj.transform.localPosition = offset;
        //
        //         obj.hideFlags = SvgPartsHideFlags;
        //
        //         _svgParts.Add(obj);
        //     }
        // }

        [Serializable]
        private class Build
        {
            public string Svg;
            public bool DidCreateSvgParts;

            [NonSerialized] public Task<string> LatexToSvgTask;

            public Key Source;

            public Build(Key source)
            {
                Source = source;
            }

            public class Key
            {
                public readonly List<string> Headers;
                public readonly string Latex;

                public Key(LatexRendererComponent source)
                {
                    Latex = source.latex;
                    Headers = new List<string>(source.headers);
                }

                public static bool operator ==(Key a, Key b)
                {
                    if (a is null) return b is null;

                    return a.Equals(b);
                }

                public static bool operator !=(Key a, Key b)
                {
                    return !(a == b);
                }

                public override bool Equals(object obj)
                {
                    return Equals(obj as Key);
                }

                public bool Equals(Key other)
                {
                    return Latex == other.Latex && Headers.SequenceEqual(other.Headers);
                }
            }
        }

#if UNITY_EDITOR
        // This needs to be private (or internal) because SpriteDirectRenderer is internal
        [Tooltip(
            "Which mesh features to visualize. Gizmos are only ever visible in the Unity editor.")]
        [SerializeField]
        private SpriteDirectRenderer.GizmoMode gizmos = SpriteDirectRenderer.GizmoMode.Nothing;

        private void OnDrawGizmos()
        {
            _renderer.DrawWireGizmos(transform, gizmos);
        }
#endif
    }
}