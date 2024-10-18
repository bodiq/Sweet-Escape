using System;
using System.Collections;
using Audio;
using DG.Tweening;
using Enemy;
using Enums;
using UnityEngine;

public class SlamBlock : MonoBehaviour, IEnemy
{
    [SerializeField] private OrientationEnum orientation;

    [SerializeField] private float durationSlamBlockMovingIn = 0.2f;
    [SerializeField] private float durationSlamBlockMovingOut = 2f;

    [SerializeField] private Ease easeSlamBlockMovingIn = Ease.Flash;
    [SerializeField] private Ease easeSlamBlockMovingOut = Ease.OutCubic;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private BoxCollider2D collider;

    [Header("DevSettings")]
    [SerializeField] private SlamBlocksEnum slamBlockEnum;

    private float _defaultDurationSlamBlockMoveIn;
    private float _defaultDurationSlamBlockMoveOut;
    
    private bool _isActivated;

    private const float SlamBlockOffset = 1f;
    private const float SlamBlockAllowedDistanceToCaught = 1.1f;

    private Vector3 _initialPosition;
    private Vector3 _endPosition;

    private Vector3 _originalPos;

    private Coroutine _slamBlockCoroutine;
    private Coroutine _spikeSlamBlockCoroutine;

    private Player _player;

    private WaitForSeconds _timeSlamBlockMoveIn;
    private WaitForSeconds _timeSlamBlockMoveOut;

    private DirectionEnum _firstDirection = DirectionEnum.None;
    private DirectionEnum _secondDirection = DirectionEnum.None;

    private DirectionEnum _enemyDirection;

    private bool _isVisible;

    private void Awake()
    {
        GameManager.Instance.Enemies.Add(this);
        _timeSlamBlockMoveIn = new WaitForSeconds(durationSlamBlockMovingIn);
        _timeSlamBlockMoveOut = new WaitForSeconds(durationSlamBlockMovingOut);

        _defaultDurationSlamBlockMoveIn = durationSlamBlockMovingIn;
        _defaultDurationSlamBlockMoveOut = durationSlamBlockMovingOut;
    }

    private void OnEnable()
    {
        CalculateMovePositions();

        if (slamBlockEnum is SlamBlocksEnum.HorizontalBlock or SlamBlocksEnum.VerticalBlock)
        {
            _slamBlockCoroutine ??= StartCoroutine(StartMovingSlamBlock());
        }
        else
        {
            _isActivated = true;
            _spikeSlamBlockCoroutine ??= StartCoroutine(StartMovingSpikeSlamBlock());
        }

        if (audioSource && !GameManager.Instance.IsTestEnvironment)
        {
            AudioManager.Instance.AddEnemyAudioSourceToPool(audioSource);
            SetSoundData();
        }
    }

    private void CalculateMovePositions()
    {
        switch (orientation)
        {
            case OrientationEnum.Horizontal:
                var rightHitPoint = CalculateHitPoint(transform.position + Vector3.up * 0.5f, transform.position + Vector3.down * 0.5f, Vector3.right);
                var leftHitPoint = CalculateHitPoint(transform.position + Vector3.up * 0.5f, transform.position + Vector3.down * 0.5f, Vector3.left);
                _initialPosition = new Vector3(leftHitPoint.x + SlamBlockOffset, leftHitPoint.y, 0);
                _endPosition = new Vector3(rightHitPoint.x - SlamBlockOffset, rightHitPoint.y, 0);
                _firstDirection = DirectionEnum.Right;
                _secondDirection = DirectionEnum.Left;
                break;
            case OrientationEnum.Vertical:
                var upHitPoint = CalculateHitPoint(transform.position + Vector3.right * 0.5f, transform.position + Vector3.left * 0.5f, Vector3.up);
                var downHitPoint = CalculateHitPoint(transform.position + Vector3.right * 0.5f, transform.position + Vector3.left * 0.5f, Vector3.down);
                _initialPosition = new Vector3(upHitPoint.x, upHitPoint.y - SlamBlockOffset, 0);
                _endPosition = new Vector3(downHitPoint.x, downHitPoint.y + SlamBlockOffset, 0);
                _firstDirection = DirectionEnum.Down;
                _secondDirection = DirectionEnum.Up;
                break;
        }
    }

