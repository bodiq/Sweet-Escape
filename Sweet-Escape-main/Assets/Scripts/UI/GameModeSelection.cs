using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using API;
using Audio;
using Configs;
using DG.Tweening;
using Enums;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using AudioType = Audio.AudioType;

public class GameModeSelection : UIScreen
{
    [SerializeField] private Button playButton;
    [SerializeField] private Image characterIcon;

    [SerializeField] private Transform gameModeSelectionButton;
    [SerializeField] private Transform upperSectionCoinBox;
    [SerializeField] private Transform upperSectionUsernameBox;
    [SerializeField] private Button leftArrowGameMode;
    [SerializeField] private Button rightArrowGameMode;
    [SerializeField] private Transform downSideBigBox;
    [SerializeField] private Transform scoreBox;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private CanvasGroup descriptionBox;
    [SerializeField] private CanvasGroup backgroundGreyLines;
    [SerializeField] private CanvasGroup backgroundCharacterName;
    [SerializeField] private List<GameModeSelectionUIData> gameModeList = new();

    [SerializeField] private TextMeshProUGUI lastScore;
    [SerializeField] private TextMeshProUGUI bestScore;

    private Characters _selectedCharacter;

    private Vector3 _initialUpperSectionCoinBox;
    private Vector3 _initialUpperSectionUsernameBox;
    
    private Vector3 _initialDownSection;
    private Vector3 _initialGameModeSelectionButtonPos;
    private Vector3 _initialLeftArrowGameModePos;
    private Vector3 _initialRightArrowGameModePos;
    private Vector3 _initialDownSideBigBoxPos;
    private Vector3 _initialCharacterPos;

    private Vector3 _initialGameModeBoxPos;
    
    private Tweener _mainDownSectionTween;
    private Tweener _downSideBigBoxTween;
    private Tweener _characterIconTween;
    private Tweener _upperUsernameBoxTween;
    private Tweener _upperCoinBoxTween;
    private Tweener _gameModeSelectionTween;
    private Tweener _leftArrowGameModeTween;
    private Tweener _rightArrowGameModeTween;
    private Tweener _descriptionBoxTween;
    private Tweener _scoreBoxTween;
    private Tweener _backgroundGreyLineTween;
    private Tweener _backgroundCharacterNameTween;
    private Tween _characterAnimationAfterAppearTween;
    private Tween _currentUpperBoxTween;
    private Tween _currentDownBoxTween;
    private Tween _nextDownBoxTween;
    private Tween _nextUpperBoxTween;

    private MainDownSection _mainDownSection;
    
    private Coroutine _UIShowCoroutine;

    private bool _hasAnimationDone;

    private int _currentGameModeBoxIndex;

    private Rect _rect;

    private readonly WaitForSeconds _waitBeforeStartShow = new(DelayBeforeStartUIShow);
    private readonly WaitForSeconds _waitForDefaultAnimationDuration = new(DefaultUIItemShowDuration);
    private readonly WaitForSeconds _waitForQuarterSecond = new(QuarterOfSecondDuration);
    private readonly WaitForSeconds _waitForSecond = new(FullSecondDuration);
    
    private const float DelayBeforeStartUIShow = 0.3f;
    private const float DefaultUIItemShowDuration = 0.7f;
    private const float HalfSecondDuration = 0.5f;
    private const float QuarterOfSecondDuration = 0.2f;
    private const float FullSecondDuration = 1f;
    private const float DurationCharacterNameAppears = 0.15f;

    private const Ease DefaultEaseAnimation = Ease.InOutCirc;
    private const Ease OutBackEaseAnimation = Ease.OutBack;

    protected override void Awake()
    {
        _mainDownSection = UIManager.Instance.MainMenuScreen.MainDownSection;
        
        _initialLeftArrowGameModePos = leftArrowGameMode.transform.localPosition;
        _initialRightArrowGameModePos = rightArrowGameMode.transform.localPosition;
        _initialDownSideBigBoxPos = downSideBigBox.localPosition;
        _initialDownSection = _mainDownSection.transform.localPosition;
        _initialUpperSectionCoinBox = upperSectionCoinBox.localPosition;
        _initialUpperSectionUsernameBox = upperSectionUsernameBox.localPosition;
        
        _initialCharacterPos = characterIcon.transform.localPosition;

        _initialGameModeBoxPos = gameModeSelectionButton.localPosition;
    }

