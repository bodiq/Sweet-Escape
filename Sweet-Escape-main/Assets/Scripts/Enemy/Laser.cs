using System;
using System.Collections;
using DG.Tweening;
using Enums;
using UnityEngine;

public class Laser : MonoBehaviour, Enemy.IEnemy
{
    [SerializeField] private float durationLaserActivation;
    [SerializeField] private SpriteRenderer leftGenerator;
    [SerializeField] private SpriteRenderer rightGenerator;
    
    private const float DelayBeforeLaserActivation = 3f;
    private const float DelayBeforeLaserDeactivation = 1.5f;
    
    private static readonly WaitForSeconds DelayBeforeSpikeActivation = new(DelayBeforeLaserActivation);
    private static readonly WaitForSeconds DelayBeforeSpikeDeactivation = new(DelayBeforeLaserDeactivation);

    private Vector3 _startScale;
    private readonly Vector3 _endScale = new(2f, 0f, 1f);

    private bool _prepareActivation;
    
    private void Start()
    {
        _startScale = transform.localScale;
        StartCoroutine(LaserWorks());
    }

    private void Update()
    {
        if (!_prepareActivation) return;
        
        leftGenerator.color = Color.Lerp(Color.white, Color.magenta, Mathf.PingPong(Time.time * 2f, 1f));
        rightGenerator.color = Color.Lerp(Color.white, Color.magenta, Mathf.PingPong(Time.time * 2f, 1f));
    }

    private IEnumerator LaserWorks()
    {
        while (true)
        {
            _prepareActivation = true;
            yield return DelayBeforeSpikeActivation;
            _prepareActivation = false;
            LaserStart();
            yield return DelayBeforeSpikeDeactivation;
            LaserEnd();
        }
    }

    private void LaserStart()
    {
        MoveLaser(_startScale, durationLaserActivation, Ease.InQuad);
    }

    private void LaserEnd()
    {
        MoveLaser(_endScale, durationLaserActivation, Ease.InQuad);
    }
    
    public void OnEnter(Player player)
    {
        player.OnPlayerDamage();
    }

    public void OnExit()
    {
        Debug.Log("Exit");
    }

    public GameObject GameObject => gameObject;
    public void ChangeMovement()
    {
        
    }

    public void Freeze()
    {
        throw new NotImplementedException();
    }

    public void UnFreeze()
    {
        throw new NotImplementedException();
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
        return EnemyType.Barrel;
    }

    #region Tween

    private Tweener _laserControlTween;

    private Tweener MoveLaser(Vector3 endValue, float duration, Ease ease = Ease.Linear)
    {
        if (_laserControlTween.IsActive())
        {
            _laserControlTween.ChangeEndValue(endValue, duration, true)
                .SetEase(ease)
                .Restart();
        }
        else
        {
            _laserControlTween = transform.DOScale(endValue, duration)
                .SetEase(ease)
                .SetLink(gameObject)
                .SetAutoKill(false);
        }

        return _laserControlTween;
    }
    #endregion
}
