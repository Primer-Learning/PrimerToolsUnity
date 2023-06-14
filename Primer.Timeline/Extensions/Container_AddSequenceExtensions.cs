using UnityEngine;

namespace Primer.Timeline
{
    public static class Container_AddSequenceExtensions
    {
        public static SequenceRunner AddSequence<TSequence>(this Container container, string name = null,
            bool worldPositionStays = false)
            where TSequence : Sequence
        {
            var sequence = container.Add<TSequence>(name, worldPositionStays);
            return new SequenceRunner(sequence);
        }

        /// <summary>
        ///   This method requires two generics
        ///   1. The T in Container&lt;T&gt;
        ///   2. The type of the Sequence we want to create
        /// </summary>
        /// <example>
        ///   var myContainer = new Container&lt;MeshRenderer&gt;();
        ///   myContainer.AddSequence&lt;MeshRenderer, MySequence&gt;();
        /// </example>
        public static SequenceRunner AddSequence<TComponent, TSequence>(this Container<TComponent> container,
            string name = null, bool worldPositionStays = false)
            where TComponent : Component
            where TSequence : Sequence
        {
            var sequence = container.Add<TSequence>(name, worldPositionStays);
            return new SequenceRunner(sequence);
        }
    }
}
