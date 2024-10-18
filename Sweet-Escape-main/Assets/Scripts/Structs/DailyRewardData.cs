using System;
using UnityEngine;

namespace Structs
{
    [Serializable]
    public struct DailyRewardData
    {
        public Enums.DailyReward dailyRewardType;
        public Sprite rewardIconOnIceCream;
        public Sprite rewardIconOnPopup;
        public string rewardTextOnPopup;
        public string reward;
        public int count;
    }
}