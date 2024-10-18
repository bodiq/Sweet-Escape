using System;
using System.Collections.Generic;
using API;
using Audio;
using Configs;
using DG.Tweening;
using Enums;
using Sirenix.Utilities;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using AudioType = Audio.AudioType;

public class AchievementBoxManager : MonoBehaviour
{
    [SerializeField] private AchievementBox achievementBoxToSpawn;
    [SerializeField] private Image achievementIcon;
    [SerializeField] private Button claimRewardButton;
    [SerializeField] private TextMeshProUGUI tittleAchievement;
    [SerializeField] private TextMeshProUGUI descriptionAchievement;
    [SerializeField] private TextMeshProUGUI rewardCount;
    [SerializeField] private Slider slider;
    [SerializeField] private Image mainBoxImage;
    [SerializeField] private List<GameObject> achievementListParents;
    [SerializeField] private List<Image> pageIndexes = new();

    [SerializeField] private Sprite pageIndexOn;
    [SerializeField] private Sprite pageIndexOff;

    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [SerializeField] private TextMeshProUGUI sliderFillText;

    [SerializeField] private GameObject apiConnectedStatus;
    [SerializeField] private Button signInButton;

    [SerializeField] private Transform defaultAchievementBoxPos;

    private AchievementBox _selectedAchievementBox;
    
    private readonly List<AchievementBox> _achievementBoxes = new();
    
    private readonly List<Image> _pageIndexesActive = new();

    private bool _setSelected;

    private int _achievementListParentIndex;
    private int _achievementIndex;
    
    private int _currentAchievementBoxIndex;
    
    private Vector3 _leftOutScreenAchievementBoxPos;
    private Vector3 _rightOutScreenAchievementBoxPos;
    private Vector3 _defaultAchievementBoxPos;
    
    private Tween _currentAchievementBoxTween;
    private Tween _nextAchievementBoxTween;
    
    private const float DurationAchievementBoxChange = 0.5f;

    private const string ClaimedString = "Claimed";

    private Image _lastPageActivated;

    public UnityEvent<AchievementsTypes, int> onAchievementRefresh;
    
    public static readonly List<AchievementsTypes> AvailableAchievementsTypes = new();

    public static int CountOfAvailableClaimableAchievements = 0;
    public static int CountOfClaimedAchievements = 0;

    private void Start()
    {
        RefreshAchievements();
    }

    private void OnEnable()
    {
        leftButton.onClick.AddListener(PreviousPage);
        rightButton.onClick.AddListener(NextPage);
        
        claimRewardButton.onClick.AddListener(ClaimReward);

        if (PlayerPrefs.HasKey(GameManager.AccessTokenAPIUserKey))
        {
            apiConnectedStatus.SetActive(true);
            signInButton.gameObject.SetActive(false);
        }
        else
        {
            apiConnectedStatus.SetActive(false);
            signInButton.gameObject.SetActive(true);
            signInButton.onClick.AddListener(GoToAccountLogin);
        }

        if (_selectedAchievementBox != null)
        {
            _selectedAchievementBox.ChangeToSelectedState();
        }
        else if(!_achievementBoxes.IsNullOrEmpty())
        {
            _selectedAchievementBox = _achievementBoxes[0];
            _selectedAchievementBox.ChangeToSelectedState();
        }
    }

    public void RefreshAchievements()
    {
        _achievementListParentIndex = 0;
        _achievementIndex = 0;

        var rect = UIManager.Instance.RectTransform.rect;

        var boxLocalPosition = defaultAchievementBoxPos.localPosition;
        
        _leftOutScreenAchievementBoxPos = new Vector3(boxLocalPosition.x - rect.width, boxLocalPosition.y);
        _rightOutScreenAchievementBoxPos = new Vector3(boxLocalPosition.x + rect.width, boxLocalPosition.y);
        _defaultAchievementBoxPos = boxLocalPosition;

        foreach (var achievement in AchievementsDataConfig.Instance.AchievementInfo)
        {
            AvailableAchievementsTypes.Add(achievement.AchievementsTypes);
            
            var count = 0;
            var achievementStatusType = AchievementStatusType.Default;
            var key = achievement.apiIdentifier;
            
            if (PlayerPrefs.HasKey(key))
            {
                count = PlayerPrefs.GetInt(key);
                if (count >= achievement.countToDo)
                {
                    if (PlayerPrefs.HasKey(key + ClaimedString))
                    {
                        achievementStatusType = AchievementStatusType.Finished;
                        CountOfClaimedAchievements++;
                    }
                    else
                    {
                        achievementStatusType = AchievementStatusType.Claimable;
                    }
                }
                //TODO: Check from API if the achievement is Collected
            }
            else
            {
                PlayerPrefs.SetInt(key, 0);
            }

            var achievementBox = Instantiate(achievementBoxToSpawn, achievementListParents[_achievementListParentIndex].transform);
            achievementBox.Initialize(achievement, achievementStatusType, count);
            _achievementBoxes.Add(achievementBox);
            
            if (!_setSelected)
            {
                achievementBox.ChangeToSelectedState();
                SelectAchievement(achievementBox);
                _setSelected = true;
            }
            
            _achievementBoxes.Add(achievementBox);
            _achievementIndex++;

            if (_achievementIndex < 9) continue;
            
            _achievementListParentIndex++;
            _achievementIndex = 0;
        }

        for (var i = 0; i < _achievementListParentIndex + 1; i++)
        {
            _pageIndexesActive.Add(pageIndexes[i]);
            _pageIndexesActive[i].sprite = pageIndexOff;
            _pageIndexesActive[i].gameObject.SetActive(true);
        }

        _pageIndexesActive[0].sprite = pageIndexOn;
        _lastPageActivated = _pageIndexesActive[0];
        
        PlayerPrefs.SetInt(GameManager.CompletedAchievementsKey, CountOfClaimedAchievements);
    }
    

