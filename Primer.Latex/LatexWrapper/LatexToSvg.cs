using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    internal class LatexToSvg
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

        public TempDir RootTempDir { get; }


        #region Creation/Destruction
        public LatexToSvg(TempDir rootTempDir) => RootTempDir = rootTempDir;
        #endregion


        public Task<string> RenderToSvg(LatexRenderConfig config, CancellationToken ct) {
            lock (executionLock) {
                if (currentTask is not null && !currentTask.IsCompleted) {
                    throw new Exception("A LaTeX rendering task is already running.");
                }

                return Task.Run(() => RenderToSvgSync(config, ct), ct);
            }
        }

        string RenderToSvgSync(LatexRenderConfig config, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var dir = RootTempDir.CreateChild("build-", autoIncrement: true);
            var sourcePath = dir.GetChildPath("source.tex");
            var outputPath = dir.GetChildPath("output.svg");

            ct.ThrowIfCancellationRequested();
            File.WriteAllText(sourcePath, @$"
                {string.Join(Environment.NewLine, config.Headers)}
                \begin{{document}}
                \color{{white}}
                {config.Latex}
                \end{{document}}
            ");

            ExecuteXelatex(dir, sourcePath, ct);
            ExecuteDvisvgm(dir, outputPath, ct);

            ct.ThrowIfCancellationRequested();
            return File.ReadAllText(outputPath);
        }

        void ExecuteXelatex(TempDir tmpDir, string sourcePath, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var args = xelatexArguments.Append($"-output-directory={tmpDir}", sourcePath);
            var result = LatexBinaries.Xelatex(tmpDir, args, ct);

            ct.ThrowIfCancellationRequested();
            DumpStandardOutputs(tmpDir, result, "xelatex");

            if (result.exitCode != 0) {
                var errors = LatexBinaries.GetXelatexErrorLogsFrom(tmpDir, "source.log") ?? result.stderr;
                throw new Exception($"Got xelatex error(s): {errors}");
            }
        }

        void ExecuteDvisvgm(TempDir workingDirectory, string outputPath, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var dviPath = workingDirectory.GetChildPath("source.xdv");
            var args = dvisvgmArguments.Append(dviPath, $"--output={outputPath}");

            var result = LatexBinaries.Dvisvgm(workingDirectory, args, ct);
            DumpStandardOutputs(workingDirectory, result, "dvisvgm");

            if (result.exitCode != 0) {
                throw new Exception($"Got dvisvgm error(s): {result.stderr}");
            }
        }

        static void DumpStandardOutputs(TempDir workingDirectory, CliProgram.ExecutionResult result, string name) {
            var stdout = workingDirectory.GetChildPath($"{name}.stdout");
            File.WriteAllText(stdout, result.stdout);

            var stderr = workingDirectory.GetChildPath($"{name}.stderr");
            File.WriteAllText(stderr, result.stderr);
        }
    }
}
