using System;
using UnityEngine;

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


    [Serializable]
    public abstract class Scrubbable : StrategyPattern, IBoundScrubbable<Transform>
    {
        public Transform target { get; set; }

        public virtual void Prepare() {}

        public virtual void Cleanup() {}

        public abstract void Update(float t);
    }
}
