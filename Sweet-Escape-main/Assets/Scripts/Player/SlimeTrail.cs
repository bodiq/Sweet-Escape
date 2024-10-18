using System;
using System.Collections;
using Configs;
using DG.Tweening;
using Enums;
using Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class SlimeTrail : SerializedMonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private readonly WaitForSeconds _timeBeforeSlimeDisappear = new(SlimeConfig.DelayBeforeSlimeDisappear);
    private Coroutine _slimeCoroutine;

    private SlimeAnimationInfo slimeAnimationInfo;

    [Serializable]
    public struct SlimeAnimationInfo
    {
        public Sprite defaultSprite;
        public AnimationClip animationClip;
    }

    public void Initialize()
    {
        var slimeType = (SlimeEnum)typeof(SlimeEnum).GetRandomEnumValue();
        
        if (!SlimeConfig.Instance.slimeData.TryGetValue(GameManager.Instance.skinEnum, out var material)) return;
        if(!SlimeConfig.Instance.slimeDataAnimationClips.TryGetValue(slimeType, out slimeAnimationInfo)) return;

        spriteRenderer.sprite = slimeAnimationInfo.defaultSprite;
        spriteRenderer.material = material;
    }

    private void OnEnable()
    {
        spriteRenderer.DOFade(SlimeConfig.EndFadeValue, SlimeConfig.Instance.durationFadeIn);
        animator.Play(slimeAnimationInfo.animationClip.name);
        _slimeCoroutine ??= StartCoroutine(StartSlimeCoroutine());
    }

    private IEnumerator StartSlimeCoroutine()
    { 
        yield return _timeBeforeSlimeDisappear;
        spriteRenderer.DOFade(SlimeConfig.StartFadeValue, SlimeConfig.Instance.durationFadeOut).OnComplete(() =>
        {
            transform.position = Vector3.zero;
            gameObject.SetActive(false);
        });
    }

    private void OnDisable()
    {
        if (_slimeCoroutine == null)
        {
            return;
        }
        
        spriteRenderer.color = Color.white;
        StopCoroutine(_slimeCoroutine);
        _slimeCoroutine = null;
    }
}
