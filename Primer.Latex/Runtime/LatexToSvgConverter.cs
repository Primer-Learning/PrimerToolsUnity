using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Codice.CM.SemanticMerge.Gui;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace LatexRenderer
{
    public class LatexToSvgConverter : IDisposable
    {
        public static string LatexExecutablePath;
        public static string DvisvgmExecutablePath;

        private readonly DirectoryInfo _temporaryDirectoryRoot;
        private readonly string _templateText;

        private const int Timeout = 10 * 1000;

        private static string FindInPath(string name)
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var pathParts = Environment.GetEnvironmentVariable("PATH")?.Split(isWindows ? ";" : ":");
            if (pathParts is null)
            {
                return null;
            }

            foreach (var directory in pathParts)
            {
                var plain = Path.Combine(directory, name);
                if (File.Exists(plain))
                {
                    return plain;
                }

                var exe = $"{plain}.exe";
                if (File.Exists(exe))
                {
                    return exe;
                }
            }

            return null;
        }

        private static string FindLatexExecutablePath()
        {
            if (LatexExecutablePath is not null && LatexExecutablePath.Length > 0)
            {
                return LatexExecutablePath;
            }
            else
            {
                var found = FindInPath("latex");
                if (found is null)
                {
                    throw new FileNotFoundException(
                        "Could not find latex in your PATH. Add it or add the location in your Unity editor preferences.");
                }

                return found;
            }
        }

        private static string FindDvisvgmExecutablePath()
        {
            if (DvisvgmExecutablePath is not null && DvisvgmExecutablePath.Length > 0)
            {
                return DvisvgmExecutablePath;
            }
            else
            {
                var found = FindInPath("dvisvgm");
                if (found is null)
                {
                    throw new FileNotFoundException(
                        "Could not find dvisvgm in your PATH. Add it or add the location in your Unity editor preferences.");
                }

                return found;
            }
        }

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
            return Task.Run(() => RenderLatexToSvgSync(latex));
        }

        private static int ExecuteProcess(int millisecondsTimeout, DirectoryInfo workingDirectory, string name,
            IEnumerable<string> arguments)
        {
            var startInfo = new ProcessStartInfo(name)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory.FullName,
                CreateNoWindow = true,
            };

            foreach (var arg in arguments)
            {
                startInfo.ArgumentList.Add(arg);
            }

            using var process = new Process() { StartInfo = startInfo };

            process.Start();
            process.WaitForExit(millisecondsTimeout);

            var wasKilled = false;
            if (!process.HasExited)
            {
                process.Kill();
                wasKilled = true;
            }

            using (var stdoutLogFile =
                   File.Open(Path.Combine(workingDirectory.FullName, "stdout.txt"), FileMode.Append))
            {
                process.StandardOutput.BaseStream.CopyTo(stdoutLogFile);
            }

            using (var stderrLogFile =
                   File.Open(Path.Combine(workingDirectory.FullName, "stderr.txt"), FileMode.Append))
            {
                process.StandardError.BaseStream.CopyTo(stderrLogFile);
            }

            if (wasKilled)
            {
                throw new TimeoutException($"Process {name} did not exit after {millisecondsTimeout}ms");
            }

            return process.ExitCode;
        }

        private string RenderLatexToSvgSync(string latex)
        {
            var temporaryDirectory = _temporaryDirectoryRoot.CreateSubdirectory(Guid.NewGuid().ToString());

            var sourcePath = Path.Combine(temporaryDirectory.FullName, "source.tex");
            File.WriteAllText(sourcePath, _templateText.Replace("[tex_expression]", latex));

            if (ExecuteProcess(Timeout, temporaryDirectory, FindLatexExecutablePath(), new string[]
                {
                    "-interaction=batchmode",
                    "-halt-on-error",
                    $"-output-directory={temporaryDirectory.FullName}",
                    sourcePath,
                }) != 0)
            {
                var errors =
                    from line in File.ReadAllLines(Path.Combine(temporaryDirectory.FullName, "source.log"))
                    where line.StartsWith("! ") && line.Length > 2
                    select line[2..];
                throw new Exception($"Got LaTeX error(s): {string.Join(", ", errors)}");
            }

            var dviPath = Path.Combine(temporaryDirectory.FullName, "source.dvi");

            var outputPath = Path.Combine(temporaryDirectory.FullName, "output.svg");
            ExecuteProcess(Timeout, temporaryDirectory, FindDvisvgmExecutablePath(), new string[]
            {
                dviPath,
                "--no-fonts",
                $"--output={outputPath}",
            });

            return File.ReadAllText(outputPath);
        }

        private void ReleaseUnmanagedResources()
        {
            void DeleteRecursively(DirectoryInfo directory)
            {
                foreach (var subDirectory in directory.EnumerateDirectories())
                {
                    DeleteRecursively(subDirectory);
                }

                directory.Delete(true);
            }

            DeleteRecursively(_temporaryDirectoryRoot);
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
}