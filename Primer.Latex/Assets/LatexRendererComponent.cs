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
    private static async Task ExecuteProcess(string name, IEnumerable<string> arguments)
    {
        var startInfo = new ProcessStartInfo(name)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.AddRange(arguments);

        var process = new Process()
        {
            StartInfo = startInfo
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
    
    private static DirectoryInfo CreateTempDirectory()
    {
        return Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"unity-latex-{Guid.NewGuid().ToString()}"));
    }

    private static async Task RecursivelyDeleteDirectory(DirectoryInfo directory)
    {
        await Task.Factory.StartNew(() => directory.Delete(true));
    }

    private async Task<string> GenerateSvg()
    {
        DirectoryInfo temporaryDirectory = CreateTempDirectory();
        Debug.Log($"LaTeX build directory: {temporaryDirectory.FullName}");

        var sourcePath = Path.Combine(temporaryDirectory.FullName, "source.tex");
        await File.WriteAllTextAsync(sourcePath, latex);

        try
        {
            await ExecuteProcess("/Library/TeX/texbin/latex", new string[] {
                "-interaction=batchmode",
                "-halt-on-error",
                $"-output-directory={temporaryDirectory.FullName}",
                sourcePath,
            });
            var dviPath = Path.Combine(temporaryDirectory.FullName, "source.dvi");

            var outputPath = Path.Combine(temporaryDirectory.FullName, "output.svg");
            await ExecuteProcess("/Library/TeX/texbin/dvisvgm", new string[]
            {
                dviPath,
                "--no-fonts",
                $"--output={outputPath}",
            });

            return await File.ReadAllTextAsync(outputPath);
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
            _svg = await GenerateSvg();
            _lastRenderedLatex = latex;
            
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
