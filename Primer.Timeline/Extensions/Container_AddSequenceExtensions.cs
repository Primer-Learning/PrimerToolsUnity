using UnityEngine;

namespace Primer.Timeline
{
    public static class Container_AddSequenceExtensions
    {
        public static SequenceRunner AddSequence<TSequence>(this Gnome gnome, string name = null,
            ChildOptions options = null)
            where TSequence : Sequence
        {
            var sequence = gnome.Add<TSequence>(name, options);
            return new SequenceRunner(sequence);
        }

        /// <summary>
        ///   This method requires two generics
        ///   1. The T in Gnome&lt;T&gt;
        ///   2. The type of the Sequence we want to create
        /// </summary>
        /// <example>
        ///   var myContainer = new Gnome&lt;MeshRenderer&gt;();
        ///   myContainer.AddSequence&lt;MeshRenderer, MySequence&gt;();
        /// </example>
        public static SequenceRunner AddSequence<TComponent, TSequence>(this Gnome<TComponent> gnome,
            string name = null, ChildOptions options = null)
            where TComponent : Component
            where TSequence : Sequence
        {
            var sequence = gnome.Add<TSequence>(name, options);
            return new SequenceRunner(sequence);
        }
    }
}
