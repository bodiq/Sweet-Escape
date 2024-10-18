using System.Collections.Generic;
using Audio;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using AudioType = Audio.AudioType;

namespace Configs
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/AudioConfig")]
    public class AudioConfig : ConfigSingleton<AudioConfig>
    {
        [DictionaryDrawerSettings(IsReadOnly = false, KeyLabel = "Type", ValueLabel = "MusicPreset")]
        [OdinSerialize] public SerializedDictionary<AudioType, AudioPreset> audioMusicPresets = new();
        
        [DictionaryDrawerSettings(IsReadOnly = false, KeyLabel = "Type", ValueLabel = "SFXPreset")]
        [OdinSerialize] public SerializedDictionary<AudioType, AudioPreset> audioSFXPresets = new();
        
        public IReadOnlyDictionary<AudioType, AudioPreset> AudioMusicPresets => audioMusicPresets;
        public IReadOnlyDictionary<AudioType, AudioPreset> AudioSFXPresets => audioSFXPresets;
    }
}