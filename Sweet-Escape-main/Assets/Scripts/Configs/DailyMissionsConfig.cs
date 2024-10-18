using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "DailyMissionsConfig", menuName = "Configs/DailyMissionsConfig")]
    public class DailyMissionsConfig : ConfigSingleton<DailyMissionsConfig>
    {
        [SerializeField] private Sprite uncompletedDailyBoxSprite;
        [SerializeField] private Sprite completedDailyBoxSprite;
        [SerializeField] private Sprite selectedDailyBoxSprite;

        [SerializeField] private Sprite defaultDailyMissionsBox;
        [SerializeField] private Sprite selectedDailyMissionsBox;

        [SerializeField] private Color whiteColor;
        [SerializeField] private Color brownColor;
        [SerializeField] private Color waffleColor;

        [SerializeField] private List<DailyMissionData> dailyMissionsData = new();

        [SerializeField] private List<string> dailyFacts;

        public Sprite UncompletedDailyBoxSprite => uncompletedDailyBoxSprite;
        public Sprite CompletedDailyBoxSprite => completedDailyBoxSprite;
        public Sprite SelectedDailyBoxSprite => selectedDailyBoxSprite;

        public Sprite DefaultDailyMissionsBoxSprite => defaultDailyMissionsBox;
        public Sprite SelectedDailyMissionsBoxSprite => selectedDailyMissionsBox;

        public Color WhiteColor => whiteColor;
        public Color BrownColor => brownColor;
        public Color WaffleColor => waffleColor;
        
        public List<DailyMissionData> DailyMissionsData => dailyMissionsData;

        public List<string> DailyFacts => dailyFacts;
    }
    
}