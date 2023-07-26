using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

public class FruitTree : MonoBehaviour
{
    public PrefabProvider fruitPrefab;
    public static float xAngleMax = 5f;
    public static float yAngleMax = 360f;
    public static float zAngleMax = 5f;

    [Title("Flowers")]
    public List<Transform> flowers = new();
    [Range(1, 4)] public int fruitCount = 4;

    public void Reset()
    {
        flowers.GetChildren().SetScale(0);
    }
    
    public Tween GrowFruit(int index, float delayRange = 0)
    {
        RandomlyRotateFlower(index);
        
        if (flowers[index].childCount == 0) {}
        
        var container = new Container(flowers[index]);
        var fruit = container.Add(fruitPrefab, options: new ChildOptions{zeroScale = true});
        return fruit.ScaleTo(1) with {delay = Rng.Range(delayRange)};
    }

    public Tween GrowRandomFruits(int number = 1, float delayRange = 0)
    {
        if (number > flowers.Count)
        {
            number = flowers.Count;
            Debug.LogWarning($"Cannot grow {number} fruit, only {flowers.Count} flowers available");
        }
        // Make sure not to grow the same fruit twice
        var indices = Enumerable.Range(0, flowers.Count).Shuffle().Take(number);
        
        return GrowSpecificFruits(indices.ToArray(), delayRange);
    }

    public Tween GrowSpecificFruits(int[] indices, float delayRange = 0)
    {
        // Create tweens, giving each a random delay between 0 and delayRange
        return indices
            .Select((index, i) => GrowFruit(index, delayRange))
            .RunInParallel();
    }

    private void RandomlyRotateFlower(int index)
    {
        flowers[index].localRotation = Quaternion.Euler(Rng.Range(xAngleMax), Rng.Range(yAngleMax), Rng.Range(zAngleMax));
    }

    public Transform HarvestFruit(Component closestTo = null)
    {
        var candidates = flowers.Where(x => x.childCount > 0);

        if (closestTo is not null) {
            var target = closestTo.transform.position;
            candidates = candidates.OrderBy(x => Vector3.Distance(x.position, target));
        }

        var flower = candidates.FirstOrDefault();
        var fruit = flower != null ? flower.GetChild(0) : null;

        if (fruit is null)
            return null;

        fruit.SetParent(null, worldPositionStays: true);
        return fruit;
    }
}
