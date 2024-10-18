using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Enums;
using MoreMountains.Feedbacks;
using UnityEngine;
using AudioType = Audio.AudioType;

public class SmallBot : MonoBehaviour, Enemy.IEnemy
{
    [SerializeField] private Animator animator;
    [SerializeField] private OrientationEnum orientation;
    [SerializeField] private BoxCollider2D collider;

    private bool _isFrozen;
    private Vector3 _targetPosition;

    private Vector3 _firstPoint;
    private Vector3 _secondPoint;

    private Coroutine _patrollingCoroutine;

    private DirectionEnum _firstDirection = DirectionEnum.None;
    private DirectionEnum _secondDirection = DirectionEnum.None;
    private DirectionEnum _currentDirection = DirectionEnum.None;

    private readonly Dictionary<DirectionEnum, string> _animations = new()
    {
        { DirectionEnum.Down, "SmallBotDown" },
        { DirectionEnum.Up, "SmallBotUp" },
        { DirectionEnum.Right, "SmallBotRight" },
        { DirectionEnum.Left, "SmallBotLeft" },
    };

    private void Awake()
    {
        GameManager.Instance.Enemies.Add(this);
    }

    private void OnEnable()
    {
        GameManager.Instance.OnPlayerRespawn += UnFreeze;

        CalculateMovePositions();

        _currentDirection = _firstDirection;
        _targetPosition = _secondPoint;
        _patrollingCoroutine = StartCoroutine(PatrollingCoroutine());

        GameManager.Instance.OnResumeGame += TurnOnSound;
        GameManager.Instance.OnStopGame += TurnOffSound;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPlayerRespawn -= UnFreeze;

        StopCoroutine(_patrollingCoroutine);
        _patrollingCoroutine = null;
        
        GameManager.Instance.OnResumeGame -= TurnOnSound;
        GameManager.Instance.OnStopGame -= TurnOffSound;
    }

    private void CalculateMovePositions()
    {
        switch (orientation)
        {
            case OrientationEnum.Horizontal:
                var rightHitPoint = CalculateHitPoint(transform.position, Vector3.right);
                var leftHitPoint = CalculateHitPoint(transform.position, Vector3.left);
                _firstPoint = new Vector3(leftHitPoint.x + 0.5f, leftHitPoint.y, 0);
                _secondPoint = new Vector3(rightHitPoint.x - 0.5f, rightHitPoint.y, 0);
                _firstDirection = DirectionEnum.Right;
                _secondDirection = DirectionEnum.Left;
                break;
            case OrientationEnum.Vertical:
                var upHitPoint = CalculateHitPoint(transform.position, Vector3.up);
                var downHitPoint = CalculateHitPoint(transform.position, Vector3.down);
                _firstPoint = new Vector3(upHitPoint.x, upHitPoint.y - 0.5f, 0);
                _secondPoint = new Vector3(downHitPoint.x, downHitPoint.y + 0.5f, 0);
                _firstDirection = DirectionEnum.Down;
                _secondDirection = DirectionEnum.Up;
                break;
        }
    }

    private Vector2 CalculateHitPoint(Vector3 transformPosition, Vector3 direction)
    {
        var hit = Physics2D.Raycast(transformPosition, direction, Mathf.Infinity,
            1 << LayerMask.NameToLayer("Default"));

        if (hit.collider == null)
        {
            throw new NotImplementedException("Small bot doesn't have borders to bounce.");
        }

        return hit.point;
    }

    private void OnBecameVisible()
    {
        _isVisible = true;
        _flyingAudioSource = AudioManager.Instance.PlaySFX(AudioType.SmallBotFly, true);
    }

    private void OnBecameInvisible()
    {
        _isVisible = false;
        if (_flyingAudioSource)
        {
            _flyingAudioSource.Stop();
            _flyingAudioSource.gameObject.SetActive(false);
            _flyingAudioSource = null;
        }
    }
    
    private void TurnOnSound()
    {
        if (_flyingAudioSource && _flyingAudioSource.enabled && _isVisible)
        {
            _flyingAudioSource.Play();
        }
    }

    private void TurnOffSound()
    {
        if (_flyingAudioSource && _flyingAudioSource.enabled)
        {
            _flyingAudioSource.Stop();
        }
    }
    
    private AudioSource _flyingAudioSource;
    private bool _isVisible = false;

    private IEnumerator PatrollingCoroutine()
    {
        while ((transform.position - _targetPosition).magnitude >= SmallBotConfig.Instance.ReachTargetAccuracy)
        {
            if (!_isFrozen)
            {
                transform.position += (_targetPosition - transform.position).normalized * (SmallBotConfig.Instance.MoveSpeed * Time.deltaTime);
            }

            yield return null;
        }

        _currentDirection = _currentDirection == _firstDirection ? _secondDirection : _firstDirection;
        _targetPosition = _targetPosition == _firstPoint ? _secondPoint : _firstPoint;
        animator.Play(_animations[_currentDirection]);

        yield return PatrollingCoroutine();
    }

    public void OnEnter(Player player)
    {
        if (!player.HasImmunity)
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
        _isFrozen = true;
    }

    public void UnFreeze()
    {
        animator.enabled = true;
        _isFrozen = false;
    }

    public DirectionEnum GetEnemyDirection()
    {
        return _currentDirection;
    }

    public void TurnCollider(bool isActive)
    {
        collider.isTrigger = !isActive;
    }

    public EnemyType GetEnemyType()
    {
        return EnemyType.SmallBot;
    }
}