namespace Primer
{
    public record ChildOptions
    {
        public bool worldPositionStays { get; init; } = false;
        public bool ignoreSiblingOrder { get; init; } = false;
        public uint? siblingIndex { get; init; } = 0;
        public bool enable { get; init; } = true;
    }
}
