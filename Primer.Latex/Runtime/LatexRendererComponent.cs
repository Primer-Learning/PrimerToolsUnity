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

        [SerializeField] [HideInInspector] internal List<GameObject> _svgParts = new();

        private LatexToSvgConverter _converter;

        public void Start()
        {
            // HACK: When undoing a "Release SVG Parts" operation, the hideFlags are not restored. Unfortunately I can't
            // figure out why, so we make sure they're properly set here.
            foreach (var part in _svgParts) part.hideFlags = SvgPartsHideFlags;
        }

        public async void Update()
        {
            var currentKey = new Build.Key(this);
            if (_currentBuild?.Source != currentKey) _currentBuild = new Build(currentKey);

            if (_currentBuild.LatexToSvgTask is null)
            {
                _currentBuild.LatexToSvgTask = _converter.RenderLatexToSvg(latex, headers);
                _currentBuild.Svg = await _currentBuild.LatexToSvgTask;

#if UNITY_EDITOR
                EditorApplication.QueuePlayerLoopUpdate();
#endif
            }
            else if (_currentBuild.Svg is not null && !_currentBuild.DidCreateSvgParts)
            {
                // This must be done within the player update loop, so it's important that this isn't run after any
                // await calls in this function. If it's done outside of it, there will be an error when creating the
                // sprite.
                CreateSvgParts(BuildSprites(_currentBuild.Svg));
                _currentBuild.DidCreateSvgParts = true;
            }
        }

        public void OnEnable()
        {
            _converter ??= LatexToSvgConverter.Create();

            foreach (var part in _svgParts) part.SetActive(true);
        }

        public void OnDisable()
        {
            _converter = null;

            foreach (var part in _svgParts) part.SetActive(false);
        }

        public void OnDestroy()
        {
            var wasDeletedInEditor = Application.isEditor && gameObject.scene.isLoaded;
            if (wasDeletedInEditor) DestroySvgParts(true);
        }

        private void DestroySvgParts(bool undoable = false)
        {
            foreach (var part in _svgParts)
            {
#if UNITY_EDITOR
                if (Application.isEditor)
                {
                    if (undoable)
                        Undo.DestroyObjectImmediate(part);
                    else
                        DestroyImmediate(part);

                    continue;
                }
#endif

                Destroy(part);
            }

            _svgParts = new List<GameObject>();
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

        private void CreateSvgParts(List<(Vector2, Sprite)> sprites)
        {
            DestroySvgParts();

            var partNumber = 0;
            foreach (var (offset, sprite) in sprites)
            {
                var obj = new GameObject($"SvgPart {partNumber++}");

                obj.AddComponent<SpriteRenderer>().sprite = sprite;

                obj.transform.parent = gameObject.transform;
                obj.transform.localPosition = offset;

                obj.hideFlags = SvgPartsHideFlags;

                _svgParts.Add(obj);
            }
        }

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
    }
}