    private void Start()
    {
        ChangeSelectedCharacter(Characters.Noob);

        lastScore.text = PlayerPrefs.GetInt(GameManager.LastScoreKey).ToString();
        bestScore.text = PlayerPrefs.GetInt(GameManager.HighScoreKey).ToString();
    }

    private void OnEnable()
    {
        _rect = UIManager.Instance.RectTransform.rect;
        
        GameManager.Instance.ResetAnimationInfo += ResetAnimationInfo;
        GameManager.Instance.OnCharacterSkinChange += CharacterSkinChanged;
        
        leftArrowGameMode.onClick.AddListener(PreviousGameMode);
        rightArrowGameMode.onClick.AddListener(NextGameMode);
        
        playButton.onClick.AddListener(StartGame);

        if (!_hasAnimationDone)
        {
            ResetUIElementsToPreAnimationPos();
            TurnOnStartUIAnimation();
        }
    }

    public void UpdateScore(int score)
    {
        PlayerPrefs.SetInt(GameManager.LastScoreKey, score);

        var bestSavedScore = PlayerPrefs.GetInt(GameManager.HighScoreKey);

        if (score > bestSavedScore)
        {
            PlayerPrefs.SetInt(GameManager.HighScoreKey, score);
            APIManager.Instance.userInGameData.high_score = score;
            bestScore.text = score.ToString();
        }

        lastScore.text = score.ToString();
    }

    public void ResetUpperSectionPos()
    {
        var upperSectionCoinBoxPos = upperSectionCoinBox.localPosition;
        var upperSectionUsernameBoxPos = upperSectionUsernameBox.localPosition;

        upperSectionCoinBox.localPosition = new Vector3(upperSectionCoinBoxPos.x + _rect.width / 2, upperSectionCoinBoxPos.y, upperSectionCoinBoxPos.z);
        upperSectionUsernameBox.localPosition = new Vector3(upperSectionUsernameBoxPos.x - _rect.width / 2, upperSectionUsernameBoxPos.y, upperSectionUsernameBoxPos.z);
    }

    public void StartUpperSectionUIShow()
    {
        _upperCoinBoxTween = upperSectionCoinBox.DOLocalMoveX(_initialUpperSectionCoinBox.x, DefaultUIItemShowDuration).SetEase(DefaultEaseAnimation);
        _upperUsernameBoxTween = upperSectionUsernameBox.DOLocalMoveX(_initialUpperSectionUsernameBox.x, DefaultUIItemShowDuration).SetEase(DefaultEaseAnimation);
    }

    public void CharacterSkinChanged(Characters character, SkinEnum skinEnum)
    {
        if (!CharacterConfig.Instance.CharacterData.TryGetValue(character, out var characterPreset)) return;
        
        foreach (var skinData in characterPreset.skinData.Where(skinData => skinData.skinEnum == skinEnum))
        {
            characterIcon.sprite = skinData.characterSkinMenuSprite;
        }
    }

    public void ResetScore(int lastScore, int highScore)
    {
        this.lastScore.text = lastScore.ToString();
        bestScore.text = highScore.ToString();
    }

