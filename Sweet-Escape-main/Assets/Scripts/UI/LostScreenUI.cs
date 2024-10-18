using System.Collections;
using System.Linq;
using API;
using Audio;
using Configs;
using DG.Tweening;
using Enums;
using Sirenix.Utilities;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AudioType = Audio.AudioType;

public class LostScreenUI : UIScreen
{
    [SerializeField] private Button exitButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button payToContinueButton;
    [SerializeField] private Button watchToContinueButton;

    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI coinCostToRevive;

    [SerializeField] private TextMeshProUGUI countDown;

    [SerializeField] private GameObject lostScreenObjects;

    [SerializeField] private CanvasGroup redBackground;
    [SerializeField] private CanvasGroup payToContinueCanvas;

    private AudioSource _audioSource;

    private HUDScreen _hudScreen;

    private int _coins;
    private int _points;

    private readonly int _startNumberForCountdown = 3;

    private int _priceToRevive = 200;

    private Coroutine _countDownCoroutine;

    private readonly WaitForSecondsRealtime _waitForDoorTransEnd = new(1);

    private void Start()
    {
        _hudScreen = UIManager.Instance.GetUIScreen<HUDScreen>();
        AdsManager.Instance.LoadRewardedAd();
    }

    private void OnEnable()
    {
        restartButton.onClick.AddListener(RestartPlay);
        exitButton.onClick.AddListener(Exit);

        GameManager.Instance.OnPlayerRespawn += OnPlayerRespawn;

        watchToContinueButton.onClick.AddListener(WatchAdvToRevive);
        payToContinueButton.onClick.AddListener(PayToContinue);
    }

    private void OnDisable()
    {
        restartButton.onClick.RemoveListener(RestartPlay);
        exitButton.onClick.RemoveListener(Exit);

        GameManager.Instance.OnPlayerRespawn -= OnPlayerRespawn;

        watchToContinueButton.onClick.RemoveListener(WatchAdvToRevive);
        payToContinueButton.onClick.RemoveListener(PayToContinue);
    }

