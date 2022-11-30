using System;
using System.IO;

namespace Primer
{
    public class TempDir : IDisposable
    {
        readonly DirectoryInfo info;
        int childrenCount = 0;
        readonly bool isRoot;
        bool exists = false;

        public string FullPath => info.FullName;

        public TempDir() {
            var tempPath = Path.GetTempPath();
            var tmpDir = Path.Combine(tempPath, $"unity-primer-{Guid.NewGuid()}");
            info = Directory.CreateDirectory(tmpDir);
            isRoot = true;
            exists = true;
        }

        TempDir(DirectoryInfo info) {
            this.info = info;
            isRoot = false;
            exists = true;
        }

        ~TempDir() => CleanUpResources();
        public void Dispose() => CleanUpResources();

        public string GetChildPath(string filename) =>
            Path.Combine(info.FullName, filename);

        public TempDir CreateChild(string name, bool autoIncrement = false) {
            if (autoIncrement) {
                name += childrenCount++;
            }
            else if (string.IsNullOrWhiteSpace(name)) {
                name = $"{childrenCount++}";
            }

            var child = info.CreateSubdirectory(name);
            return new TempDir(child);
        }

        public override string ToString() => info.FullName;

        void CleanUpResources() {
            if (!exists || !isRoot) return;

            void DeleteRecursively(DirectoryInfo directory) {
                foreach (var subDirectory in directory.EnumerateDirectories())
                    DeleteRecursively(subDirectory);
                directory.Delete(true);
            }

            DeleteRecursively(info);
            exists = false;
        }
    }
}
