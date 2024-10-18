using System;
using System.Collections;
using Audio;
using DG.Tweening;
using Enemy;
using Enums;
using Extensions;
using UnityEngine;

public class FireCannon : MonoBehaviour, Enemy.IEnemy
{
    [SerializeField] private Transform startFirePoint;
    [SerializeField] private SpriteRenderer flameSpriteRenderer;
    [SerializeField] private GameObject fireBall;
    [SerializeField] private GameObject fireImpact;
    [SerializeField] private BoxCollider2D damageCollider;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip startFlameClip;
    [SerializeField] private AudioClip workFlameClip;
    [SerializeField] private AudioClip endFlameClip;

    [Header("Animators")]
    [SerializeField] private Animator gunAnimator;

    [Header("TimingSettings")]
    [SerializeField] private float fireCannonDelayBeforeStart = 1.5f;
    [SerializeField] private float fireCannonActivationDuration = 2f;
    [SerializeField] private float fireCannonShootingDuration = 1.5f;
    [SerializeField] private float fireCannonDeactivationDuration = 0.5f;
    [SerializeField] private float flameMovingToDestinationDuration = 0.15f;
    [SerializeField] private float flameMovingToDestinationDelay = 0.35f;
    
    private WaitForSeconds _waitCannonReloading;
    private WaitForSeconds _waitCannonActivating;
    private WaitForSeconds _waitCannonShooting;
    private WaitForSeconds _waitCannonDeactivating;

    private const string IdleCannonGunAnimationState = "Idle";
    private const string StartShootingAnimationState = "StartShooting";
    private const string ShootingAnimationState = "Shooting";
    private const string StopShootingAnimationState = "StopShooting";

    private string _currentState;
    
    private float _distance;
    
    private const float EndFlameSpriteHeight = 0.5f;
    private const float EndFlameColliderHeight = 0.5f;

    private readonly Vector2 _startFlameSpriteSize = new (0f, 0.5f);
    private readonly Vector2 _startColliderOffset = Vector2.zero;
    private readonly Vector2 _startColliderSize = Vector2.zero;
    private Vector2 _endFlameSpriteSize;
    private Vector2 _endColliderOffset;
    private Vector2 _endColliderSize;
    
    private Vector3 _endFlamePosition;
    private Vector3 _initialFireBallPosition;

    private Coroutine _flameCoroutine;

    private void Awake()
    {
        GameManager.Instance.Enemies.Add(this);
    }

    private void Start()
    {
        var raycastHit = Physics2D.Raycast(startFirePoint.position, transform.right, Mathf.Infinity, 1 << LayerMask.NameToLayer("Default"));
        _endFlamePosition = raycastHit.point;
        
        _distance = Vector2.Distance(startFirePoint.position, _endFlamePosition);
        _endColliderOffset = new Vector2(_distance / 2, 0f);
        _endColliderSize = new Vector2(_distance, EndFlameColliderHeight);
        
        fireImpact.transform.position = _endFlamePosition;
        _initialFireBallPosition = fireBall.transform.position;
        _endFlameSpriteSize = new Vector2(_distance, EndFlameSpriteHeight);
        
        _waitCannonReloading = new WaitForSeconds(fireCannonDelayBeforeStart);
        _waitCannonActivating = new WaitForSeconds(fireCannonActivationDuration);
        _waitCannonShooting = new WaitForSeconds(fireCannonShootingDuration);
        _waitCannonDeactivating = new WaitForSeconds(fireCannonDeactivationDuration);
        
        _flameCoroutine ??= StartCoroutine(StartFireGun());
    }

    private void OnEnable()
    {
        var raycastHit = Physics2D.Raycast(startFirePoint.position, transform.right, Mathf.Infinity, 1 << LayerMask.NameToLayer("Default"));
        _endFlamePosition = raycastHit.point;
        
        _distance = Vector2.Distance(startFirePoint.position, _endFlamePosition);
        _endColliderOffset = new Vector2(_distance / 2, 0f);
        _endColliderSize = new Vector2(_distance, EndFlameColliderHeight);
        
        fireImpact.transform.position = _endFlamePosition;
        _initialFireBallPosition = fireBall.transform.position;
        _endFlameSpriteSize = new Vector2(_distance, EndFlameSpriteHeight);
        
        _flameCoroutine ??= StartCoroutine(StartFireGun());
        GameManager.Instance.OnStopGame += TurnOffSound;
        GameManager.Instance.OnResumeGame += TurnOnSound;

        if (!GameManager.Instance.IsTestEnvironment)
        {
            AudioManager.Instance.AddEnemyAudioSourceToPool(audioSource);
            SetSoundData();   
        }
    }

    private void SetSoundData()
    {
        var volume = PlayerPrefs.GetFloat(GameManager.UserSoundFXVolumeKey);
        audioSource.volume = volume;
    }

    private void TurnOffSound()
    {
        if (audioSource.enabled)
        {
            audioSource.Stop();
        }
    }

