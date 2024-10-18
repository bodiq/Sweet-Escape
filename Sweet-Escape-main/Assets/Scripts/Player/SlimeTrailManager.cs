using UnityEngine;

public class SlimeTrailManager : MonoBehaviour
{
    public static SlimeTrailManager Instance;

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
        foreach (var slimeTrail in ManualObjectPool.SharedInstance.pooledSlimeTrailObjects)
        {
            if (slimeTrail.gameObject.activeSelf)
            {
                slimeTrail.gameObject.SetActive(false);
            }
        }
    }
}