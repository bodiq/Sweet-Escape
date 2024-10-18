using UnityEngine;

public class CameraData : MonoBehaviour
{
    public static CameraData Instance { get; private set; }
    
    private void Awake()
    {
        InitializeSingleton();
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
