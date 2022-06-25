using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[ExecuteInEditMode]
[SelectionBase]
public class LatexRendererComponent : MonoBehaviour
{
    public string latex;
    private string _lastRenderedLatex;

    private LatexToSvgConverter _converter;

    private string _svg;
    private string _lastRenderedSvg;

    public List<GameObject> _svgParts = new List<GameObject>();

    public async void Update()
    {
        if (_svg != _lastRenderedSvg)
        {
            // This must be done within the player update loop, so it's important that this is before any await calls
            // in this function. If it's done outside of it, there will be an error when creating the sprite.
            CreateSvgParts(BuildSprites(_svg));
            _lastRenderedSvg = _svg;
        }

        if (latex != _lastRenderedLatex)
        {
            try
            {
                // TODO: This only needs to be in update when running in the editor. I'd like a better pattern for
                //       dealing with changed properties in the editor, but I haven't found one yet.
                _svg = await _converter.RenderLatexToSvg(latex);
                _lastRenderedLatex = latex;

                EditorApplication.QueuePlayerLoopUpdate();
            }
            catch (OperationCanceledException)
            {
                // This will happen when we've already started rendering a different LaTeX string
            }
        }
    }

    public void OnEnable()
    {
        _converter ??= LatexToSvgConverter.Create(Resources.Load<TextAsset>("tex_template").text);
    }

    public void OnDisable()
    {
        _converter = null;
        
        DestroySvgParts();
        _lastRenderedLatex = null;
        _lastRenderedSvg = null;
        _svg = null;
    }

    private void DestroySvgParts()
    {
        foreach (var part in _svgParts)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(part);
            }
            else
            {
                Destroy(part);
            }
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
        catch (System.Exception e)
        {
            Debug.LogError($"Invalid SVG, got error: {e.ToString()}");
            return null;
        }

        List<VectorUtils.Geometry> allGeometry = VectorUtils.TessellateScene(
            sceneInfo.Scene,
            new VectorUtils.TessellationOptions()
            {
                StepDistance = 100.0f,
                MaxCordDeviation = 0.5f,
                MaxTanAngleDeviation = 0.1f,
                SamplingStepSize = 0.01f
            });
        
        Rect scaledBounds = VectorUtils.Bounds(
            from geometry in allGeometry
            from vertex in geometry.Vertices
            select geometry.WorldTransform * vertex);
        
        // Holds an (offset, sprite) for each shape in the SVG 
        var sprites = new List<(Vector2, Sprite)>(allGeometry.Count);
        
        foreach (var geometry in allGeometry)
        {
            Vector2 offset = VectorUtils.Bounds(
                from vertex in geometry.Vertices select geometry.WorldTransform * vertex).min;

            // This matches the way flipYAxis would work in BuildSprite if we gave it all of the geometry in the SVG
            // rather than just one at a time.
            offset.y = scaledBounds.height - offset.y;
            
            sprites.Add((
                offset,
                VectorUtils.BuildSprite(
                    new List<VectorUtils.Geometry>() { geometry },
                    1f,
                    VectorUtils.Alignment.TopLeft,
                    Vector2.zero,
                    128,
                    true)));
        }

        return sprites;
    }

    private void CreateSvgParts(List<(Vector2, Sprite)> sprites)
    {
        DestroySvgParts();

        int partNumber = 0;
        foreach (var (offset, sprite) in sprites)
        {
            var obj = new GameObject($"SvgPart {partNumber++}");

            obj.AddComponent<SpriteRenderer>().sprite = sprite;
            
            obj.transform.parent = gameObject.transform;
            obj.transform.localPosition = (Vector3)offset;

            obj.hideFlags =
                HideFlags.DontSaveInBuild |
                HideFlags.DontSaveInEditor |
                HideFlags.NotEditable;
            
            _svgParts.Add(obj);
        }
    }
}