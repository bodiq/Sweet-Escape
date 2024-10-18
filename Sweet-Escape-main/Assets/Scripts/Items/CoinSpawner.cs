using Sirenix.Utilities;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public static CoinSpawner Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        GameManager.Instance.OnPlayerRespawn += ResetAll;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPlayerRespawn -= ResetAll;
    }

    private void ResetAll()
    {
        if (!Coin.PoolUsed.IsNullOrEmpty())
        {
            foreach (var coinUsed in Coin.PoolUsed)
            {
                if (coinUsed)
                {
                    coinUsed.SetActive(true);
                }
            }
        }

        Coin.PoolUsed.Clear();
    }
}