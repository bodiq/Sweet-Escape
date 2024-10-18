using System.Collections;
using System.Linq;
using Audio;
using Configs;
using Enums;
using UnityEngine;

namespace PowerUps
{
    public class ChillBlastPowerUp : MonoBehaviour, PowerUp
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private Animator animator;
        [SerializeField] private Sprite defaultSprite;
        
        private const string DestroyAnimationState = "Destroy";
        private const string DestroyAnimationClip = "ChillBlastCollect";
        
        private const Enums.PowerUps PowerUp = Enums.PowerUps.ChillBlast;
        
        private Player _player;
        private Coroutine _chillBlastCoroutine;
        private Coroutine _chillBlastAnimationCoroutine;
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

        private void OnDisable()
        {
            GameManager.Instance.OnPlayerRespawn -= ResetPowerUp;
            
            GameManager.Instance.OnStopGame -= StopSound;
            GameManager.Instance.OnResumeGame -= ResumeSound;

            StopCoroutines();
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
            _isEntered = true;
            UIManager.Instance.HUDScreen.UIPowerUpsManager.InitializePowerUp(PowerUp, this, 0, _totalPowerUpDuration);
            
            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.UsePowerUp))
            {
                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.UsePowerUp, null, 1);
            }

            GameManager.Instance.countPowerUpsTakenPerRun++;
            
            _chillBlastAnimationCoroutine = StartCoroutine(OnAnimationDone());
            _chillBlastCoroutine = StartCoroutine(PowerUpActive());
            animator.Play(DestroyAnimationState);
            boxCollider2D.enabled = false;
        }

        public void StopCoroutines()
        {
            if (_chillBlastCoroutine != null)
            {
                StopCoroutine(_chillBlastCoroutine);
                _chillBlastCoroutine = null;
            }
            
            if (_chillBlastAnimationCoroutine != null)
            {
                StopCoroutine(_chillBlastAnimationCoroutine);
                _chillBlastAnimationCoroutine = null;
            }
        }

        private IEnumerator PowerUpActive()
        {
            AudioManager.Instance.PlaySFX(Audio.AudioType.ChillBlastStart);
            UIManager.Instance.HUDScreen.TurnFreezeScreen(true);
            
            if (AudioManager.Instance.TimerPowerUpsAudioSource.TryGetValue(PowerUp, out var source))
            {
                source.Stop();
                source.loop = false;
                source.gameObject.SetActive(false);
                
                _timerAudioSource = AudioManager.Instance.PlaySFX(Audio.AudioType.ChillBlastTimer, true);
                AudioManager.Instance.TimerPowerUpsAudioSource[PowerUp] = _timerAudioSource;
            }
            else
            {
                _timerAudioSource = AudioManager.Instance.PlaySFX(Audio.AudioType.ChillBlastTimer, true);
                AudioManager.Instance.TimerPowerUpsAudioSource[PowerUp] = _timerAudioSource;
            }
            
            foreach (var enemy in GameManager.Instance.Enemies.Where(enemy => enemy.GameObject.activeInHierarchy))
            {
                enemy.Freeze();
            }
            
            yield return _powerUpActive;
            AudioManager.Instance.PlaySFX(Audio.AudioType.ChillBlastEnd);
            UIManager.Instance.HUDScreen.TurnFreezeScreen(false);
            
            _timerAudioSource.Stop();
            _timerAudioSource.loop = false;
            _timerAudioSource.gameObject.SetActive(false);
            
            foreach (var enemy in GameManager.Instance.Enemies.Where(enemy => enemy.GameObject.activeInHierarchy))
            {
                enemy.UnFreeze();
            }
            _chillBlastCoroutine = null;
            
            gameObject.SetActive(false);
        }

        private IEnumerator OnAnimationDone()
        {
            AudioManager.Instance.PlaySFX(Audio.AudioType.ChillBlastCollect);
            
            yield return _animationUpActive;
            
            _isEntered = false;
            transform.position = _firstPosition;
            animator.enabled = false;
            spriteRenderer.enabled = false;
            spriteRenderer.sprite = defaultSprite;
            _chillBlastAnimationCoroutine = null;
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