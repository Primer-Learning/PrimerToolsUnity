using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VectorGraphics;
using UnityEngine;

[ExecuteInEditMode]
public class LatexRendererComponent : MonoBehaviour
{
    private void GenerateSprite()
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
    
    public string svgText;
    private string _lastRenderedSvgTextValue;

    public void Update()
    {
        if (_lastRenderedSvgTextValue != svgText)
        {
            GenerateSprite();
            _lastRenderedSvgTextValue = svgText;
        }
    }
}
