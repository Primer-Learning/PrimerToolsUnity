using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Primer.Latex
{
    internal class LatexProcessingCache : ProcessingPipeline
    {
        public LatexProcessingCache(ILatexProcessor innerProcessor) : base(innerProcessor) {}

        public override Task<LatexExpression> Process(LatexInput config, CancellationToken cancellationToken = default)
        {
            var cached = GetFromCache(config);

            if (cached is not null)
                return cached;

            var task = processor.Process(config, cancellationToken);
            SaveToCache(config, task);
            return task;
        }


        private static Task<LatexExpression> GetFromCache(LatexInput config)
        {
            var fromMemory = GetFromMemory(config);

            if (fromMemory is not null) {
                Debug.Log($"{config.code} found in memory!");
                return fromMemory;
            }

            var fromDisk = ReadFromDisk(config);

            if (fromDisk is not null) {
                Debug.Log($"{config.code} found in disk!");
                return Task.FromResult(fromDisk);
            }

            return null;
        }


        private static async void SaveToCache(LatexInput config, Task<LatexExpression> task)
        {
            // We save the task to memory so that if the same request is made again
            // while the task is still running, we can return the same task.
            SaveToMemory(config, task);
            SaveToDisk(config, await task);
            RemoveFromMemory(config);
        }


        #region Memory cache
        private static readonly Dictionary<int, Task<LatexExpression>> memoryCache = new();

        private static Task<LatexExpression> GetFromMemory(LatexInput config)
        {
            var hash = config.GetHashCode();
            return memoryCache.ContainsKey(hash) ? memoryCache[hash] : null;
        }

        private static void SaveToMemory(LatexInput config, Task<LatexExpression> task)
        {
            memoryCache.Add(config.GetHashCode(), task);
        }

        private static void RemoveFromMemory(LatexInput config)
        {
            memoryCache.Remove(config.GetHashCode());
        }
        #endregion


        #region Disk cache
        private static string cacheDir => Path.Join(Application.persistentDataPath, "latex-cache");

        private static string GetCachePath(LatexInput config)
        {
            Debug.Log($"{config.code} - {config.GetHashCode()}");
            var filename = string.Join("_", config.code.Split(Path.GetInvalidFileNameChars()));
            return Path.Join(cacheDir, $"{filename}_{config.GetHashCode()}.bin");
        }

        private static void SaveToDisk(LatexInput config, LatexExpression chars)
        {
            var serializable = chars.Select(x => new SerializableLatexChar(x)).ToArray();

            Directory.CreateDirectory(cacheDir);
            Debug.Log($"{config.code} saved to disk");

            var bf = new BinaryFormatter();
            var fs = new FileStream(GetCachePath(config), FileMode.Create);

            bf.Serialize(fs, serializable);
            fs.Close();
        }

        private static LatexExpression ReadFromDisk(LatexInput config)
        {
            var path = GetCachePath(config);

            if (!File.Exists(path))
                return null;

            Debug.Log($"{config.code} found in disk!");
            var bf = new BinaryFormatter();
            var fs = new FileStream(path, FileMode.Open);
            var serialized = (SerializableLatexChar[])bf.Deserialize(fs);
            fs.Close();

            var chars = serialized.Select(x => x.ToLatexChar()).ToArray();
            return new LatexExpression(chars);
        }

        [Serializable]
        private struct SerializableLatexChar
        {
            public readonly (float x, float y, float z) position;
            public readonly (float x1, float y1, float x2, float y2) bounds;
            public readonly float scale;
            public readonly SerializableMesh mesh;

            public SerializableLatexChar(LatexChar latexChar)
            {
                mesh = new SerializableMesh(latexChar.mesh);
                scale = latexChar.scale;
                position = (
                    latexChar.position.x,
                    latexChar.position.y,
                    latexChar.position.z
                );
                bounds = (
                    latexChar.symbol.bounds.min.x,
                    latexChar.symbol.bounds.min.y,
                    latexChar.symbol.bounds.max.x,
                    latexChar.symbol.bounds.max.y
                );
            }

            public LatexChar ToLatexChar()
            {
                var rect = new Rect(bounds.x1, bounds.y1, bounds.x2, bounds.y2);

                return new LatexChar(
                    new LatexSymbol(mesh.GetMesh(), rect),
                    new Vector3(position.x, position.y, position.z),
                    scale
                );
            }
        }
        #endregion
    }
}
