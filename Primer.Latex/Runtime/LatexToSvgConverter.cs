using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace LatexRenderer
{
    public class LatexToSvgConverter : IDisposable
    {
        private const int Timeout = 30 * 1000;

        /// <summary>Will be used instead of searching the PATH env variable.</summary>
        public static string LatexExecutablePath;

        /// <summary>Will be used instead of searching the PATH env variable.</summary>
        public static string DvisvgmExecutablePath;

        private readonly object _currentTaskLock = new();

        private readonly DirectoryInfo _temporaryDirectoryRoot;
        private Task<string> _currentTask;

        private LatexToSvgConverter(DirectoryInfo temporaryDirectoryRoot)
        {
            _temporaryDirectoryRoot = temporaryDirectoryRoot;
            Debug.Log($"Initialized LaTeX build directory: {_temporaryDirectoryRoot.FullName}");
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private static string PrependPath(IEnumerable<string> toAdd)
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var delimiter = isWindows ? ";" : ":";
            var pathParts = Environment.GetEnvironmentVariable("PATH")?.Split(delimiter) ??
                            new string[] { };
            return string.Join(delimiter, toAdd.Concat(pathParts));
        }

        private static string FindInPath(string name)
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var pathParts = Environment.GetEnvironmentVariable("PATH")
                ?.Split(isWindows ? ";" : ":");
            if (pathParts is null) return null;

            foreach (var directory in pathParts)
            {
                var plain = Path.Combine(directory, name);
                if (File.Exists(plain)) return plain;

                var exe = $"{plain}.exe";
                if (File.Exists(exe)) return exe;
            }

            return null;
        }

        private static string FindXelatexExecutablePath()
        {
            if (LatexExecutablePath is not null && LatexExecutablePath.Length > 0)
                return LatexExecutablePath;

            var found = FindInPath("xelatex");
            if (found is null)
                throw new FileNotFoundException(
                    "Could not find latex in your PATH. Add it or add the location in your Unity editor preferences.");

            return found;
        }

        private static string FindDvisvgmExecutablePath()
        {
            if (DvisvgmExecutablePath is not null && DvisvgmExecutablePath.Length > 0)
                return DvisvgmExecutablePath;

            var found = FindInPath("dvisvgm");
            if (found is null)
                throw new FileNotFoundException(
                    "Could not find dvisvgm in your PATH. Add it or add the location in your Unity editor preferences.");

            return found;
        }

        /// <summary>Creates a new converter with its own (also new) temporary directory.</summary>
        public static LatexToSvgConverter Create()
        {
            return new LatexToSvgConverter(Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"unity-latex-{Guid.NewGuid().ToString()}")));
        }

        public Task<string> RenderLatexToSvg(string latex, List<string> headers)
        {
            lock (_currentTaskLock)
            {
                if (_currentTask is not null && !_currentTask.IsCompleted)
                    throw new Exception("A LaTeX rendering task is already running.");

                _currentTask = Task.Run(() => RenderLatexToSvgSync(latex, headers));
                return _currentTask;
            }
        }

        private static int ExecuteProcess(int millisecondsTimeout, DirectoryInfo workingDirectory,
            string name, IEnumerable<string> arguments)
        {
            var startInfo = new ProcessStartInfo(name)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory.FullName,
                CreateNoWindow = true
            };

            startInfo.Environment.Add("PATH", PrependPath(new[] { Path.GetDirectoryName(name) }));

            foreach (var arg in arguments) startInfo.ArgumentList.Add(arg);

            using var process = new Process { StartInfo = startInfo };

            process.Start();
            process.WaitForExit(millisecondsTimeout);

            var wasKilled = false;
            if (!process.HasExited)
            {
                process.Kill();
                wasKilled = true;
            }

            using (var stdoutLogFile =
                   File.Open(Path.Combine(workingDirectory.FullName, "stdout.txt"),
                       FileMode.Append))
            {
                process.StandardOutput.BaseStream.CopyTo(stdoutLogFile);
            }

            using (var stderrLogFile =
                   File.Open(Path.Combine(workingDirectory.FullName, "stderr.txt"),
                       FileMode.Append))
            {
                process.StandardError.BaseStream.CopyTo(stderrLogFile);
            }

            if (wasKilled)
                throw new TimeoutException(
                    $"Process {name} did not exit after {millisecondsTimeout}ms");

            return process.ExitCode;
        }

        private string RenderLatexToSvgSync(string latex, List<string> headers)
        {
            var temporaryDirectory =
                _temporaryDirectoryRoot.CreateSubdirectory(Guid.NewGuid().ToString());

            var sourcePath = Path.Combine(temporaryDirectory.FullName, "source.tex");
            File.WriteAllText(sourcePath, @$"
            {string.Join(Environment.NewLine, headers)}
            \begin{{document}}
            \color{{white}}
            {latex}
            \end{{document}}
            ");

            if (ExecuteProcess(Timeout, temporaryDirectory, FindXelatexExecutablePath(),
                    new[]
                    {
                        "-no-pdf", "-interaction=batchmode", "-halt-on-error",
                        $"-output-directory={temporaryDirectory.FullName}", sourcePath
                    }) != 0)
            {
                var errors =
                    from line in File.ReadAllLines(Path.Combine(temporaryDirectory.FullName,
                        "source.log"))
                    where line.StartsWith("! ") && line.Length > 2
                    select line[2..];
                throw new Exception($"Got xelatex error(s): {string.Join(", ", errors)}");
            }

            var dviPath = Path.Combine(temporaryDirectory.FullName, "source.xdv");

            var outputPath = Path.Combine(temporaryDirectory.FullName, "output.svg");
            ExecuteProcess(Timeout, temporaryDirectory, FindDvisvgmExecutablePath(),
                new[] { dviPath, "--no-fonts=1", $"--output={outputPath}" });

            return File.ReadAllText(outputPath);
        }

        private void ReleaseUnmanagedResources()
        {
            void DeleteRecursively(DirectoryInfo directory)
            {
                foreach (var subDirectory in directory.EnumerateDirectories())
                    DeleteRecursively(subDirectory);

                directory.Delete(true);
            }

            DeleteRecursively(_temporaryDirectoryRoot);
        }

        ~LatexToSvgConverter()
        {
            ReleaseUnmanagedResources();
        }
    }
}