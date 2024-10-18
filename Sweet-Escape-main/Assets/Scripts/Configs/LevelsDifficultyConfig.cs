using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelsDifficultyConfig", menuName = "Configs/LevelsDifficultyConfig")]
public class LevelsDifficultyConfig : ConfigSingleton<LevelsDifficultyConfig>
{
    public int LevelsDifficultyPeriod = 10;
    public int LevelsBiomePeriod = 10;

    [MinValue(0)] public int HardLevelsInitialAmount = 2;
    [MinValue(0)] public int MediumLevelsInitialAmount = 3;

    [Range(0f, 1f)] public float HardLevelsCoefficient = 0.5f;
    [Range(0f, 1f)] public float MediumLevelsCoefficient = 0.5f;

    public int TestLevelsGroupsAmount;
    [ReadOnly] public List<LevelsGroup> TestLevelsGroups;

    private void OnValidate()
    {
        TestLevelsGroups = DifficultyManager.Instance.GetAllLevelsGroups(TestLevelsGroupsAmount);
    }
}

[Serializable]
public class LevelsGroup
{
    public int HardLevelsAmount;
    public int MediumLevelsAmount;
    public int EasyLevelsAmount;
}