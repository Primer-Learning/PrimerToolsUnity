using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class LatexToSvgConverter : IDisposable
{
    private readonly DirectoryInfo _temporaryDirectoryRoot;
    private readonly string _templateText;
    
    // Allows the last call to RenderLatexToSvg to be cancelled
    private CancellationTokenSource _latestCancellationTokenSource;

    public static LatexToSvgConverter Create(string templateText)
    {
        return new LatexToSvgConverter(templateText, Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"unity-latex-{Guid.NewGuid().ToString()}")));
    }

    private LatexToSvgConverter(string templateText, DirectoryInfo temporaryDirectoryRoot)
    {
        _templateText = templateText;
        _temporaryDirectoryRoot = temporaryDirectoryRoot;
        Debug.Log($"Initialized LaTeX build directory: {_temporaryDirectoryRoot.FullName}");
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

    private static void ExecuteProcess(int millisecondsTimeout, DirectoryInfo workingDirectory, string name,
        IEnumerable<string> arguments)
    {
        var startInfo = new ProcessStartInfo(name)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory.FullName,
        };
        startInfo.ArgumentList.AddRange(arguments);

        using var process = new Process() { StartInfo = startInfo };

        process.Start();
        process.WaitForExit(millisecondsTimeout);

        var wasKilled = false;
        if (!process.HasExited)
        {
            process.Kill();
            wasKilled = true;
        }

        using (var stdoutLogFile = File.Open(Path.Combine(workingDirectory.FullName, "stdout.txt"), FileMode.Append))
        {
            process.StandardOutput.BaseStream.CopyTo(stdoutLogFile);
        }

        using (var stderrLogFile = File.Open(Path.Combine(workingDirectory.FullName, "stderr.txt"), FileMode.Append))
        {
            process.StandardError.BaseStream.CopyTo(stderrLogFile);
        }

        if (wasKilled)
        {
            throw new TimeoutException($"Process {name} did not exit after {millisecondsTimeout}ms");
        }
    }

    public string RenderLatexToSvgSync(string latex)
    {
        var _temporaryDirectory = _temporaryDirectoryRoot.CreateSubdirectory(Guid.NewGuid().ToString());
        
        var sourcePath = Path.Combine(_temporaryDirectory.FullName, "source.tex");
        File.WriteAllText(sourcePath, _templateText.Replace("[tex_expression]", latex));
        
        ExecuteProcess(1000, _temporaryDirectory, "/Library/TeX/texbin/latex", new string[] {
            "-interaction=batchmode",
            "-halt-on-error",
            $"-output-directory={_temporaryDirectory.FullName}",
            sourcePath,
        });
        var dviPath = Path.Combine(_temporaryDirectory.FullName, "source.dvi");

        var outputPath = Path.Combine(_temporaryDirectory.FullName, "output.svg");
        ExecuteProcess(1000, _temporaryDirectory, "/Library/TeX/texbin/dvisvgm", new string[]
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