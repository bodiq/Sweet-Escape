using Items;
using Sirenix.Utilities;
using UnityEngine;

public class SprinkleSpawner : MonoBehaviour
{
    public static SprinkleSpawner Instance;

    private void Awake()
    {
        InitializeSingleton();
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
        if (!Sprinkle.PoolUsed.IsNullOrEmpty())
        {
            foreach (var sprinkleUsed in Sprinkle.PoolUsed)
            {
                if (sprinkleUsed)
                {
                    sprinkleUsed.gameObject.SetActive(true);
                }
            }
        }

        Sprinkle.PoolUsed.Clear();
    }

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}