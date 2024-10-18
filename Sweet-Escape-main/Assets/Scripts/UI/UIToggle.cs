using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIToggle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool isMusic = false;
    [SerializeField] private bool isSound = false;

    [SerializeField] private Sprite enabledSprite;
    [SerializeField] private Sprite disabledSprite;

    [SerializeField] private TextMeshProUGUI textDescription;

    private const string MusicOff = "Music Off";
    private const string MusicOn = "Music On";

    private const string SoundOn = "Sound On";
    private const string SoundOff = "Sound Off";

    private bool _value = true;
    private Image _image;

    public event Action<bool> Changed;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ChangeValue();
        Changed?.Invoke(_value);
    }

    private void SetValue(bool isEnabled)
    {
        _value = isEnabled;
        _image.sprite = isEnabled ? enabledSprite : disabledSprite;

        if (isMusic)
        {
            textDescription.text = isEnabled ? MusicOn : MusicOff;
        }
        else if (isSound)
        {
            textDescription.text = isEnabled ? SoundOn : SoundOff;
        }
    }

    private void ChangeValue()
    {
        SetValue(!_value);
    }
}