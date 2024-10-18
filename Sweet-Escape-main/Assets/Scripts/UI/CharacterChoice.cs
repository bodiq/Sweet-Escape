using System.Linq;
using Audio;
using Configs;
using DG.Tweening;
using Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AudioType = Audio.AudioType;

namespace UI
{
    public class CharacterChoice : MonoBehaviour
    {
        [SerializeField] private Characters character;
        [SerializeField] private Button button;
        [SerializeField] private GameObject selectedState;
        [SerializeField] private Image mainButtonImage;
        [SerializeField] private TextMeshProUGUI characterNameField;
        [SerializeField] private TextMeshProUGUI characterPriceField;
        [SerializeField] private GameObject characterPriceObject;

        public Characters Character => character;
        
        private const float EndScaleVectorYValue = 0.8f;
        private const float AnimationDurationValue = 0.2f;

        private Tween _animationScaleTween;

        private void Start()
        {
            if (!CharacterConfig.Instance.CharacterData.TryGetValue(character, out var preset)) return;
            
            if (PlayerPrefs.GetInt(character.ToString()) > 0)
            {
                mainButtonImage.sprite = preset.characterSelectionIconSprite;
                characterNameField.gameObject.SetActive(true);
                characterNameField.text = preset.name;
                characterPriceObject.SetActive(false);
                characterPriceField.text = preset.priceToUnlock.ToString();
            }
            else
            {
                characterNameField.gameObject.SetActive(false);
                characterPriceObject.SetActive(true);
                characterPriceField.text = preset.priceToUnlock.ToString();
            }
        }

        private void OnEnable()
        {
            button.onClick.AddListener(ChooseCharacter);
        }

        private void ChooseCharacter()
        {
            if (PlayerPrefs.GetInt(character.ToString()) > 0)
            {
                if (!selectedState.gameObject.activeSelf)
                {
                    if (!_animationScaleTween.IsActive())
                    {
                        _animationScaleTween = transform.DOScaleX(EndScaleVectorYValue, AnimationDurationValue).SetEase(Ease.OutElastic).OnComplete(() =>
                        {
                            _animationScaleTween = transform.DOScaleX(1f, AnimationDurationValue).SetEase(Ease.OutElastic).SetAutoKill(true);

                        });
                    }
                }
                
                TurnOnSelectedState();
                CharacterSelectManager.Instance.CharacterChanged?.Invoke(this);
            }
            else
            {
                if (CharacterConfig.Instance.CharacterData.TryGetValue(character, out var characterPreset))
                {
                    var price = characterPreset.priceToUnlock;

                    if (GameManager.Instance.UserData.Coins >= price)
                    {
                        var coinsLeft = GameManager.Instance.UserData.Coins -= price;
                        PlayerPrefs.SetInt(GameManager.CoinsNameKey, coinsLeft);
                        PlayerPrefs.SetInt(character.ToString(), 1);
                        GameManager.Instance.CharactersData[character].Level = 1;
                        
                        TurnOnSelectedState();

                        characterNameField.gameObject.SetActive(true);
                        characterNameField.text = characterPreset.name;
                        characterPriceObject.SetActive(false);

                        mainButtonImage.sprite = characterPreset.characterSelectionIconSprite;
                        PlayerPrefs.SetInt(characterPreset.skinData[0].skinEnum.ToString(), 1);
                        UIManager.Instance.CharactersSection.SkinSelection.Initialize(character);
                        UIManager.Instance.ShopSection.RefreshPlayerSkinsData();
                        UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(coinsLeft);

                        AudioManager.Instance.PlaySFX(AudioType.CollectCoinUI);
                        
                        var pooledFeedbackPopup = ObjectUIPool.Instance.GetPooledFeedbackPopup();
                        if (pooledFeedbackPopup)
                        {
                            pooledFeedbackPopup.transform.position = button.transform.position;
                            pooledFeedbackPopup.gameObject.SetActive(true);
                            pooledFeedbackPopup.StartSelfDestroyCoroutine();
                        }
                        
                        CharacterSelectManager.Instance.CharacterChanged?.Invoke(this);

                        if (AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementsTypes.UnlockCharacter))
                        {
                            UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh?.Invoke(AchievementsTypes.UnlockCharacter, 1);
                        }
                    }
                    else
                    {
                        //TODO: Send to shop
                    }
                }
            }
        }

        public void TurnOffSelectedState()
        {
            selectedState.SetActive(false);
        }

        public void TurnOnSelectedState()
        {
            selectedState.SetActive(true);
        }
    }
}
