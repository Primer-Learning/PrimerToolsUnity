using UnityEngine;

namespace Primer
{
    public partial class Gnome
    {
        public Gnome AddContainer(string name, ChildOptions options = null)
        {
            return Add(name, options).ToContainer(connectToParent: true);
        }

        public Gnome<TChild> AddContainer<TChild>(string name, ChildOptions options = null) where TChild : Component
        {
            return Add<TChild>(name, options).ToContainer(connectToParent: true);
        }
    }
}
