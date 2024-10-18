using Audio;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using AudioType = Audio.AudioType;

public class LeaderboardDailySectionManager : UIScreen
{
    [SerializeField] private DailyBoxManager dailyBoxManager;
    [SerializeField] private AchievementBoxManager achievementBoxManager;
    [SerializeField] private RankingBoxManager rankingBoxManager;

    [SerializeField] private Button dailyButton;
    [SerializeField] private Button missionsButton;
    [SerializeField] private Button rankButton;

    public DailyBoxManager DailyBoxManager => dailyBoxManager;
    public AchievementBoxManager AchievementBoxManager => achievementBoxManager;
    
    private void OnEnable()
    {
        dailyButton.onClick.AddListener(OpenDailySection);
        missionsButton.onClick.AddListener(OpenMissionsSection);
        rankButton.onClick.AddListener(OpenRankingSection);
    }

    private void OpenDailySection()
    {
        AudioManager.Instance.PlaySFX(AudioType.MenuSwap);
        dailyBoxManager.gameObject.SetActive(true);
        achievementBoxManager.gameObject.SetActive(false);
        rankingBoxManager.gameObject.SetActive(false);
    }
    
    private void OpenMissionsSection()
    {
        AudioManager.Instance.PlaySFX(AudioType.MenuSwap);
        dailyBoxManager.gameObject.SetActive(false);
        achievementBoxManager.gameObject.SetActive(true);
        rankingBoxManager.gameObject.SetActive(false);
    }
    
    private void OpenRankingSection()
    {
        AudioManager.Instance.PlaySFX(AudioType.MenuSwap);
        dailyBoxManager.gameObject.SetActive(false);
        achievementBoxManager.gameObject.SetActive(false);
        rankingBoxManager.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        dailyButton.onClick.RemoveListener(OpenDailySection);
        missionsButton.onClick.RemoveListener(OpenMissionsSection);
        rankButton.onClick.RemoveListener(OpenRankingSection);
    }
}
