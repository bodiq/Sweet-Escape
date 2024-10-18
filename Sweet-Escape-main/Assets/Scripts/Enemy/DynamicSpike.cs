using System.Collections;
using DG.Tweening;
using Enums;
using UnityEngine;

public class DynamicSpike : MonoBehaviour, Enemy.IEnemy
{
    [Header("Activation Duration")]
    [SerializeField] private float spikeTimeActivation;
    [SerializeField] private float spikeTimeDeactivation;

    [Space] [Header("Activation Delay")] 
    [SerializeField] private float spikeDelayActivation;
    [SerializeField] private float spikeDelayDeactivation;

    [SerializeField] private BoxCollider2D collider;
    
    private readonly Vector3 _endPosValue = new(0f, 1f, 0f);
    
    private bool _spikeActivated;

    private WaitForSeconds _waitForSpikeActivation;
    
    private Coroutine _spikeActivationCoroutine;

    private Player _player;

    private void Start()
    {
        _waitForSpikeActivation = new WaitForSeconds(spikeTimeDeactivation);
    }

    public void OnEnter(Player player)
    {
        _player = player;
        
        if (!_spikeActivated)
        {
            _spikeActivationCoroutine ??= StartCoroutine(SpikeActivation());
        }
        else
        {
            player.OnPlayerDamage();
        }
    }

    public void OnExit()
    {
        _player = null;
    }

    public GameObject GameObject => gameObject;
    public void ChangeMovement()
    {
        
    }

    public void Freeze()
    {
        
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
        return EnemyType.HiddenSpike;
    }

    private IEnumerator SpikeActivation()
    {
        MoveSpike(_endPosValue, spikeTimeActivation, spikeDelayActivation).OnPlay(() =>
        {
            if (_player == null) return;
            
            _player.OnPlayerDamage();
            _player = null;
            
        }).OnComplete(() =>
        {
            MoveSpike(Vector3.zero, spikeTimeDeactivation, spikeDelayDeactivation).OnPlay(null).OnComplete(() =>
            {
                _spikeActivated = false;
                _spikeActivationCoroutine = null;
            });
        });
        yield return _waitForSpikeActivation;
        _spikeActivated = true;
    }

    #region Tween

    private Tweener _movingSpikeTween;

    private Tweener MoveSpike(Vector3 endValue, float duration, float delay, Ease ease = Ease.Linear)
    {
        if (_movingSpikeTween.IsActive())
        {
            _movingSpikeTween.ChangeEndValue(endValue, duration, true)
                .SetEase(ease)
                .Restart();
        }
        else
        {
            _movingSpikeTween = transform.DOLocalMove(endValue, duration)
                .SetEase(ease)
                .SetLink(gameObject)
                .SetDelay(delay)
                .SetAutoKill(false);
        }

        return _movingSpikeTween;
    }

    #endregion
}
