using System;
using Audio;
using Enemy;
using Enums;
using UnityEngine;
using UnityEngine.UI;
using AudioType = Audio.AudioType;

public class HiddenSpike : MonoBehaviour, IEnemy, ITrigger
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform spikeTransform;
    [SerializeField] private Sprite defaultSpikeSprite;
    [SerializeField] private SpriteRenderer image;

    Transform ITrigger.Transform => spikeTransform;

    private void Awake()
    {
        GameManager.Instance.Enemies.Add(this);
    }

    public void Trigger(Player player)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0))
        {
            animator.Play("HiddenSpikeAttack", -1, 0f);
        }
    }

    private void OnEnable()
    {
        GameManager.Instance.OnPlayerRespawn += SetToDefaultState;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPlayerRespawn -= SetToDefaultState;
    }

    private void SetToDefaultState()
    {
        spikeTransform.gameObject.SetActive(false);
        image.sprite = defaultSpikeSprite;
    }

    public void PlayHiddenSpikeSound()
    {
        AudioManager.Instance.PlaySFX(AudioType.HiddenSpikes);
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
        return;
    }

    public EnemyType GetEnemyType()
    {
        return EnemyType.HiddenSpike;
    }
}