    private Vector2 CalculateHitPoint(Vector3 firstPosition, Vector3 secondPosition, Vector3 direction)
    {
        var firstHit = Physics2D.Raycast(firstPosition, direction, Mathf.Infinity, 1 << LayerMask.NameToLayer("Default"));
        var secondHit = Physics2D.Raycast(secondPosition, direction, Mathf.Infinity, 1 << LayerMask.NameToLayer("Default"));
        RaycastHit2D hit = default;
        if (firstHit.collider != null && (firstHit.distance < secondHit.distance || secondHit.collider == null))
        {
            hit = firstHit;
        }
        else if (secondHit.collider != null)
        {
            hit = secondHit;
        }

        if (hit == default)
        {
            throw new NotImplementedException("SlamBlock doesn't have borders to bounce.");
        }

        var hitPoint = Math.Abs(firstPosition.x - secondPosition.x) < 0.01f
            ? new Vector2(hit.point.x, (firstPosition.y + secondPosition.y) / 2)
            : new Vector2((firstPosition.x + secondPosition.x) / 2, hit.point.y);

        return hitPoint;
    }

    private void OnBecameVisible()
    {
        _isVisible = true;
    }

    private void OnBecameInvisible()
    {
        _isVisible = false;
    }

    private void SetSoundData()
    {
        var volume = PlayerPrefs.GetFloat(GameManager.UserSoundFXVolumeKey);
        audioSource.volume = volume;
    }

    private void OnDisable()
    {
        transform.position = _initialPosition;
        _movingSlamBlockTween.Kill();

        _isActivated = false;
        if (_spikeSlamBlockCoroutine != null)
        {
            StopCoroutine(_spikeSlamBlockCoroutine);
            _spikeSlamBlockCoroutine = null;
        }

        if (_slamBlockCoroutine != null)
        {
            StopCoroutine(_slamBlockCoroutine);
            _slamBlockCoroutine = null;
        }

        if (audioSource)
        {
            AudioManager.Instance.enemyAudioSources.Remove(audioSource);
        }
    }

    private IEnumerator StartMovingSlamBlock()
    {
        while (true)
        {
            _isActivated = false;
            _movingSlamBlockTween = MoveSlamBlock(_endPosition, _defaultDurationSlamBlockMoveIn, easeSlamBlockMovingIn);
            _enemyDirection = _firstDirection;
            yield return _timeSlamBlockMoveIn;
            if (audioSource && audioSource.enabled)
            {
                audioSource.Play();
            }

            if (_isVisible)
            {
                if (TilemapManager.Instance.MoveCameraImpulse.CanPlay)
                {
                    TilemapManager.Instance.SlamBlockImpulse.PlayFeedbacks();
                }
            }

            _isActivated = false;
            _movingSlamBlockTween = MoveSlamBlock(_initialPosition, _defaultDurationSlamBlockMoveOut, easeSlamBlockMovingOut);
            _enemyDirection = _secondDirection;
            yield return _timeSlamBlockMoveOut;
            if (audioSource && audioSource.enabled)
            {
                audioSource.Play();
            }

            if (_isVisible)
            {
                if (TilemapManager.Instance.MoveCameraImpulse.CanPlay)
                {
                    TilemapManager.Instance.SlamBlockImpulse.PlayFeedbacks();
                }
            }
        }
    }

    private IEnumerator StartMovingSpikeSlamBlock()
    {
        while (true)
        {
            _movingSlamBlockTween = MoveSlamBlock(_endPosition, _defaultDurationSlamBlockMoveIn, easeSlamBlockMovingIn);
            yield return _timeSlamBlockMoveIn;
            _movingSlamBlockTween = MoveSlamBlock(_initialPosition, _defaultDurationSlamBlockMoveOut, easeSlamBlockMovingOut);
            yield return _timeSlamBlockMoveOut;
        }
    }

