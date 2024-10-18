using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TilemapManagerConfig", menuName = "Configs/TilemapManagerConfig")]
public class TilemapManagerConfig : ConfigSingleton<TilemapManagerConfig>
{
    [MinValue(4)] public int ActiveTilemapRoomsAmount;
}