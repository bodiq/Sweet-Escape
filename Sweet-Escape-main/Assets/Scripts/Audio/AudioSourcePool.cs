using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio
{
    public class AudioSourcePool
    {
        protected List<AudioSource> _MusicPool;
        protected List<AudioSource> _SFXPool;

        public void FillAudioSourceMusicPool(int poolSize, Transform parent)
        {
            if (_MusicPool == null)
            {
                _MusicPool = new List<AudioSource>();
            }

            if (poolSize <= 0 || _MusicPool.Count >= poolSize)
            {
                return;
            }

            foreach (var source in _MusicPool)
            {
                Object.Destroy(source.gameObject);
            }

            for (var i = 0; i < poolSize; i++)
            {
                var audioSourceGameObject = new GameObject("AudioSourceMusicPool" + i);
                SceneManager.MoveGameObjectToScene(audioSourceGameObject.gameObject, parent.gameObject.scene);
                var audioSource = audioSourceGameObject.AddComponent<AudioSource>();
                audioSourceGameObject.transform.SetParent(parent);
                audioSourceGameObject.SetActive(false);
                _MusicPool.Add(audioSource);
            }
        }

        public void ChangeAllMusicVolume(float volume)
        {
            foreach (var source in _MusicPool)
            {
                source.volume = volume;
            }
        }

        public void ChangeAllSoundFXVolume(float volume)
        {
            foreach (var source in _SFXPool)
            {
                source.volume = volume;
            }
        }
        
        public void FillAudioSourceSFXPool(int poolSize, Transform parent)
        {
            if (_SFXPool == null)
            {
                _SFXPool = new List<AudioSource>();
            }

            if (poolSize <= 0 || _SFXPool.Count >= poolSize)
            {
                return;
            }

            foreach (var source in _SFXPool)
            {
                Object.Destroy(source.gameObject);
            }

            for (var i = 0; i < poolSize; i++)
            {
                var audioSourceGameObject = new GameObject("AudioSourceSFXPool" + i);
                SceneManager.MoveGameObjectToScene(audioSourceGameObject.gameObject, parent.gameObject.scene);
                var audioSource = audioSourceGameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSourceGameObject.transform.SetParent(parent);
                audioSourceGameObject.SetActive(false);
                _SFXPool.Add(audioSource);
            }
        }

        public IEnumerator AutoDisableAudioSource(float duration, AudioSource source, AudioClip clip, bool doNotAutoRecycleIfNotDonePlaying)
        {
            yield return new WaitForSeconds(duration);
            if (source.clip != clip)
            {
                yield break;
            }
            if (doNotAutoRecycleIfNotDonePlaying)
            {
                while (source.time < source.clip.length)
                {
                    yield return null;
                }
            }
            source.gameObject.SetActive(false);
        }
        
        public AudioSource GetAvailableMusicAudioSource(bool poolCanExpand, Transform parent)
        {
            foreach (var source in _MusicPool)
            {
                if (!source.gameObject.activeInHierarchy)
                {
                    source.gameObject.SetActive(true);
                    return source;
                }
            }

            if (poolCanExpand)
            {
                var audioSourceGameObject = new GameObject("AudioSourcePool" + _MusicPool.Count);
                SceneManager.MoveGameObjectToScene(audioSourceGameObject.gameObject, parent.gameObject.scene);
                var audioSource = audioSourceGameObject.AddComponent<AudioSource>();
                audioSourceGameObject.transform.SetParent(parent);
                audioSourceGameObject.SetActive(true);
                _MusicPool.Add(audioSource);
                return audioSource;
            }

            return null;
        }
        
        public AudioSource GetAvailableSFXAudioSource(bool poolCanExpand, Transform parent)
        {
            foreach (var source in _SFXPool)
            {
                if (!source.gameObject.activeInHierarchy)
                {
                    source.gameObject.SetActive(true);
                    return source;
                }
            }

            if (poolCanExpand)
            {
                var audioSourceGameObject = new GameObject("AudioSourcePool" + _SFXPool.Count);
                SceneManager.MoveGameObjectToScene(audioSourceGameObject.gameObject, parent.gameObject.scene);
                var audioSource = audioSourceGameObject.AddComponent<AudioSource>();
                audioSourceGameObject.transform.SetParent(parent);
                audioSourceGameObject.SetActive(true);
                _SFXPool.Add(audioSource);
                return audioSource;
            }

            return null;
        }
    }
    
}