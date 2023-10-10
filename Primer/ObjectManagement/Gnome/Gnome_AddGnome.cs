using UnityEngine;

namespace Primer
{
    // This part defines the methods to create children and wrap them into Gnomes
    public partial class Gnome
    {
        public Gnome AddGnome(string name, ChildOptions options = null)
        {
            return Add(name, options).ToGnome(connectToParent: true);
        }

        public Gnome<TChild> AddGnome<TChild>(string name, ChildOptions options = null) where TChild : Component
        {
            return Add<TChild>(name, options).ToGnome(connectToParent: true);
        }
    }
}
