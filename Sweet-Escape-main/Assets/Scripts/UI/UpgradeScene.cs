using System.Collections;
using System.Collections.Generic;
using Audio;
using DG.Tweening;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AudioType = Audio.AudioType;

public class UpgradeScene : MonoBehaviour
{
    [SerializeField] private Transform levelUpBox;
    [SerializeField] private Transform abilityBox;
    
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image characterIcon;
    [SerializeField] private List<CharacterLevelStar> levelStars = new();
    
    [SerializeField] private CanvasGroup upgradeSceneCanvas;
    
    [SerializeField] private TextMeshProUGUI abilityDescription;
    [SerializeField] private Animator lockerAnimation;
    [SerializeField] private Image abilityDescriptionImage;
    [SerializeField] private Image lockerImage;
    [SerializeField] private Sprite unlockedAbilityBox;
    [SerializeField] private Sprite lockedAbilityBox;
    [SerializeField] private Sprite defaultLockerSprite;

    [SerializeField] private Button continueButton;
    [SerializeField] private CanvasGroup continueButtonCanvas;
    
    private int _level;

    private Vector3 _initialLevelUpBoxPos;
    private Vector3 _initialAbilityBoxPos;
    private Vector3 _endLevelUpBoxPos;

    private const string LockerAnimationState = "LockerUnlockingAnim";
    
    private Coroutine _upgradeUIShow;
    
    private Tween _upgradeSceneCanvasTween;
    private Tween _levelUpBoxTween;
    private Tween _abilityBoxTween;
    private Tween _continueButtonCanvasTween;

    private const float OneFifthSecondsDuration = 0.2f;
    private const float HalfSecondDuration = 0.5f;
    
    private const float DurationWaitForChangeBoxes = 2f;
    private const float DurationWaitAppearingLevelText = 1.1f;
    private const float DurationWaitForLevelUpBoxAppears = 0.3f;
    private const float DurationWaitContinueButtonAppears = 1.5f;

    private readonly WaitForSeconds _waitForBoxChanges = new(DurationWaitForChangeBoxes);
    private readonly WaitForSeconds _waitLevelTextAppears = new(DurationWaitAppearingLevelText);
    private readonly WaitForSeconds _waitLevelUpAppears = new(DurationWaitForLevelUpBoxAppears);
    private readonly WaitForSeconds _waitContinueButtonAppears = new(DurationWaitContinueButtonAppears);
    private readonly WaitForSeconds _waitAbilityDescriptionAppears = new(DurationWaitAppearingLevelText);

    private float _defaultBackgroundVolume = 0.15f;
        
    private void Awake()
    {
        var levelUpBoxPos = levelUpBox.localPosition;
        var abilityBoxPos = abilityBox.localPosition;
        
        _initialLevelUpBoxPos = levelUpBoxPos;
        _initialAbilityBoxPos = abilityBoxPos;

        var rect = UIManager.Instance.RectTransform.rect;
        
        _endLevelUpBoxPos = levelUpBoxPos + new Vector3(0f, rect.height, 0f);
    }

    public void Initialize(CharacterPreset characterPreset, int level)
    {
        var rect = UIManager.Instance.RectTransform.rect;
        
        var levelUpBoxPos = levelUpBox.localPosition;
        var abilityBoxPos = abilityBox.localPosition;
        
        levelUpBoxPos = new Vector3(levelUpBoxPos.x, _initialLevelUpBoxPos.y - rect.height, levelUpBoxPos.z);
        abilityBoxPos = new Vector3(abilityBoxPos.x, _initialAbilityBoxPos.y - rect.height, abilityBoxPos.z);
        
        abilityBox.localPosition = abilityBoxPos;
        levelUpBox.localPosition = levelUpBoxPos;

        _level = level;
        levelText.text = "Level " + (_level - 1);
        
        foreach (var levelStar in levelStars)
        {
            levelStar.EmptyStar();
        }
        
        abilityDescriptionImage.sprite = lockedAbilityBox;
        abilityDescription.gameObject.SetActive(false);
        
        lockerAnimation.enabled = false;
        lockerImage.sprite = defaultLockerSprite;
        
        characterIcon.sprite = characterPreset.fullRectIcon;

        if (_level > 1)
        {
            abilityDescription.text = characterPreset.abilityList[_level - 1].AbilityDescription;
        }
        
        for(var i = 0; i < _level - 1; i++)
        {
            levelStars[i].FullStar();
        }

        continueButtonCanvas.alpha = 0f;
        continueButton.onClick.AddListener(ExitUpgradeScene);

        _defaultBackgroundVolume = AudioManager.Instance.mainBackgroundAudioSource.volume;
        AudioManager.Instance.mainBackgroundAudioSource.volume = _defaultBackgroundVolume / 4;

        StartUpgradeUIShow();
    }
    
