namespace Primer
{
    /// <summary>Options to be passed to Gnome when creating a child.</summary>
    public record ChildOptions
    {
        public bool worldPositionStays { get; init; } = false;
        public bool ignoreSiblingOrder { get; init; } = false;
        public uint? siblingIndex { get; init; } = null;
        public bool enable { get; init; } = true;
        public bool zeroScale { get; init; } = false;
    }
}