    private void TurnOnSound()
    {
        if (audioSource.enabled)
        {
            audioSource.Play();
        }
    }

    private void OnDisable()
    {
        if (_flameCoroutine != null)
        {
            StopCoroutine(_flameCoroutine);
            _flameCoroutine = null;
        }
        
        if (audioSource)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
        }
        
        _movingCannonTween.Kill();
        ResetAllObjectsToStartPosition();
        GameManager.Instance.OnStopGame -= TurnOffSound;
        GameManager.Instance.OnResumeGame -= TurnOnSound;
        
        AudioManager.Instance.enemyAudioSources.Remove(audioSource);
    }

    private IEnumerator StartFireGun()
    {
        while (true)
        {
            yield return _waitCannonReloading;
            if (audioSource.enabled)
            {
                audioSource.clip = startFlameClip;
                audioSource.loop = false;
                audioSource.Play();
            }
            ChangeAnimationState(StartShootingAnimationState, gunAnimator);
            MoveFlame(flameMovingToDestinationDuration, flameMovingToDestinationDelay).OnPlay(() =>
            {
                fireBall.SetActive(true);
                damageCollider.enabled = true;
            }).OnComplete(() =>
            {
                fireImpact.SetActive(true);
            });
            yield return _waitCannonActivating;
            if (audioSource.enabled)
            {
                audioSource.clip = workFlameClip;
                audioSource.loop = true;
                audioSource.Play();   
            }
            ChangeAnimationState(ShootingAnimationState, gunAnimator);
            yield return _waitCannonShooting;
            if (audioSource.enabled)
            {
                audioSource.clip = endFlameClip;
                audioSource.loop = false;
                audioSource.Play();
            }
            ResetAllObjectsToStartPosition();
            ChangeAnimationState(StopShootingAnimationState, gunAnimator);
            yield return _waitCannonDeactivating;
            ChangeAnimationState(IdleCannonGunAnimationState, gunAnimator);
        }
    }
    
    private void ChangeAnimationState(string newState, Animator animator)
    {
        if (_currentState == newState)
        {
            return;
        }
        
        animator.Play(newState);

        _currentState = newState;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject == GameManager.Instance.player.gameObject)
        {
            GameManager.Instance.player.CheckForFireImmune();
        }
    }

    public void OnEnter(Player player)
    {
        if (!player.HasImmunity)
        {
            player.CheckForFireImmune();
        }
    }

    public void OnExit()
    {
    }

    public GameObject GameObject => gameObject;
    public void ChangeMovement()
    {
        
    }

    public void Freeze()
    {
        ResetAllObjectsToStartPosition();
        
        if (_flameCoroutine != null)
        {
            StopCoroutine(_flameCoroutine);
            _flameCoroutine = null;
        }
        
        if (audioSource && audioSource.enabled)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
        }
        
        _movingCannonTween.Kill();
    }

    public void UnFreeze()
    {
        _flameCoroutine ??= StartCoroutine(StartFireGun());
    }

    public DirectionEnum GetEnemyDirection()
    {
        return DirectionEnum.None;
    }

    public void TurnCollider(bool isActive)
    {
        damageCollider.isTrigger = !isActive;
    }

    public EnemyType GetEnemyType()
    {
        return EnemyType.FireCannon;
    }

    private void ResetAllObjectsToStartPosition()
    {
        fireBall.SetActive(false);
        fireImpact.SetActive(false);
        
        if (gunAnimator.gameObject.activeInHierarchy)
        {
            ChangeAnimationState(IdleCannonGunAnimationState, gunAnimator);
        }
        
        damageCollider.offset = _startColliderOffset;
        damageCollider.size = _startColliderSize;
        damageCollider.enabled = false;
        flameSpriteRenderer.size = _startFlameSpriteSize;
        fireBall.transform.position = _initialFireBallPosition;
    }
    
    #region Tween

    private Tweener _movingCannonTween;
    private IEnemy _enemyImplementation;

    private Tweener MoveFlame(float duration, float delay, Ease ease = Ease.Linear)
    {
        if (_movingCannonTween.IsActive())
        {
            _movingCannonTween.ChangeValues(0f, 1f, duration)
                .SetEase(ease)
                .SetDelay(delay)
                .Restart();
        }
        else
        {
            _movingCannonTween = DOTween.To(Setter, 0f, 1f, duration)
                .SetEase(ease)
                .SetLink(gameObject)
                .SetDelay(delay)
                .SetAutoKill(false);
        }

        return _movingCannonTween;
    }

    private void Setter(float t)
    {
        fireBall.transform.position = Vector2.Lerp(_initialFireBallPosition, _endFlamePosition, t);
        flameSpriteRenderer.size = Vector2.Lerp(_startFlameSpriteSize, _endFlameSpriteSize, t);
        damageCollider.size = Vector2.Lerp(_startColliderSize, _endColliderSize, t);
        damageCollider.offset = Vector2.Lerp(_startColliderOffset, _endColliderOffset, t);
    }
    
    #endregion
}
