using System;
using Audio;
using Enemy;
using Enums;
using UnityEngine;
using AudioType = Audio.AudioType;

public class Barrel : MonoBehaviour, IEnemy
{
    [SerializeField] private Animator animator;
    [SerializeField] private CircleCollider2D collider;

    private bool _isDropping;
    
    private AudioSource _dropAudioSource;
    private AudioSource _explodesAudioSource;

    private void Awake()
    {
        GameManager.Instance.Enemies.Add(this);
    }

    private void OnEnable()
    {
        GameManager.Instance.OnStopGame += TurnMusicOff;
        GameManager.Instance.OnResumeGame += TurnMusicOn;
    }

    private void TurnMusicOn()
    {
        if (_dropAudioSource.enabled)
        {
            _dropAudioSource.Play();
        }
    }

    private void TurnMusicOff()
    {
        if (_dropAudioSource.enabled)
        {
            _dropAudioSource.Stop();
        }
    }

    private void Update()
    {
        if (_isDropping)
        {
            transform.position += Vector3.down * (BarrelDropperConfig.Instance.BarrelSpeed * Time.deltaTime);
        }
    }

    private void OnBecameInvisible()
    {
        _dropAudioSource.Stop();
        _dropAudioSource.gameObject.SetActive(false);
        Disable();
    }

    private void Disable()
    {
        _isDropping = false;
        gameObject.SetActive(false);
    }

    public void Drop(Vector3 position)
    {
        transform.position = position;
        _isDropping = true;
        gameObject.SetActive(true);
        animator.Play("BarrelFlying");
        _dropAudioSource = AudioManager.Instance.PlaySFX(AudioType.BarrelDropperBombFall);
    }

    public void OnEnter(Player player)
    {
        if (!player.HasImmunity)
        {
            player.OnPlayerDamage();
            animator.Play("BarrelExplode");
            _isDropping = false;
            _dropAudioSource.Stop();
            _dropAudioSource.gameObject.SetActive(false);
            AudioManager.Instance.PlaySFX(AudioType.BarrelDropperBombExplode);
        }
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
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
        Disable();
    }

    public void UnFreeze()
    {
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
        return EnemyType.Barrel;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnStopGame -= TurnMusicOff;
        GameManager.Instance.OnResumeGame -= TurnMusicOn;
    }
}