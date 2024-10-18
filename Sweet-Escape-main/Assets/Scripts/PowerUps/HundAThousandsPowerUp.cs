using System.Collections;
using Audio;
using Configs;
using Enums;
using UnityEngine;
using AudioType = Audio.AudioType;

namespace PowerUps
{
    public class HundAThousandsPowerUp: MonoBehaviour, PowerUp
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private Animator animator;
        [SerializeField] private Sprite defaultSprite;
        
        private const string DestroyAnimationState = "Destroy";
        private const string DestroyAnimationClip = "HunAndThounCollect";
        
        private const Enums.PowerUps PowerUp = Enums.PowerUps.HundAThousands;
        private Player _player;
        private Coroutine _extraSprinklesCoroutine;
        private Coroutine _sprinklesCoroutine;
        private Vector3 _firstPosition;
        
        private WaitForSeconds _powerUpActive = new(10);
        private WaitForSeconds _animationUpActive;

        private bool _isEntered;

        private float _totalPowerUpDuration;
        private AudioSource _timerAudioSource;

        public GameObject GameObject => gameObject;
        Transform ITrigger.Transform => transform;

        private void Awake()
        {
            _totalPowerUpDuration = GameManager.Instance.PowerUpData[PowerUp].DurationTime;
            _powerUpActive = new WaitForSeconds(_totalPowerUpDuration);
            
            var animationClips = animator.runtimeAnimatorController.animationClips;

            foreach (var animationClip in animationClips)
            {
                if (animationClip.name == DestroyAnimationClip)
                {
                    _animationUpActive = new WaitForSeconds(animationClip.length);
                }
            }
        }

        private void OnEnable()
        {
            GameManager.Instance.OnPlayerRespawn += ResetPowerUp;
            
            GameManager.Instance.OnStopGame += StopSound;
            GameManager.Instance.OnResumeGame += ResumeSound;
            
            
            _firstPosition = transform.position;
        }
        
        private void StopSound()
        {
            if (_timerAudioSource && _timerAudioSource.isPlaying && _timerAudioSource.enabled)
            {
                _timerAudioSource.Stop();
            }
        }

        private void ResumeSound()
        {
            if (_timerAudioSource && !_timerAudioSource.isPlaying && _timerAudioSource.enabled)
            {
                _timerAudioSource.Play();
            }
        }

        private void OnDisable()
        {
            GameManager.Instance.OnPlayerRespawn -= ResetPowerUp;
                        
            GameManager.Instance.OnStopGame -= StopSound;
            GameManager.Instance.OnResumeGame -= ResumeSound;
        }

        private void Update()
        {
            if (_isEntered)
            {
                transform.position = _player.transform.position;
            }
        }

        public void Trigger(Player player)
        {
            _player = player;
            transform.SetParent(_player.transform);
            _player.multiplierSprinkle = GameManager.Instance.PowerUpData[PowerUp].Coefficient;
            _isEntered = true;
            UIManager.Instance.HUDScreen.UIPowerUpsManager.InitializePowerUp(PowerUp, this, 0, _totalPowerUpDuration);
            
            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.UsePowerUp))
            {
                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.UsePowerUp, null, 1);
            }

            GameManager.Instance.countPowerUpsTakenPerRun++;
            
            _extraSprinklesCoroutine = StartCoroutine(PowerUpActive());
            _sprinklesCoroutine = StartCoroutine(OnAnimationDone());
            animator.Play(DestroyAnimationState);
            boxCollider2D.enabled = false;
        }

        public void StopCoroutines()
        {
            if (_extraSprinklesCoroutine != null)
            {
                StopCoroutine(_extraSprinklesCoroutine);
                _extraSprinklesCoroutine = null;
            }
            
            if (_sprinklesCoroutine != null)
            {
                StopCoroutine(_sprinklesCoroutine);
                _sprinklesCoroutine = null;
            }
        }

        private IEnumerator PowerUpActive()
        {
            GameManager.Instance.OnPlayerPowerUpStart?.Invoke(Enums.PowerUps.HundAThousands);

            if (AudioManager.Instance.TimerPowerUpsAudioSource.TryGetValue(PowerUp, out var source))
            {
                source.Stop();
                source.loop = false;
                source.gameObject.SetActive(false);
                
                _timerAudioSource = AudioManager.Instance.PlaySFX(AudioType.HundAndThousTimer, true);
                AudioManager.Instance.TimerPowerUpsAudioSource[PowerUp] = _timerAudioSource;
            }
            else
            {
                _timerAudioSource = AudioManager.Instance.PlaySFX(AudioType.HundAndThousTimer, true);
                AudioManager.Instance.TimerPowerUpsAudioSource[PowerUp] = _timerAudioSource;
            }
            
            yield return _powerUpActive;
            
            AudioManager.Instance.PlaySFX(AudioType.HundAndThousEnd);

            if (_timerAudioSource)
            {
                _timerAudioSource.Stop();
                _timerAudioSource.loop = false;
                _timerAudioSource.gameObject.SetActive(false);
                _timerAudioSource = null;
            }
            
            _player.multiplierSprinkle = 1;
            _extraSprinklesCoroutine = null;
            
            gameObject.SetActive(false);
        }

        private IEnumerator OnAnimationDone()
        {
            AudioManager.Instance.PlaySFX(AudioType.HundAndThousCollect);
            yield return _animationUpActive;
            _isEntered = false;
            transform.position = _firstPosition;
            animator.enabled = false;
            spriteRenderer.enabled = false;
            spriteRenderer.sprite = defaultSprite;
            _sprinklesCoroutine = null;
        }
        
        private void ResetPowerUp()
        {
            gameObject.transform.position = _firstPosition;
            _isEntered = false;

            if (spriteRenderer)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = defaultSprite;
            }
            
            if (animator)
            {
                animator.enabled = true;
            }
            
            StopCoroutines();
            
            boxCollider2D.enabled = true;

            if (_timerAudioSource)
            {
                _timerAudioSource.Stop();
                _timerAudioSource.loop = false;
                _timerAudioSource.gameObject.SetActive(false);
                _timerAudioSource = null;
            }
            
            gameObject.SetActive(false);
        }
    }
}