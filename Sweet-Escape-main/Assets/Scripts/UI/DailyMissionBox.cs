using System;
using Audio;
using Configs;
using Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyMissionBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Button refreshMissionsButton;
    [SerializeField] private GameObject claimRewardObject;
    [SerializeField] private Image backgroundBoxImage;
    [SerializeField] private Slider slider;
    [SerializeField] private Button claimRewardButton;
    [SerializeField] private TextMeshProUGUI fillText;
    [SerializeField] private TextMeshProUGUI watchAdvButtonText;
    
    [SerializeField] private GameObject rerollPopupBoxDown;
    [SerializeField] private GameObject rerollPopupBoxUp;
    
    [SerializeField] private Button watchAdvButtonDown;
    [SerializeField] private Button watchAdvButtonUp;
    
    private DailyMissionsRewardType _dailyMissionsRewardType;
    private int _currentCount;
    private int _maxCount;

    private DailyMissionState _dailyMissionState = DailyMissionState.Uncompleted;
    public DailyMissionData data;

    private string _prefsMissionKey;

    private Button _watchAdvToReroll;
    private GameObject _rerollPopupBox;

    public bool _isLastObject;
    
    public void Initialize(int currentCount, DailyMissionData _data, bool isLastObject)
    {
        data = _data;
        _maxCount = data.countToDo;
        slider.minValue = 0;
        slider.maxValue = _maxCount;
        _dailyMissionsRewardType = data.rewardType;
        _currentCount = currentCount;

        slider.maxValue = _maxCount;

        if (_watchAdvToReroll)
        {
            _watchAdvToReroll.onClick.RemoveAllListeners();
        }
        
        if (isLastObject)
        {
            _watchAdvToReroll = watchAdvButtonUp;
            _rerollPopupBox = rerollPopupBoxUp;
        }
        else
        {
            _watchAdvToReroll = watchAdvButtonDown;
            _rerollPopupBox = rerollPopupBoxDown;
        }

        if (_currentCount >= _maxCount)
        {
            _dailyMissionState = DailyMissionState.Completed;
            ChangeToCompletedMissions();
            slider.value = _maxCount;
        }
        else
        {
            _dailyMissionState = DailyMissionState.Uncompleted;
            ChangeToUncompletedMission();
            slider.value = _currentCount;
        }

        _isLastObject = isLastObject;
        
        fillText.text = slider.value + "/" + slider.maxValue;
        rewardImage.sprite = data.rewardIcon;
        descriptionText.text = data.dailyMissionDescription;
        
        rewardText.text = data.rewardType switch
        {
            DailyMissionsRewardType.Coins => data.rewardCount + " coins",
            _ => throw new ArgumentOutOfRangeException()
        };

        _prefsMissionKey = _data.dailyMissions.ToString() + data.countToDo;
        UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh.AddListener(RefreshValueData);
        
        _watchAdvToReroll.onClick.AddListener(OnWatchAdvToReroll);
    }

    private void OnEnable()
    {
        _watchAdvToReroll.onClick.RemoveAllListeners();
        
        refreshMissionsButton.onClick.AddListener(OpenSelectedMission);
        if (_watchAdvToReroll != null)
        {
            _watchAdvToReroll.onClick.AddListener(OnWatchAdvToReroll);
        }
        else
        {
            Debug.LogError("NullWatchAdvButton");
        }
    }

    private void SetSliderValue()
    {
        fillText.text = _currentCount + "/" + _maxCount;
    }

    private void RefreshValueData(DailyMissions dailyMissionType, GameObject gameObjectToCheck, int count)
    {
        if (dailyMissionType != data.dailyMissions) return;
        
        switch (dailyMissionType)
        {
            case DailyMissions.CollectSprinklesTotal:
            case DailyMissions.PlayArcadeRun:
            case DailyMissions.UpgradeHero:
            case DailyMissions.GoThroughRooms:
            case DailyMissions.LandOnSlamBlock:
            case DailyMissions.ReviveByWatchingAdv:
            case DailyMissions.UsePowerUp:
                _currentCount += count;
                slider.value = _currentCount;
                SetSliderValue();
                if (_currentCount >= slider.maxValue)
                {
                    ChangeToCompletedMissions();
                }
                PlayerPrefs.SetInt(_prefsMissionKey, _currentCount);
                break;
            case DailyMissions.CollectSprinkleOneRun:
                _currentCount = count;
                if (_currentCount >= data.countToDo)
                {
                    PlayerPrefs.SetInt(_prefsMissionKey, _currentCount);
                    ChangeToCompletedMissions();
                }
                break;
            case DailyMissions.GetPowerUpsInOneRun:
                _currentCount = count;
                if (_currentCount >= data.countToDo)
                {
                    PlayerPrefs.SetInt(_prefsMissionKey, _currentCount);
                    ChangeToCompletedMissions();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dailyMissionType), dailyMissionType, null);
        }
    }

    public void ChangeToUncompletedMission()
    {
        backgroundBoxImage.sprite = DailyMissionsConfig.Instance.UncompletedDailyBoxSprite;
        claimRewardObject.SetActive(false);
        refreshMissionsButton.gameObject.SetActive(true);
        claimRewardButton.enabled = false;
        slider.gameObject.SetActive(true);
        _dailyMissionState = DailyMissionState.Uncompleted;
        _rerollPopupBox.SetActive(false);
    }

    private void ChangeToCompletedMissions()
    {
        _dailyMissionState = DailyMissionState.Completed;
        backgroundBoxImage.sprite = DailyMissionsConfig.Instance.CompletedDailyBoxSprite;
        claimRewardObject.SetActive(true);
        refreshMissionsButton.gameObject.SetActive(false);
        claimRewardButton.enabled = true;
        claimRewardButton.onClick.AddListener(ClaimReward);
        slider.gameObject.SetActive(false);
        _rerollPopupBox.SetActive(false);
    }

    private void OpenSelectedMission()
    {
        if (_dailyMissionState == DailyMissionState.Selected) return;

        watchAdvButtonText.text = PlayerPrefs.HasKey(GameManager.DailyMissionsUsedFreeRerollKey) ? "Watch Adv" : "Free";
        
        _dailyMissionState = DailyMissionState.Selected;
        backgroundBoxImage.sprite = DailyMissionsConfig.Instance.SelectedDailyBoxSprite;
        claimRewardObject.SetActive(false);
        UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onSelectedDailyTask?.Invoke(this);
        _rerollPopupBox.SetActive(true);
    }

    private void OnDisable()
    {
        refreshMissionsButton.onClick.RemoveListener(OpenSelectedMission);
        if (_watchAdvToReroll != null)
        {
            _watchAdvToReroll.onClick.AddListener(OnWatchAdvToReroll);
        }
        else
        {
            Debug.LogError("NullWatchAdvButton");
        }
    }

    private void OnWatchAdvToReroll()
    {
        if (!PlayerPrefs.HasKey(GameManager.DailyMissionsUsedFreeRerollKey))
        {
            UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onAdvRerollClicked?.Invoke();
            PlayerPrefs.SetInt(GameManager.DailyMissionsUsedFreeRerollKey, 0);
        }
        else
        {
            AdsManager.Instance.ShowRewardedAd(Constants.AdPlacementRerollDailyMission);
        }
    }

    private void ClaimReward()
    {
        switch (_dailyMissionsRewardType)
        {
            case DailyMissionsRewardType.Coins:
                var coinsNow = GameManager.Instance.UserData.Coins + data.rewardCount;
                GameManager.Instance.UserData.Coins = coinsNow;
                PlayerPrefs.SetInt(GameManager.CoinsNameKey, coinsNow);
                var pooledFeedbackPopup = ObjectUIPool.Instance.GetPooledFeedbackPopup();
                if (pooledFeedbackPopup)
                {
                    pooledFeedbackPopup.transform.position = claimRewardButton.transform.position;
                    pooledFeedbackPopup.gameObject.SetActive(true);
                    pooledFeedbackPopup.StartSelfDestroyCoroutine();
                }

                AudioManager.Instance.PlaySFX(Audio.AudioType.CollectCoinUI);
                
                DailyBoxManager.AvailableDailyMissions--;
                DeleteData();
                gameObject.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void DeleteData()
    {
        UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh.RemoveListener(RefreshValueData);
        PlayerPrefs.DeleteKey(_prefsMissionKey);
        DailyBoxManager.SavedDailyTasksKeys.Remove(_prefsMissionKey);
        DailyBoxManager.AvailableDailyMissionsTypes.Remove(data.dailyMissions);
    }
} 
