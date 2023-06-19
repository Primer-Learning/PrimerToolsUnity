using UnityEngine;

namespace Primer
{
    public partial class Container
    {
        public Container AddContainer(string name)
        {
            return Add(name).ToContainer(connectToParent: true);
        }

        public Container<TChild> AddContainer<TChild>(string name) where TChild : Component
        {
            return Add<TChild>(name).ToContainer(connectToParent: true);
        }
    }
}
