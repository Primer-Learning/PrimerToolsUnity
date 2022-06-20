using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Unity.VectorGraphics;
using UnityEngine;
using Debug = UnityEngine.Debug;

[ExecuteInEditMode]
public class LatexRendererComponent : MonoBehaviour
{
    private static DirectoryInfo CreateTempDirectory()
    {
        return Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"unity-latex-{Guid.NewGuid().ToString()}"));
    }

    private static async void RecursivelyDeleteDirectory(DirectoryInfo directory)
    {
        await Task.Factory.StartNew(() => directory.Delete(true));
    }

    private async void GenerateDvi()
    {
        DirectoryInfo temporaryDirectory = CreateTempDirectory();
        Debug.Log($"LaTeX build directory: {temporaryDirectory.FullName}");

        var sourcePath = Path.Combine(temporaryDirectory.FullName, "source.tex");
        await using (var sourceFile = new FileStream(sourcePath, FileMode.CreateNew))
        {
            await using (var writer = new StreamWriter(sourceFile))
            {
                await writer.WriteAsync(latex);
            }
        };

        try
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo("/Library/TeX/texbin/latex")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    ArgumentList =
                    {
                        "-interaction=batchmode",
                        "-halt-on-error",
                        $"-output-directory={temporaryDirectory.FullName}",
                        sourcePath,
                    }
                }
            };

            using (process)
            {
                process.Start();
                
                process.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log($"stdout: {args.Data}");
                process.ErrorDataReceived += (sender, args) => UnityEngine.Debug.Log($"stderr: {args.Data}");

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                // TODO: asyncify
                process.WaitForExit();
            }
        }
        finally
        {
            //RecursivelyDeleteDirectory(temporaryDirectory);
        }
    }
    
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

    public async void Update()
    {
        if (latex != _lastRenderedLatex)
        {
            GenerateDvi();
            _lastRenderedLatex = latex;
        }
    }
}
