using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Debug = UnityEngine.Debug;

public class LatexToSvgConverter : IDisposable
{
    private readonly DirectoryInfo _temporaryDirectory;
    
    // Allows the last call to RenderLatexToSvg to be cancelled
    private CancellationTokenSource _latestCancellationTokenSource;

    public static LatexToSvgConverter Create()
    {
        return new LatexToSvgConverter(Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"unity-latex-{Guid.NewGuid().ToString()}")));
    }

    private LatexToSvgConverter(DirectoryInfo temporaryDirectory)
    {
        _temporaryDirectory = temporaryDirectory;
        Debug.Log($"Initialized LaTeX build directory: {_temporaryDirectory.FullName}");
    }

    public Task<string> RenderLatexToSvg(string latex)
    {
        if (_latestCancellationTokenSource is not null && !_latestCancellationTokenSource.IsCancellationRequested)
        {
            _latestCancellationTokenSource.Cancel();
        }

        var currentCancellationTokenSource = new CancellationTokenSource();
        _latestCancellationTokenSource = currentCancellationTokenSource;

        return Task.Run(() => RenderLatexToSvgSync(latex), currentCancellationTokenSource.Token);
    }

    private static void ExecuteProcess(int millisecondsTimeout, string name, IEnumerable<string> arguments)
    {
        var startInfo = new ProcessStartInfo(name)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.AddRange(arguments);

        using var process = new Process() { StartInfo = startInfo };

        process.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log($"stdout: {args.Data}");
        process.ErrorDataReceived += (sender, args) => UnityEngine.Debug.Log($"stderr: {args.Data}");
        
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit(millisecondsTimeout);

        if (!process.HasExited)
        {
            process.Kill();
            throw new TimeoutException($"Process {name} did not exit after {millisecondsTimeout}ms");
        }
    }
    
    public string RenderLatexToSvgSync(string latex)
    {
        var sourcePath = Path.Combine(_temporaryDirectory.FullName, "source.tex");
        File.WriteAllText(sourcePath, latex);
        
        ExecuteProcess(1000, "/Library/TeX/texbin/latex", new string[] {
            "-interaction=batchmode",
            "-halt-on-error",
            $"-output-directory={_temporaryDirectory.FullName}",
            sourcePath,
        });
        var dviPath = Path.Combine(_temporaryDirectory.FullName, "source.dvi");

        var outputPath = Path.Combine(_temporaryDirectory.FullName, "output.svg");
        ExecuteProcess(1000, "/Library/TeX/texbin/dvisvgm", new string[]
        {
            dviPath,
            "--no-fonts",
            $"--output={outputPath}",
        });

        return File.ReadAllText(outputPath);
    }

    private void ReleaseUnmanagedResources()
    {
        // void DeleteRecursively(DirectoryInfo directory)
        // {
        //     foreach (var subDirectory in directory.EnumerateDirectories())
        //     {
        //         DeleteRecursively(subDirectory);
        //     }
        //     
        //     directory.Delete(true);
        // }
        
        Task.Run(() =>
        {
            _temporaryDirectory.Delete(true);
        });
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~LatexToSvgConverter()
    {
        ReleaseUnmanagedResources();
    }
}