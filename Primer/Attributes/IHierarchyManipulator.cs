namespace Primer
{
    /// <summary>
    ///  This interface will add the relevant attributes to the implementations of the method in it.
    /// </summary>
    public interface IHierarchyManipulator
    {
        public void UpdateChildren();
        public void RegenerateChildren();
    }
}