    public override void TurnOn()
    {
        base.TurnOn();
        
        if (_hasAnimationDone)
        {
            characterIcon.transform.localPosition = _initialCharacterPos;
            _characterAnimationAfterAppearTween = characterIcon.transform.DOLocalMoveY(_initialCharacterPos.y + 15f, 1.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
        }
    }

    public override void TurnOff()
    {
        base.TurnOff();
        if (_UIShowCoroutine != null)
        {
            StopCoroutine(_UIShowCoroutine);
            _UIShowCoroutine = null;
        }
    }
    
    private void PreviousGameMode()
    {
        leftArrowGameMode.interactable = false;
        rightArrowGameMode.interactable = false;

        AudioManager.Instance.PlaySFX(AudioType.GameModeButtonChange);

        var gameModeIndexToShow = _currentGameModeBoxIndex - 1;
        if (gameModeIndexToShow < 0)
        {
            gameModeIndexToShow = gameModeList.Count - 1;
        }

        var upperBoxLocalPos = gameModeList[_currentGameModeBoxIndex].upperBox.transform.localPosition;
        var downBoxLocalPos = gameModeList[_currentGameModeBoxIndex].downBox.transform.localPosition;
        
        var currentEndPosUpperBoxToMove = upperBoxLocalPos.x - _rect.width * 2;
        var screenDownBoxStartPos = downBoxLocalPos.y - _rect.height / 2;

        gameModeList[gameModeIndexToShow].upperBox.transform.localPosition = new Vector3(upperBoxLocalPos.x + _rect.width * 2, upperBoxLocalPos.y, upperBoxLocalPos.z);
        gameModeList[gameModeIndexToShow].downBox.transform.localPosition = new Vector3(downBoxLocalPos.x, screenDownBoxStartPos, downBoxLocalPos.z);
        
        gameModeList[gameModeIndexToShow].upperBox.SetActive(true);
        gameModeList[gameModeIndexToShow].downBox.SetActive(true);

        _currentUpperBoxTween = gameModeList[_currentGameModeBoxIndex].upperBox.transform.DOLocalMoveX(currentEndPosUpperBoxToMove, DefaultUIItemShowDuration);
        _currentDownBoxTween = gameModeList[_currentGameModeBoxIndex].downBox.transform.DOLocalMoveY(screenDownBoxStartPos, DefaultUIItemShowDuration).OnComplete((() =>
        {
            gameModeList[_currentGameModeBoxIndex].upperBox.SetActive(false);
            gameModeList[_currentGameModeBoxIndex].downBox.SetActive(false);
        }));

        _nextUpperBoxTween = gameModeList[gameModeIndexToShow].upperBox.transform.DOLocalMoveX(upperBoxLocalPos.x, DefaultUIItemShowDuration).OnComplete(() =>
        {
            _nextDownBoxTween = gameModeList[gameModeIndexToShow].downBox.transform.DOLocalMoveY(downBoxLocalPos.y, DefaultUIItemShowDuration).OnComplete(() =>
            {
                _currentGameModeBoxIndex = gameModeIndexToShow;
                leftArrowGameMode.interactable = true;
                rightArrowGameMode.interactable = true;
            });
        });
    }

    private void NextGameMode()
    {
        leftArrowGameMode.interactable = false;
        rightArrowGameMode.interactable = false;
        
        AudioManager.Instance.PlaySFX(AudioType.GameModeButtonChange);
        
        var gameModeIndexToShow = _currentGameModeBoxIndex + 1;
        if (gameModeIndexToShow > gameModeList.Count - 1)
        {
            gameModeIndexToShow = 0;
        }

        var upperBoxLocalPos = gameModeList[_currentGameModeBoxIndex].upperBox.transform.localPosition;
        var downBoxLocalPos = gameModeList[_currentGameModeBoxIndex].downBox.transform.localPosition;
        
        var currentEndPosUpperBoxToMove = upperBoxLocalPos.x + _rect.width * 2;
        var screenDownBoxStartPos = downBoxLocalPos.y - _rect.height / 2;

        gameModeList[gameModeIndexToShow].upperBox.transform.localPosition = new Vector3(upperBoxLocalPos.x - _rect.width * 2, upperBoxLocalPos.y, upperBoxLocalPos.z);
        gameModeList[gameModeIndexToShow].downBox.transform.localPosition = new Vector3(downBoxLocalPos.x, screenDownBoxStartPos, downBoxLocalPos.z);
        
        gameModeList[gameModeIndexToShow].upperBox.SetActive(true);
        gameModeList[gameModeIndexToShow].downBox.SetActive(true);

        _currentUpperBoxTween = gameModeList[_currentGameModeBoxIndex].upperBox.transform.DOLocalMoveX(currentEndPosUpperBoxToMove, DefaultUIItemShowDuration);
        _currentDownBoxTween = gameModeList[_currentGameModeBoxIndex].downBox.transform.DOLocalMoveY(screenDownBoxStartPos, DefaultUIItemShowDuration).OnComplete((() =>
        {
            gameModeList[_currentGameModeBoxIndex].upperBox.SetActive(false);
            gameModeList[_currentGameModeBoxIndex].downBox.SetActive(false);
        }));

        _nextUpperBoxTween = gameModeList[gameModeIndexToShow].upperBox.transform.DOLocalMoveX(upperBoxLocalPos.x, DefaultUIItemShowDuration).OnComplete(() =>
        {
            _nextDownBoxTween = gameModeList[gameModeIndexToShow].downBox.transform.DOLocalMoveY(downBoxLocalPos.y, DefaultUIItemShowDuration).OnComplete(() =>
            {
                _currentGameModeBoxIndex = gameModeIndexToShow;
                leftArrowGameMode.interactable = true;
                rightArrowGameMode.interactable = true;
            });
        });
    }

    private void StartGame()
    {
        TurnOff();
        UIManager.Instance.MainMenuScreen.StartGame(_selectedCharacter);
        GameManager.Instance.ResetAnimationInfo?.Invoke();
    }

    public void ChangeSelectedCharacter(Characters character)
    {
        _selectedCharacter = character;
        if (CharacterConfig.Instance.CharacterData.TryGetValue(_selectedCharacter, out var characterPreset))
        {
            characterIcon.sprite = characterPreset.fullRectIcon;
            characterName.text = characterPreset.name;
        }
    }
    
    private void ResetUIElementsToPreAnimationPos()
    {
        var mainDownSectionPos = _mainDownSection.transform.localPosition;
        var leftArrowPos = leftArrowGameMode.transform.localPosition;
        var rightArrowPos = rightArrowGameMode.transform.localPosition;
        var downSideBoxPos = downSideBigBox.localPosition;
        var characterIconPos = characterIcon.transform.localPosition;

        var screenHeightHalf = _rect.height;
        var screenWidth = _rect.width;

        _mainDownSection.transform.localPosition = new Vector3(mainDownSectionPos.x, mainDownSectionPos.y - screenHeightHalf, mainDownSectionPos.z);

        ResetUpperSectionPos();

        leftArrowGameMode.transform.localPosition = new Vector3(leftArrowPos.x + 150f, leftArrowPos.y, leftArrowPos.z);
        rightArrowGameMode.transform.localPosition = new Vector3(rightArrowPos.x - 150f, rightArrowPos.y, rightArrowPos.z);
        downSideBigBox.localPosition = new Vector3(downSideBoxPos.x, downSideBoxPos.y - screenHeightHalf, downSideBoxPos.z);
        characterIcon.transform.localPosition = new Vector3(characterIconPos.x - screenWidth, characterIconPos.y, characterIconPos.z);

        gameModeSelectionButton.localScale = Vector3.zero;
        scoreBox.localScale = Vector3.zero;
        descriptionBox.alpha = 0f;
        backgroundGreyLines.alpha = 0f;
        backgroundCharacterName.alpha = 0f;
        leftArrowGameMode.gameObject.SetActive(false);
        rightArrowGameMode.gameObject.SetActive(false);
        descriptionBox.gameObject.SetActive(false);
    }

    private void TurnOnStartUIAnimation()
    {
        _UIShowCoroutine = StartCoroutine(StartUIShow());
    }

    private IEnumerator StartUIShow()
    {
        yield return _waitBeforeStartShow;

        _mainDownSectionTween = _mainDownSection.transform.DOLocalMoveY(_initialDownSection.y, DefaultUIItemShowDuration).SetEase(DefaultEaseAnimation);
        StartUpperSectionUIShow();

        yield return _waitForDefaultAnimationDuration;
        
        _downSideBigBoxTween = downSideBigBox.DOLocalMoveY(_initialDownSideBigBoxPos.y, DefaultUIItemShowDuration).SetEase(DefaultEaseAnimation);

        yield return _waitForQuarterSecond;
        
        descriptionBox.gameObject.SetActive(true);
        _descriptionBoxTween = descriptionBox.DOFade(1f, DefaultUIItemShowDuration).SetEase(DefaultEaseAnimation).SetDelay(HalfSecondDuration);
        
        _gameModeSelectionTween = gameModeSelectionButton.transform.DOScale(Vector3.one, DefaultUIItemShowDuration).SetEase(OutBackEaseAnimation).OnComplete((() =>
        {
            leftArrowGameMode.gameObject.SetActive(true);
            rightArrowGameMode.gameObject.SetActive(true);

            _leftArrowGameModeTween = leftArrowGameMode.transform.DOLocalMove(_initialLeftArrowGameModePos, DefaultUIItemShowDuration).SetEase(OutBackEaseAnimation);
            _rightArrowGameModeTween = rightArrowGameMode.transform.DOLocalMove(_initialRightArrowGameModePos, DefaultUIItemShowDuration).SetEase(OutBackEaseAnimation);
            _scoreBoxTween = scoreBox.DOScale(Vector3.one, DefaultUIItemShowDuration).SetEase(OutBackEaseAnimation);
        }));
        
        yield return _waitForSecond;

        _backgroundGreyLineTween = backgroundGreyLines.DOFade(1f, DefaultUIItemShowDuration);
        _characterIconTween = characterIcon.transform.DOLocalMoveX(_initialCharacterPos.x, DefaultUIItemShowDuration).SetEase(OutBackEaseAnimation).OnComplete(() =>
        {
            _characterAnimationAfterAppearTween = characterIcon.transform.DOLocalMoveY(_initialCharacterPos.y + 15f, 1.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
        });
        
        _backgroundCharacterNameTween = backgroundCharacterName.DOFade(1f, DefaultUIItemShowDuration).SetDelay(DurationCharacterNameAppears);
        
        _hasAnimationDone = true;
    }

    private void ResetAnimationInfo()
    {
        _hasAnimationDone = false;
    }
    
    private void SetAnimationObjectsToDefaultPos()
    {
        _hasAnimationDone = true;
        gameModeSelectionButton.localScale = Vector3.one;
        scoreBox.localScale = Vector3.one;
        descriptionBox.alpha = 1f;
        backgroundGreyLines.alpha = 1f;
        backgroundCharacterName.alpha = 1f;
        leftArrowGameMode.gameObject.SetActive(true);
        rightArrowGameMode.gameObject.SetActive(true);
        descriptionBox.gameObject.SetActive(true);
        
        leftArrowGameMode.transform.localPosition = _initialLeftArrowGameModePos;
        rightArrowGameMode.transform.localPosition = _initialRightArrowGameModePos;
        downSideBigBox.localPosition = _initialDownSideBigBoxPos;
        _mainDownSection.transform.localPosition = _initialDownSection;
        upperSectionCoinBox.localPosition = _initialUpperSectionCoinBox;
        upperSectionUsernameBox.localPosition = _initialUpperSectionUsernameBox;
        characterIcon.transform.localPosition = _initialCharacterPos;
        gameModeSelectionButton.localPosition = _initialGameModeBoxPos;
    }
    
    private void OnDisable()
    {
        playButton.onClick.RemoveListener(StartGame);

        if (_UIShowCoroutine != null)
        {
            StopCoroutine(_UIShowCoroutine);
            _UIShowCoroutine = null;
        }

        _characterAnimationAfterAppearTween?.Kill();
        _scoreBoxTween?.Kill();
        _backgroundGreyLineTween?.Kill();
        _backgroundCharacterNameTween?.Kill();
        _mainDownSectionTween?.Kill();
        _downSideBigBoxTween?.Kill();
        _characterIconTween?.Kill();
        _upperCoinBoxTween?.Kill();
        _upperUsernameBoxTween?.Kill();
        _gameModeSelectionTween?.Kill();
        _leftArrowGameModeTween?.Kill();
        _rightArrowGameModeTween?.Kill();
        _descriptionBoxTween?.Kill();
        _currentUpperBoxTween?.Kill();
        _currentDownBoxTween?.Kill();
        _nextDownBoxTween?.Kill(); 
        _nextUpperBoxTween?.Kill();

        SetAnimationObjectsToDefaultPos();

        GameManager.Instance.OnCharacterSkinChange -= CharacterSkinChanged;
    }
}

[Serializable]
public struct GameModeSelectionUIData
{
    public GameMode gameMode;
    public GameObject upperBox;
    public GameObject downBox;
}
