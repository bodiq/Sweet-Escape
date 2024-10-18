using System.Collections.Generic;
using Configs;
using DG.Tweening;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelection : MonoBehaviour
{
    [SerializeField] private List<Skin> skinData;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button closeButton;
    [SerializeField] private SpriteRenderer characterSkinIcon;
    [SerializeField] private Animator animator;

    private const float DurationFadeIn = 0.5f;
    private const float DurationFadeOut = 0.5f;
    
    private Skin _lastSelectedSkin;
    private Characters _character;

    private void Start()
    {
        _lastSelectedSkin = skinData[0];
    }
    
    private void OnEnable()
    {
        closeButton.onClick.AddListener(FadeOut);

        if (_lastSelectedSkin)
        {
            _lastSelectedSkin.TurnOnSelected();
        }
        else
        {
            _lastSelectedSkin = skinData[0];
            _lastSelectedSkin.TurnOnSelected();
        }
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(FadeOut);
    }

    public void Initialize(Characters character)
    {
        if (!CharacterConfig.Instance.CharacterData.TryGetValue(character, out var data)) return;

        _character = character;
        
        for (var i = 0; i < skinData.Count; i++)
        {
            var isSelected = false;

            if (i == 0)
            {
                isSelected = true;
                _lastSelectedSkin = skinData[i];
            }

            var available = PlayerPrefs.HasKey(data.skinData[i].skinEnum.ToString());
            skinData[i].Initialize(data.skinData[i].skinIcon, data.skinData[i].skinName, available, isSelected, data.skinData[i].skinEnum, data.skinData[i].characterSkinSprite, data.skinData[i].defaultAnimatorController);
        }

        characterSkinIcon.sprite = data.skinData[0].characterSkinSprite;
        animator.runtimeAnimatorController = data.skinData[0].defaultAnimatorController;
    }

    public void ChangeSelectedSkin(SkinEnum skinEnum, Skin skin, Sprite skinSpite, RuntimeAnimatorController animatorController)
    {
        _lastSelectedSkin.TurnOffSelected();
        _lastSelectedSkin = skin;
        skin.TurnOnSelected();
        characterSkinIcon.sprite = skinSpite;
        animator.runtimeAnimatorController = animatorController;

        GameManager.Instance.skinEnum = skinEnum;
        GameManager.Instance.OnCharacterSkinChange?.Invoke(_character, skinEnum);
    }

    public void FadeIn()
    {
        canvasGroup.DOFade(1f, DurationFadeIn).SetEase(Ease.OutBack);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        characterSkinIcon.gameObject.SetActive(true);
    }

    private void FadeOut()
    {
        canvasGroup.DOFade(0f, DurationFadeOut).SetEase(Ease.OutBack);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        characterSkinIcon.gameObject.SetActive(false);
    }
}
