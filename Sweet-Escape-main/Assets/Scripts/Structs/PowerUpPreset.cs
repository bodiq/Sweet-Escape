using System;
using Sirenix.Serialization;

namespace Structs
{
    [Serializable]
    public struct PowerUpPreset
    {
        [OdinSerialize] public string powerUpName;
        [OdinSerialize] public float duration;
        [OdinSerialize] public string startDescriptionText;
        [OdinSerialize] public string endDescriptionText;
        
        [OdinSerialize] public float startCoefficientMultiplier;
        [OdinSerialize] public float perLevelAdditionalMultiplier;

        [OdinSerialize] public float startDuration;
        [OdinSerialize] public float perLevelAdditionalDuration;
    }
}