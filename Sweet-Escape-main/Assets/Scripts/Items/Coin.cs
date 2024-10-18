using System.Collections;
using System.Collections.Generic;
using Audio;
using Configs;
using DG.Tweening;
using UnityEngine;
using AudioType = Audio.AudioType;
using Random = UnityEngine.Random;

public class Coin : MonoBehaviour, ITrigger
{
    [SerializeField] private int reward;
    [SerializeField] private float durationJumpingAnimation = 1f;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform body;

    public static readonly List<GameObject> PoolUsed = new();

    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private Vector3 _initialPosition;

    private Coroutine _coinCoroutine;
    private Coroutine _coinCollectCoroutine;

    private readonly Vector3 _heightOffsetAnimation = new(0f, 0.05f, 0f);

    private WaitForSeconds _timeForFlyAnimation;
    private WaitForSeconds _timeForCollectAnimation;

    private bool _isEntered;

    private float _animationCollectingDuration = 0.4f;

    public bool goldenSpoon;

    public bool IsEntered => _isEntered;
    public Transform Transform => transform;

    private void Awake()
    {
        var clips = animator.runtimeAnimatorController.animationClips;

        foreach (var clip in clips)
        {
            if (clip.name == "CoinClaiming")
            {
                _animationCollectingDuration = clip.length;
            }
        }

        _timeForCollectAnimation = new WaitForSeconds(_animationCollectingDuration);
    }

    private void Start()
    {
        _timeForFlyAnimation = new WaitForSeconds(durationJumpingAnimation);
    }

    private void OnEnable()
    {
        _startPosition = transform.position + _heightOffsetAnimation;
        _endPosition = transform.position - _heightOffsetAnimation;

        _initialPosition = transform.position;
        _coinCoroutine ??= StartCoroutine(StartAnimation());

        _isEntered = false;
    }

    private void OnDisable()
    {
        transform.position = _initialPosition;

        if (_coinCoroutine != null)
        {
            StopCoroutine(_coinCoroutine);
            _coinCoroutine = null;
        }

        if (_coinCollectCoroutine != null)
        {
            StopCoroutine(_coinCollectCoroutine);
            _coinCollectCoroutine = null;
        }

        _movingCoinTween.Kill();
    }

    private IEnumerator StartAnimation()
    {
        while (true)
        {
            MoveCoin(_startPosition, durationJumpingAnimation);
            yield return _timeForFlyAnimation;
            MoveCoin(_endPosition, durationJumpingAnimation);
            yield return _timeForFlyAnimation;
        }
    }

    private Vector3 startPosition;
    private Transform endPoint;

    public void Trigger(Player player)
    {
        if (!_isEntered)
        {
            _isEntered = true;
            startPosition = transform.position;
            endPoint = player.transform;

            var playerMultiplierCoinsPowerUp = reward * player.multiplierCoins;

            var duration = Random.Range(CharacterConfig.Instance.MinimumDurationItemMoveToPlayer,
                CharacterConfig.Instance.MaximumDurationItemMoveToPlayer);

            DOTween.To(Setter, 0, 1, duration).SetEase(Ease.InQuint).OnComplete(() =>
            {
                player.OnPlayerGetCoin(Mathf.RoundToInt(playerMultiplierCoinsPowerUp));

                AudioManager.Instance.PlaySFX(AudioType.CollectCoin);

                if (gameObject is { activeSelf: true, activeInHierarchy: true })
                {
                    _coinCollectCoroutine = StartCoroutine(CollectAnimationCoroutine());
                }

                if (!goldenSpoon)
                {
                    PoolUsed.Add(gameObject);
                }
            });
        }
    }

    public void GrabCoinWithTongue(Player player)
    {
        if (!_isEntered)
        {
            _isEntered = true;
        
            var playerMultiplierCoinsPowerUp = reward * player.multiplierCoins;
        
            player.OnPlayerGetCoin(Mathf.RoundToInt(playerMultiplierCoinsPowerUp));
        
            AudioManager.Instance.PlaySFX(AudioType.CollectCoin);
        
            if (!goldenSpoon)
            {
                PoolUsed.Add(gameObject);
            }
            animator.Play("CoinClaiming");
            gameObject.SetActive(false);
        }
    }

    private IEnumerator CollectAnimationCoroutine()
    {
        animator.Play("CoinClaiming");
        yield return _timeForCollectAnimation;
        gameObject.SetActive(false);
    }

    private void Setter(float t)
    {
        var endPosition = endPoint.position - body.transform.localPosition;
        transform.position = Vector3.LerpUnclamped(startPosition, endPosition, t);
    }

    #region Tween

    private Tweener _movingCoinTween;

    private Tweener MoveCoin(Vector3 endValue, float duration, Ease ease = Ease.Linear)
    {
        if (_movingCoinTween.IsActive())
        {
            _movingCoinTween.ChangeEndValue(endValue, duration, true)
                .SetEase(ease)
                .Restart();
        }
        else
        {
            _movingCoinTween = transform.DOMove(endValue, duration)
                .SetEase(ease)
                .SetLink(gameObject)
                .SetAutoKill(false);
        }

        return _movingCoinTween;
    }

    #endregion

    public void Activate()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            PoolUsed.Remove(gameObject);
        }
    }
}