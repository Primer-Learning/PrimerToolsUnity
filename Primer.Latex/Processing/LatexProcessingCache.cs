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
        public static bool disableCache = false;

        public LatexProcessingCache(ILatexProcessor innerProcessor) : base(innerProcessor) {}

        public override Task<LatexExpression> Process(LatexInput config, CancellationToken cancellationToken = default)
        {
            if (disableCache)
                return processor.Process(config, cancellationToken);

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

            if (fromMemory is not null)
                return fromMemory;

            var fromDisk = ReadFromDisk(config);

            if (fromDisk is not null)
                return Task.FromResult(fromDisk);

            return null;
        }


        private static async void SaveToCache(LatexInput config, Task<LatexExpression> task)
        {
            SaveToMemory(config, task);
            SaveToDisk(config, await task);
        }


        #region Memory cache
        private static readonly Dictionary<int, Task<LatexExpression>> memoryCache = new();

        private static Task<LatexExpression> GetFromMemory(LatexInput config)
        {
            var hash = config.GetDeterministicHashCode();
            return memoryCache.ContainsKey(hash) ? memoryCache[hash] : null;
        }

        private static void SaveToMemory(LatexInput config, Task<LatexExpression> task)
        {
            memoryCache.Add(config.GetDeterministicHashCode(), task);
        }
        #endregion


        #region Disk cache
        private static string cacheDir => Path.Join(Application.persistentDataPath, "latex-cache");

        private static string GetCachePath(LatexInput config)
        {
            var filename = string.Join("_", config.code.Split(Path.GetInvalidFileNameChars()));
            return Path.Join(cacheDir, $"{filename}_{config.GetDeterministicHashCode()}.bin");
        }

        private static void SaveToDisk(LatexInput config, LatexExpression chars)
        {
            var serializable = chars.Select(x => new SerializableLatexChar(x)).ToArray();

            Directory.CreateDirectory(cacheDir);

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

            var bf = new BinaryFormatter();
            using var fs = new FileStream(path, FileMode.Open);
            var serialized = (SerializableLatexChar[]) bf.Deserialize(fs);
            fs.Close();

            var chars = serialized.Select(x => x.ToLatexChar());
            return new LatexExpression(config, chars.ToArray());
        }

        [Serializable]
        private struct SerializableLatexChar
        {
            public readonly (float x, float y) position;
            public readonly (float x, float y, float w, float h) bounds;
            public readonly SerializableMesh mesh;

            public SerializableLatexChar(LatexChar latexChar)
            {
                mesh = new SerializableMesh(latexChar.mesh);
                position = (latexChar.position.x, latexChar.position.y);
                bounds = (
                    x: latexChar.bounds.xMin,
                    y: latexChar.bounds.yMin,
                    w: latexChar.bounds.width,
                    h: latexChar.bounds.height
                );
            }

            public LatexChar ToLatexChar() => new(
                mesh.GetMesh(),
                new Rect(bounds.x, bounds.y, bounds.w, bounds.h),
                new Vector2(position.x, position.y)
            );
        }
        #endregion

#if UNITY_EDITOR
        public static void OpenCacheDir()
        {
            System.Diagnostics.Process.Start($"{cacheDir}{Path.DirectorySeparatorChar}");
        }

        public static void ClearCache()
        {
            new DirectoryInfo(cacheDir).Delete(true);
        }
#endif
    }
}
