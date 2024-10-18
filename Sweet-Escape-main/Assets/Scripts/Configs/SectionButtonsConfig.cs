using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Structs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Configs
{
    [CreateAssetMenu(fileName = "SectionButtonsConfig", menuName = "Configs/SectionButtonsConfig")]
    public class SectionButtonsConfig : ConfigSingleton<SectionButtonsConfig>
    {
        [DictionaryDrawerSettings(IsReadOnly = true, KeyLabel = "Type", ValueLabel = "Preset")]
        [OdinSerialize] public SerializedDictionary<SectionButton, SectionButtonData.SectionButtonsData> _sectionButtonsData = new();
        
        [OnInspectorInit]
        private void OnInspectorInit()
        {
            var sectionTypes = Enum.GetValues(typeof(SectionButton)).Cast<SectionButton>().ToArray();
            //_sectionButtonsData.SetupKeys(sectionTypes);
        }
        
        public IReadOnlyDictionary<SectionButton, SectionButtonData.SectionButtonsData> SectionButtonsData => _sectionButtonsData;
    }
}