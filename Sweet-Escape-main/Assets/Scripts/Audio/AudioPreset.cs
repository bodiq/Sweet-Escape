using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Audio
{
    [Serializable] [HideReferenceObjectPicker]
    public struct AudioPreset
    {
        [OdinSerialize] public AudioClip audioClip;
        [ShowIf(nameof(audioClip))] [OdinSerialize] public float volume;
    }
}