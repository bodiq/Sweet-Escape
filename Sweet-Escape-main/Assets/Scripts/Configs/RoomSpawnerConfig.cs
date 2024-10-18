using UnityEngine;

[CreateAssetMenu(fileName = "RoomSpawnerConfig", menuName = "Configs/RoomSpawnerConfig")]
    public class RoomSpawnerConfig : ConfigSingleton<RoomSpawnerConfig>
    {
        public int ActiveRooms = 8;
        public int DeactivatePeriod = 4;
    }