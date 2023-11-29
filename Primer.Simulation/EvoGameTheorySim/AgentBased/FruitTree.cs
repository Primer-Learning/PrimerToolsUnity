using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class FruitTree : MonoBehaviour
{
    public Transform fruitPrefab;
    public static float xAngleMax = 5f;
    public static float yAngleMax = 360f;
    public static float zAngleMax = 5f;
    public Rng rng;

    [HideInInspector] public bool skipAnimations = false;
    private static Pool mangoPool => Pool.GetPool(CommonPrefabs.Mango);
    
    public bool hasFruit => flowers.Any(x => x.childCount > 0);
    public Transform[] fruits => flowers.Where(x => x.childCount > 0 && x.GetChild(0).GetChild(0).localScale.x > 0).Select(x => x.GetChild(0)).ToArray();
    public Transform[] highFruits => highFlowers.Where(x => x.childCount > 0 && x.GetChild(0).GetChild(0).localScale.x > 0).Select(x => x.GetChild(0)).ToArray();
    
    [Title("Flowers")]
    public List<Transform> flowers;
    public List<Transform> highFlowers;

    public void Reset()
    {
        flowers.GetChildren().Dispose();
        highFlowers.GetChildren().Dispose();
    }
    
    public Tween GrowFruit(int index)
    {
        return GrowFruit(flowers[index]);
    }
    public Tween GrowFruit(Transform flower)
    {
        Transform fruit;
        
        var noFruit = flower.childCount == 0;
        var shrunkenFruit = flower.childCount > 0 && flower.GetChild(0).GetChild(0).localScale.x < 1;
        
        if (noFruit || shrunkenFruit)
        {
            RandomlyRotateFlower(flower);
            
            fruit = noFruit ? mangoPool.GiveToParent(flower) : flower.GetChild(0);
            
            // Set the transform of the fruit's child to match the prefab
            // We need to do this because the child of the prefab may have been moved by physics.
            // Because I corrected the pivot point by nesting the fruit under an empty game object.
            // Which I don't think I should do going forward.
            var actualFruit = fruit.GetChild(0);
            actualFruit.GetComponent<Rigidbody>().isKinematic = true;
            actualFruit.localRotation =
                new Quaternion(-0.10410507f, -0.00516443793f, -0.0492771529f, 0.993331432f);
            actualFruit.localPosition = new Vector3(-0.012f, -0.444f, 0.006f);
            actualFruit.localScale = Vector3.one;
            fruit.localScale = Vector3.zero;
        }
        else
        {
            fruit = flower.GetChild(0);
        }

        return fruit.ScaleTo(1) with {duration = skipAnimations ? 0 : 0.5f};
    }
    public Tween GrowRandomFruitsUpToTotal(int total, float delayRange = 0)
    {
        if (total > flowers.Count)
        {
            total = flowers.Count;
            Debug.LogWarning($"Cannot grow {total} fruit, only {flowers.Count} flowers available");
        }

        // Get indices where there is already a fruit
        var existingFruitIndices = Enumerable.Range(0, flowers.Count)
            .Where(i => flowers[i].childCount > 0).ToArray();
        
        // Choose random indices where there's not already a fruit
        var newFruitIndices = Enumerable.Range(0, flowers.Count)
            .Where(i => flowers[i].childCount == 0)
            .Shuffle(rng: rng)
            .Take(total - existingFruitIndices.Length);
        
        return GrowSpecificFruits(newFruitIndices.Concat(existingFruitIndices).ToArray(), delayRange);
    }

    public Tween GrowSpecificFruits(int[] indices, float delayRange = 0)
    {
        // Create tweens, giving each a random delay between 0 and delayRange
        return indices
            .Select(index => GrowFruit(index) with {delay = skipAnimations ? 0 : rng.RangeFloat(delayRange)})
            .RunInParallel();
    }
    public Tween GrowHighFruits(float delayRange = 0)
    {
        return highFlowers
            .Select(x => GrowFruit(x) with {delay = skipAnimations ? 0 : rng.RangeFloat(delayRange)})
            .RunInParallel();
    }

    private void RandomlyRotateFlower(Transform flower)
    {
        flower.localRotation = Quaternion.Euler(rng.RangeFloat(xAngleMax), rng.RangeFloat(yAngleMax), rng.RangeFloat(zAngleMax));
    }
    public Transform DetachFruit(Transform fruit)
    {
        // fruit.SetParent(transform, worldPositionStays: true);
        var actualFruit = fruit.GetChild(0);
        actualFruit.GetComponent<Rigidbody>().isKinematic = false;

        return actualFruit;
    }
}