    private void PayToContinue()
    {
        if (_coins >= _priceToRevive)
        {
            var prevCoinCount = _coins;
            _coins -= _priceToRevive;
            _priceToRevive += CharacterConfig.Instance.StepPriceToRevive;
            _hudScreen.SetCoinsAmount(_coins);

            AudioManager.Instance.PlaySFX(AudioType.CollectCoinUI);
            
            var pooledFeedbackPopup = ObjectUIPool.Instance.GetPooledFeedbackPopup();
            if (pooledFeedbackPopup)
            {
                pooledFeedbackPopup.transform.position = payToContinueButton.transform.position;
                pooledFeedbackPopup.gameObject.SetActive(true);
                pooledFeedbackPopup.StartSelfDestroyCoroutine();
            }
            
            GameManager.Instance.player.ReviveAtThePosition();

            if (AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementsTypes.SpendCoins))
            {
                UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh?.Invoke(AchievementsTypes.SpendCoins, prevCoinCount - _coins);
            }

            StartCountDownCoroutine();
            SendAchievementCallback();
        }
    }

    private void SendAchievementCallback()
    {
        if (AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementsTypes.Revive))
        {
            UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh?.Invoke(
                AchievementsTypes.Revive, 1);
        }
    }

    private void Exit()
    {
        //SceneManager.LoadScene(0);
        StartCoroutine(LoadSceneAsync());
        UIManager.Instance.DoorTransition.TurnOn();
        UIManager.Instance.DoorTransition.CloseDoor();

        if (!GameManager.Instance.Enemies.IsNullOrEmpty())
        {
            GameManager.Instance.Enemies.Clear();
        }
        
        TilemapManager.Instance.ResetParallaxData();

        if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.PlayArcadeRun))
        {
            UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(
                DailyMissions.PlayArcadeRun, null, 1);
        }

        if (GameManager.Instance.countPowerUpsTakenPerRun > 0)
        {
            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.GetPowerUpsInOneRun))
            {
                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(
                    DailyMissions.GetPowerUpsInOneRun, null, GameManager.Instance.countPowerUpsTakenPerRun);
            }

            GameManager.Instance.countPowerUpsTakenPerRun = 0;
        }

        if (Random.value <= 0.01f && GameManager.Instance.SelectedCharacter == Characters.Meltie)
        {
            PlayerPrefs.SetInt(SkinEnum.MeltieRocketFuel.ToString(), 1);
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator LoadSceneAsync()
    {
        yield return _waitForDoorTransEnd;

        var asyncLoad = SceneManager.LoadSceneAsync(1);
        while (!asyncLoad.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        _hudScreen.UIPowerUpsManager.TurnAllPowerUpsCount(false);
        _hudScreen.UIPowerUpsManager.TurnOffAllNumericAnimations();
        _hudScreen.TurnOff();

        TurnMusic(true);
        TurnSFX(true);

        if (_countDownCoroutine != null)
        {
            StopCoroutine(_countDownCoroutine);
            _countDownCoroutine = null;
        }

        lostScreenObjects.SetActive(false);
        StopTheMusic();
        ResetToDefaultLostScreenSettings();

        UIManager.Instance.MainMenuScreen.TurnOnGameModeSelection(true);
        UIManager.Instance.DoorTransition.OpenDoor();
    }

    private void TurnMusic(bool isEnabled)
    {
        AudioManager.Instance.TurnAllMusic(isEnabled);
    }

    private void TurnSFX(bool isEnabled)
    {
        AudioManager.Instance.TurnAllSFX(isEnabled);
    }

    public void StartCountDownCoroutine()
    {
        _countDownCoroutine = StartCoroutine(CountDownCoroutine());
    }

    private IEnumerator CountDownCoroutine()
    {
        var maxNumber = _startNumberForCountdown;

        lostScreenObjects.SetActive(false);

        countDown.gameObject.SetActive(true);
        countDown.text = maxNumber.ToString();

        GameManager.Instance.player.ableToMove = false;

        while (maxNumber > 0)
        {
            AudioManager.Instance.PlaySFX(AudioType.CountDownBeep);
            yield return new WaitForSecondsRealtime(1);

            maxNumber--;
            countDown.text = maxNumber.ToString();
        }
        
        GameManager.Instance.player.ableToMove = true;

        Time.timeScale = 1f;
        GameManager.Instance.OnResumeGame?.Invoke();

        FadeOutRedBackground();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        countDown.gameObject.SetActive(false);

        StopTheMusic();
        _hudScreen.TurnOnInGameMusic();

        _hudScreen.TurnUpperSection(true);
        _hudScreen.UIPowerUpsManager.ResumeAllNumericAnimations();
        _hudScreen.UIPowerUpsManager.TurnAllPowerUpsCount(true);
        _hudScreen.PlayAnimationBar();
    }

    private void ResetToDefaultLostScreenSettings()
    {
        Time.timeScale = 1f;
        redBackground.alpha = 0f;
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        lostScreenObjects.SetActive(false);

        _priceToRevive = CharacterConfig.Instance.DefaultPriceToRevive;
        coinCostToRevive.text = _priceToRevive.ToString();
    }

    public void FadeInRedBackground()
    {
        redBackground.DOFade(1f, 0.8f);
        _canvasGroup.DOFade(1f, 0.8f);
    }

    private void FadeOutRedBackground()
    {
        redBackground.DOFade(0f, 0.5f);
        _canvasGroup.DOFade(0f, 0.5f);
    }

    public void RefreshPointsField(int count)
    {
        _points = count;
        pointsText.text = _points.ToString();
        UIManager.Instance.GameModeSelection.UpdateScore(_points);

        if (GameManager.Instance.SelectedCharacter == Characters.Kermit && _points >= 15000)
        {
            PlayerPrefs.SetInt(SkinEnum.KermitToxic.ToString(), 1);
        }

        if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.CollectSprinkleOneRun))
        {
            UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.CollectSprinkleOneRun, null, _points);
        }

        if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.CollectSprinklesTotal))
        {
            UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.CollectSprinklesTotal, null, _points);
        }

        if (AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementsTypes.GodMode))
        {
            UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh?.Invoke(AchievementsTypes.GodMode, _points);
        }

        if (AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementsTypes.ScoreOneRun))
        {
            UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh?.Invoke(AchievementsTypes.ScoreOneRun, _points);
        }

        if (AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementsTypes.CollectSprinkles))
        {
            UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh?.Invoke(AchievementsTypes.CollectSprinkles, _points);
        }
        
        var totalScore = PlayerPrefs.GetInt(GameManager.TotalScoreKey);
        totalScore += _points;
        APIManager.Instance.userInGameData.total_score = totalScore;
        PlayerPrefs.SetInt(GameManager.TotalScoreKey, totalScore);
    }

    public void RefreshCoinsField(int count)
    {
        _coins = GameManager.Instance.UserData.Coins;
        
        coinsText.text = count.ToString();
    }

    private void WatchAdvToRevive()
    {
        if (GameManager.Instance.isRemovedAdv)
        {
            TurnOffAdvReward();
            GameManager.Instance.player.ReviveAtThePosition();
            StartCountDownCoroutine();
            AudioManager.Instance.mainBackgroundAudioSource.Play();

            SendAchievementCallback();
        }
        else
        {
            if (_audioSource)
            {
                AudioManager.Instance.mainBackgroundAudioSource = _audioSource;
                _audioSource.Stop();
            }

            AdsManager.Instance.ShowRewardedAd(Constants.AdPlacementRevive);
        }
    }

    private void TurnOnMusic()
    {
        _audioSource = AudioManager.Instance.PlayMusic(AudioType.Death, true);
    }

    private void OnPlayerRespawn()
    {
        watchToContinueButton.gameObject.SetActive(true);
        StopTheMusic();
    }

    private void StopTheMusic()
    {
        if (_audioSource)
        {
            AudioManager.Instance.Stop(_audioSource);
        }
    }

    public void TurnOffAdvReward()
    {
        watchToContinueButton.gameObject.SetActive(false);
    }

    public void OnPlayerDeath()
    {
        _hudScreen.TurnOffHudScreenAndSaveData();
        countDown.gameObject.SetActive(false);
        lostScreenObjects.SetActive(true);
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        if (_coins >= _priceToRevive)
        {
            payToContinueCanvas.alpha = 1f;
            payToContinueCanvas.interactable = true;
        }
        else
        {
            payToContinueCanvas.alpha = 0.6f;
            payToContinueCanvas.interactable = false;
        }

        TurnOnMusic();
        coinCostToRevive.text = _priceToRevive.ToString();
        GameManager.Instance.OnStopGame?.Invoke();
        Time.timeScale = 0f;
    }


    public void RestartPlay()
    {
        ResetToDefaultLostScreenSettings();

        _hudScreen.UIPowerUpsManager.TurnOffAllNumericAnimations();
        _hudScreen.TurnUpperSection(true);

        if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.PlayArcadeRun))
        {
            UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(
                DailyMissions.PlayArcadeRun, null, 1);
        }

        if (GameManager.Instance.countPowerUpsTakenPerRun > 0)
        {
            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.GetPowerUpsInOneRun))
            {
                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(
                    DailyMissions.GetPowerUpsInOneRun, null, GameManager.Instance.countPowerUpsTakenPerRun);
            }

            GameManager.Instance.countPowerUpsTakenPerRun = 0;
        }

        GameManager.Instance.OnPlayerRespawn?.Invoke();

        if (Random.value <= 0.01f && GameManager.Instance.SelectedCharacter == Characters.Meltie)
        {
            PlayerPrefs.SetInt(SkinEnum.MeltieRocketFuel.ToString(), 1);
        }
    }
}