    public void RefreshLoginStatus(bool isActive)
    {
        signInButton.gameObject.SetActive(!isActive);
        apiConnectedStatus.SetActive(isActive);
    }

    private void GoToAccountLogin()
    {
        UIManager.Instance.AccountScreenManager.TurnOn();
    }

    private void ClaimReward()
    {
        if (_selectedAchievementBox == null) return;
        
        PlayerPrefs.SetInt(_selectedAchievementBox.AchievementData.apiIdentifier + ClaimedString, 0);

        AudioManager.Instance.PlaySFX(AudioType.AchievementCompleted);
        
        APIManager.Instance.PutUserAchievement(_selectedAchievementBox.AchievementData.apiIdentifier, "true");
        
        mainBoxImage.sprite = AchievementsDataConfig.Instance.DefaultAchievementBoxSprite;
        slider.gameObject.SetActive(true);
        claimRewardButton.gameObject.SetActive(false);
        _selectedAchievementBox.AchievementStatusType = AchievementStatusType.Finished;
        _selectedAchievementBox.ChangeToCompletedState();
        
        CountOfClaimedAchievements++;
        
        PlayerPrefs.SetInt(GameManager.CompletedAchievementsKey, CountOfClaimedAchievements);
        
        GameManager.Instance.UserData.Coins += _selectedAchievementBox.AchievementData.rewardCount;
        UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(GameManager.Instance.UserData.Coins);
        PlayerPrefs.SetInt(GameManager.CoinsNameKey, GameManager.Instance.UserData.Coins);
        
        AudioManager.Instance.PlaySFX(AudioType.CollectCoinUI);
        
        var pooledFeedbackPopup = ObjectUIPool.Instance.GetPooledFeedbackPopup();
        if (pooledFeedbackPopup)
        {
            pooledFeedbackPopup.transform.position = claimRewardButton.transform.position;
            pooledFeedbackPopup.gameObject.SetActive(true);
            pooledFeedbackPopup.StartSelfDestroyCoroutine();
        }
        
        CountOfAvailableClaimableAchievements--;

        PlayerPrefs.SetInt(GameManager.AvailableClaimableAchievementsKey, CountOfAvailableClaimableAchievements);
        UIManager.Instance.MainDownSection.TurnAchievementClaimableItem(CountOfAvailableClaimableAchievements > 0);

        RefreshSlideValue(true);
    }

    private void PreviousPage()
    {
        leftButton.interactable = false;
        rightButton.interactable = false;

        AudioManager.Instance.PlaySFX(AudioType.MenuSwap);

        var achievementBoxIndexToShow = _currentAchievementBoxIndex - 1;

        if (achievementBoxIndexToShow < 0)
        {
            achievementBoxIndexToShow = achievementListParents.Count - 1;
        }

        achievementListParents[achievementBoxIndexToShow].transform.localPosition = _leftOutScreenAchievementBoxPos;

        _currentAchievementBoxTween = achievementListParents[_currentAchievementBoxIndex].transform.DOLocalMove(_rightOutScreenAchievementBoxPos, DurationAchievementBoxChange);
        _nextAchievementBoxTween = achievementListParents[achievementBoxIndexToShow].transform.DOLocalMove(_defaultAchievementBoxPos, DurationAchievementBoxChange).OnComplete(() =>
            {
                _currentAchievementBoxIndex = achievementBoxIndexToShow;
                leftButton.interactable = true;
                rightButton.interactable = true;
                _lastPageActivated.sprite = pageIndexOff;
                _lastPageActivated = _pageIndexesActive[achievementBoxIndexToShow];
                _lastPageActivated.sprite = pageIndexOn;
            });
    }

