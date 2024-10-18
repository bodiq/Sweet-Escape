using Enums;
using Items;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

namespace Configs
{
        [CreateAssetMenu(fileName = "SprinkleConfig", menuName = "Configs/SprinkleConfig")]
        public class SprinkleConfig : ConfigSingleton<SprinkleConfig>
        {
            [OdinSerialize] public SerializedDictionary<Sprinkles, Sprinkle.SprinkleData> sprinkleData;
        }
}