namespace Primer.Latex
{
    public interface ILatexCharProvider
    {
        bool isReady { get; }
        LatexChar[] characters { get; }
    }
}
