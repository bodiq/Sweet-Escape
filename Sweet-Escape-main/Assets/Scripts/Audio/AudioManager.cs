using System;
using System.Collections.Generic;
using Configs;
using Enums;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private int audioSourcePoolSize = 10;
        [SerializeField] private Transform audioSourceMusicPoolParent;
        [SerializeField] private Transform audioSourceSFXPoolParent;
        [SerializeField] private bool poolCanExpand;

        private AudioSourcePool _audioSourcePool;
        private GameObject _tempAudioSourceGameObject;
        
        public List<AudioType> sprinkleAudioTypes = new();
        public List<AudioType> moveSquishAudioTypes = new();

        public readonly Dictionary<Enums.PowerUps, AudioSource> TimerPowerUpsAudioSource = new();

        public List<AudioSource> enemyAudioSources = new();

        public AudioSource mainBackgroundAudioSource;

        public static AudioManager Instance { get; private set; }

        public bool isMusicOff;
        public bool isSfxOff;

        private void Awake()
        {
            InitializeSingleton();
            InitializeSoundSettings();
            
            sprinkleAudioTypes.Add(AudioType.Sprinkle1);
            sprinkleAudioTypes.Add(AudioType.Sprinkle2);
            sprinkleAudioTypes.Add(AudioType.Sprinkle3);
            sprinkleAudioTypes.Add(AudioType.Sprinkle4);
            sprinkleAudioTypes.Add(AudioType.Sprinkle5);
            
            moveSquishAudioTypes.Add(AudioType.Move1);
            moveSquishAudioTypes.Add(AudioType.Move2);
            moveSquishAudioTypes.Add(AudioType.Move3);
            moveSquishAudioTypes.Add(AudioType.Move4);
            moveSquishAudioTypes.Add(AudioType.Move5);
        }

        private void Start()
        {
            if (!PlayerPrefs.HasKey(GameManager.UserMusicVolumeKey))
            {
                PlayerPrefs.SetFloat(GameManager.UserMusicVolumeKey, 1f);
                ChangeAllVolume(MusicType.Music, 1f);
            }
            else
            {
                var volume = PlayerPrefs.GetFloat(GameManager.UserMusicVolumeKey);
                ChangeAllVolume(MusicType.Music, volume);
            }

            if (!PlayerPrefs.HasKey(GameManager.UserSoundFXVolumeKey))
            {
                PlayerPrefs.SetFloat(GameManager.UserSoundFXVolumeKey, 1f);
                ChangeAllVolume(MusicType.SoundFX, 1f);
            }
            else
            {
                var volume = PlayerPrefs.GetFloat(GameManager.UserSoundFXVolumeKey);
                ChangeAllVolume(MusicType.SoundFX, volume);
            }
        }

        public AudioType GetRandomSprinkleAudioType()
        {
            var randomIndex = Random.Range(0, sprinkleAudioTypes.Count - 1);
            return sprinkleAudioTypes[randomIndex];
        }

        public AudioType GetRandomMoveSquishAudioType()
        {
            var randomIndex = Random.Range(0, moveSquishAudioTypes.Count - 1);
            return moveSquishAudioTypes[randomIndex];
        }

        public void MainBackgroundMusicChangeVolume(float newVolume)
        {
            mainBackgroundAudioSource.volume = newVolume;
        }
        
        public AudioSource PlayMusic(AudioType audioType, bool isLoop = false)
        {
            if (!AudioConfig.Instance.AudioMusicPresets.TryGetValue(audioType, out var audioPreset))
            {
                return null;
            }

            var clip = audioPreset.audioClip;

            var audioSource = _audioSourcePool.GetAvailableMusicAudioSource(poolCanExpand, audioSourceMusicPoolParent);

            if (audioSource != null && !isLoop)
            {
                StartCoroutine(_audioSourcePool.AutoDisableAudioSource(clip.length / Mathf.Abs(audioSource.pitch), audioSource, clip, false));
            }

            if (audioSource == null)
            {
                _tempAudioSourceGameObject = new GameObject("AudioSource_" + clip.name);
                SceneManager.MoveGameObjectToScene(_tempAudioSourceGameObject, gameObject.scene);
                audioSource = _tempAudioSourceGameObject.AddComponent<AudioSource>();
            }
            
            audioSource.loop = isLoop;
            audioSource.clip = clip;

            if (audioSourceMusicPoolParent.gameObject.activeSelf)
            {
                audioSource.Play();
            }

            return audioSource;
        }
        
        public AudioSource PlaySFX(AudioType audioType, bool isLoop = false)
        {
            if (!AudioConfig.Instance.AudioSFXPresets.TryGetValue(audioType, out var audioPreset))
            {
                return null;
            }

            var clip = audioPreset.audioClip;

            var audioSource = _audioSourcePool.GetAvailableSFXAudioSource(poolCanExpand, audioSourceSFXPoolParent);

            if (audioSource != null && !isLoop)
            {
                StartCoroutine(_audioSourcePool.AutoDisableAudioSource(clip.length / Mathf.Abs(audioSource.pitch), audioSource, clip, false));
            }

            if (audioSource == null)
            {
                _tempAudioSourceGameObject = new GameObject("AudioSource_" + clip.name);
                SceneManager.MoveGameObjectToScene(_tempAudioSourceGameObject, gameObject.scene);
                audioSource = _tempAudioSourceGameObject.AddComponent<AudioSource>();
            }
            
            audioSource.loop = isLoop;
            audioSource.clip = clip;

            if (audioSourceSFXPoolParent.gameObject.activeSelf)
            {
                audioSource.Play();
            }

            return audioSource;
        }

        public void Stop(AudioSource audioSource)
        {
            audioSource.Stop();
            audioSource.gameObject.SetActive(false);
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public void ChangeAllVolume(MusicType musicType, float value)
        {
            switch (musicType)
            {
                case MusicType.Music:
                    _audioSourcePool.ChangeAllMusicVolume(value);
                    break;
                case MusicType.SoundFX:
                    _audioSourcePool.ChangeAllSoundFXVolume(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(musicType), musicType, null);
            }
        }

        public void AddEnemyAudioSourceToPool(AudioSource audioSource)
        {
            enemyAudioSources.Add(audioSource);
            if (isSfxOff)
            {
                audioSource.enabled = false;
            }
        }

        private void InitializeSoundSettings()
        {
            if (_audioSourcePool == null)
            {
                _audioSourcePool = new AudioSourcePool();
            }
            
            _audioSourcePool.FillAudioSourceMusicPool(audioSourcePoolSize, audioSourceMusicPoolParent);
            _audioSourcePool.FillAudioSourceSFXPool(audioSourcePoolSize, audioSourceSFXPoolParent);
        }

        public void TurnAllMusic(bool isOn)
        {
            isMusicOff = isOn;
            audioSourceMusicPoolParent.gameObject.SetActive(isOn);
        }

        public void TurnAllSFX(bool isOn)
        {
            isSfxOff = !isOn;
            
            if (!enemyAudioSources.IsNullOrEmpty())
            {
                foreach (var audioSource in enemyAudioSources)
                {
                    audioSource.enabled = isOn;
                }
            }
            audioSourceSFXPoolParent.gameObject.SetActive(isOn);
        }
    }
}