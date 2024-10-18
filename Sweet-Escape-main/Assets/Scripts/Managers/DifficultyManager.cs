using System;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager
{
    private static DifficultyManager instance;

    public static DifficultyManager Instance => instance ??= new DifficultyManager();

    public List<LevelsGroup> GetAllLevelsGroups(int difficulty)
    {
        var difficulties = new List<LevelsGroup>();
        for (int i = 0; i < difficulty; i++)
        {
            var levelsGroup = GetSelectedGroup(i);
            difficulties.Add(levelsGroup);
        }

        return difficulties;
    }

    public LevelsGroup GetSelectedGroup(int difficulty)
    {
        var hard = LevelsDifficultyConfig.Instance.HardLevelsInitialAmount +
            (int)Mathf.Pow(difficulty + 1, LevelsDifficultyConfig.Instance.HardLevelsCoefficient) - 1;
        var amountOfLevelsInOneDifficulty = LevelsDifficultyConfig.Instance.LevelsDifficultyPeriod;
        hard = (int)MathF.Min(amountOfLevelsInOneDifficulty, hard);

        var medium = LevelsDifficultyConfig.Instance.MediumLevelsInitialAmount +
            (int)Mathf.Pow(difficulty + 1, LevelsDifficultyConfig.Instance.MediumLevelsCoefficient) - 1;
        medium = (int)Mathf.Min(MathF.Min(amountOfLevelsInOneDifficulty, medium), amountOfLevelsInOneDifficulty - hard);

        int easy = amountOfLevelsInOneDifficulty - (hard + medium);

        var difficulties = new LevelsGroup
            { EasyLevelsAmount = easy, MediumLevelsAmount = medium, HardLevelsAmount = hard };
        return difficulties;
    }
}