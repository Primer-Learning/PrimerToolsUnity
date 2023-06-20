using UnityEngine;

namespace Primer
{
    public partial class Container
    {
        public Container AddContainer(string name, ChildOptions options = null)
        {
            return Add(name, options).ToContainer(connectToParent: true);
        }

        public Container<TChild> AddContainer<TChild>(string name, ChildOptions options = null) where TChild : Component
        {
            return Add<TChild>(name, options).ToContainer(connectToParent: true);
        }
    }
}
