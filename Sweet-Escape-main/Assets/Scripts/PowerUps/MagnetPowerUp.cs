using System.Collections;
using Audio;
using Cinemachine;
using Configs;
using DG.Tweening;
using Enums;
using MoreMountains.Feedbacks;
using UnityEngine;
using AudioType = Audio.AudioType;

namespace PowerUps
{
    public class MagnetPowerUp: MonoBehaviour, PowerUp
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private Animator animator;
        [SerializeField] private Sprite defaultSprite;

        [SerializeField] private float minDurationItemMoveToTarget;
        [SerializeField] private float maxDurationItemMoveToTarget;
        
        private const string DestroyAnimationState = "Destroy";
        private const string MagnetWindAnimationState = "MagnetWind";
        
        private const string DestroyAnimationClip = "MagnetCollect";
        private const string MagnetWindAnimationClip = "MagnetWind";

        private Player _player;
        
        private Vector3 _firstPosition;
        
        private Coroutine _magnetActivation;
        private Coroutine _magnetAnimationActivation;
        
        private WaitForSeconds _powerUpActive;
        private WaitForSeconds _animationActive;

        private float _defaultMinDurationItemMove;
        private float _defaultMaxDurationItemMove;
        
        private bool _isEntered;

        private CinemachineVirtualCamera _camera;
        private MMF_Player _magnetFeedback;

        private float _initialOrthoSize;
        private float _startOrthoSize;
        private float _endOrthoSize;
        
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

                if (animationClip.name == MagnetWindAnimationClip)
                {
                    _powerUpActive = new WaitForSeconds(animationClip.length);
                    maxDurationItemMoveToTarget = animationClip.length;
                }
            }

            _defaultMinDurationItemMove = CharacterConfig.Instance.MinimumDurationItemMoveToPlayer;
            _defaultMaxDurationItemMove = CharacterConfig.Instance.MaximumDurationItemMoveToPlayer;
        }

        private void Start()
        {
            _camera = TilemapManager.Instance.Camera;
            _magnetFeedback = TilemapManager.Instance.MagnetCameraImpulse;
            
            _initialOrthoSize = _camera.m_Lens.OrthographicSize;
            _startOrthoSize = _initialOrthoSize;
            _endOrthoSize = _initialOrthoSize - 2;
        }

        private void OnEnable()
        {
            GameManager.Instance.OnPlayerRespawn += ResetPowerUp;
            _firstPosition = transform.position;
        }

        private void OnDisable()
        {
            GameManager.Instance.OnPlayerRespawn -= ResetPowerUp;
            if (_camera)
            {
                _camera.m_Lens.OrthographicSize = _initialOrthoSize;
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
            _isEntered = true;
            
            CharacterConfig.Instance.MinimumDurationItemMoveToPlayer = minDurationItemMoveToTarget;
            CharacterConfig.Instance.MaximumDurationItemMoveToPlayer = maxDurationItemMoveToTarget;
            
            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.UsePowerUp))
            {
                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.UsePowerUp, null, 1);
            }

            GameManager.Instance.countPowerUpsTakenPerRun++;
            
            animator.Play(DestroyAnimationState);
            _magnetAnimationActivation = StartCoroutine(PowerUpAnimationActive());
            boxCollider2D.enabled = false;
        }

        public void StopCoroutines()
        {
            if (_magnetActivation != null)
            {
                StopCoroutine(_magnetActivation);
                _magnetActivation = null;
            }
            
            if (_magnetAnimationActivation != null)
            {
                StopCoroutine(_magnetAnimationActivation);
                _magnetAnimationActivation = null;
            }
        }

        private IEnumerator PowerUpAnimationActive()
        {
            AudioManager.Instance.PlaySFX(AudioType.MagnetCollect);
            yield return _animationActive;

            ZoomInCamera(_initialOrthoSize, _endOrthoSize, 0.2f);
            
            _magnetFeedback.PlayFeedbacks();
            _player.PlayerMagnet.MagnetTurn(true);
            animator.Play(MagnetWindAnimationState);
            _magnetActivation = StartCoroutine(PowerUpActive());
            _magnetAnimationActivation = null;
        }
        
        private IEnumerator PowerUpActive()
        {
            _player.magnetSprinkles = true;
            
            yield return _powerUpActive;
            
            ZoomInCamera(_endOrthoSize, _initialOrthoSize, 0.2f);
            
            _magnetFeedback.StopFeedbacks();
            _isEntered = false;
            CharacterConfig.Instance.MinimumDurationItemMoveToPlayer = _defaultMinDurationItemMove;
            CharacterConfig.Instance.MaximumDurationItemMoveToPlayer = _defaultMaxDurationItemMove;
            
            _player.PlayerMagnet.MagnetTurn(false);
            
            animator.enabled = false;
            spriteRenderer.enabled = false;
            spriteRenderer.sprite = defaultSprite;
            
            _magnetActivation = null;

            _player.magnetSprinkles = false;
            gameObject.SetActive(false);
        }
        
        private void ResetPowerUp()
        {
            gameObject.transform.position = _firstPosition;
            _isEntered = false;
            
            CharacterConfig.Instance.MinimumDurationItemMoveToPlayer = _defaultMinDurationItemMove;
            CharacterConfig.Instance.MaximumDurationItemMoveToPlayer = _defaultMaxDurationItemMove;
            
            if (spriteRenderer)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = defaultSprite;
            }

            if (animator)
            {
                animator.enabled = true;
            }
            
            if (_magnetActivation != null)
            {
                StopCoroutine(_magnetActivation);
                _magnetActivation = null;
            }

            if (_magnetAnimationActivation != null)
            {
                StopCoroutine(_magnetAnimationActivation);
                _magnetAnimationActivation = null;
            }
            
            boxCollider2D.enabled = true;
        }

        private Tween Tween;
        
        private void ZoomInCamera(float start, float end, float duration)
        {
            _startOrthoSize = start;
            _endOrthoSize = end;

            Tween = DOTween.To(Setter, 0, 1, duration).SetEase(Ease.Linear);
        }

        private void Setter(float t)
        {
            _camera.m_Lens.OrthographicSize = Mathf.Lerp(_startOrthoSize, _endOrthoSize, t);
        }
    }
}