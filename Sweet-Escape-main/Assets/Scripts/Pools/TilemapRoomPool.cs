using System;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

public class TilemapRoomPool : MonoBehaviour
{
    public static TilemapRoomPool Instance;

    [SerializeField] Transform gridParent;

    private readonly Dictionary<BiomeEnum, Dictionary<LevelDifficultyEnum, List<TilemapRoom>>> _rooms = new();

    private void Awake()
    {
        foreach (var biome in (BiomeEnum[])Enum.GetValues(typeof(BiomeEnum)))
        {
            _rooms.Add(biome, new());
            foreach (var difficulty in (LevelDifficultyEnum[])Enum.GetValues(typeof(LevelDifficultyEnum)))
            {
                _rooms[biome].Add(difficulty, new());
            }
        }
    }

    private void Start()
    {
        Instance = this;

        var mapVariants = MapVariations.Instance;

        foreach (var biomeDifficultyPair in mapVariants.Rooms)
        {
            foreach (var difficultyLevelPair in biomeDifficultyPair.Value)
            {
                int amountOfRoomsToAdd = Mathf.CeilToInt((float)TilemapManagerConfig.Instance.ActiveTilemapRoomsAmount /
                                                         difficultyLevelPair.Value.Count);
                for (int i = 0; i < amountOfRoomsToAdd; i++)
                {
                    foreach (var mapDataStruct in difficultyLevelPair.Value)
                    {
                        var room = Instantiate(mapDataStruct.tilemapRoom, Vector3.zero, Quaternion.identity,
                            gridParent);
                        room.gameObject.SetActive(false);
                        _rooms[biomeDifficultyPair.Key][difficultyLevelPair.Key].Add(room);
                    }
                }
            }
        }
    }

    public TilemapRoom GetRoom(RoomInfo roomInfo)
    {
        var rooms = _rooms[roomInfo.Biome][roomInfo.Difficulty];
        var randomRoomIndex = Random.Range(0, rooms.Count);
        var tilemapRoom = rooms[randomRoomIndex];
        rooms.RemoveAt(randomRoomIndex);
        return tilemapRoom;
    }

    public void ReturnRoom(TilemapRoom tilemapRoom)
    {
        _rooms[tilemapRoom.RoomInfo.Biome][tilemapRoom.RoomInfo.Difficulty].Add(tilemapRoom);
    }
}