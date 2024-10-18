using System.Collections;
using Audio;
using Configs;
using Enums;
using Items;
using UnityEngine;
using AudioType = Audio.AudioType;

namespace PowerUps
{
    public class GoldenSpoonPowerUp: MonoBehaviour, PowerUp
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private Animator animator;
        [SerializeField] private Sprite defaultSprite;
        
        private const string DestroyAnimationState = "Destroy";
        private const string DestroyAnimationClip = "GoldenSpoonCollect";
        
        private const Enums.PowerUps PowerUp = Enums.PowerUps.GoldSpoon;
        
        private Player _player;
        private Vector3 _firstPosition;
        
        private WaitForSeconds _powerUpActive = new(10);
        private WaitForSeconds _animationUpActive;

        private bool _isEntered;

        private float _totalPowerUpDuration;

        private AudioSource _timerAudioSource;

        private static Coroutine _goldToothCoroutine;
        private Coroutine _goldToothAnimationCoroutine;

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
        }
        
        private void Update()
        {
            if (_isEntered)
            {
                transform.position = _player.transform.position;
            }
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
        
        public void Trigger(Player player)
        {
            if (_isEntered)
            {
                return;
            }
            
            if (_goldToothCoroutine == null)
            {
                _goldToothCoroutine = GameManager.Instance.StartCoroutine(PowerUpActive());
            }
            else
            {
                DeleteCoroutine(ref _goldToothCoroutine);
                _goldToothCoroutine = GameManager.Instance.StartCoroutine(PowerUpReactive());
            }

            _player = player;
            transform.SetParent(_player.transform);
            _isEntered = true;
            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.UsePowerUp))
            {
                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.UsePowerUp, null, 1);
            }
            GameManager.Instance.countPowerUpsTakenPerRun++;
            
            UIManager.Instance.HUDScreen.UIPowerUpsManager.InitializePowerUp(PowerUp, this, 0, _totalPowerUpDuration);
            _goldToothAnimationCoroutine = StartCoroutine(OnAnimationDone());
            animator.Play(DestroyAnimationState);
            boxCollider2D.enabled = false;
        }

        public void StopCoroutines()
        {
            if (_goldToothCoroutine != null)
            {
                DeleteCoroutine(ref _goldToothCoroutine);
            }
            
            if (_goldToothAnimationCoroutine != null)
            {
                DeleteCoroutine(ref _goldToothAnimationCoroutine);
            }
        }

        private static void DeleteCoroutine(ref Coroutine coroutine)
        {
            GameManager.Instance.StopCoroutine(coroutine);
            coroutine = null;
        }

        private IEnumerator PowerUpActive()
        {
            GameManager.Instance.ResetPoolItems();

            if (AudioManager.Instance.TimerPowerUpsAudioSource.TryGetValue(PowerUp, out var source))
            {
                source.Stop();
                source.loop = false;
                source.gameObject.SetActive(false);
                
                _timerAudioSource = AudioManager.Instance.PlaySFX(AudioType.GoldenSpoonTimer, true);
                AudioManager.Instance.TimerPowerUpsAudioSource[PowerUp] = _timerAudioSource;
            }
            else
            {
                _timerAudioSource = AudioManager.Instance.PlaySFX(AudioType.GoldenSpoonTimer, true);
                AudioManager.Instance.TimerPowerUpsAudioSource[PowerUp] = _timerAudioSource;
            }
            
            GameManager.Instance.isGoldenSpoonActivate = true;
            
            yield return _powerUpActive;
            TurnOffGoldenSpoon();
        }

        private IEnumerator PowerUpReactive()
        {
            yield return _powerUpActive;
            TurnOffGoldenSpoon();
        }

        private void TurnOffGoldenSpoon()
        {
            GameManager.Instance.isGoldenSpoonActivate = false;
            AudioManager.Instance.PlaySFX(AudioType.HundAndThousEnd);

            if (_timerAudioSource)
            {
                _timerAudioSource.Stop();
                _timerAudioSource.loop = false;
                _timerAudioSource.gameObject.SetActive(false);
                _timerAudioSource = null;
            }

            for (var i = 0; i < GameManager.Instance.Coins.Count; i++)
            {
                var coin = GameManager.Instance.Coins[i];
                if (!coin.IsEntered && coin.gameObject.activeSelf)
                {
                    GameManager.Instance.Sprinkles[i].gameObject.SetActive(true);
                }
                else
                {
                    Sprinkle.PoolUsed.Add(GameManager.Instance.Sprinkles[i]);
                }

                coin.gameObject.SetActive(false);
            }

            ManualObjectPool.SharedInstance.ResetCoinIndex();

            _goldToothCoroutine = null;

            gameObject.SetActive(false);
        }

        private IEnumerator OnAnimationDone()
        {
            AudioManager.Instance.PlaySFX(AudioType.GoldenSpoonCollect);
            yield return _animationUpActive;
            _isEntered = false;
            transform.position = _firstPosition;
            animator.enabled = false;
            spriteRenderer.enabled = false;
            spriteRenderer.sprite = defaultSprite;
            _goldToothAnimationCoroutine = null;
        }
        
        private void ResetPowerUp()
        {
            gameObject.transform.position = _firstPosition;
            _isEntered = false;
            
            GameManager.Instance.isGoldenSpoonActivate = false;
            
            for (var i = 0; i < GameManager.Instance.Coins.Count; i++)
            {
                if (GameManager.Instance.Coins[i].gameObject.activeSelf)
                {
                    GameManager.Instance.Sprinkles[i].gameObject.SetActive(true);
                }
                GameManager.Instance.Coins[i].gameObject.SetActive(false);
            }

            GameManager.Instance.ResetPoolItems();

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
            
            ManualObjectPool.SharedInstance.ResetCoinIndex();
            
            if (_timerAudioSource)
            {
                _timerAudioSource.Stop();
                _timerAudioSource.loop = false;
                _timerAudioSource.gameObject.SetActive(false);
                _timerAudioSource = null;
            }
            
            Destroy(this);
        }
    }
}