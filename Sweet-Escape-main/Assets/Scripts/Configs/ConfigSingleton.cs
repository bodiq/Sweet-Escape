using Sirenix.OdinInspector;
using UnityEngine;

public class ConfigSingleton<T> : SerializedScriptableObject where T : ScriptableObject
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                string name = typeof(T).Name;
                _instance = Resources.Load<T>(name);

                if (_instance == null)
                {
                    Debug.LogError(
                        $"Failed to load config: '{name}'.\nConfig file must be placed at: Resources/{name}.asset");
                }
            }

            return _instance;
        }
    }
}