using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Configs;
using DG.Tweening;
using Enums;
using Extensions;
using UnityEngine;
using AudioType = Audio.AudioType;
using Random = UnityEngine.Random;

namespace Items
{
    public class Sprinkle : MonoBehaviour, ITrigger
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform bodyTransform;

        public static readonly List<Sprinkle> PoolUsed = new();

        private bool _isEntered;
        private SprinkleData _data;

        private Vector3 _startPosition;
        private Vector3 _initialPosition;

        private Transform targetTransform;
        private Vector3 _startScale;
        private Quaternion _startRotation;
        
        private Vector3 _endRotation = new(0f, 0f, 0f);
        private readonly Vector3 _endScale = new(1.7f, 1.7f, 1.7f);
        
        private Transform endPoint;

        private AudioType _audioType;

        private bool _isActivatedGoldenSpoonTick;
        
        private Tweener _rotationTween;
        private Tweener _scaleTween;
        
        private Coroutine _endRotationCoroutine;

        private float _durationSpin = 0.3f;
        private float _durationPowerUp = 3f;

        public Transform Transform => transform;

        [Serializable]
        public struct SprinkleData
        {
            public List<Sprite> sprites;
        }

        private void Start()
        {
            var sprinkleType = (Sprinkles)typeof(Sprinkles).GetRandomEnumValue();
            if (!SprinkleConfig.Instance.sprinkleData.TryGetValue(sprinkleType, out _data)) return;
            var randomFrame = Random.Range(0, _data.sprites.Count);
            spriteRenderer.sprite = _data.sprites[randomFrame];

            _audioType = AudioManager.Instance.GetRandomSprinkleAudioType();
        }

        private void OnEnable()
        {
            _isEntered = false;
            _initialPosition = transform.position;
            _startScale = bodyTransform.localScale;
            _startRotation = bodyTransform.rotation;
            _isActivatedGoldenSpoonTick = false;
            _endRotation = new Vector3(0f, 0f, Random.Range(-120f, -90f));
            _durationSpin = Random.Range(0.1f, 0.35f);

            if(GameManager.Instance.PowerUpData.TryGetValue(Enums.PowerUps.HundAThousands, out var value))
            {
                _durationPowerUp = value.DurationTime;
            }

            GameManager.Instance.OnPlayerPowerUpStart += OnHundAndThousandsStarted;
        }

        private void Update()
        {
            if (GameManager.Instance.isGoldenSpoonActivate && spriteRenderer.isVisible && !_isActivatedGoldenSpoonTick)
            {
                var coin = ManualObjectPool.SharedInstance.GetPooledCoinObject();
                coin.transform.position = transform.position;
                coin.gameObject.SetActive(true);
                GameManager.Instance.Sprinkles.Add(this);
                GameManager.Instance.Coins.Add(coin);
                gameObject.SetActive(false);
                _isActivatedGoldenSpoonTick = true;
            }
        }

        private void OnHundAndThousandsStarted(Enums.PowerUps powerUp)
        {
            if (powerUp != Enums.PowerUps.HundAThousands)
            {
                return;
            }
            
            if (_endRotationCoroutine == null)
            {
                _endRotationCoroutine = StartCoroutine(StartUIShow());
            }
            else
            {
                ResetUIShow();
                _endRotationCoroutine = StartCoroutine(StartUIShow());
            }
        }

        private IEnumerator StartUIShow()
        {
            _rotationTween = bodyTransform.DORotate(_endRotation, _durationSpin).SetLoops(-1, LoopType.Incremental);
            _scaleTween = bodyTransform.DOScale(_endScale, 1f);

            yield return new WaitForSeconds(_durationPowerUp);

            ResetUIShow();
        }


        private void ResetUIShow()
        {
            if (_endRotationCoroutine != null)
            {
                StopCoroutine(_endRotationCoroutine);
                _endRotationCoroutine = null;
            }
            
            _rotationTween?.Kill();
            _scaleTween?.Kill();
            
            bodyTransform.localScale = _startScale;
            bodyTransform.rotation = _startRotation;
        }

        private void OnDestroy()
        {
            PoolUsed.Remove(this);
        }

        private void OnDisable()
        {
            GameManager.Instance.OnPlayerPowerUpStart -= OnHundAndThousandsStarted;

            ResetUIShow();
        }

        private void OnBecameVisible()
        {
            if (GameManager.Instance.isGoldenSpoonActivate)
            {
                var coin = ManualObjectPool.SharedInstance.GetPooledCoinObject();
                if (coin == null)
                {
                    return;
                }

                coin.transform.position = transform.position;
                coin.gameObject.SetActive(true);
                GameManager.Instance.Sprinkles.Add(this);
                GameManager.Instance.Coins.Add(coin);
                gameObject.SetActive(false);
            }
        }

        public void Trigger(Player player)
        {
            if (_isEntered)
            {
                return;
            }

            _isEntered = true;
            gameObject.SetActive(false);
            PoolUsed.Add(this);
            player.OnPlayerGetPoint(Mathf.RoundToInt(1 * player.multiplierSprinkle));
        }

        public void OnEnter(Player player)
        {
            if (_isEntered)
            {
                return;
            }

            _isEntered = true;
            _startPosition = transform.position;
            targetTransform = player.transform;

            var duration = Random.Range(CharacterConfig.Instance.MinimumDurationItemMoveToPlayer,
                CharacterConfig.Instance.MaximumDurationItemMoveToPlayer);

            DOTween.To(Setter, 0, 1, duration).SetEase(Ease.InQuint).OnComplete(() =>
            {
                AudioManager.Instance.PlaySFX(_audioType);
                gameObject.SetActive(false);
                transform.position = _initialPosition;
                PoolUsed.Add(this);

                if (player.magnetSprinkles)
                {
                    player.OnPlayerGetPoint(1 *  player.multiplierSprinkle);
                }
            });
        }

        private void Setter(float t)
        {
            var endPosition = targetTransform.position - new Vector3(0.5f, 0.5f, 0f);
            transform.position = Vector3.LerpUnclamped(_startPosition, endPosition, t);
        }

        public void Activate()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                PoolUsed.Remove(this);
            }
        }
    }
}