using Audio;
using DG.Tweening;
using Enemy;
using Enums;
using UnityEngine;
using AudioType = Audio.AudioType;

public class BarrelDropper : MonoBehaviour, IEnemy
{
    [SerializeField] private Transform body;
    [SerializeField] private Barrel barrel;
    [SerializeField] private Animator animator;
    [SerializeField] private BarrelAttackZone barrelAttackZone;
    [SerializeField] private BarrelDropperBody barrelDropperBody;

    private BarrelDropperStates _state;
    private Camera _mainCamera;
    private Vector3 _startPosition;
    private Vector3 _dropPosition;
    
    private AudioSource _flyAudioSource;

    private void Awake()
    {
        GameManager.Instance.Enemies.Add(this);
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        barrelAttackZone.PlayerCollided += StartDropping;
        barrelDropperBody.BecameInvisible += OnBodyBecameInvisible;
        barrelDropperBody.DropAnimationFinished += Drop;
        barrelDropperBody.FlyAwayAnimationStarted += FlyAway;
        GameManager.Instance.OnPlayerRespawn += ResetState;
        
        GameManager.Instance.OnStopGame += TurnMusicOff;
        GameManager.Instance.OnResumeGame += TurnMusicOn;
    }

    private void TurnMusicOn()
    {
        if (_flyAudioSource.enabled)
        {
            _flyAudioSource.Play();
        }
    }

    private void TurnMusicOff()
    {
        if (_flyAudioSource.enabled)
        {
            _flyAudioSource.Stop();
        }
    }

    private void OnDisable()
    {
        barrelAttackZone.PlayerCollided -= StartDropping;
        barrelDropperBody.BecameInvisible -= OnBodyBecameInvisible;
        barrelDropperBody.DropAnimationFinished -= Drop;
        barrelDropperBody.FlyAwayAnimationStarted -= FlyAway;
        GameManager.Instance.OnPlayerRespawn -= ResetState;
        
        GameManager.Instance.OnStopGame -= TurnMusicOff;
        GameManager.Instance.OnResumeGame -= TurnMusicOn;
    }

    private void Update()
    {
        switch (_state)
        {
            case BarrelDropperStates.FlyingAway:
                body.position += Vector3.up * (BarrelDropperConfig.Instance.MoveSpeed * Time.deltaTime);
                break;
        }
    }

    private void OnBodyBecameInvisible()
    {
        if (_state == BarrelDropperStates.FlyingAway)
        {
            Disable();
        }
    }

    private void Disable()
    {
        _state = BarrelDropperStates.Disabled;
        barrelDropperBody.gameObject.SetActive(false);
    }

    private void StartDropping()
    {
        if (_state != BarrelDropperStates.Waiting)
        {
            return;
        }

        
        animator.Play(BarrelDropperConfig.Instance.FlyingDownClip.name);
        _flyAudioSource = AudioManager.Instance.PlaySFX(AudioType.BarrelDropperFly, true);
        
        var screenTopPoint = _mainCamera.ScreenToWorldPoint(new Vector3(0, _mainCamera.pixelHeight, 0)).y;
        var position = body.position;
        
        _startPosition = new Vector3(position.x, screenTopPoint, 0) + Vector3.up;
        _dropPosition = new Vector3(position.x, screenTopPoint, 0) + Vector3.down * 5;
        position = _startPosition;
        body.position = position;
        body.gameObject.SetActive(true);
        _state = BarrelDropperStates.FlyingIn;
        body.DOMove(_dropPosition, BarrelDropperConfig.Instance.MoveSpeed).SetSpeedBased().OnComplete(() =>
        {
            _state = BarrelDropperStates.Dropping;
            animator.Play(BarrelDropperConfig.Instance.DroppingClip.name);
        });
    }

    private void Drop()
    {
        barrel.Drop(_dropPosition);
    }

    private void FlyAway()
    {
        animator.Play(BarrelDropperConfig.Instance.FlyingUpClip.name);
        _state = BarrelDropperStates.FlyingAway;
        _flyAudioSource.Stop();
        _flyAudioSource.gameObject.SetActive(false);
    }

    public void OnEnter(Player player)
    {
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
        //TODO: change logic
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
        return;
    }

    public EnemyType GetEnemyType()
    {
        return EnemyType.BarrelDropper;
    }

    private void ResetState()
    {
        _state = BarrelDropperStates.Waiting;
    }
}