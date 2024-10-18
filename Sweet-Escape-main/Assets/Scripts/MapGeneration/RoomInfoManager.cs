using System.Collections.Generic;
using System.Linq;
using Extensions;
using MoreMountains.Tools;
using ScriptableObjects;
using Random = UnityEngine.Random;

public class RoomInfoManager
{
    private static RoomInfoManager _instance;
    public static RoomInfoManager Instance => _instance ??= new RoomInfoManager();

    private readonly List<RoomInfo> _roomInfos = new();
    private readonly List<BiomeEnum> _allBiomes = new();
    private List<BiomeEnum> _availableBiomes;

    private RoomInfoManager()
    {
        _availableBiomes = (from biomeRooms in MapVariations.Instance.Rooms
            where biomeRooms.Value.Values.Any(list => list.Count!=0)
            select biomeRooms.Key).ToList();
    }

    public RoomInfo GetRoomInfo(int index)
    {
        while (_roomInfos.Count <= index)
        {
            GenerateNextValues();
        }

        return _roomInfos[index];
    }

    private void GenerateNextValues()
    {
        var difficultyPeriod = LevelsDifficultyConfig.Instance.LevelsDifficultyPeriod;
        var biomePeriod = LevelsDifficultyConfig.Instance.LevelsBiomePeriod;
        var roomsInfoCount = _roomInfos.Count;

        AddNewEmptyElements(difficultyPeriod, biomePeriod);

        GenerateDifficulties(difficultyPeriod, biomePeriod, roomsInfoCount);

        GenerateBiomes(difficultyPeriod, biomePeriod, roomsInfoCount);
    }

    private void AddNewEmptyElements(int difficultyPeriod, int biomePeriod)
    {
        var newElementsAmount = difficultyPeriod * biomePeriod;
        _roomInfos.AddEmptyElements(newElementsAmount);
    }

    private void GenerateDifficulties(int difficultyPeriod, int biomePeriod, int roomsInfoCount)
    {
        var currentDifficulty = roomsInfoCount / difficultyPeriod;
        for (int i = 0; i < biomePeriod; i++)
        {
            var difficultiesList = GetDifficultiesList(currentDifficulty + i);

            for (int j = 0; j < difficultyPeriod; j++)
            {
                _roomInfos[roomsInfoCount + i * difficultyPeriod + j].Difficulty = difficultiesList[j];
            }
        }
    }

    private List<LevelDifficultyEnum> GetDifficultiesList(int difficulty)
    {
        var difficultyInfo = DifficultyManager.Instance.GetSelectedGroup(difficulty);
        var difficultiesList = new List<LevelDifficultyEnum>();

        difficultiesList.AddRange(Enumerable.Repeat(LevelDifficultyEnum.III, difficultyInfo.HardLevelsAmount));
        difficultiesList.AddRange(Enumerable.Repeat(LevelDifficultyEnum.II, difficultyInfo.MediumLevelsAmount));
        difficultiesList.AddRange(Enumerable.Repeat(LevelDifficultyEnum.I, difficultyInfo.EasyLevelsAmount));

        difficultiesList.MMShuffle();
        return difficultiesList;
    }

    private void GenerateBiomes(int difficultyPeriod, int biomePeriod, int roomsInfoCount)
    {
        for (int i = 0; i < difficultyPeriod; i++)
        {
            var biomes = new List<BiomeEnum>(_availableBiomes);

            biomes.Remove(BiomeEnum.None);
            if (_allBiomes.Count != 0 && biomes.Count != 1)
            {
                biomes.Remove(_allBiomes[^1]);
            }

            var nextBiome = biomes[Random.Range(0, biomes.Count)];
            _allBiomes.Add(nextBiome);

            for (int j = 0; j < biomePeriod; j++)
            {
                _roomInfos[roomsInfoCount + i * biomePeriod + j].Biome = nextBiome;
            }
        }
    }
}