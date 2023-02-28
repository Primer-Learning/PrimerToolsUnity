using System.Linq;
using Cysharp.Threading.Tasks;

namespace Primer.Latex
{
    public static class LatexCache
    {
        public static void Disable()
        {
            LatexProcessingCache.disableCache = true;
        }

        public static void Enable()
        {
            LatexProcessingCache.disableCache = false;
        }

        public static async UniTask Preload(params string[] inputs)
        {
            var headers = LatexInput.GetDefaultHeaders();
            await UniTask.WhenAll(inputs.Select(x => Preload(new LatexInput(x, headers))));
        }

        public static async UniTask Preload(LatexInput input)
        {
            await LatexProcessor.GetInstance().Process(input);
        }
    }
}
