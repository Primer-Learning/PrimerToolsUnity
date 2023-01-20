namespace Primer
{
    public interface IScrubbable
    {
        void Prepare();
        void Cleanup();
        void Update(float t);
    }


    public interface IBoundScrubbable<out T> : IScrubbable
    {
        T target { get; }
    }


    public abstract class Scrubbable<T> : IBoundScrubbable<T>
    {
        public T target { get; set; }

        public virtual void Prepare() {}

        public virtual void Cleanup() {}

        public abstract void Update(float t);
    }
}
