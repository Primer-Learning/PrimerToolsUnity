namespace Primer.Timeline
{
    internal class SequenceClip : PrimerClip
    {
        protected override PrimerPlayable template { get; } = new SequencePlayable();

        public override string clipName => $"[{template.clipIndex + 1}] {trackTransform?.name ?? "No sequence"}";
    }
}
