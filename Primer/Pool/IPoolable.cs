namespace Primer
{
    public interface IPoolable
    {
        void OnReuse();
        void OnRecycle();
    }
}
