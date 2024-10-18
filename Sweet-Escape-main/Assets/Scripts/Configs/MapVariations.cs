using System;
using System.Collections.Generic;
using MapGeneration;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MapVariations", menuName = "Configs/MapVariations")]
    public class MapVariations : ConfigSingleton<MapVariations>
    {
        public string MapsDirectory;

        public Dictionary<BiomeEnum, Dictionary<LevelDifficultyEnum, List<MapDataStruct>>> Rooms = new();

        public Color factoryColorBackground;
        public Color severColorBackground;
        public Color radiationColorBackground;

        public Sprite factorySpriteBackground;
        public Sprite severSpriteBackground;
        public Sprite radiationSpriteBackground;

        public void InitializeWithEmptyValues()
        {
            var allBiomes = (BiomeEnum[])Enum.GetValues(typeof(BiomeEnum));
            var allDifficulties = (LevelDifficultyEnum[])Enum.GetValues(typeof(LevelDifficultyEnum));

            foreach (var biome in allBiomes)
            {
                if (Rooms.ContainsKey(biome) && Rooms[biome] != null)
                {
                    continue;
                }

                Rooms.Add(biome, new());
                foreach (var difficulty in allDifficulties)
                {
                    if (Rooms[biome].ContainsKey(difficulty) && Rooms[biome][difficulty] != null)
                    {
                        continue;
                    }

                    Rooms[biome].Add(difficulty, new());
                }
            }
        }
    }
}