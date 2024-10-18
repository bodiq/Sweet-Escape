using System.Collections;
using Audio;
using Enums;
using UnityEngine;
using AudioType = Audio.AudioType;

namespace PowerUps
{
    public class BubblePowerUp : MonoBehaviour, PowerUp
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private Animator animator;
        [SerializeField] private Sprite defaultSprite;
        
        private const string DestroyAnimationState = "Destroy";
        private const string DestroyAnimationClip = "BubbleCollect";

        private Player _player;
        private Coroutine _shieldActivation;
        private Vector3 _firstPosition;
        
        private WaitForSeconds _animationActive;
        
        private bool _isEntered;

        public GameObject GameObject => gameObject;
        Transform ITrigger.Transform => transform;

        private void Awake()
        {
            var animationClips = animator.runtimeAnimatorController.animationClips;

            foreach (var animationClip in animationClips)
            {
                if (animationClip.name == DestroyAnimationClip)
                {
                    _animationActive = new WaitForSeconds(animationClip.length);
                }
            }
        }

        private void OnEnable()
        {
            GameManager.Instance.OnPlayerRespawn += ResetPowerUp;
            _firstPosition = transform.position;
        }

        private void OnDisable()
        {
            GameManager.Instance.OnPlayerRespawn -= ResetPowerUp;
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
            _isEntered = true;
            animator.Play(DestroyAnimationState);
            _player.SetShields(1, false, true);
            
            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.UsePowerUp))
            {
                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.UsePowerUp, null, 1);
            }

            GameManager.Instance.countPowerUpsTakenPerRun++;
            
            AudioManager.Instance.PlaySFX(AudioType.CollectBubble);
            _shieldActivation = StartCoroutine(PowerUpActive());
            boxCollider2D.enabled = false;
        }

        public void StopCoroutines()
        {
            if (_shieldActivation != null)
            {
                StopCoroutine(_shieldActivation);
                _shieldActivation = null;
            }
        }

        private IEnumerator PowerUpActive()
        {
            yield return _animationActive;
            _isEntered = false;
            transform.position = _firstPosition;
            animator.enabled = false;
            spriteRenderer.enabled = false;
            spriteRenderer.sprite = defaultSprite;
            _shieldActivation = null;
            gameObject.SetActive(false);
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

            if (_shieldActivation != null)
            {
                StopCoroutine(_shieldActivation);
                _shieldActivation = null;
            }
            
            boxCollider2D.enabled = true;
        }
    }
}