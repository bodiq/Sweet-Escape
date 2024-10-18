using System.Collections;
using Audio;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HUDScreen : UIScreen
    {
        [SerializeField] private Button pauseButton;
        
        [SerializeField] private TextMeshProUGUI coinsCount;
        [SerializeField] private TextMeshProUGUI pointsCount;
        [SerializeField] private TextMeshProUGUI coinsCountNewBar;

        [SerializeField] private Animator pointsBox;
        [SerializeField] private Animator coinsBox;

        [SerializeField] private UIPowerUpsManager uiPowerUpsManager;
        
        [SerializeField] private CanvasGroup freezeScreenObject;
        
        [SerializeField] private float durationFreezeCanvasFadeIn;
        [SerializeField] private float durationFreezeCanvasFadeOut;

        [SerializeField] private CoinUIAnimation coinUIAnimation;

        private const string BarAnimationNameState = "BarAnimation";
        private const string BarAnimationNameClip = "CoinBarAnimation";

        private PauseMenuScreen _pauseMenuScreen;
        private LostScreenUI _lostScreenUI;
        
        private AudioSource _audioSource;

        private int _coinsCount;
        private float _pointCount;
        private int _pointsCountRounded;

        private float _coinBarAnimationTimeStop;
        private float _sprinkleBarAnimationTimeStop;
        private float _durationBarAnimation;

        private WaitForSeconds _waitForBarAnimation;

        private Coroutine _activateCoinBarCoroutine;

        public UIPowerUpsManager UIPowerUpsManager => uiPowerUpsManager;

        private void Awake()
        {
            var clips = coinsBox.runtimeAnimatorController.animationClips;

            foreach (var clip in clips)
            {
                if (clip.name == BarAnimationNameClip)
                {
                    _durationBarAnimation = clip.length;
                }
            }

            _waitForBarAnimation = new WaitForSeconds(_durationBarAnimation);
        }

        private void Start()
        {
            _pauseMenuScreen = UIManager.Instance.GetUIScreen<PauseMenuScreen>();
            _lostScreenUI = UIManager.Instance.GetUIScreen<LostScreenUI>();
        }

        private void OnEnable()
        {
            GameManager.Instance.OnPlayerRespawn += OnRespawn;
            GameManager.Instance.OnPlayerDeath += OnPlayerDeath;

            pauseButton.onClick.AddListener(PauseGame);

            TurnOnInGameMusic();

            _coinsCount = GameManager.Instance.UserData.Coins;
            _pointCount = 0;
            
            pointsCount.text = _pointCount.ToString();
            coinsCount.text = _coinsCount.ToString();
            coinsCountNewBar.text = _coinsCount.ToString();
        }

        public void TurnOnInGameMusic()
        {
            if (_audioSource)
            {
                if (!_audioSource.isPlaying)
                {
                    _audioSource = AudioManager.Instance.PlayMusic(Audio.AudioType.InGameBackgroundMusic, true);
                }
            }
            else
            {
                _audioSource = AudioManager.Instance.PlayMusic(Audio.AudioType.InGameBackgroundMusic, true);
            }
        }

        private void OnPlayerDeath()
        {
            TurnOffInGameMusic();
            _lostScreenUI.RefreshCoinsField(GameManager.Instance.player.coinsPerOneRun);
            _lostScreenUI.RefreshPointsField(_pointsCountRounded);
            _lostScreenUI.OnPlayerDeath();
        }
        

        private void TurnOffInGameMusic()
        {
            if (_audioSource)
            {
                AudioManager.Instance.Stop(_audioSource);
            }
        }

        private void OnDisable()
        {
            GameManager.Instance.OnPlayerRespawn -= OnRespawn;
            GameManager.Instance.OnPlayerDeath -= OnPlayerDeath;

            pauseButton.onClick.RemoveListener(PauseGame);
        
            if (_audioSource != null)
            {
                AudioManager.Instance.Stop(_audioSource);
                _audioSource = null;
            }

            if (_activateCoinBarCoroutine != null)
            {
                StopCoroutine(_activateCoinBarCoroutine);
                _activateCoinBarCoroutine = null;
            }

            _coinBarAnimationTimeStop = 0f;
            _sprinkleBarAnimationTimeStop = 0f;
            
            coinUIAnimation.gameObject.SetActive(false);
            freezeScreenObject.alpha = 0f;
        }

        public void RefreshCoinsCount(int amount)
        {
            GameManager.Instance.OnGetCoin?.Invoke();
            _coinsCount += amount;
            coinsCount.text = _coinsCount.ToString();
            coinsCountNewBar.text = _coinsCount.ToString();
        }

        public void IncreasePointsCount(float count)
        {
            _pointCount += count;
            _pointsCountRounded = Mathf.FloorToInt(_pointCount);
            pointsCount.text = _pointsCountRounded.ToString();
        }

        public void TurnFreezeScreen(bool isActive)
        {
            if (isActive)
            {
                freezeScreenObject.DOFade(1, durationFreezeCanvasFadeIn);
            }
            else
            {
                freezeScreenObject.DOFade(0, durationFreezeCanvasFadeOut);
            }
        }

        private void OnRespawn()
        {
            UIPowerUpsManager.TurnAllPowerUpsCount(false);
            RefreshPointsCount();
            TurnOnInGameMusic();
            
            _coinBarAnimationTimeStop = 0f;
            _sprinkleBarAnimationTimeStop = 0f;
            
            PlayAnimationBar();
            freezeScreenObject.alpha = 0f;
        }

        private void RefreshPointsCount()
        {
            _pointCount = 0;
            _pointsCountRounded = 0;
            pointsCount.text = _pointCount.ToString();
        }

        public void SetCoinsAmount(int count)
        {
            _coinsCount = count;
            coinsCount.text = _coinsCount.ToString();
            coinsCountNewBar.text = _coinsCount.ToString();
        }

        private void Update()
        {
            if (!IsOn)
            {
                return;
            }

            /*if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }*/
        }

        public void PlayAnimationBar()
        {
            pointsBox.Play(BarAnimationNameState, 0, _sprinkleBarAnimationTimeStop);
            coinsBox.Play(BarAnimationNameState, 0, _coinBarAnimationTimeStop);

            _activateCoinBarCoroutine = StartCoroutine(ActivateCoinObject());
        }

        private IEnumerator ActivateCoinObject()
        {
            yield return _waitForBarAnimation;
            coinsBox.gameObject.SetActive(false);
            coinUIAnimation.gameObject.SetActive(true);
            _activateCoinBarCoroutine = null;
        }

        public void LoadGameScene()
        {
            TurnOn();
            TurnUpperSection(true);
            PlayAnimationBar();
        }

        public void TurnOffHudScreenAndSaveData()
        {
            _coinBarAnimationTimeStop = coinsBox.GetCurrentAnimatorStateInfo(0).normalizedTime > _durationBarAnimation ? 0f : coinsBox.GetCurrentAnimatorStateInfo(0).normalizedTime;
            _sprinkleBarAnimationTimeStop = pointsBox.GetCurrentAnimatorStateInfo(0).normalizedTime > _durationBarAnimation ? 0f : pointsBox.GetCurrentAnimatorStateInfo(0).normalizedTime;
            
            UIPowerUpsManager.StopAllNumericAnimations();
            TurnUpperSection(false);
        }

        private void PauseGame()
        {
            TurnOffHudScreenAndSaveData();
            
            GameManager.Instance.OnStopGame?.Invoke();
            _pauseMenuScreen.TurnOn();
            _pauseMenuScreen.RefreshCoinsField(GameManager.Instance.player.coinsPerOneRun);
            _pauseMenuScreen.RefreshPointsField(_pointsCountRounded);
        }

        public void TurnUpperSection(bool isActive)
        {
            pauseButton.gameObject.SetActive(isActive);
            coinsBox.gameObject.SetActive(isActive);
            pointsBox.gameObject.SetActive(isActive);
            uiPowerUpsManager.gameObject.SetActive(isActive);
            coinUIAnimation.gameObject.SetActive(false);
        }
    }
}