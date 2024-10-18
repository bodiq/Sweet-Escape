using System.Collections;
using Audio;
using DG.Tweening;
using Enums;
using UnityEngine;

namespace Enemy
{
    public class Buzzsaw : MonoBehaviour, IEnemy
    {
        [SerializeField] private Animator animator;
        [SerializeField] private BuzzsawEnums buzzSawType;
        [SerializeField] private Transform endPoint;

        [SerializeField] private AudioSource audioSource;
        
        [Header("MovingBuzzsaw settings")]
        [SerializeField] private float buzzsawDurationMoving = 3f;
        private WaitForSeconds _waitBetweenChangeBuzzsawSide;

        [Header("HidingBuzzsaw settings")]
        [SerializeField] private float buzzsawReloadingDelay = 3f;
        [SerializeField] private float buzzsawWorkingTime = 3f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite defaultSpriteMovingBuzzsaw;
        [SerializeField] private BoxCollider2D collider;
        
        
        private WaitForSeconds _waitBuzzsawReloading;
        private WaitForSeconds _waitBuzzsawOpening;
        private WaitForSeconds _waitBuzzsawWorking;
        private WaitForSeconds _waitBuzzsawHiding;
        
        private Vector3 _initialPosition;
        private string _currentState;
        
        private float _openingDuration;
        private float _workingDuration;
        private float _hidingDuration;

        private const string OpeningAnimationState = "BuzzsawHiding_Opening";
        private const string WorkingAnimationState = "BuzzsawHiding_Working";
        private const string HidingAnimationState = "BuzzsawHiding_Hiding";
        private const string IdleAnimationState = "BuzzsawHiding_Idle";

        private Coroutine _buzzSawMovingCoroutine;
        private Coroutine _buzzSawHidingCoroutine;

        private void Awake()
        {
            var animationClips = animator.runtimeAnimatorController.animationClips;

            foreach (var clip in animationClips)
            {
                switch (clip.name)
                {
                    case OpeningAnimationState:
                        _openingDuration = clip.length;
                        break;
                    case WorkingAnimationState:
                        _workingDuration = clip.length;
                        break;
                    case HidingAnimationState:
                        _hidingDuration = clip.length;
                        break;
                }
            }
            
            switch (buzzSawType)
            {
                case BuzzsawEnums.Moving:
                {
                    _initialPosition = transform.position;
                    _waitBetweenChangeBuzzsawSide = new WaitForSeconds(buzzsawDurationMoving);

                    if (audioSource && audioSource.enabled)
                    {
                        audioSource.Play();
                    }

                    _buzzSawMovingCoroutine ??= StartCoroutine(StartMoveBuzzsawMoving());
                    break;
                }
                case BuzzsawEnums.Hiding:
                    _waitBuzzsawReloading = new WaitForSeconds(buzzsawReloadingDelay);
                    _waitBuzzsawOpening = new WaitForSeconds(_openingDuration);
                    _waitBuzzsawWorking = new WaitForSeconds(buzzsawWorkingTime);
                    _waitBuzzsawHiding = new WaitForSeconds(_hidingDuration);
                    _buzzSawHidingCoroutine ??= StartCoroutine(StartMoveBuzzsawHiding());
                    break;
                case BuzzsawEnums.Static:
                {
                    if (audioSource && audioSource.enabled)
                    {
                        audioSource.Play();
                    }

                    break;
                }
            }
            
            GameManager.Instance.Enemies.Add(this);
        }

        private void OnEnable()
        {
            _initialPosition = transform.position;
            
            switch (buzzSawType)
            {
                case BuzzsawEnums.Moving:
                    _buzzSawMovingCoroutine ??= StartCoroutine(StartMoveBuzzsawMoving());
                    break;
                case BuzzsawEnums.Hiding:
                    _buzzSawHidingCoroutine ??= StartCoroutine(StartMoveBuzzsawHiding());
                    break;
                case BuzzsawEnums.Static:
                {
                    if (audioSource && audioSource.enabled)
                    {
                        audioSource.Play();
                    }

                    break;
                }
            }
            
            GameManager.Instance.OnStopGame += TurnOffSound;
            GameManager.Instance.OnResumeGame += TurnOnSound;

            if (!GameManager.Instance.IsTestEnvironment)
            {
                AudioManager.Instance.AddEnemyAudioSourceToPool(audioSource);
                SetSoundData();
            }
        }
        
        private void SetSoundData()
        {
            var volume = PlayerPrefs.GetFloat(GameManager.UserSoundFXVolumeKey);
            audioSource.volume = volume;
        }

