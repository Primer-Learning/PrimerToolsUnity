using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Primer.Latex
{
    internal static class LatexBinaries
    {
        internal static string latexBinDir;
        internal static string xelatex;
        internal static string dvisvgm;


        public static CliProgram.ExecutionResult Xelatex(string cwd, string[] args, CancellationToken ct) {
            var program = GetCliProgram(xelatex, "xelatex");
            return program.Execute(cwd, args, ct);
        }

        public static CliProgram.ExecutionResult Dvisvgm(string cwd, string[] args, CancellationToken ct) {
            var program = GetCliProgram(dvisvgm, "dvisvgm");
            return program.Execute(cwd, args, ct);
        }

        public static string GetXelatexErrorLogsFrom(string workingDirectory, string filename) {
            var logFile = Path.Combine(workingDirectory, filename);
            if (!File.Exists(logFile)) return null;

            var errors =
                from line in File.ReadAllLines(logFile)
                where line.StartsWith("! ") && line.Length > 2
                select line[2..];

            return string.Join(", ", errors);
        }


        static CliProgram GetCliProgram(string setting, string filename) {
            var path = GetBinary(setting, filename);
            if (string.IsNullOrWhiteSpace(path)) return null;

            var binDir = string.IsNullOrWhiteSpace(latexBinDir)
                ? Path.GetDirectoryName(path)
                : latexBinDir;

            var cli = new CliProgram(path);
            cli.EnvVars["PATH"] = binDir;
            return cli;
        }

        static string GetBinary(string setting, string filename) {
            if (!string.IsNullOrWhiteSpace(setting)) {
                return setting;
            }

            if (!string.IsNullOrWhiteSpace(latexBinDir)) {
                var found = FindBinary(latexBinDir, filename);
                if (found is not null) return found;
            }

            foreach (var path in PathEnvVar()) {
                var found = FindBinary(path, filename);
                if (found is not null) return found;
            }

            throw new FileNotFoundException($"Could not find {filename} in your PATH. You can configure it in Unity editor preferences.");
        }

        static string FindBinary(string path, string filename) {
            var plain = Path.Combine(path, filename);
            if (File.Exists(plain)) return plain;

            var exe = $"{plain}.exe";
            return File.Exists(exe) ? exe : null;
        }

        static IEnumerable<string> PathEnvVar() {
            var delimiter = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":";
            var path = Environment.GetEnvironmentVariable("PATH");
            return path?.Split(delimiter) ?? new string[] { };
        }
    }
}
