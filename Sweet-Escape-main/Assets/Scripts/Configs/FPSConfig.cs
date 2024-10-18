using UnityEngine;

[CreateAssetMenu(fileName = "FPSConfig", menuName = "Configs/FPSConfig")]
public class FPSConfig : ConfigSingleton<FPSConfig>
{
    public int FPS;
}