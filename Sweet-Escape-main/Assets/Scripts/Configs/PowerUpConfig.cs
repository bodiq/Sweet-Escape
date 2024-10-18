using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Extensions;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Structs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Configs
{
    [CreateAssetMenu(fileName = "PowerUpConfig", menuName = "Configs/PowerUpConfig")]
    public class PowerUpConfig : ConfigSingleton<PowerUpConfig>
    {
        [OdinSerialize] public int costPerLevel;
        
        [DictionaryDrawerSettings(IsReadOnly = false, KeyLabel = "Type", ValueLabel = "PowerUpPreset")]
        [OdinSerialize] public SerializedDictionary<Enums.PowerUps, PowerUpPreset> powerUpPresets = new();
        
        [DictionaryDrawerSettings(ValueLabel = "PowerUpPrefabs")]
        [OdinSerialize] public SerializedDictionary<Enums.PowerUps, GameObject> powerUpPrefabs;
        
        [DictionaryDrawerSettings(ValueLabel = "PowerUpAppearance")]
        [OdinSerialize] public SerializedDictionary<Characters, RandomPack<Enums.PowerUps>> powerUpAppearance = new();
            

        [OnInspectorInit]
        private void OnInspectorInit()
        {
            var powerUps = Enum.GetValues(typeof(Enums.PowerUps)).Cast<Enums.PowerUps>().ToArray();
            powerUpPresets.SetupKeys(powerUps);

            var characters = Enum.GetValues(typeof(Characters)).Cast<Characters>().ToArray();
            powerUpAppearance.SetupKeys(characters);
            
            powerUpPrefabs.SetupKeys(powerUps);
        }
        
        public IReadOnlyDictionary<Enums.PowerUps, PowerUpPreset> PowerUpPresets => powerUpPresets;
        public IReadOnlyDictionary<Characters, RandomPack<Enums.PowerUps>> PowerUpAppearance => powerUpAppearance;
        public IReadOnlyDictionary<Enums.PowerUps, GameObject> PowerUpPrefabs => powerUpPrefabs;
    }
}