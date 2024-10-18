using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "LeaderboardConfig", menuName = "Configs/LeaderboardConfig")]
    public class LeaderboardConfig : ConfigSingleton<LeaderboardConfig>
    {
        [SerializeField] private Sprite goldLeaderboardPlate;
        [SerializeField] private Sprite silverLeaderboardPlate;
        [SerializeField] private Sprite defaultLeaderboardPlate;
        
        public Sprite GoldLeaderboardPlate => goldLeaderboardPlate;
        public Sprite SilverLeaderboardPlate => silverLeaderboardPlate;
        public Sprite DefaultLeaderboardPlate => defaultLeaderboardPlate;
    }
}