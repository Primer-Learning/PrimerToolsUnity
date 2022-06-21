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
    private LatexToSvgConverter _converter = LatexToSvgConverter.Create();

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
        if (_svg != _lastRenderedSvg)
        {
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
                //
            }
        }
    }
}
