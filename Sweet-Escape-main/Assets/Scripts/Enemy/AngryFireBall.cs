using System;
using System.Collections;
using Audio;
using Enums;
using UnityEngine;
using AudioType = Audio.AudioType;

public class AngryFireBall : MonoBehaviour, Enemy.IEnemy
{
    [SerializeField] private Animator animator;
    [SerializeField] private bool isInflated;
    [SerializeField] private float breathingAnimationDuration;
    [SerializeField] private BoxCollider2D collider;

    private Coroutine _animationCoroutine;

    private WaitForSeconds _animationExplodes;
    private WaitForSeconds _animationBreathing;

    private float _explodesAnimationDuration;

    private const string BreathingAnimationState = "AngryFireBallBreathing";
    private const string ExplodingAnimationState = "AngryFireBallExplodes";

    private AudioSource _fireballExplodeAudioSource;
    private AudioSource _fireballIdleAudioSource;
    
    private void Awake()
    {
        GameManager.Instance.Enemies.Add(this);
    }

    private void Start()
    {
        var clips = animator.runtimeAnimatorController.animationClips;

        foreach (var clip in clips)
        {
            if (clip.name == ExplodingAnimationState)
            {
                _explodesAnimationDuration = clip.length;
            }
        }
        
        _animationBreathing = new WaitForSeconds(breathingAnimationDuration);
        _animationExplodes = new WaitForSeconds(_explodesAnimationDuration);
    }

    private void OnEnable()
    {
        GameManager.Instance.OnStopGame += TurnOffSound;
        GameManager.Instance.OnResumeGame += TurnOnSound;
    }

    private void TurnOnSound()
    {
        if (_fireballIdleAudioSource && _fireballIdleAudioSource.enabled)
        {
            _fireballIdleAudioSource.Play();
        }
    }

    private void TurnOffSound()
    {
        if (_fireballExplodeAudioSource && _fireballExplodeAudioSource.enabled)
        {
            _fireballExplodeAudioSource.Stop();
        }

        if (_fireballIdleAudioSource && _fireballIdleAudioSource.enabled)
        {
            _fireballIdleAudioSource.Stop();
        }
    }

    private void OnBecameVisible()
    {
        if (!animator.enabled)
        {
            animator.enabled = true;
        }

        _animationCoroutine = StartCoroutine(StartAnimation());
    }

    private void OnBecameInvisible()
    {
        animator.enabled = false;
        if (_fireballExplodeAudioSource != null)
        {
            _fireballExplodeAudioSource.gameObject.SetActive(false);
        }

        if (_fireballIdleAudioSource != null)
        {
            _fireballIdleAudioSource.gameObject.SetActive(false);
        }
        
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
    }

    private IEnumerator StartAnimation()
    {
        _fireballIdleAudioSource = AudioManager.Instance.PlaySFX(AudioType.FireballIdle, true);
        while (true)
        {
            animator.Play(BreathingAnimationState);
            yield return _animationBreathing;
            _fireballExplodeAudioSource = AudioManager.Instance.PlaySFX(AudioType.FireballExplode);
            animator.Play(ExplodingAnimationState);
            yield return _animationExplodes;
            _fireballExplodeAudioSource.Stop();
            _fireballExplodeAudioSource.gameObject.SetActive(false);
        }
    }

    public void OnEnter(Player player)
    {
        if (isInflated)
        {
            player.OnPlayerDamage();
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
        animator.enabled = false;
    }

    public void UnFreeze()
    {
        animator.enabled = true;
    }

    public DirectionEnum GetEnemyDirection()
    {
        return DirectionEnum.None;
    }

    public void TurnCollider(bool isActive)
    {
        collider.isTrigger = !isActive;
    }

    public EnemyType GetEnemyType()
    {
        return EnemyType.AngryFireBall;
    }

    public void Activate()
    {
        gameObject.tag = "Enemy";
        gameObject.layer = 2;
        isInflated = true;
    }

    public void Deactivate()
    {
        gameObject.tag = "Untagged";
        gameObject.layer = 0;
        isInflated = false;
    }
}