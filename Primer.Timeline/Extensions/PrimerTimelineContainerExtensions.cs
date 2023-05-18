namespace Primer.Timeline
{
    public static class PrimerTimelineContainerExtensions
    {
        // Sadly this only works on Container, not on Container<T>
        public static SequenceRunner AddSequence<TSequence>(this Container container, string name = null,
            bool worldPositionStays = false)
            where TSequence : Sequence
        {
            var sequence = container.Add<TSequence>(name, worldPositionStays);
            return new SequenceRunner(sequence);
        }
    }
}
