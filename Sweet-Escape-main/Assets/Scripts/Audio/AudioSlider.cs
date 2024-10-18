using System;
using Audio;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private MusicType musicType;

    private void Start()
    {
        switch (musicType)
        {
            case MusicType.Music:
                if (!PlayerPrefs.HasKey(GameManager.UserMusicVolumeKey))
                {
                    PlayerPrefs.SetFloat(GameManager.UserMusicVolumeKey, 1f);
                }
                else
                {
                    LoadMusic();
                }
                break;
            case MusicType.SoundFX:
                if (!PlayerPrefs.HasKey(GameManager.UserSoundFXVolumeKey))
                {
                    PlayerPrefs.SetFloat(GameManager.UserSoundFXVolumeKey, 1f);
                }
                else
                {
                    LoadSoundFX();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ChangeVolume()
    {
        AudioManager.Instance.ChangeAllVolume(musicType, volumeSlider.value);
        Save();
    }

    private void Save()
    {
        if (musicType == MusicType.Music)
        {
            PlayerPrefs.SetFloat(GameManager.UserMusicVolumeKey, volumeSlider.value);
        }
        else
        {
            PlayerPrefs.SetFloat(GameManager.UserSoundFXVolumeKey, volumeSlider.value);
        }
    }

    private void LoadSoundFX()
    {
        volumeSlider.value = PlayerPrefs.GetFloat(GameManager.UserSoundFXVolumeKey);
    }
    
    private void LoadMusic()
    {
        volumeSlider.value = PlayerPrefs.GetFloat(GameManager.UserMusicVolumeKey);
    }
}
