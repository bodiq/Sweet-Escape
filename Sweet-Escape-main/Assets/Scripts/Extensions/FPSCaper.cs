using UnityEngine;

public static class FPSCaper
{
    private static bool _isInitialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void CapFPS()
    {
        if (!_isInitialized)
        {
            Application.targetFrameRate = FPSConfig.Instance.FPS;
            _isInitialized = true;
        }
    }
}