    public void OnEnter(Player player)
    {
        if (player == null || player == _player)
        {
            return;
        }

        _player = player;

        if (slamBlockEnum is not SlamBlocksEnum.SpikeBlock)
        {
            if (_player.PlayerMovement.IsPlayerStanding)
            {
                Vector3 slamPosition;
                switch (slamBlockEnum)
                {
                    case SlamBlocksEnum.HorizontalBlock:
                        slamPosition = transform.position;
                        var roundedPosYSide = _player.transform.position.y > slamPosition.y ? 0.5f : -0.5f;

                        if (player.transform.position.x > transform.position.x)
                        {
                            _player.PlayerMovement.PlayerStandOnBlockSide = DirectionEnum.Right;
                            _player.PlayerMovement.IsPlayerOnBlock = true;
                            _player.PlayerMovement.StopMovement();
                            _player.SetParent(transform);
                            _player.PlayerMovement.ChangeLandingAnimationState(DirectionEnum.Left);
                            _player.PlayerMovement.RotatePlayer(DirectionEnum.Left);
                            _player.PlayerMovement.ChangeLastDirection(DirectionEnum.Left);

                            _player.direction = DirectionEnum.Left;
                            _player.transform.localPosition = new Vector3(1.45f, roundedPosYSide);
                        }
                        else if (player.transform.position.x < transform.position.x)
                        {
                            _player.PlayerMovement.PlayerStandOnBlockSide = DirectionEnum.Left;
                            _player.PlayerMovement.IsPlayerOnBlock = true;
                            _player.PlayerMovement.StopMovement();
                            _player.SetParent(transform);
                            _player.PlayerMovement.ChangeLandingAnimationState(DirectionEnum.Right);
                            _player.PlayerMovement.RotatePlayer(DirectionEnum.Right);
                            _player.PlayerMovement.ChangeLastDirection(DirectionEnum.Right);

                            _player.direction = DirectionEnum.Right;
                            _player.transform.localPosition = new Vector3(-1.45f, roundedPosYSide);
                        }

                        break;
                    case SlamBlocksEnum.VerticalBlock:
                        slamPosition = transform.position;
                        var roundedPosXSide = _player.transform.position.x > slamPosition.x ? 0.5f : -0.5f;

                        if (player.transform.position.y > transform.position.y)
                        {
                            _player.PlayerMovement.PlayerStandOnBlockSide = DirectionEnum.Up;
                            _player.PlayerMovement.IsPlayerOnBlock = true;
                            _player.PlayerMovement.StopMovement();
                            _player.SetParent(transform);
                            _player.PlayerMovement.ChangeLandingAnimationState(DirectionEnum.Down);
                            _player.PlayerMovement.RotatePlayer(DirectionEnum.Down);
                            _player.PlayerMovement.ChangeLastDirection(DirectionEnum.Down);

                            _player.direction = DirectionEnum.Down;
                            _player.transform.localPosition = new Vector3(roundedPosXSide, 1.45f);
                        }
                        else if (player.transform.position.y < transform.position.y)
                        {
                            _player.PlayerMovement.PlayerStandOnBlockSide = DirectionEnum.Down;
                            _player.PlayerMovement.IsPlayerOnBlock = true;
                            _player.PlayerMovement.StopMovement();
                            _player.SetParent(transform);
                            _player.PlayerMovement.ChangeLandingAnimationState(DirectionEnum.Up);
                            _player.PlayerMovement.RotatePlayer(DirectionEnum.Up);
                            _player.PlayerMovement.ChangeLastDirection(DirectionEnum.Up);

                            _player.direction = DirectionEnum.Up;
                            _player.transform.localPosition = new Vector3(roundedPosXSide, -1.45f);
                        }

                        break;
                    case SlamBlocksEnum.SpikeBlock:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return;
            }

            switch (player.direction)
            {
                case DirectionEnum.Left or DirectionEnum.Right:
                {
                    if (Mathf.Abs(_player.transform.position.y - transform.position.y) <
                        SlamBlockAllowedDistanceToCaught)
                    {
                        var position = transform.position;
                        var roundedPosY = _player.transform.position.y > position.y ? 0.5f : -0.5f;

                        if (player.transform.position.x > transform.position.x)
                        {
                            _player.PlayerMovement.PlayerStandOnBlockSide = DirectionEnum.Right;
                            _player.PlayerMovement.IsPlayerOnBlock = true;
                            _player.PlayerMovement.StopMovement();
                            _player.SetParent(transform);
                            _player.transform.localPosition = new Vector3(1.45f, roundedPosY);

                            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.LandOnSlamBlock))
                            {
                                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.LandOnSlamBlock, null, 1);
                            }
                        }
                        else if (player.transform.position.x < transform.position.x)
                        {
                            _player.PlayerMovement.PlayerStandOnBlockSide = DirectionEnum.Left;
                            _player.PlayerMovement.IsPlayerOnBlock = true;
                            _player.PlayerMovement.StopMovement();
                            _player.SetParent(transform);
                            _player.transform.localPosition = new Vector3(-1.45f, roundedPosY);
                            
                            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.LandOnSlamBlock))
                            {
                                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.LandOnSlamBlock, null, 1);
                            }
                        }
                    }

                    break;
                }
                case DirectionEnum.Down or DirectionEnum.Up:
                {
                    if (Mathf.Abs(_player.transform.position.x - transform.position.x) <
                        SlamBlockAllowedDistanceToCaught)
                    {
                        var position = transform.position;
                        var roundedPosX = _player.transform.position.x > position.x ? 0.5f : -0.5f;

                        if (player.transform.position.y > transform.position.y)
                        {
                            _player.PlayerMovement.PlayerStandOnBlockSide = DirectionEnum.Up;
                            _player.PlayerMovement.IsPlayerOnBlock = true;
                            _player.PlayerMovement.StopMovement();
                            _player.SetParent(transform);
                            _player.transform.localPosition = new Vector3(roundedPosX, 1.45f);
                            
                            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.LandOnSlamBlock))
                            {
                                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.LandOnSlamBlock, null, 1);
                            }
                        }
                        else if (player.transform.position.y < transform.position.y)
                        {
                            _player.PlayerMovement.PlayerStandOnBlockSide = DirectionEnum.Down;
                            _player.PlayerMovement.IsPlayerOnBlock = true;
                            _player.PlayerMovement.StopMovement();
                            _player.SetParent(transform);
                            _player.transform.localPosition = new Vector3(roundedPosX, -1.45f);
                            
                            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.LandOnSlamBlock))
                            {
                                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.LandOnSlamBlock, null, 1);
                            }
                        }
                    }

                    break;
                }
            }
        }

        if (!_isActivated) return;

        /*switch (slamBlockEnum)
        {
            case SlamBlocksEnum.HorizontalBlock:
            {
                if (Mathf.Abs(transform.position.y - _player.transform.position.y) < 1.5f)
                {
                    player.OnPlayerDamage();
                }

                break;
            }
            case SlamBlocksEnum.VerticalBlock:
            {
                if (Mathf.Abs(transform.position.x - _player.transform.position.x) < 1.5f)
                {
                    player.OnPlayerDamage();
                }

                break;
            }
            case SlamBlocksEnum.SpikeBlock:
            default:
                if (!player.HasImmunity)
                {
                    player.OnPlayerDamage();
                }

                break;
        }*/
    }

    public void OnExit()
    {
        ChangeMovement();
    }

    public GameObject GameObject => gameObject;

    public void ChangeMovement()
    {
        if (_player != null)
        {
            _player.SetParent(null);
            _player.PlayerMovement.IsPlayerOnBlock = false;
            _player = null;
        }
    }

    public void Freeze()
    {
        _defaultDurationSlamBlockMoveIn = durationSlamBlockMovingIn * 2f;
        _defaultDurationSlamBlockMoveOut = durationSlamBlockMovingOut * 2f;
        
        _timeSlamBlockMoveIn = new WaitForSeconds(_defaultDurationSlamBlockMoveIn);
        _timeSlamBlockMoveOut = new WaitForSeconds(_defaultDurationSlamBlockMoveOut);
        
        /*_movingSlamBlockTween?.Kill();

        CalculateMovePositions();
        
        if (slamBlockEnum is SlamBlocksEnum.HorizontalBlock or SlamBlocksEnum.VerticalBlock)
        {
            if (_slamBlockCoroutine != null)
            {
                StopCoroutine(_slamBlockCoroutine);
                _slamBlockCoroutine = null;
            }

            //_slamBlockCoroutine ??= StartCoroutine(StartMovingSlamBlock());
        }
        else
        {
            _isActivated = true;
            
            if (_spikeSlamBlockCoroutine != null)
            {
                StopCoroutine(_spikeSlamBlockCoroutine);
                _spikeSlamBlockCoroutine = null;
            }
            
           //_spikeSlamBlockCoroutine ??= StartCoroutine(StartMovingSpikeSlamBlock());
        }*/
    }

    public void UnFreeze()
    {
        _defaultDurationSlamBlockMoveIn = durationSlamBlockMovingIn;
        _defaultDurationSlamBlockMoveOut = durationSlamBlockMovingOut;
        
        _timeSlamBlockMoveIn = new WaitForSeconds(_defaultDurationSlamBlockMoveIn);
        _timeSlamBlockMoveOut = new WaitForSeconds(_defaultDurationSlamBlockMoveOut);
        
        /*_movingSlamBlockTween?.Kill();
        
        CalculateMovePositions();
        
        if (slamBlockEnum is SlamBlocksEnum.HorizontalBlock or SlamBlocksEnum.VerticalBlock)
        {
            if (_slamBlockCoroutine != null)
            {
                StopCoroutine(_slamBlockCoroutine);
                _slamBlockCoroutine = null;
            }

            _slamBlockCoroutine ??= StartCoroutine(StartMovingSlamBlock());
        }
        else
        {
            _isActivated = true;
            
            if (_spikeSlamBlockCoroutine != null)
            {
                StopCoroutine(_spikeSlamBlockCoroutine);
                _spikeSlamBlockCoroutine = null;
            }
            
            _spikeSlamBlockCoroutine ??= StartCoroutine(StartMovingSpikeSlamBlock());
        }*/
    }

    public DirectionEnum GetEnemyDirection()
    {
        return _enemyDirection;
    }

    public void TurnCollider(bool isActive)
    {
        collider.isTrigger = !isActive;
    }

    public EnemyType GetEnemyType()
    {
        switch (slamBlockEnum)
        {
            case SlamBlocksEnum.HorizontalBlock:
            case SlamBlocksEnum.VerticalBlock:
                return EnemyType.SlamBlock;
            case SlamBlocksEnum.SpikeBlock:
                return EnemyType.SlamBlockSpiked;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #region Tween

    private Tweener _movingSlamBlockTween;

    private Tweener MoveSlamBlock(Vector3 endValue, float duration, Ease ease = Ease.Linear)
    {
        if (_movingSlamBlockTween.IsActive())
        {
            _movingSlamBlockTween.ChangeEndValue(endValue, duration, true)
                .SetEase(ease)
                .Restart();
        }
        else
        {
            _movingSlamBlockTween = transform.DOMove(endValue, duration)
                .SetEase(ease)
                .SetLink(gameObject)
                .SetAutoKill(false);
        }

        return _movingSlamBlockTween;
    }

    #endregion
}