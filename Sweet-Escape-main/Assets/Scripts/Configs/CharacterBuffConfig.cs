using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "CharacterBuffConfig", menuName = "Configs/CharacterBuffConfig")]
    public class CharacterBuffConfig : ConfigSingleton<CharacterBuffConfig>
    {
        [SerializeField] private float addPercentCoinRewardPerLevel;
        [SerializeField] private float addPercentPowerUpEndurancePerLevel;
        [SerializeField] private float addPercentPowerUpAppearingPerLevel;
        
        //WTF
        [SerializeField] private float xEnemyTypeAvoidance;

        public float AdditionalPercentCoinRewardPerLevel => addPercentCoinRewardPerLevel / 100;
        public float AdditionalPercentPowerUpEndurancePerLevel => addPercentPowerUpEndurancePerLevel / 100;
        public float AdditionalPercentPowerUpAppearingPerLevel => addPercentPowerUpAppearingPerLevel / 100;
        public float XEnemyTypeAvoidance => xEnemyTypeAvoidance;
    }
}