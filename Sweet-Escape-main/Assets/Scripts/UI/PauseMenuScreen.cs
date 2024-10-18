using System;
using System.Collections;
using Audio;
using Sirenix.Utilities;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AudioType = Audio.AudioType;

public class PauseMenuScreen : UIScreen
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI pointText;

    [SerializeField] private UIToggle musicToggle;
    [SerializeField] private UIToggle sfxToggle;
    [SerializeField] private UIToggle vibrationsToggle;

    [SerializeField] private Button exitButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;

    [SerializeField] private GameObject mainPauseSection;
    [SerializeField] private GameObject downButtons;

    [SerializeField] private TextMeshProUGUI countDown;

    private MainMenuScreen _mainMenuScreen;
    private HUDScreen _hudScreen;

    public event Action Paused;
    public event Action Unpaused;

    private int _coins;
    private int _points;

    private const int StartNumber = 3;

    private Coroutine _countDownCoroutine;

    private readonly WaitForSecondsRealtime _waitForDoorTransEnd = new(1);

    private void Start()
    {
        _mainMenuScreen = UIManager.Instance.GetUIScreen<MainMenuScreen>();
        _hudScreen = UIManager.Instance.GetUIScreen<HUDScreen>();
    }

    private void OnEnable()
    {
        exitButton.onClick.AddListener(Exit);
        continueButton.onClick.AddListener(StartCountDownToContinue);
        restartButton.onClick.AddListener(Restart);
        musicToggle.Changed += TurnMusic;
        sfxToggle.Changed += TurnSFX;
        vibrationsToggle.Changed += TurnVibrations;
    }

    private void OnDisable()
    {
        exitButton.onClick.RemoveListener(Exit);
        continueButton.onClick.RemoveListener(StartCountDownToContinue);
        restartButton.onClick.RemoveListener(Restart);
        musicToggle.Changed -= TurnMusic;
        sfxToggle.Changed -= TurnSFX;
        vibrationsToggle.Changed -= TurnVibrations;
    }

    public override void TurnOn()
    {
        base.TurnOn();
        PauseGame();
    }

    public void RefreshPointsField(int count)
    {
        _points = count;
        pointText.text = _points.ToString();
    }

    public void RefreshCoinsField(int count)
    {
        _coins = count;
        coinText.text = _coins.ToString();
    }

    public override void TurnOff()
    {
        base.TurnOff();

        mainPauseSection.SetActive(true);
        downButtons.SetActive(true);
        countDown.gameObject.SetActive(false);

        if (_countDownCoroutine != null)
        {
            StopCoroutine(_countDownCoroutine);
            _countDownCoroutine = null;
        }

        ContinueGame();
    }

    private void PauseGame()
    {
        Paused?.Invoke();
        Time.timeScale = 0f;
    }

    private void ContinueGame()
    {
        Unpaused?.Invoke();
        Time.timeScale = 1f;
    }

    private void Exit()
    {
        //SceneManager.LoadScene(0);
        StartCoroutine(LoadSceneAsync());
        
        TilemapManager.Instance.ResetParallaxData();
        
        UIManager.Instance.DoorTransition.TurnOn();
        UIManager.Instance.DoorTransition.CloseDoor();
        if (!GameManager.Instance.Enemies.IsNullOrEmpty())
        {
            GameManager.Instance.Enemies.Clear();
        }
    }

    private IEnumerator LoadSceneAsync()
    {
        yield return _waitForDoorTransEnd;

        var asyncLoad = SceneManager.LoadSceneAsync(1);
        while (!asyncLoad.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        TurnOff();
        _hudScreen.UIPowerUpsManager.TurnOffAllPreviousPowerUps();
        _hudScreen.UIPowerUpsManager.TurnAllPowerUpsCount(false);
        _hudScreen.UIPowerUpsManager.TurnOffAllNumericAnimations();
        _hudScreen.TurnOff();
        _mainMenuScreen.TurnOnGameModeSelection();
        TurnMusic(true);
        TurnSFX(true);

        UIManager.Instance.DoorTransition.OpenDoor();
    }

    private void Restart()
    {
        TurnOff();
        _hudScreen.UIPowerUpsManager.TurnOffAllNumericAnimations();
        _hudScreen.UIPowerUpsManager.TurnOffAllPreviousPowerUps();

        _hudScreen.TurnUpperSection(true);
        GameManager.Instance.OnPlayerRespawn?.Invoke();
    }

    private void StartCountDownToContinue()
    {
        _countDownCoroutine = StartCoroutine(CountDownCoroutine());
    }

    private IEnumerator CountDownCoroutine()
    {
        var maxNumber = StartNumber;

        mainPauseSection.SetActive(false);
        downButtons.SetActive(false);

        countDown.gameObject.SetActive(true);
        countDown.text = maxNumber.ToString();

        while (maxNumber > 0)
        {
            AudioManager.Instance.PlaySFX(AudioType.CountDownBeep);
            yield return new WaitForSecondsRealtime(1);
            maxNumber--;
            countDown.text = maxNumber.ToString();
        }

        Close();
    }

    private void Close()
    {
        TurnOff();
        GameManager.Instance.OnResumeGame?.Invoke();
        _hudScreen.TurnUpperSection(true);
        _hudScreen.UIPowerUpsManager.ResumeAllNumericAnimations();
        _hudScreen.UIPowerUpsManager.TurnAllPowerUpsCount(true);
        _hudScreen.PlayAnimationBar();
    }

    private void TurnMusic(bool isEnabled)
    {
        AudioManager.Instance.TurnAllMusic(isEnabled);
    }

    private void TurnSFX(bool isEnabled)
    {
        AudioManager.Instance.TurnAllSFX(isEnabled);
    }

    private void TurnVibrations(bool isEnabled)
    {
    }
}