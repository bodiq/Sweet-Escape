using UnityEngine;

[CreateAssetMenu(fileName = "SmallBotConfig", menuName = "Configs/SmallBotConfig")]
public class SmallBotConfig : ConfigSingleton<SmallBotConfig>
{
    public float MoveSpeed;
    public float ReachTargetAccuracy = 0.1f;
}