using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StartScreen : UIScreen
    {
        [SerializeField] private Button startButton;
        [SerializeField] private GameObject text;

        [SerializeField] private GameObject gradientFirst;
        [SerializeField] private GameObject gradientSecond;
        [SerializeField] private GameObject gradientThird;
        
        [SerializeField] private GameObject wall;
        [SerializeField] private CanvasGroup lava;
        [SerializeField] private CanvasGroup geluBlack;
        [SerializeField] private CanvasGroup gelu;
        [SerializeField] private GameObject characterSmoke;
        [SerializeField] private GameObject smokeRight;
        [SerializeField] private GameObject smokeLeft;
        [SerializeField] private CanvasGroup geluFront;
        [SerializeField] private CanvasGroup leftFlame;
        [SerializeField] private CanvasGroup rightFlame;
        [SerializeField] private GameObject saw;
        [SerializeField] private GameObject spike;
        [SerializeField] private GameObject fireGuy;
        [SerializeField] private GameObject ghost;
        [SerializeField] private GameObject alan;
        [SerializeField] private GameObject title;
        

        [SerializeField] private float timeScaleIn = 0.5f;
        [SerializeField] private float timeScaleOut = 0.5f;
        
        [SerializeField] private Transform camTransform;
        
        [SerializeField] private float shakeAmount = 0.7f;
        [SerializeField] private float decreaseFactor = 1.0f;

        [SerializeField] private CanvasGroup canvasGroup;
        
        private readonly Vector3 _startScale = Vector3.one;
        private readonly Vector3 _endScale = new (1.2f, 1.2f, 1.2f);
        
        private WaitForSeconds _timeTextScaleIn;
        private WaitForSeconds _timeTextScaleOut;
        
        private WaitForSeconds _waitForTransition;
        private WaitForSeconds _waitForHalfSeconds;
        private WaitForSeconds _waitForAnotherTime;
        private WaitForSeconds _waitForExiting;

        private Coroutine _textCoroutine;
        private Coroutine _startShowCoroutine;
        private Coroutine _transitionCoroutine;
        private Coroutine _exitCoroutine;

        private Vector3 _initialWallPos;
        private Vector3 _initialLavaPos;
        private Vector3 _initialGeluBlackPos;
        private Vector3 _initialGeluPos;
        private Vector3 _initialCharacterSmokePos;
        private Vector3 _initialSmokeRightPos;
        private Vector3 _initialSmokeLeftPos;
        private Vector3 _initialGeluFrontPos;
        private Vector3 _initialLeftFlamePos;
        private Vector3 _initialRightFlamePos;
        private Vector3 _initialSawPos;
        private Vector3 _initialSpikePos;
        private Vector3 _initialFireGuyPos;
        private Vector3 _initialGhostPos;
        private Vector3 _initialAlanPos;
        private Vector3 _initialTittlePos;

        private readonly Vector3 _endScaleGameObject = new(0.7f, 0.7f, 0.7f);
        private readonly Vector3 _endScaleGradientObject = new(1.6f, 1.6f, 1.6f);
        
        private readonly Vector3 _startScaleGradientObject = new(1.6f, 1.6f, 1.6f);
        private readonly Vector3 _startScaleGameObject = new (0.65f, 0.65f, 0.65f);

        private Tweener _smokeLeftTween;
        private Tweener _smokeRightTween;
        private Tweener _characterSmokeTween;
        private Tweener _geluTween;
        private Tweener _geluBlackTween;
        private Tweener _geluFrontTween;
        private Tweener _fireGuyTween;
        private Tweener _ghostTween;
        private Tweener _alanTween;
        private Tweener _spikeTween;
        private Tweener _sawTween;
        private Tweener _tittleTween;
        
        private float _shakeDuration;

        private const float DurationCameraFadeInOut = 1.5f;
        
        private Vector3 _originalPos;

        private void Start()
        {
            _originalPos = camTransform.localPosition;
            _initialCharacterSmokePos = characterSmoke.transform.localPosition;
            _initialSmokeRightPos = smokeRight.transform.localPosition;
            _initialSmokeLeftPos = smokeLeft.transform.localPosition;
            _initialSawPos = saw.transform.localPosition;
            _initialSpikePos = spike.transform.localPosition;
            _initialFireGuyPos = fireGuy.transform.localPosition;
            _initialGhostPos = ghost.transform.localPosition;
            _initialAlanPos = alan.transform.localPosition;
            _initialTittlePos = title.transform.localPosition;

            transform.localScale = _startScaleGameObject;
            gradientFirst.transform.localScale = _startScaleGradientObject;
            gradientSecond.transform.localScale = _startScaleGradientObject;
            gradientThird.transform.localScale = _startScaleGradientObject;
            
            //characterSmoke.transform.position = new Vector3(characterSmoke.transform.position.x, characterSmoke.transform.position.y + Screen.height + 500f, characterSmoke.transform.position.y);
            //smokeRight.transform.position += new Vector3(1500f, 0f, 0f);
            //smokeLeft.transform.position = new Vector3(smokeLeft.transform.position.x - Screen.width, smokeLeft.transform.position.y, smokeLeft.transform.position.z);
            //saw.transform.position = new Vector3(saw.transform.position.x - Screen.width,saw.transform.position.y - Screen.height, saw.transform.position.z);
            //spike.transform.position = new Vector3(spike.transform.position.x + Screen.width, spike.transform.position.y - Screen.height, spike.transform.position.z);
            //fireGuy.transform.position = new Vector3(fireGuy.transform.position.x - Screen.width, fireGuy.transform.position.y, fireGuy.transform.position.z);
            //ghost.transform.position = new Vector3(ghost.transform.position.x + Screen.width, ghost.transform.position.y, ghost.transform.position.z);
            //alan.transform.position = new Vector3(alan.transform.position.x, alan.transform.position.y + Screen.height + 600f,alan.transform.position.z);
            //title.transform.position = new Vector3(title.transform.position.x,title.transform.position.y + Screen.height,title.transform.position.z);
            
            startButton.onClick.AddListener(StartGame);
            _timeTextScaleIn = new WaitForSeconds(timeScaleIn);
            _timeTextScaleOut = new WaitForSeconds(timeScaleOut);
            
            _waitForTransition = new WaitForSeconds(1);
            _waitForHalfSeconds = new WaitForSeconds(0.5f);
            _waitForAnotherTime = new WaitForSeconds(0.2f);
            _waitForExiting = new WaitForSeconds(0.7f);
            
            _startShowCoroutine = StartCoroutine(StartUIShow());
        }
        
        private void Update()
        {
            if (_shakeDuration > 0)
            {
                camTransform.localPosition = _originalPos + Random.insideUnitSphere * shakeAmount;
			
                _shakeDuration -= Time.deltaTime * decreaseFactor;
            }
            else
            {
                _shakeDuration = 0f;
                camTransform.localPosition = _originalPos;
            }
        }

        private void StartGame()
        {
            _exitCoroutine = StartCoroutine(OnStartButtonClicked());
        }

        private IEnumerator OnStartButtonClicked()
        {
            startButton.gameObject.SetActive(false);
            transform.DOScale(_endScaleGameObject, DurationCameraFadeInOut);
            gradientFirst.transform.DOScale(_endScaleGradientObject, DurationCameraFadeInOut);
            gradientSecond.transform.DOScale(_endScaleGradientObject, DurationCameraFadeInOut);
            gradientThird.transform.DOScale(_endScaleGradientObject, DurationCameraFadeInOut);

            yield return _waitForExiting;
            
            UIManager.Instance.DoorTransition.TurnOn();
            UIManager.Instance.DoorTransition.CloseDoor();

            _transitionCoroutine = StartCoroutine(TransitionAfterStartScreen());
        }

        private IEnumerator TransitionAfterStartScreen()
        {
            yield return _waitForTransition;
            TurnOff();
            
            UIManager.Instance.DoorTransition.OpenDoor();
            
            var isClaimableReward = PlayerPrefs.GetInt(GameManager.ClaimableRewardKey) != 0;
            if (isClaimableReward)
            {
                UIManager.Instance.MainMenuScreen.OpenDailyLogin();
            }
            else
            {
                UIManager.Instance.MainMenuScreen.TurnOnGameModeSelection();
            }
        }
        
        
        private IEnumerator StartUIShow()
        {
            yield return _waitForHalfSeconds;
            
            canvasGroup.DOFade(0f, 4f);
            transform.DOScale(Vector3.one, DurationCameraFadeInOut).SetEase(Ease.OutSine);
            gradientFirst.transform.DOScale(Vector3.one, DurationCameraFadeInOut).SetEase(Ease.OutSine);
            gradientSecond.transform.DOScale(Vector3.one, DurationCameraFadeInOut).SetEase(Ease.OutSine);
            gradientThird.transform.DOScale(Vector3.one, DurationCameraFadeInOut).SetEase(Ease.OutSine).OnComplete((() =>
            {
                _spikeTween = spike.transform.DOLocalMoveY(spike.transform.localPosition.y + 10f, 3.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
                _sawTween = saw.transform.DOLocalMoveY(saw.transform.localPosition.y + 10f, 3.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            }));

            //fireGuy.transform.DOMove(_initialFireGuyPos, 0.8f).SetEase(Ease.OutBack);
            
            /*ghost.transform.DOMove(_initialGhostPos, 0.8f).SetEase(Ease.OutBack).OnComplete((() =>
            {
                _fireGuyTween = fireGuy.transform.DOMoveY(fireGuy.transform.position.y + 8f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
                _ghostTween = ghost.transform.DOMoveY(ghost.transform.position.y - 8f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            }));*/
            
            _fireGuyTween = fireGuy.transform.DOLocalMoveY(fireGuy.transform.localPosition.y + 8f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            _ghostTween = ghost.transform.DOLocalMoveY(ghost.transform.localPosition.y - 8f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            _alanTween = alan.transform.DOLocalMoveY(alan.transform.localPosition.y + 10f, 3f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            
            yield return new WaitForSeconds(0.5f);
            
            //leftFlame.DOFade(1f, 2f);
            //rightFlame.DOFade(1f, 2f);
            
            _smokeLeftTween = smokeLeft.transform.DOScale(1.015f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            _smokeRightTween = smokeRight.transform.DOScale(1.015f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            //_characterSmokeTween = characterSmoke.transform.DOScale(1.025f, 3f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);

            //yield return new WaitForSeconds(0.2f);
            
            //smokeLeft.transform.DOMove(_initialSmokeLeftPos, 1).SetEase(Ease.OutBack);
            //smokeRight.transform.DOMove(_initialSmokeRightPos, 1).SetEase(Ease.OutBack);
            
            /*gelu.DOFade(1f, 2f).OnStart((() =>
            {
                _geluTween = gelu.transform.DOScale(1.2f, 4f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InSine);
            }));*/
            
            _geluTween = gelu.transform.DOScale(1.2f, 4f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InSine);
            
            gelu.DOFade(1f, 1.5f);
            geluBlack.DOFade(1f, 1.5f);
            geluFront.DOFade(1f, 1.5f);

            //yield return _waitForHalfSeconds;
            
            //geluBlack.DOFade(1f, 2f);

            //lava.DOFade(1f, 10f);

            /*yield return _waitForHalfSeconds;*/

            /*alan.transform.DOScale(1f, 1.2f).SetEase(Ease.OutBack).OnComplete((() =>
            {
                _alanTween = alan.transform.DOMoveY(alan.transform.position.y + 10f, 3f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            }));*/

            //yield return _waitForHalfSeconds;

            //spike.transform.DOMove(_initialSpikePos, 0.5f).SetEase(Ease.Linear);
            /*saw.transform.DOMove(_initialSawPos, 0.5f).SetEase(Ease.Linear).OnComplete((() =>
            {
                _spikeTween = spike.transform.DOMoveY(spike.transform.position.y + 10f, 3.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
                _sawTween = saw.transform.DOMoveY(saw.transform.position.y + 10f, 3.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            }));*/

            /*
            geluFront.DOFade(1f, 2f).OnComplete((() =>
            {
                _geluBlackTween = geluBlack.transform.DOMoveX(geluBlack.transform.position.x + 9f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
                _geluFrontTween = geluFront.transform.DOMoveX(geluFront.transform.position.x - 9f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            }));*/
            
            _geluBlackTween = geluBlack.transform.DOLocalMoveX(geluBlack.transform.localPosition.x + 9f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            _geluFrontTween = geluFront.transform.DOLocalMoveX(geluFront.transform.localPosition.x - 9f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            
            yield return _waitForHalfSeconds;

            title.transform.DOScale(1f, 0.5f).SetEase(Ease.InCirc).OnComplete((() =>
            {
                _shakeDuration = 0.3f;
                startButton.gameObject.SetActive(true);
                _textCoroutine = StartCoroutine(StartAnimatingText());
                _tittleTween = title.transform.DOScale(1.04f, 4f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            }));
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(StartGame);
            
            _smokeLeftTween.Kill();
            _smokeRightTween.Kill();
            _characterSmokeTween.Kill();
            _geluTween.Kill();
            _geluBlackTween.Kill();
            _geluFrontTween.Kill();
            _fireGuyTween.Kill();
            _ghostTween.Kill();
            _alanTween.Kill();
            _spikeTween.Kill();
            _sawTween.Kill();
            _tittleTween.Kill();
            _textTweener.Kill();

            if (_exitCoroutine != null)
            {
                StopCoroutine(_exitCoroutine);
                _exitCoroutine = null;
            }

            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
                _transitionCoroutine = null;
            }

            if (_startShowCoroutine != null)
            {
                StopCoroutine(_startShowCoroutine);
                _startShowCoroutine = null;
            }

            if (_textCoroutine != null)
            {
                StopCoroutine(_textCoroutine);
                _textCoroutine = null;
            }
        }
        
        private IEnumerator StartAnimatingText()
        {
            while (true)
            {
                ScaleTextTweener(_endScale, timeScaleIn);
                yield return _timeTextScaleIn;
                ScaleTextTweener(_startScale, timeScaleOut);
                yield return _timeTextScaleOut;
            }
        }

        #region Tween

        private Tweener _textTweener;

        private Tweener ScaleTextTweener(Vector3 endValue, float duration, Ease ease = Ease.Linear)
        {
            if (_textTweener.IsActive())
            {
                _textTweener.ChangeEndValue(endValue, duration, true)
                    .SetEase(ease)
                    .Restart();
            }
            else
            {
                _textTweener = text.transform.DOScale(endValue, duration)
                    .SetEase(ease)
                    .SetLink(gameObject)
                    .SetAutoKill(false);
            }

            return _textTweener;
        }

        #endregion
    }
}