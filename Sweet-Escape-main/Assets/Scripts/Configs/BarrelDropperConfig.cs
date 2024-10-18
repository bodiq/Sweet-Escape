using UnityEngine;

[CreateAssetMenu(fileName = "BarrelDropperConfig", menuName = "Configs/BarrelDropperConfig")]
public class BarrelDropperConfig : ConfigSingleton<BarrelDropperConfig>
{
    public float MoveSpeed;
    public float BarrelSpeed;

    public AnimationClip FlyingDownClip;
    public AnimationClip FlyingUpClip;
    public AnimationClip DroppingClip;
}