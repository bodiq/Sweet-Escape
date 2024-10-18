using System;
using System.Collections.Generic;
using UnityEngine;

public class ManualObjectPool : MonoBehaviour
{
    public Transform slimeParent;
    public Transform coinParent;

    public static ManualObjectPool SharedInstance;

    public SlimeTrail slimeToPool;
    public int amountSlimesToPool;

    public Coin coinsToPool;
    public int amountCoinsToPool;

    public List<SlimeTrail> pooledSlimeTrailObjects;
    public List<Coin> pooledCoinsObjects;

    private int _lastCoinIndex;

    private void Awake()
    {
        SharedInstance = this;

        pooledSlimeTrailObjects = new List<SlimeTrail>();
        pooledCoinsObjects = new List<Coin>();

        for (var i = 0; i < amountSlimesToPool; i++)
        {
            var slimeTrail = Instantiate(slimeToPool, slimeParent);
            slimeTrail.Initialize();
            slimeTrail.gameObject.SetActive(false);
            pooledSlimeTrailObjects.Add(slimeTrail);
        }

        for (var i = 0; i < amountCoinsToPool; i++)
        {
            var coin = Instantiate(coinsToPool, coinParent);
            coin.gameObject.SetActive(false);
            coin.goldenSpoon = true;
            pooledCoinsObjects.Add(coin);
        }
    }

    public SlimeTrail GetPooledSlimeTrailObject()
    {
        for (var i = 0; i < amountSlimesToPool; i++)
        {
            if (!pooledSlimeTrailObjects[i].gameObject.activeInHierarchy)
            {
                return pooledSlimeTrailObjects[i];
            }
        }

        return null;
    }

    public Coin GetPooledCoinObject()
    {
        if (_lastCoinIndex == pooledCoinsObjects.Count - 1)
        {
            ResetCoinIndex();
            return null;
        }

        return pooledCoinsObjects[_lastCoinIndex++];
    }

    public void ResetCoinIndex()
    {
        _lastCoinIndex = 0;
    }
}