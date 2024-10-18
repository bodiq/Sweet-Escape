using System;
using Configs;
using DG.Tweening;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class AchievementBox : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject selectedImage;
    [SerializeField] private Image achievementStatus;
    [SerializeField] private Button mainButton;

    private AchievementData _achievementData;

    private const float EndScaleVectorYValue = 0.8f;
    private const float AnimationDurationValue = 0.2f;

    private Tween _animationScaleTween;

    private bool _isSelected;

    private int _currentCount;

    public AchievementData AchievementData => _achievementData;
    public AchievementStatusType AchievementStatusType { get; set; }

    private void OnEnable()
    {
        mainButton.onClick.AddListener(SelectAchievement);
    }

    public void Initialize(AchievementData data, AchievementStatusType achievementStatusType, int count)
    {
        _achievementData = data;

        AchievementStatusType = achievementStatusType;

        _currentCount = count;

        if (data.AchievementsTypes == AchievementsTypes.FirstGame && AchievementStatusType != AchievementStatusType.Finished)
        {
            AchievementBoxManager.CountOfAvailableClaimableAchievements++;
        }
        
        switch (AchievementStatusType)
        {
            case AchievementStatusType.Finished:
                ChangeToCompletedState();
                break;
            case AchievementStatusType.Claimable:
                ChangeToClaimableState();
                break;
            case AchievementStatusType.Default:
                ChangeToDefaultState();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void RefreshAchievementData(AchievementsTypes achievementsType, int count)
    {
        if (achievementsType != _achievementData.AchievementsTypes) return;

        switch (achievementsType)
        {
            case AchievementsTypes.Fart:
            case AchievementsTypes.UnlockCharacter:
            case AchievementsTypes.Revive:
            case AchievementsTypes.LevelUp:
            case AchievementsTypes.SpendCoins:
            case AchievementsTypes.FirstGame:
            case AchievementsTypes.CollectSprinkles:
                _currentCount += count;
                if (_currentCount >= _achievementData.countToDo)
                {
                    AchievementBoxManager.CountOfAvailableClaimableAchievements++;
                    ChangeToClaimableState();
                }
                PlayerPrefs.SetInt(_achievementData.apiIdentifier, _currentCount);
                break;
            case AchievementsTypes.GodMode:
            case AchievementsTypes.ScoreOneRun:
            case AchievementsTypes.UnlockAllDailyRewards:
            case AchievementsTypes.LevelUpToFive:
                _currentCount = count;
                if (count >= _achievementData.countToDo)
                {
                    AchievementBoxManager.CountOfAvailableClaimableAchievements++;
                    ChangeToClaimableState();
                    PlayerPrefs.SetInt(_achievementData.apiIdentifier, _currentCount);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(achievementsType), achievementsType, null);
        }

        if (AchievementBoxManager.CountOfAvailableClaimableAchievements > 0)
        {
            UIManager.Instance.MainDownSection.TurnAchievementClaimableItem(true);
        }
        else
        {
            UIManager.Instance.MainDownSection.TurnAchievementClaimableItem(false);
        }
        
        PlayerPrefs.SetInt(GameManager.AvailableClaimableAchievementsKey, AchievementBoxManager.CountOfAvailableClaimableAchievements);
    }

    private void SelectAchievement()
    {
        if (!_isSelected)
        {
            if (!_animationScaleTween.IsActive())
            {
                _animationScaleTween = transform.DOScaleY(EndScaleVectorYValue, AnimationDurationValue).SetEase(Ease.OutElastic).OnComplete(() =>
                {
                    _animationScaleTween = transform.DOScaleY(1f, AnimationDurationValue).SetEase(Ease.OutElastic).SetAutoKill(true);

                });
            }
        }
        
        ChangeToSelectedState();
        UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.SelectAchievement(this);
    }

    public void ChangeToSelectedState()
    {
        selectedImage.SetActive(true);
        _isSelected = true;
    }

    public void ChangeToUnselectedState()
    {
        selectedImage.SetActive(false);
        _isSelected = false;
    }

    public void ChangeToCompletedState()
    {
        achievementStatus.gameObject.SetActive(true);
        achievementStatus.sprite = AchievementsDataConfig.Instance.CompletedAchievementIcon;
        icon.sprite = _achievementData.selectedIcon;
        AchievementStatusType = AchievementStatusType.Finished;
        UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh.RemoveListener(RefreshAchievementData);
        
        if (AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementData.AchievementsTypes))
        {
            AchievementBoxManager.AvailableAchievementsTypes.Remove(AchievementData.AchievementsTypes);
        }
        
        if (AchievementBoxManager.CountOfAvailableClaimableAchievements > 0)
        {
            UIManager.Instance.MainDownSection.TurnAchievementClaimableItem(true);
        }
    }

    private void ChangeToClaimableState()
    {
        achievementStatus.gameObject.SetActive(true);
        achievementStatus.sprite = AchievementsDataConfig.Instance.ClaimAchievementIcon;
        icon.sprite = _achievementData.unselectedIcon;
        AchievementStatusType = AchievementStatusType.Claimable;
        UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh.RemoveListener(RefreshAchievementData);

        if (AchievementBoxManager.CountOfAvailableClaimableAchievements > 0)
        {
            UIManager.Instance.MainDownSection.TurnAchievementClaimableItem(true);
        }
        
        if (AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementData.AchievementsTypes))
        {
            AchievementBoxManager.AvailableAchievementsTypes.Remove(AchievementData.AchievementsTypes);
        }
    }

    private void ChangeToDefaultState()
    {
        achievementStatus.gameObject.SetActive(false);
        icon.sprite = _achievementData.unselectedIcon;
        AchievementStatusType = AchievementStatusType.Default;
        UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh.AddListener(RefreshAchievementData);
    }

    private void OnDisable()
    {
        mainButton.onClick.RemoveListener(SelectAchievement);
        
        _animationScaleTween?.Kill();
    }
}
