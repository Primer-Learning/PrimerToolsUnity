namespace Primer.Tools
{
    public partial class PrimerArrow2 : IPoolable
    {
        public static readonly IPool<PrimerArrow2> pool = new PrefabPool<PrimerArrow2>("Arrow");

        public void OnReuse()
        {
            // nothing
        }

        public void OnRecycle()
        {
            SetDefaults();
        }
    }
}
