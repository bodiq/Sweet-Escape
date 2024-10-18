using System.Collections;
using System.Linq;
using Audio;
using DG.Tweening;
using Enums;
using Managers;
using UnityEngine;
using AudioType = Audio.AudioType;

namespace UI
{
    public class MainMenuScreen : UIScreen
    {
        [SerializeField] private GameObject factoryShopTittleBackground;
        [SerializeField] private GameObject oldTVScanlinesGameObject;
        [SerializeField] private CanvasGroup canvasGroup;

        private SettingsScreen _settingsScreen;
        private HUDScreen _hudScreen;
        private DailyRewardManager _dailyLogin;
        private StartScreen _startScreen;
        private MainUpperSection _mainUpperSection;
        private MainDownSection _mainDownSection;
        private GameModeSelection _gameModeSelection;
        private CharactersSection _charactersSection;
        private LeaderboardDailySectionManager _rankingSection;
        private ShopSection _shopSection;
        private SettingsSection _settingsSection;
        private DoubleCoinsPopup _doubleCoinsPopup;

        private AudioSource _audioSource;

        private UIScreen _lastOpenedUISection;

        public MainDownSection MainDownSection => _mainDownSection;
        public MainUpperSection MainUpperSection => _mainUpperSection;
        public DoubleCoinsPopup DoubleCoinsPopup => _doubleCoinsPopup;

        public GameObject FactoryShopTittleBackground => factoryShopTittleBackground;
        
        private Coroutine _shopOpeningCoroutine;

        private float _lastCoinCount;

        private const float HeightOfDefaultFactoryShopBackgroundState = 375f;

        private Vector3 _topShopBackgroundPos;
        private Vector3 _defaultShopBackgroundPos;
        private Vector3 _downShopBackgroundPos;
        
        private bool _hasAnimationDone;

        public CanvasGroup MainMenuScreenCanvas => canvasGroup;

        protected override void Awake()
        {
            base.Awake();

            _settingsScreen = UIManager.Instance.GetUIScreen<SettingsScreen>();
            _hudScreen = UIManager.Instance.GetUIScreen<HUDScreen>();
            _startScreen = UIManager.Instance.GetUIScreen<StartScreen>();
            _mainUpperSection = UIManager.Instance.GetUIScreen<MainUpperSection>();
            _dailyLogin = UIManager.Instance.GetUIScreen<DailyRewardManager>();
            _mainDownSection = UIManager.Instance.GetUIScreen<MainDownSection>();
            _gameModeSelection = UIManager.Instance.GetUIScreen<GameModeSelection>();
            _charactersSection = UIManager.Instance.GetUIScreen<CharactersSection>();
            _rankingSection = UIManager.Instance.LeaderboardDailySectionManager;
            _shopSection = UIManager.Instance.GetUIScreen<ShopSection>();
            _settingsSection = UIManager.Instance.GetUIScreen<SettingsSection>();
            _doubleCoinsPopup = UIManager.Instance.GetUIScreen<DoubleCoinsPopup>();
        }

        private void Start()
        {
            _startScreen.TurnOn();
            
            _audioSource = AudioManager.Instance.PlayMusic(AudioType.MainMenuBackgroundMusic, true);
            AudioManager.Instance.mainBackgroundAudioSource = _audioSource;
            
            var rect = UIManager.Instance.RectTransform.rect;

            _defaultShopBackgroundPos = factoryShopTittleBackground.transform.localPosition;
            
            _topShopBackgroundPos = new Vector3(_defaultShopBackgroundPos.x, _defaultShopBackgroundPos.y + HeightOfDefaultFactoryShopBackgroundState, _defaultShopBackgroundPos.z);
            _downShopBackgroundPos = new Vector3(_defaultShopBackgroundPos.x, _defaultShopBackgroundPos.y - rect.height, _defaultShopBackgroundPos.z);
            ResetFactoryBackgroundPos();
        }

        private void OnEnable()
        {
            GameManager.Instance.OnCharacterSkinChange += ChangeCharacterSkinIcon;
            GameManager.Instance.ResetAnimationInfo += ResetAnimationInfo;
        }

        private void ResetAnimationInfo()
        {
            _hasAnimationDone = false;
        }

        private void ChangeCharacterSkinIcon(Characters character, SkinEnum skinEnum)
        {
            _gameModeSelection.CharacterSkinChanged(character, skinEnum);
            CharacterSelectManager.Instance.LastOpenedCharacterUI.ChangeCharacterSkinIcon(character, skinEnum);
        }
        
        public void TurnScanLinesBackground(bool isOn)
        {
            oldTVScanlinesGameObject.SetActive(isOn);
        }

        private void ResetFactoryBackgroundPos()
        {
            factoryShopTittleBackground.transform.localPosition = _topShopBackgroundPos;
        }

        public void StartGame(Characters character)
        {
            UIManager.Instance.DoorTransition.TurnOn();
            UIManager.Instance.DoorTransition.CloseDoor();
            TurnOff();
            UIManager.Instance.PopupManager.TurnOffAllPopups();
            GameManager.Instance.LoadScene(character);
        }

        public void OpenDailyLogin()
        {
            if (_lastOpenedUISection)
            {
                _lastOpenedUISection.TurnOff();
            }
            
            _lastOpenedUISection = _dailyLogin;
            
            _dailyLogin.TurnOn();
            _dailyLogin.StartUIShow();
        }

        public void ChangeUsername(string username)
        {
            _mainUpperSection.SetUsername(username);
        }

