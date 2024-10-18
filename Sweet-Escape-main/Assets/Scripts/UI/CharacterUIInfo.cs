using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUIInfo : MonoBehaviour
{
    [SerializeField] private List<CharacterSectionAbilityUI> abilitiesUI;
    [SerializeField] private List<CharacterLevelStar> characterLevelStars;
    [SerializeField] private Image characterSkinIcon;
    
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private TextMeshProUGUI nameLabel;

    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button skinButton;
    [SerializeField] private TextMeshProUGUI upgradePrice;

    [SerializeField] private Animator levelUpAnimator;
    [SerializeField] private GameObject objectMask;
    [SerializeField] private Mask mask;

    [SerializeField] private GameObject coinImage;
    [SerializeField] private GameObject maxLevelText;

    private Coroutine _upgradeAnimationCoroutine;

    private int _level;
    private int _upgradePrice;
    private Characters _character;

    private void OnEnable()
    {
        upgradeButton.onClick.AddListener(UpgradeCharacter);
        skinButton.onClick.AddListener(SkinChangerFadeIn);
    }

    private void SkinChangerFadeIn()
    {
        UIManager.Instance.CharactersSection.SkinSelection.FadeIn();
    }

    private void UpgradeCharacter()
    {
        var coins = GameManager.Instance.UserData.Coins;

        if (coins >= _upgradePrice && _level <= 4)
        {
            LevelUpCharacter();
            UIManager.Instance.MainMenuScreen.ChangeSelectedCharacter(_character);
        }
        else if (_level == 5)
        {
            UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.MaxLevel);
        }
        else if(coins < _upgradePrice)
        {
            ShowErrorPopup();
        }
    }

    private void SetUpgradeButtonToMaxLevel()
    {
        upgradeButton.interactable = false;
        upgradePrice.gameObject.SetActive(false);
        maxLevelText.SetActive(true);

        coinImage.SetActive(false);
    }

    private void UpdateLevel()
    {
        if (GameManager.Instance.CharactersData.TryGetValue(_character, out var characterData))
        {
            _level = characterData.Level;
        }
    }

    public void InitializeCharacterUIInfo(Characters character)
    {
        _character = character;
        GameManager.Instance.CharactersData.TryGetValue(_character, out var characterData);

        if (characterData != null)
        {
            _level = characterData.Level;
            nameLabel.text = _character.ToString();
            
            if(_level == 5)
            {
                SetUpgradeButtonToMaxLevel();
            }
            else
            {
                RefreshUpgradePrice();
            }

            RefreshUICharacterInfo();
        }
        else
        {
            Debug.LogError("CharacterData is null");
        }
    }

    private void LevelUpCharacter()
    {
        var coinsLeft = GameManager.Instance.UserData.Coins - _upgradePrice;
        GameManager.Instance.UserData.Coins = coinsLeft;
        var characterData = GameManager.Instance.CharactersData[_character];
        _level = characterData.Level += 1;

        if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.UpgradeHero))
        {
            UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.UpgradeHero, null, 1);
        }

        if (_level == 5)
        {
            if (AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementsTypes.LevelUpToFive))
            {
                UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh?.Invoke(AchievementsTypes.LevelUpToFive, 1);
            }
        }

        if (AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementsTypes.LevelUp))
        {
            UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh?.Invoke(AchievementsTypes.LevelUp, 1);
        }

        UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(coinsLeft);
        
        var characterLevelBuffsStartWorking = _level - 1;
        
        var newCoinPercentReward = CharacterBuffConfig.Instance.AdditionalPercentCoinRewardPerLevel * characterLevelBuffsStartWorking;

        characterData.AdditionalPercentCoinReward = newCoinPercentReward;
        characterData.Level = _level;

        GameManager.Instance.CharactersData[_character] = characterData;

        if (PowerUpConfig.Instance.PowerUpAppearance.TryGetValue(_character, out var powerUpData))
        {
            foreach (var powerUpElements in powerUpData.Elements)
            {
                switch (powerUpElements.value)
                {
                    case Enums.PowerUps.Nothing:
                        powerUpElements.chance -= 0.05f;
                        break;
                    case Enums.PowerUps.WaffleShield:
                    case Enums.PowerUps.Magnet:
                    case Enums.PowerUps.ChillBlast:
                    case Enums.PowerUps.GoldSpoon:
                    case Enums.PowerUps.HundAThousands:
                        powerUpElements.chance += 0.01f;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            powerUpData.Normalize();
        }

        PlayerPrefs.SetInt(_character.ToString(), _level);
        PlayerPrefs.SetInt(GameManager.CoinsNameKey, coinsLeft);
        
        PlayerPrefs.SetFloat(_character + GameManager.AddPercentCoinRewardKey, newCoinPercentReward);

        if (CharacterConfig.Instance.CharacterData.TryGetValue(_character, out var characterPreset))
        {
            UIManager.Instance.CharactersSection.UpgradeScene.gameObject.SetActive(true);
            UIManager.Instance.CharactersSection.UpgradeScene.Initialize(characterPreset, _level);
        }
        
        RefreshUICharacterInfo();
        RefreshUpgradePrice();

        if (_level == 5)
        {
            SetUpgradeButtonToMaxLevel();
        }
    }

    public void ChangeCharacterSkinIcon(Characters character, SkinEnum skinEnum)
    {
        if (!CharacterConfig.Instance.CharacterData.TryGetValue(character, out var characterPreset)) return;
        
        foreach (var skinData in characterPreset.skinData.Where(skinData => skinData.skinEnum == skinEnum))
        {
            characterSkinIcon.sprite = skinData.characterSkinMenuSprite;
        }
    }

    private void RefreshUICharacterInfo()
    {
        UpdateLevel();
        
        if (CharacterConfig.Instance.CharacterData.TryGetValue(_character, out var characterPreset))
        {
            characterSkinIcon.sprite = characterPreset.fullRectIcon;
            levelLabel.text = "Level " + _level;

            if (_level > 1)
            {
                for (var i = 1; i < _level; i++)
                {
                    var ability = characterPreset.abilityList[i];
                    abilitiesUI[i - 1].Initialize(ability, i);
                }
            }
            for (var i = 0; i < _level; i++)
            {
                characterLevelStars[i].FullStar();
            }
        }
        else
        {
            Debug.LogError("CharacterPreset is null");
        }
    }

    private void ShowErrorPopup()
    {
        UIManager.Instance.MainMenuScreen.OpenShopSection();
        UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.NotEnoughCoins);
    }

    private void RefreshUpgradePrice()
    {
        _upgradePrice = _level * CharacterConfig.Instance.UpgradePricePerLevel * 2;
        upgradePrice.text = _upgradePrice.ToString();
    }

    private void OnDisable()
    {
        upgradeButton.onClick.RemoveListener(UpgradeCharacter);
        skinButton.onClick.RemoveListener(SkinChangerFadeIn);
        
        levelUpAnimator.gameObject.SetActive(false);
        objectMask.SetActive(false);
        mask.enabled = false;
        
        if (_upgradeAnimationCoroutine != null)
        {
            StopCoroutine(_upgradeAnimationCoroutine);
            _upgradeAnimationCoroutine = null;
        }
    }
}
