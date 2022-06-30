using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBucket : PrimerObject 
{
    [SerializeField] GameObject coinPrefab = null;
    [SerializeField] GameObject bucketPrefab = null;
    PrimerObject bucket = null;

    public List<Coin> coins = new List<Coin>();

    Vector3 coinOrigin = new Vector3(0, 10, 0);

    void Start() {
        AddBucket();
    }

    void AddBucket(bool transparent = false) {
        if (bucketPrefab == null) {
            bucket = GameObject.CreatePrimitive(PrimitiveType.Plane).MakePrimerObject();
        }
        else {
            bucket = Instantiate(bucketPrefab).MakePrimerObject();
        }
        bucket.transform.parent = transform;
        bucket.transform.localPosition = Vector3.zero;
        bucket.transform.localScale = Vector3.one;
        if (transparent) {
            bucket.FadeOut(duration: 0);
        }
    }
    public void AddCoins(int numToAdd) {
        StartCoroutine(addCoins(numToAdd));
    }
    IEnumerator addCoins(int numToAdd) {
        List<Coin> newCoins = new List<Coin>();
        float diameter = 4;
        int rotOffset = Director.sceneRandom2.Next(360);
        for (int i = 0; i < numToAdd; i++)
        {
            Coin c = Instantiate(coinPrefab).AddComponent<Coin>();
            c.GetComponent<Rigidbody>().isKinematic = true;
            c.value = 1;
            newCoins.Add(c);
            c.transform.parent = transform;
            c.transform.localScale = Vector3.one;
            Vector3 disp = Quaternion.Euler(0, rotOffset + i * 360 / numToAdd, 0) * (diameter * Vector3.right);
            c.transform.localPosition = coinOrigin + disp;
            c.transform.localRotation = Quaternion.Euler(10, rotOffset + i * 360 / numToAdd, 0);
            c.SetIntrinsicScale();
            c.GetComponent<Rigidbody>().velocity = Vector3.zero;
            c.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            c.ScaleUpFromZero();
        }
        yield return new WaitForSeconds(0.5f);
        foreach (Coin c in newCoins)
        {
            c.GetComponent<Rigidbody>().isKinematic = false;
        }
        // A delay before the coins are usable by other processes, so they can drop
        yield return new WaitForSeconds(0.5f);
        foreach (Coin c in newCoins)
        {
            coins.Add(c);
        }
    }

    public List<Coin> PluckCoins(int num = 1, bool animate = true) {
        List<Coin> removed = new List<Coin>();
        for (int i = 0; i < num; i++)
        {
            Coin c = coins[Director.sceneRandom2.Next(coins.Count)];
            coins.Remove(c);
            removed.Add(c);
        }
        if (animate) { StartCoroutine(pluckCoinAnimation(removed)); }
        return removed;
    }
    IEnumerator pluckCoinAnimation(List<Coin> coins) {
        if (coins.Count == 1) {
            coins[0].GetComponent<Rigidbody>().isKinematic = true;
            coins[0].MoveTo(coinOrigin);
            coins[0].RotateTo(Quaternion.Euler(0, 180, 0));
        }
        else {
            float diameter = 4;
            int rotOffset = Director.sceneRandom2.Next(360);
            for (int i = 0; i < coins.Count; i++)
            {   
                Vector3 disp = Quaternion.Euler(0, rotOffset + i * 360 / coins.Count, 0) * (diameter * Vector3.right);
                coins[i].GetComponent<Rigidbody>().isKinematic = true;
                coins[i].MoveTo(coinOrigin + disp);
                coins[i].RotateTo(Quaternion.Euler(0, rotOffset + i * 360 / coins.Count, 0));
            }
        }
        yield return new WaitForSeconds(0.5f);
        foreach (Coin c in coins)
        {
            c.Disappear();
        }
    }
}
