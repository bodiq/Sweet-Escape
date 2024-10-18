using System.Collections.Generic;
using Sirenix.Serialization;
using Structs;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "DailyRewardConfig", menuName = "Configs/DailyRewardConfig")]
    public class DailyRewardConfig : ConfigSingleton<DailyRewardConfig>
    {
        [OdinSerialize] public List<DailyRewardList> dailyRewards;
    }
}