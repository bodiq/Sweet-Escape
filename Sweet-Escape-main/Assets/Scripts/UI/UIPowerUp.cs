using System;
using System.Collections;
using System.Collections.Generic;
using PowerUps;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPowerUp : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Image animatorImage;
    [SerializeField] private TextMeshProUGUI powerUpCount;
    [SerializeField] private Image mainDefaultImage;
    [SerializeField] private Enums.PowerUps powerUp;

    [SerializeField] private Sprite defaultSpritePowerUp;

    private readonly List<string> _animationStates = new();
 
    private const string CreateAnimationState = "Create";
    private const string DestroyAnimationState = "Destroy";
    private const string DefaultAnimationState = "Default";

    private const string MinusOneAnimationState = "MinusOne";
    private const string MinusTwoAnimationState = "MinusTwo";
    private const string MinusThreeAnimationState = "MinusThree";
    private const string MinusFourAnimationState = "MinusFour";

    private const int StaticTickCounts = 5;

    private int _shieldCount;
    private int _lastIndexAnimationState;
    
    private float _creatingAnimationDuration;
    private float _destroyingAnimationDuration;
    private float _durationBetweenPowerUpTicks;
    private float _animationTimeStop;
    private float _passedTickTime;
    private float _newTickDuration;
    private float _newTickDestroyDuration;

    private bool _isTickStart;
    private bool _startBubbleAnimation;
    private bool _endBubbleAnimation;

    private Coroutine _startBubbleCoroutine;
    private Coroutine _endBubbleCoroutine;
    private Coroutine _startPowerUpTimingCoroutine;

    private WaitForSeconds _powerUpCreationWait;
    private WaitForSeconds _powerUpDestroyingWait;
    private WaitForSeconds _powerUpTickWait;

    private PowerUp _powerUp;

    private void Awake()
    {
        var animatorClips = animator.runtimeAnimatorController.animationClips;

        foreach (var clip in animatorClips)
        {
            switch (clip.name)
            {
                case CreateAnimationState:
                    _creatingAnimationDuration = clip.length;
                    _powerUpCreationWait = new WaitForSeconds(_creatingAnimationDuration);
                    break;
                case DestroyAnimationState:
                    _destroyingAnimationDuration = clip.length;
                    _powerUpDestroyingWait = new WaitForSeconds(_destroyingAnimationDuration);
                    break;
            }
        }
        
        _animationStates.Add(CreateAnimationState);
        _animationStates.Add(MinusOneAnimationState);
        _animationStates.Add(MinusTwoAnimationState);
        _animationStates.Add(MinusThreeAnimationState);
        _animationStates.Add(MinusFourAnimationState);
        _animationStates.Add(DestroyAnimationState);
    }

    private void Update()
    {
        if (_isTickStart)
        {
            _passedTickTime += Time.deltaTime;
        }
    }

    private void OnDisable()
    {
        ResetCoroutines();
    }

    public void SetPreviousPowerUpNull()
    {
        _powerUp = null;
    }

    private void ResetCoroutines()
    {
        if (_startBubbleCoroutine != null)
        {
            StopCoroutine(_startBubbleCoroutine);
            _startBubbleCoroutine = null;
        }

        if (_startPowerUpTimingCoroutine != null)
        {
            StopCoroutine(_startPowerUpTimingCoroutine);
            _startPowerUpTimingCoroutine = null;
        }
    }

    public void PowerUpCountTurn(bool isActive)
    {
        if (isActive)
        {
            if (powerUpCount && _shieldCount > 1)
            {
                powerUpCount.enabled = isActive;
            }
        }
        else
        {
            if (powerUpCount)
            {
                powerUpCount.enabled = isActive;
            }
        }
    }

    public void CreateUIPowerUp(PowerUp powerUpObject, int count = 0, float duration = 0)
    {
        mainDefaultImage.enabled = false;
        _shieldCount = count;
        _lastIndexAnimationState = 0;        
        animatorImage.sprite = defaultSpritePowerUp;
        _animationTimeStop = 0;
        _isTickStart = false;
        _newTickDuration = 0;
        _newTickDestroyDuration = 0;
        
        PowerUpCountTurn(false);

        if (_startPowerUpTimingCoroutine != null)
        {
            StopCoroutine(_startPowerUpTimingCoroutine);
            _startPowerUpTimingCoroutine = null;
        }

        if (powerUpObject != null)
        {
            _powerUp = powerUpObject;
        }

        ResetCoroutines();

        switch (powerUp)
        {
            case Enums.PowerUps.WaffleShield:
                if (_shieldCount > 0)
                {
                    _startBubbleCoroutine = StartCoroutine(StartBubbleCoroutine());
                }
                else
                {
                    PowerUpCountTurn(false);
                    mainDefaultImage.enabled = false;
                    animatorImage.enabled = false;
                }
                break;
            case Enums.PowerUps.ChillBlast:
            case Enums.PowerUps.GoldSpoon:
            case Enums.PowerUps.HundAThousands:
                _durationBetweenPowerUpTicks = duration / StaticTickCounts;
                _powerUpTickWait = new WaitForSeconds(_durationBetweenPowerUpTicks);
                _startPowerUpTimingCoroutine = StartCoroutine(StartPowerUpTimingCoroutine());
                break;
            case Enums.PowerUps.Magnet:
            case Enums.PowerUps.Nothing:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(powerUpObject), powerUpObject, null);
        }
    }

    private IEnumerator StartPowerUpTimingCoroutine()
    {
        for (var i = _lastIndexAnimationState; i < _animationStates.Count; i++)
        {
            if (i == 0)
            {
                _isTickStart = true;
                var animationStop = _animationTimeStop;
                animator.Play(_animationStates[i], 0, animationStop);
                _animationTimeStop = 0;
                _lastIndexAnimationState = i;

                if (_newTickDuration > 0)
                {
                    var tickDuration = _newTickDuration;
                    yield return new WaitForSeconds(tickDuration);
                    _isTickStart = false;
                    _passedTickTime = 0;
                    _newTickDuration = 0;
                }
                else
                {
                    yield return _powerUpTickWait;
                    _isTickStart = false;
                    _passedTickTime = 0;
                }
            }
            else if (i == _animationStates.Count - 1)
            {
                _isTickStart = true;
                var animationStop = _animationTimeStop;
                animator.Play(_animationStates[i], 0, animationStop);
                _animationTimeStop = 0;
                _lastIndexAnimationState = i;

                if (_newTickDestroyDuration > 0)
                {
                    var tickDuration = _newTickDestroyDuration;
                    yield return new WaitForSeconds(tickDuration);
                    _newTickDestroyDuration = 0;
                }
                else
                {
                    yield return _powerUpDestroyingWait;
                }
                _isTickStart = false;
                _passedTickTime = 0;
                _lastIndexAnimationState = 0;
                gameObject.SetActive(false);
            }
            else
            {
                _isTickStart = true;
                var animationStop = _animationTimeStop;
                animator.Play(_animationStates[i], 0, animationStop);
                _animationTimeStop = 0;
                _lastIndexAnimationState = i;
                if (_newTickDuration > 0)
                {
                    var tickDuration = _newTickDuration;
                    yield return new WaitForSeconds(tickDuration);
                    _isTickStart = false;
                    _passedTickTime = 0;
                    _newTickDuration = 0;
                }
                else
                {
                    yield return _powerUpTickWait;
                    _isTickStart = false;
                    _passedTickTime = 0;
                }
            }
        }
    }

    public void StopNumericAnimation()
    {
        _animationTimeStop = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        _isTickStart = false;
        _newTickDuration = _durationBetweenPowerUpTicks - _passedTickTime;
        _newTickDestroyDuration = _destroyingAnimationDuration - _passedTickTime;
    }

    public void ResumeNumericAnimation()
    {
        _startPowerUpTimingCoroutine = StartCoroutine(StartPowerUpTimingCoroutine());
    }
    
    public void TurnOffNumericAnimation()
    {
        animatorImage.sprite = defaultSpritePowerUp;
        _lastIndexAnimationState = 0;
        _animationTimeStop = 0;
        _isTickStart = false;
        _newTickDuration = 0;
        _newTickDestroyDuration = 0;

        if (_startPowerUpTimingCoroutine != null)
        {
            StopCoroutine(_startPowerUpTimingCoroutine);
            _startPowerUpTimingCoroutine = null;
        }
        
        gameObject.SetActive(false);
    }

    public void StopTwoStateAnimation()
    {
        _animationTimeStop = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        _isTickStart = false;
        _newTickDuration = _creatingAnimationDuration - _passedTickTime;
        _newTickDestroyDuration = _destroyingAnimationDuration - _passedTickTime;
    }

    public void ResumeTwoStateAnimation()
    {
        if (_startBubbleAnimation)
        {
            if (_startBubbleCoroutine != null)
            {
                StopCoroutine(_startBubbleCoroutine);
            }
            _startBubbleCoroutine = null;
            _startBubbleCoroutine = StartCoroutine(StartBubbleCoroutine());
        }
        else if(_endBubbleAnimation)
        {
            if (_endBubbleCoroutine != null)
            {
                StopCoroutine(_endBubbleCoroutine);
            }
            _endBubbleCoroutine = null;
            _endBubbleCoroutine = StartCoroutine(EndBubbleCoroutine());
        }
    }

    private IEnumerator StartBubbleCoroutine()
    {
        _startBubbleAnimation = true;
        
        var animStopTime = _animationTimeStop;
        
        animatorImage.enabled = true;
        _isTickStart = true;
        animator.Play(CreateAnimationState, 0, animStopTime);
        _animationTimeStop = 0;

        if (_newTickDuration > 0)
        {
            yield return new WaitForSeconds(_newTickDuration);
            _newTickDuration = 0;
        }
        else
        {
            yield return _powerUpCreationWait;
        }
        
        if (_shieldCount > 1)
        {
            PowerUpCountTurn(true);
            powerUpCount.text = "x" + _shieldCount;
        }
        else
        {
            PowerUpCountTurn(false);
        }
        
        _isTickStart = false;
        _startBubbleAnimation = false;
        _passedTickTime = 0;
        mainDefaultImage.enabled = true;

        _startBubbleCoroutine = null;
    }

    private IEnumerator EndBubbleCoroutine()
    {
        _endBubbleAnimation = true;
        animator.enabled = true;
        _isTickStart = true;
        var animStopTime = _animationTimeStop;
        animator.Play(DestroyAnimationState, 0, animStopTime);
        _animationTimeStop = 0;
        
        if (powerUpCount.isActiveAndEnabled)
        {
            powerUpCount.text = "x" + _shieldCount;
        }

        if (_newTickDestroyDuration > 0)
        {
            yield return new WaitForSeconds(_newTickDestroyDuration);
            _newTickDestroyDuration = 0;
        }
        else
        {
            yield return _powerUpDestroyingWait;
        }
        
        _passedTickTime = 0;
        _isTickStart = false;
        _endBubbleAnimation = false;
        
        if (_shieldCount != 0)
        {
            animator.Play(DefaultAnimationState);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void MinusShield()
    {
        _shieldCount--;
        if (_shieldCount == 0)
        {
            mainDefaultImage.enabled = false;
        }

        if (_shieldCount <= 1)
        {
            PowerUpCountTurn(false);
        }
        _endBubbleCoroutine = StartCoroutine(EndBubbleCoroutine());
    }
}