        public void ChangeCoinsAmount(int coinsCount, bool isInitialize = false, bool isSpending = true)
        {
            if (coinsCount > _lastCoinCount && !isInitialize)
            {
                GameManager.Instance.OnGetCoin?.Invoke();
            }
            else if (coinsCount < _lastCoinCount && isSpending)
            {
                var spentMoney = _lastCoinCount - coinsCount;
                if (AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementsTypes.SpendCoins))
                {
                    UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh?.Invoke(AchievementsTypes.SpendCoins, (int)spentMoney);
                }
            }
            _mainUpperSection.ChangeCoinsCount(coinsCount);
            _lastCoinCount = coinsCount;
        }

        public void CloseButtonSections()
        {
            _charactersSection.TurnOff();
            _rankingSection.TurnOff();
            _shopSection.TurnOff();
            _settingsSection.TurnOff();
        }

        public void OpenCharactersSection()
        {
            if (_lastOpenedUISection)
            {
                _lastOpenedUISection.TurnOff();
            }

            AudioManager.Instance.PlaySFX(AudioType.MenuSwap);
            
            _lastOpenedUISection = _charactersSection;
            
            _gameModeSelection.TurnOff();
            _charactersSection.TurnOn();
            _mainDownSection.TurnOnCharacter();
        }

        public void OpenRankingSection()
        {
            if (_lastOpenedUISection)
            {
                _lastOpenedUISection.TurnOff();
            }
            
            AudioManager.Instance.PlaySFX(AudioType.MenuSwap);
            
            _lastOpenedUISection = _rankingSection;
            
            _gameModeSelection.TurnOff();
            _rankingSection.TurnOn();
            _mainDownSection.TurnOnRanking();
        }
        
        public void OpenShopSection()
        {
            if (_lastOpenedUISection == _shopSection)
            {
                return;
            }

            factoryShopTittleBackground.SetActive(true);
            _mainDownSection.TurnOnShop();
            AudioManager.Instance.PlaySFX(AudioType.MenuSwap);

            if (!_hasAnimationDone)
            {
                ResetFactoryBackgroundPos();
                _shopOpeningCoroutine = StartCoroutine(OpeningShopCoroutine());
            }
            else
            {
                if (_lastOpenedUISection)
                {
                    _lastOpenedUISection.TurnOff();
                }
            
                _shopSection.TurnOn();
                AudioManager.Instance.PlaySFX(AudioType.ShopOpen);
                
                _lastOpenedUISection = _shopSection;
            
                _gameModeSelection.TurnOff();
            }
        }

        private IEnumerator OpeningShopCoroutine()
        {
            canvasGroup.blocksRaycasts = false;
            
            var hierarchyIndex = factoryShopTittleBackground.transform.GetSiblingIndex();
            factoryShopTittleBackground.transform.SetSiblingIndex(hierarchyIndex + 2);
            
            _hasAnimationDone = true;
            factoryShopTittleBackground.transform.DOLocalMove(_downShopBackgroundPos, 1.0f);

            yield return new WaitForSeconds(1.0f);
            
            if (_lastOpenedUISection)
            {
                _lastOpenedUISection.TurnOff();
            }

            _shopSection.TurnOn();
            
            _lastOpenedUISection = _shopSection;
            
            _gameModeSelection.TurnOff();
            _gameModeSelection.ResetUpperSectionPos();

            AudioManager.Instance.PlaySFX(AudioType.ShopOpen);

            factoryShopTittleBackground.transform.DOLocalMove(_defaultShopBackgroundPos, 1f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _shopSection.StartUIShow();
                canvasGroup.blocksRaycasts = true;
            });
            
            factoryShopTittleBackground.transform.SetSiblingIndex(hierarchyIndex - 2);
            
            yield return new WaitForSeconds(0.7f);
            
            _gameModeSelection.StartUpperSectionUIShow();
        }
        
        public void OpenSettingsSection()
        {
            if (_lastOpenedUISection)
            {
                _lastOpenedUISection.TurnOff();
            }
            
            AudioManager.Instance.PlaySFX(AudioType.MenuSwap);
            
            _lastOpenedUISection = _settingsSection;
            
            _gameModeSelection.TurnOff();
            _settingsSection.TurnOn();
            _mainDownSection.TurnOnSettings();
        }

        public void ChangeSelectedCharacter(Characters character)
        {
            _gameModeSelection.ChangeSelectedCharacter(character);
        }

        public void TurnOnGameModeSelection(bool isFromMatch = false)
        {
            TurnOn();
            _dailyLogin.TurnOff();
            _mainDownSection.TurnOn();
            _mainUpperSection.TurnOn();
            _gameModeSelection.TurnOn();

            _lastOpenedUISection = _gameModeSelection;

            if (_audioSource == null || !_audioSource.isPlaying)
            {
                _audioSource = AudioManager.Instance.PlayMusic(AudioType.MainMenuBackgroundMusic, true);
                AudioManager.Instance.mainBackgroundAudioSource = _audioSource;
            }

            if (isFromMatch)
            {
                _doubleCoinsPopup.TurnOn();
            }
        }

        private void OnDisable()
        {
            if (_audioSource != null)
            {
                AudioManager.Instance.Stop(_audioSource);
                _audioSource = null;
            }
            
            GameManager.Instance.OnCharacterSkinChange -= ChangeCharacterSkinIcon;
            
            if (_shopOpeningCoroutine == null) return;
                
            StopCoroutine(_shopOpeningCoroutine);
            _shopOpeningCoroutine = null;
            
            var hierarchyIndex = factoryShopTittleBackground.transform.GetSiblingIndex();
            factoryShopTittleBackground.transform.SetSiblingIndex(hierarchyIndex + 2);
        }
    }
}