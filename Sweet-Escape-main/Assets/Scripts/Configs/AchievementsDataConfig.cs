using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "AchievementsDataConfig", menuName = "Configs/AchievementsDataConfig")]
    public class AchievementsDataConfig: ConfigSingleton<AchievementsDataConfig>
    {
        [OdinSerialize] public List<AchievementData> achievementInfo;
        
        [SerializeField] private Sprite claimAchievementIcon;
        [SerializeField] private Sprite completedAchievementIcon;

        [SerializeField] private Sprite defaultAchievementBoxSprite;
        [SerializeField] private Sprite claimableAchievementBoxSprite;
        
        public List<AchievementData> AchievementInfo => achievementInfo;
        public Sprite ClaimAchievementIcon => claimAchievementIcon;
        public Sprite CompletedAchievementIcon => completedAchievementIcon;
        public Sprite DefaultAchievementBoxSprite => defaultAchievementBoxSprite;
        public Sprite ClaimableAchievementBoxSprite => claimableAchievementBoxSprite;
    }
}