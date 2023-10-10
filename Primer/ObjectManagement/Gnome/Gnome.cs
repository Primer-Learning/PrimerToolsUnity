using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 *  This is the methaphor.
 *
 *  GameObjects are expensive
 *  We create GOs them when we need them
 *  and disable GOs them when we don't
 *  we can also .Purge() to get rid of unused GameObjects
 *                 ^ this removes child GameObjects in hierarchy
 *
 *  So we ask Gnomes for GameObjects
 *  "Hey Gnome, gimme a GameObject with PrimerComponent, called POTATO"
 *
 *      new Gnome<PrimerComponent>("POTATO");
 *
 *  The gnome will not delete the object after it's done.
 *  It'll disable it at the end of the current function if you use `using` keyword.
 *  (or you dispose them manually at any point)
 *
 *      using var myGnome = new Gnome<PrimerComponent>("POTATO");
 *
 *  If there was already a "POTATO" GameObject it will take it and enable it.
 *  If there is none the Gnome will create it.
 *  From then on, you can also use it to ensure the children are what you expect.
 *
 *      myGnome.Add<PrimerComponent>("POTATO's child")
 *
 *  Or create more gnomes to manage the children too
 *
 *      using var childGnome = myGnome.AddGnome<PrimerComponent>("Another child")
 *      // or
 *      using var childGnome = myGnome.Add<PrimerComponent>("Another child").ToGnome()
 */

namespace Primer
{
    /// <summary>
    ///     Wrap a Unity component into a Gnome to manipulate it's children.
    ///     The gnome will look at the children of the component and keep track of them.
    ///     When you add a child, it will look for an unused child and use that if it exists.
    ///     After you're doing you can use .Purge() to remove all unused children.
    /// </summary>
    /// <remarks>
    ///     The idea behind this class is to be able to define the children of a GameObject in code again and again
    ///     without having to re-create them each time.
    /// </remarks>
    public partial class Gnome : IDisposable, IPrimer
    {
        public Transform transform { get; }
        public Component component { get; }

        public Gnome(string name, Component parent = null, bool setActive = true)
        {
            transform = parent is null
                ? GetRootTransform(name)
                : GetDirectChild(parent.transform, name);

            component = transform;

            if (setActive)
                transform.SetActive(true);

            Reset();
        }

        public Gnome(Transform component, bool setActive = true)
        {
            this.component = component;
            transform = component;

            if (setActive)
                transform.SetActive(true);

            Reset();
        }

        protected Gnome(Component component, bool setActive = true)
        {
            this.component = component;
            transform = component.transform;

            if (setActive)
                transform.SetActive(true);

            Reset();
        }


        public void Reset()
        {
            created.Clear();
            usedChildren.Clear();
            unusedChildren.Clear();
            unusedChildren.AddRange(ReadExistingChildren(transform));
        }

        private static Transform GetRootTransform(string name)
        {
            var scene = SceneManager.GetActiveScene();
            var rootGameObjects = scene.GetRootGameObjects();
            var found = rootGameObjects.FirstOrDefault(x => x.name == name);
            var obj = found != null ? found : new GameObject(name);
            return obj.transform;
        }

        private static Transform GetDirectChild(Transform parent, string name)
        {
            var found = parent.Find(name);

            if (found)
                return found;

            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        public static implicit operator Transform(Gnome gnome) => gnome.transform;
    }
}
