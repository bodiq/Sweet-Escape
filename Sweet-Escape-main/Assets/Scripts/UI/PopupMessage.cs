using System.Collections;
using Audio;
using DG.Tweening;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AudioType = Audio.AudioType;

public class PopupMessage : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    
    [SerializeField] private Image abilityImage;
    [SerializeField] private TextMeshProUGUI abilityName;
    [SerializeField] private TextMeshProUGUI abilityDescription;
    [SerializeField] private Button closeButton;

    [SerializeField] private TextMeshProUGUI popupMessage;
    
    [SerializeField] private float showMessageDelay;
    [SerializeField] private float durationFadeIn;
    [SerializeField] private float durationFadeOut;

    private Coroutine _showPopupCoroutine;

    private void OnEnable()
    {
        if (closeButton)
        {
            closeButton.onClick.AddListener(TurnOffPopup);
        }
    }

    private void OnDisable()
    {
        transform.localScale = Vector3.zero;
        _scalePopupTween.Kill();

        if (_showPopupCoroutine != null)
        {
            StopCoroutine(_showPopupCoroutine);
            _showPopupCoroutine = null;
        }

        if (closeButton)
        {
            closeButton.onClick.RemoveListener(TurnOffPopup);
        }
    }

    public void ShowPopupMessage(string customText = null)
    {
        if (customText != null)
        {
            popupMessage.text = customText;
        }
        _showPopupCoroutine ??= StartCoroutine(StartPopupAnimation());
    }

    public void ShowAbilityDescriptionMessage(AbilityData abilityData)
    {
        abilityImage.sprite = abilityData.AbilityIcon;
        abilityName.text = abilityData.AbilityName;
        abilityDescription.text = abilityData.AbilityDescription;
        ScalePopupTweener(Vector3.one, durationFadeIn, Ease.OutBounce);
        AudioManager.Instance.PlaySFX(AudioType.Popup);
    }

    private IEnumerator StartPopupAnimation()
    {
        ShowPopupTweener(1f, durationFadeIn);
        yield return new WaitForSeconds(showMessageDelay);
        ShowPopupTweener(0f, durationFadeOut);
        _showPopupCoroutine = null;
    }
    

    public void TurnOffPopup()
    {
        ScalePopupTweener(Vector3.zero, durationFadeOut, Ease.OutCirc).OnComplete(() =>
        {
            transform.localScale = Vector3.zero;
            _scalePopupTween.Kill(); 
        });  

        _showPopupCoroutine = null;
        
        UIManager.Instance.PopupManager.TurnBackground(false);
    }

    #region Tween

    private Tweener _fadePopupTween;
    private Tweener _scalePopupTween;

    private Tweener ShowPopupTweener(float endValue, float duration, Ease ease = Ease.Linear)
    {
        if (_fadePopupTween.IsActive())
        {
            _fadePopupTween.ChangeEndValue(endValue, duration, true)
                .SetEase(ease)
                .Restart();
        }
        else
        {
            _fadePopupTween = canvasGroup.DOFade(endValue, duration)
                .SetEase(ease)
                .SetLink(gameObject)
                .SetAutoKill(false);
        }

        return _fadePopupTween;
    }
    
    private Tweener ScalePopupTweener(Vector3 endValue, float duration, Ease ease = Ease.Linear)
    {
        if (_scalePopupTween.IsActive())
        {
            _scalePopupTween.ChangeEndValue(endValue, duration, true)
                .SetEase(ease)
                .Restart();
        }
        else
        {
            _scalePopupTween = transform.DOScale(endValue, duration)
                .SetEase(ease)
                .SetLink(gameObject)
                .SetAutoKill(false);
        }

        return _scalePopupTween;
    }

    #endregion
}