    private void NextPage()
    {
        leftButton.interactable = false;
        rightButton.interactable = false;
        
        AudioManager.Instance.PlaySFX(AudioType.MenuSwap);

        var achievementBoxIndexToShow = _currentAchievementBoxIndex + 1;

        if (achievementBoxIndexToShow > achievementListParents.Count - 1)
        {
            achievementBoxIndexToShow = 0;
        }

        achievementListParents[achievementBoxIndexToShow].transform.localPosition = _rightOutScreenAchievementBoxPos;

        _currentAchievementBoxTween = achievementListParents[_currentAchievementBoxIndex].transform.DOLocalMove(_leftOutScreenAchievementBoxPos, DurationAchievementBoxChange);
        _nextAchievementBoxTween = achievementListParents[achievementBoxIndexToShow].transform.DOLocalMove(_defaultAchievementBoxPos, DurationAchievementBoxChange).OnComplete(
            () =>
            {
                _currentAchievementBoxIndex = achievementBoxIndexToShow;
                leftButton.interactable = true;
                rightButton.interactable = true;
                _lastPageActivated.sprite = pageIndexOff;
                _lastPageActivated = _pageIndexesActive[achievementBoxIndexToShow];
                _lastPageActivated.sprite = pageIndexOn;
            });
    }
    
    public void SelectAchievement(AchievementBox achievementBox)
    {
        
        if (_selectedAchievementBox != null)
        {
            if (achievementBox == _selectedAchievementBox)
            {
                return;
            }
            _selectedAchievementBox.ChangeToUnselectedState();
        }
        
        _selectedAchievementBox = achievementBox;
        achievementIcon.sprite = _selectedAchievementBox.AchievementData.selectedIcon;
        tittleAchievement.text = _selectedAchievementBox.AchievementData.tittle;
        descriptionAchievement.text = _selectedAchievementBox.AchievementData.description;
        rewardCount.text = _selectedAchievementBox.AchievementData.rewardCount + " Coins";

        switch (_selectedAchievementBox.AchievementStatusType)
        {
            case AchievementStatusType.Finished:
                claimRewardButton.gameObject.SetActive(false);
                slider.gameObject.SetActive(true);
                mainBoxImage.sprite = AchievementsDataConfig.Instance.DefaultAchievementBoxSprite;
                RefreshSlideValue(true);
                break;
            case AchievementStatusType.Claimable:
                claimRewardButton.gameObject.SetActive(true);
                slider.gameObject.SetActive(false);
                mainBoxImage.sprite = AchievementsDataConfig.Instance.ClaimableAchievementBoxSprite;
                break;
            case AchievementStatusType.Default:
                slider.gameObject.SetActive(true);
                claimRewardButton.gameObject.SetActive(false);
                mainBoxImage.sprite = AchievementsDataConfig.Instance.DefaultAchievementBoxSprite;
                RefreshSlideValue();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void RefreshSlideValue(bool isFinished = false)
    {
        if (isFinished)
        {
            var maxValue = _selectedAchievementBox.AchievementData.countToDo;
            slider.minValue = 0;
            slider.maxValue = maxValue;
            
            slider.value = maxValue;
            sliderFillText.text = maxValue + "/" + maxValue;
        }
        else
        {
            var value = PlayerPrefs.GetInt(_selectedAchievementBox.AchievementData.apiIdentifier);

            var countToDo = _selectedAchievementBox.AchievementData.countToDo;

            slider.minValue = 0;
            slider.maxValue = countToDo;

            if (value >= countToDo)
            {
                slider.value = countToDo;
                sliderFillText.text = countToDo + "/" + countToDo;
            }
            else
            {
                slider.value = value;
                sliderFillText.text = value + "/" + countToDo;
            }
        }
    }

    private void OnDisable()
    {
        leftButton.onClick.RemoveListener(PreviousPage);
        rightButton.onClick.RemoveListener(NextPage);
        
        claimRewardButton.onClick.RemoveListener(ClaimReward);
    }
}



[Serializable]
public struct AchievementData
{
    public AchievementsTypes AchievementsTypes;
    public string tittle;
    public string description;
    public int rewardCount;
    public Sprite selectedIcon;
    public Sprite unselectedIcon;
    public string apiIdentifier;
    public int countToDo;
}
