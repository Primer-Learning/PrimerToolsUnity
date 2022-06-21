using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[ExecuteInEditMode]
public class LatexRendererComponent : MonoBehaviour
{
    private LatexToSvgConverter _converter;

    private void GenerateSprite(string svgText)
    {
        var tessOptions = new VectorUtils.TessellationOptions()
        {
            StepDistance = 100.0f,
            MaxCordDeviation = 0.5f,
            MaxTanAngleDeviation = 0.1f,
            SamplingStepSize = 0.01f
        };
    
        // Dynamically import the SVG data, and tessellate the resulting vector scene.
        SVGParser.SceneInfo sceneInfo;
        try
        {
            sceneInfo = SVGParser.ImportSVG(new StringReader(svgText));
        }
        catch (System.Exception e)
        {
            if (svgText != "")
            {
                Debug.LogError($"Invalid SVG, got error: {e.ToString()}");
            }

            GetComponent<SpriteRenderer>().sprite = null;
            return;
        }

        var geoms = VectorUtils.TessellateScene(sceneInfo.Scene, tessOptions);
        
        // Build a sprite with the tessellated geometry.
        var sprite = VectorUtils.BuildSprite(geoms, 10.0f, VectorUtils.Alignment.Center, Vector2.zero, 128, true);
        GetComponent<SpriteRenderer>().sprite = sprite;
    }
    
    public string latex;
    private string _lastRenderedLatex;

    private string _svg;
    private string _lastRenderedSvg;

    public async void Update()
    {
        // TODO: This can be moved to awake() once the implementation is a bit more stable. It's here because
        //       awake() doesn't get called when code is reloaded. 
        _converter ??= LatexToSvgConverter.Create(Resources.Load<TextAsset>("tex_template").text);
        
        if (_svg != _lastRenderedSvg)
        {
            // This must be done within the player update loops, so it's important that this is before any await calls
            // in this function. If it's done outside of it, there will be an error when creating the sprite.
            GenerateSprite(_svg);
            _lastRenderedSvg = _svg;
        }
        
        if (latex != _lastRenderedLatex)
        {
            try
            {
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
}