    private void StartUpgradeUIShow()
    {
        UIManager.Instance.MainMenuScreen.MainDownSection.TurnOff();
        UIManager.Instance.MainMenuScreen.MainUpperSection.TurnOff();

        _upgradeUIShow = StartCoroutine(StartUIShow());
    }

    public void StarAndPadlockSound()
    {
        AudioManager.Instance.PlaySFX(AudioType.Popup);
    }
    
    private IEnumerator StartUIShow()
    {
        _upgradeSceneCanvasTween = upgradeSceneCanvas.DOFade(1f, OneFifthSecondsDuration);
        upgradeSceneCanvas.interactable = true;
        upgradeSceneCanvas.blocksRaycasts = true;
        
        yield return _waitLevelUpAppears;
        
        _levelUpBoxTween = levelUpBox.DOLocalMoveY(_initialLevelUpBoxPos.y, HalfSecondDuration).OnComplete(() =>
        {
            levelStars[_level - 1].FullStar(true);
        });
        
        AudioManager.Instance.PlaySFX(AudioType.CharacterLevelUp);
        
        yield return _waitLevelTextAppears;

        levelText.text = "Level " + _level;
        AudioManager.Instance.PlaySFX(AudioType.Popup);

        yield return _waitForBoxChanges;

        _levelUpBoxTween = levelUpBox.DOLocalMoveY(_endLevelUpBoxPos.y, HalfSecondDuration);
        _abilityBoxTween = abilityBox.DOLocalMoveY(_initialAbilityBoxPos.y, HalfSecondDuration).OnComplete(() =>
        {
            lockerAnimation.enabled = true;
            lockerAnimation.Play(LockerAnimationState, 0, 0f);
        });

        yield return _waitAbilityDescriptionAppears;
        
        AudioManager.Instance.PlaySFX(AudioType.Popup);

        abilityDescription.gameObject.SetActive(true);
        abilityDescriptionImage.sprite = unlockedAbilityBox;

        yield return _waitContinueButtonAppears;

        _continueButtonCanvasTween = continueButtonCanvas.DOFade(1f, 0.3f);
        continueButtonCanvas.interactable = true;
        continueButtonCanvas.blocksRaycasts = true;
    }

    private void ExitUpgradeScene()
    {
        
        AudioManager.Instance.mainBackgroundAudioSource.volume = _defaultBackgroundVolume;
        _upgradeSceneCanvasTween = upgradeSceneCanvas.DOFade(0f, OneFifthSecondsDuration);
        upgradeSceneCanvas.interactable = false;
        upgradeSceneCanvas.blocksRaycasts = false;
        
        _upgradeSceneCanvasTween?.Kill(true);
        _levelUpBoxTween?.Kill();
        _abilityBoxTween?.Kill();
        _continueButtonCanvasTween?.Kill();

        UIManager.Instance.MainMenuScreen.MainDownSection.TurnOn();
        UIManager.Instance.MainMenuScreen.MainUpperSection.TurnOn();

        if (_upgradeUIShow != null)
        {
            StopCoroutine(_upgradeUIShow);
            _upgradeUIShow = null;
        }
        
        foreach (var levelStar in levelStars)
        {
            levelStar.EmptyStar();
        }

        continueButton.onClick.RemoveListener(ExitUpgradeScene);
    }
}