        private void OnDisable()
        {
            if (_buzzSawHidingCoroutine != null)
            {
                StopCoroutine(_buzzSawHidingCoroutine);
                _buzzSawHidingCoroutine = null;
            }

            if (_buzzSawMovingCoroutine != null)
            {
                StopCoroutine(_buzzSawMovingCoroutine);
                _buzzSawMovingCoroutine = null;
            }

            _movingBuzzsawTween.Kill();
            transform.position = _initialPosition;
            
            GameManager.Instance.OnStopGame -= TurnOffSound;
            GameManager.Instance.OnResumeGame -= TurnOnSound;

            AudioManager.Instance.enemyAudioSources.Remove(audioSource);
        }

        private void TurnOffSound()
        {
            if (audioSource && audioSource.isPlaying && audioSource.enabled)
            {
                audioSource.Stop();
            }
        }

        private void TurnOnSound()
        {
            if (audioSource && buzzSawType is BuzzsawEnums.Static && audioSource.enabled)
            {
                audioSource.Play();
            }
        }

        private void ChangeAnimationState(string newState)
        {
            if (_currentState == newState)
            {
                return;
            }
        
            animator.Play(newState);

            _currentState = newState;
        }
        
        private IEnumerator StartMoveBuzzsawHiding()
        {
            while (true)
            {
                yield return _waitBuzzsawReloading;
                ChangeAnimationState(OpeningAnimationState);
                yield return _waitBuzzsawOpening;
                if (audioSource && audioSource.enabled)
                {
                    audioSource.Play();
                }
                ChangeAnimationState(WorkingAnimationState);
                yield return _waitBuzzsawWorking;
                if (audioSource && audioSource.enabled)
                {
                    audioSource.Stop();
                }
                ChangeAnimationState(HidingAnimationState);
                yield return _waitBuzzsawHiding;
                ChangeAnimationState(IdleAnimationState);
            }
        }
        
        private IEnumerator StartMoveBuzzsawMoving()
        {
            while (true)
            {
                MoveBuzzsaw(endPoint.position, buzzsawDurationMoving);
                yield return _waitBetweenChangeBuzzsawSide;
                MoveBuzzsaw(_initialPosition, buzzsawDurationMoving);
                yield return _waitBetweenChangeBuzzsawSide;
            }
        }
    

        public void OnEnter(Player player)
        {
            if (!player.HasImmunity)
            {
                player.OnPlayerDamage();
            }
        }

        public void OnExit()
        {
            Debug.Log("Exit");
        }

        public GameObject GameObject => gameObject;

        public void ChangeMovement()
        {
            
        }

        public void Freeze()
        {
            if (buzzSawType is BuzzsawEnums.Hiding)
            {
                ChangeAnimationState(IdleAnimationState);
                spriteRenderer.sprite = defaultSpriteMovingBuzzsaw;
                collider.gameObject.SetActive(false);
            }
            
            animator.enabled = false;
            
            if (_buzzSawHidingCoroutine != null)
            {
                StopCoroutine(_buzzSawHidingCoroutine);
                _buzzSawHidingCoroutine = null;
            }

            if (_buzzSawMovingCoroutine != null)
            {
                StopCoroutine(_buzzSawMovingCoroutine);
                _buzzSawMovingCoroutine = null;
            }

            _movingBuzzsawTween.Kill();
            
            TurnOffSound();
        }

        public void UnFreeze()
        {
            animator.enabled = true;

            switch (buzzSawType)
            {
                case BuzzsawEnums.Moving:
                    _buzzSawMovingCoroutine ??= StartCoroutine(StartMoveBuzzsawMoving());
                    break;
                case BuzzsawEnums.Hiding:
                    _buzzSawHidingCoroutine ??= StartCoroutine(StartMoveBuzzsawHiding());
                    break;
            }
        }

        public DirectionEnum GetEnemyDirection()
        {
            return DirectionEnum.None;
        }

        public void TurnCollider(bool isActive)
        {
            collider.isTrigger = !isActive;
        }

        public EnemyType GetEnemyType()
        {
            return EnemyType.BuzzSaw;
        }

        #region Tween

        private Tweener _movingBuzzsawTween;

        private Tweener MoveBuzzsaw(Vector3 endValue, float duration, Ease ease = Ease.Linear)
        {
            if (_movingBuzzsawTween.IsActive())
            {
                _movingBuzzsawTween.ChangeEndValue(endValue, duration, true)
                    .SetEase(ease)
                    .Restart();
            }
            else
            {
                _movingBuzzsawTween = transform.DOMove(endValue, duration)
                    .SetEase(ease)
                    .SetLink(gameObject)
                    .SetAutoKill(false);
            }

            return _movingBuzzsawTween;
        }

        #endregion
    }
}
