using UnityEngine;

namespace Primer
{
    public partial class Container<TComponent>
    {
        public Container AddContainer(string name)
        {
            return Add(name).ToContainer();
        }

        public Container<TChild> AddContainer<TChild>(string name) where TChild : Component
        {
            return Add<TChild>(name).ToContainer();
        }
    }
}
