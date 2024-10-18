using Enums;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

namespace Configs
{
    [CreateAssetMenu(fileName = "SlimeConfig", menuName = "Configs/SlimeConfig")]
    public class SlimeConfig : ConfigSingleton<SlimeConfig>
    {
        [OdinSerialize] public SerializedDictionary<SlimeEnum, SlimeTrail.SlimeAnimationInfo> slimeDataAnimationClips;
        [OdinSerialize] public SerializedDictionary<SkinEnum, Material> slimeData;

        [SerializeField] public float durationFadeIn = 0.3f;
        [SerializeField] public float durationFadeOut = 1.5f;

        public const float EndFadeValue = 1f;
        public const float StartFadeValue = 0f;

        public const float DelayBeforeSlimeDisappear = 1.5f;
    }
}