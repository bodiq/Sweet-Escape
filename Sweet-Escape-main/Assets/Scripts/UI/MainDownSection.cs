using System;
using System.Collections.Generic;
using Audio;
using Enums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using AudioType = Audio.AudioType;

namespace UI
{
    public class MainDownSection : UIScreen
    {
        [Serializable]
        public struct ButtonPair
        {
            public SectionButton Key;
            public Button Value;
        }

        [SerializeField] private List<ButtonPair> _buttons;
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject achievementSignalObject;

        private Button _characterButton;
        private Button _rankingButton;
        private Button _shopButton;
        private Button _settingsButton;

        private Button _previousOpenSection;

        private void OnEnable()
        {
            var mainMenuScreen = UIManager.Instance.MainMenuScreen;
            foreach (var key in _buttons)
            {
                switch (key.Key)
                {
                    case SectionButton.Characters:
                        _characterButton = key.Value;
                        _characterButton.onClick.AddListener(mainMenuScreen.OpenCharactersSection);
                        break;
                    case SectionButton.Ranking:
                        _rankingButton = key.Value;
                        _rankingButton.onClick.AddListener(mainMenuScreen.OpenRankingSection);
                        break;
                    case SectionButton.Shop:
                        _shopButton = key.Value;
                        _shopButton.onClick.AddListener(mainMenuScreen.OpenShopSection);
                        break;
                    case SectionButton.Settings:
                        _settingsButton = key.Value;
                        _settingsButton.onClick.AddListener(mainMenuScreen.OpenSettingsSection);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            backButton.onClick.AddListener(ReturnToGameModeSelection);
            
            if (PlayerPrefs.HasKey(GameManager.AvailableClaimableAchievementsKey))
            {
                AchievementBoxManager.CountOfAvailableClaimableAchievements = PlayerPrefs.GetInt(GameManager.AvailableClaimableAchievementsKey);
            }
            else
            {
                PlayerPrefs.SetInt(GameManager.AvailableClaimableAchievementsKey, 0);
            }

            UIManager.Instance.MainDownSection.TurnAchievementClaimableItem(AchievementBoxManager.CountOfAvailableClaimableAchievements > 0);
        }

        private void ReturnToGameModeSelection()
        {
            AudioManager.Instance.PlaySFX(AudioType.MenuSwap);
            backButton.gameObject.SetActive(false);
            _previousOpenSection.gameObject.SetActive(true);
            UIManager.Instance.MainMenuScreen.CloseButtonSections();
            UIManager.Instance.MainMenuScreen.TurnOnGameModeSelection();
        }
        
        public void TurnOnCharacter()
        {
            RefreshButtonState(_characterButton);
        }

        public void TurnAchievementClaimableItem(bool isActive)
        {
            achievementSignalObject.SetActive(isActive);
        }
        
        public void TurnOnRanking()
        {
            RefreshButtonState(_rankingButton);
        }

        public void TurnOnShop()
        {
            RefreshButtonState(_shopButton);
        }

        public void TurnOnSettings()
        {
            RefreshButtonState(_settingsButton);
        }

        private void RefreshButtonState(Button button)
        {
            if (_previousOpenSection != null)
            {
                _previousOpenSection.gameObject.SetActive(true);
            }
            backButton.gameObject.SetActive(true);
            button.gameObject.SetActive(false);
            _previousOpenSection = button;
        }

        private void OnDisable()
        {
            var mainMenuScreen = UIManager.Instance.MainMenuScreen;
            foreach (var key in _buttons)
            {
                switch (key.Key)
                {
                    case SectionButton.Characters:
                        _characterButton = key.Value;
                        _characterButton.onClick.RemoveListener(mainMenuScreen.OpenCharactersSection);
                        break;
                    case SectionButton.Ranking:
                        _rankingButton = key.Value;
                        _rankingButton.onClick.RemoveListener(mainMenuScreen.OpenRankingSection);
                        break;
                    case SectionButton.Shop:
                        _shopButton = key.Value;
                        _shopButton.onClick.RemoveListener(mainMenuScreen.OpenShopSection);
                        break;
                    case SectionButton.Settings:
                        _settingsButton = key.Value;
                        _settingsButton.onClick.RemoveListener(mainMenuScreen.OpenSettingsSection);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            backButton.onClick.RemoveListener(ReturnToGameModeSelection);
        }
    }
}
