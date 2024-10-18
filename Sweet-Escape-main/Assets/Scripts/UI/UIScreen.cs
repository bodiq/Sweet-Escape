using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIScreen : MonoBehaviour
{
    [SerializeField, ReadOnly] private bool _isOn;

    protected CanvasGroup _canvasGroup;

    public bool IsOn => _isOn;

    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void TurnOn()
    {
        gameObject.SetActive(true);
        _isOn = true;
        if (!_canvasGroup)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public virtual void TurnOff()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            _isOn = false;
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}