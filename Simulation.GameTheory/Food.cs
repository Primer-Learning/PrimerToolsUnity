using Primer;
using Primer.Simulation;
using UnityEngine;

namespace Simulation.GameTheory
{
    public class Food : LandscapeItem
    {
        public int amount;
        public bool hasMore => amount > 0;

        public void Initialize(Rng rng)
        {
            amount = 2;

            var t = transform;
            var sim = t.ParentComponent<ISimulation>();
            var container = new Gnome(t);

            container.rotation = Quaternion.Euler(0, rng.Range(0, 180), 0);

            for (var i = 0; i < amount; i++) {
                var food = container.AddPrimitive(PrimitiveType.Sphere);
                food.SetScale(0.3);
                food.GetComponent<MeshRenderer>().SetColor(Color.green);
                var position = t.TransformPoint(i == 0 ? 0.3f : -0.3f, 0, 0);
                // food.position = sim.GetGroundAt(position) + Vector3.up * 0.15f;
            }

            container.Purge();
        }

        public Transform Consume()
        {
            if (transform.childCount is 0)
                throw new System.Exception("Food has no more items to consume.");

            amount--;

            if (transform.childCount is 1) {
                return transform;
            }

            var sphere = transform.GetChild(0);
            sphere.SetParent(null, worldPositionStays: true);
            return sphere;
        }
    }
}
