using System;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Simulation.Evolution
{
    public class Food : MonoBehaviour
    {
        public int amount;
        public bool hasMore => amount > 0;

        public void Initialize()
        {
            amount = 2;

            var terrain = transform.ParentComponent<Landscape>();
            var container = new Container(transform);

            container.rotation = Quaternion.Euler(0, Random.Range(0, 180), 0);

            for (var i = 0; i < amount; i++) {
                var food = container.AddPrimitive(PrimitiveType.Sphere);
                food.SetScale(0.3);
                food.GetComponent<MeshRenderer>().SetColor(Color.green);
                var position = transform.TransformPoint(i == 0 ? 0.3f : -0.3f, 0, 0);
                food.position = terrain.GetGroundAt(position) + Vector3.up * 0.15f;
            }

            container.Purge();
        }

        public UniTask Consume()
        {
            if (transform.childCount is 0)
                throw new System.Exception("Food has no more items to consume.");

            amount--;

            if (transform.childCount is 1)
                return transform.ShrinkAndDispose();

            var sphere = transform.GetChild(0);
            sphere.SetParent(null, worldPositionStays: true);
            return sphere.ShrinkAndDispose();
        }
    }
}
