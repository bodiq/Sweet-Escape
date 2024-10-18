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
    [CreateAssetMenu(fileName = "CharacterConfig", menuName = "Configs/CharacterConfig")]
    public class CharacterConfig : ConfigSingleton<CharacterConfig>
    {
        [SerializeField] private int priceUpgradePerLevel = 150;

        [SerializeField] private int stepPriceToRevive = 200;
        [SerializeField] private int defaultPriceToRevive = 200;

        [SerializeField] private float minimumDurationItemMoveToPlayer = 0.1f;
        [SerializeField] private float maximumDurationItemMoveToPlayer = 0.2f;

        [SerializeField] private float immunityDuration = 3f;

        [DictionaryDrawerSettings(IsReadOnly = true, KeyLabel = "Type", ValueLabel = "Preset")]
        [OdinSerialize] public SerializedDictionary<Characters, CharacterPreset> characterData = new();
        
        [OnInspectorInit]
        private void OnInspectorInit()
        {
            var characters = Enum.GetValues(typeof(Characters)).Cast<Characters>().ToArray();
            characterData.SetupKeys(characters);
        }

        public IReadOnlyDictionary<Characters, CharacterPreset> CharacterData => characterData;
        public int UpgradePricePerLevel => priceUpgradePerLevel;
        public int StepPriceToRevive => stepPriceToRevive;
        public int DefaultPriceToRevive => defaultPriceToRevive;

        public float MinimumDurationItemMoveToPlayer
        {
            get => minimumDurationItemMoveToPlayer;
            set => minimumDurationItemMoveToPlayer = value;
        }

        public float MaximumDurationItemMoveToPlayer
        {
            get => maximumDurationItemMoveToPlayer;
            set => maximumDurationItemMoveToPlayer = value;
        }

        public float ImmunityDuration => immunityDuration;
    }
}