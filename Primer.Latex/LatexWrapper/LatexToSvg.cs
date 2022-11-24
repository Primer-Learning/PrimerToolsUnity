using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    internal class LatexToSvg : IDisposable
    {
        static readonly string[] xelatexArguments = new [] {
            "-no-pdf",
            "-interaction=batchmode",
            "-halt-on-error"
        };

        static readonly string[] dvisvgmArguments = new [] {
            "--no-fonts=1"
        };

        readonly object executionLock = new();
        Task<string> currentTask;
        int nextBuildNumber;

        public DirectoryInfo RootTempDir { get; }


        #region Creation/Destruction
        public LatexToSvg(DirectoryInfo rootTempDir) => RootTempDir = rootTempDir;
        ~LatexToSvg() => ReleaseUnmanagedResources();

        public void Dispose() {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        void ReleaseUnmanagedResources() {
            void DeleteRecursively(DirectoryInfo directory) {
                foreach (var subDirectory in directory.EnumerateDirectories())
                    DeleteRecursively(subDirectory);
                directory.Delete(true);
            }

            DeleteRecursively(RootTempDir);
        }
        #endregion


        public Task<string> RenderToSvg(LatexRenderConfig config, CancellationToken ct) {
            lock (executionLock) {
                if (currentTask is not null && !currentTask.IsCompleted) {
                    throw new Exception("A LaTeX rendering task is already running.");
                }

                currentTask = Task.Run(() => RenderToSvgSync(config.Latex, config.Headers, ct), ct);
                return currentTask;
            }
        }

        string RenderToSvgSync(string latex, IEnumerable<string> headers, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var buildNumber = Interlocked.Increment(ref nextBuildNumber) - 1;
            var dir = RootTempDir.CreateSubdirectory($"build-{buildNumber}").FullName;

            var sourcePath = Path.Combine(dir, "source.tex");
            var outputPath = Path.Combine(dir, "output.svg");

            ct.ThrowIfCancellationRequested();
            File.WriteAllText(sourcePath, @$"
                {string.Join(Environment.NewLine, headers)}
                \begin{{document}}
                \color{{white}}
                {latex}
                \end{{document}}
            ");

            ExecuteXelatex(dir, sourcePath, ct);
            ExecuteDvisvgm(dir, outputPath, ct);

            ct.ThrowIfCancellationRequested();
            return File.ReadAllText(outputPath);
        }

        void ExecuteXelatex(string workingDirectory, string sourcePath, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var args = xelatexArguments.Append($"-output-directory={workingDirectory}", sourcePath);
            var result = LatexBinaries.Xelatex(workingDirectory, args, ct);

            ct.ThrowIfCancellationRequested();
            DumpStandardOutputs(workingDirectory, result, "xelatex");

            if (result.exitCode != 0) {
                var errors = LatexBinaries.GetXelatexErrorLogsFrom(workingDirectory, "source.log") ?? result.stderr;
                throw new Exception($"Got xelatex error(s): {errors}");
            }
        }

        void ExecuteDvisvgm(string workingDirectory, string outputPath, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var dviPath = Path.Combine(workingDirectory, "source.xdv");
            var args = dvisvgmArguments.Append(dviPath, $"--output={outputPath}");

            var result = LatexBinaries.Dvisvgm(workingDirectory, args, ct);
            DumpStandardOutputs(workingDirectory, result, "dvisvgm");

            if (result.exitCode != 0) {
                throw new Exception($"Got dvisvgm error(s): {result.stderr}");
            }
        }

        static void DumpStandardOutputs(string workingDirectory, CliProgram.ExecutionResult result, string name) {
            var stdout = Path.Combine(workingDirectory, $"{name}.stdout");
            File.WriteAllText(stdout, result.stdout);

            var stderr = Path.Combine(workingDirectory, $"{name}.stderr");
            File.WriteAllText(stderr, result.stderr);
        }
    }
}
