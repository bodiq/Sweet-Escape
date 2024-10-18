using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using DG.Tweening;
using Enums;
using Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerCollision playerCollision;
    [SerializeField] private Rigidbody2D rigidbody2D;
    [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Settings")] [Space(10)]
    [SerializeField] private float moveSpeed = 70f;
    [SerializeField] private float startSlidingSpeed = 10f;
    [SerializeField] private float maxSlidingSpeed = 20f;
    [SerializeField] private Ease slidingEase = Ease.InExpo;
    [SerializeField] private float timeToReachMaxSlidingSpeed = 2f;
    [SerializeField, ReadOnly] private float currentSlidingSpeed;
    [SerializeField, ReadOnly] private DirectionEnum _currentDirection = DirectionEnum.None;

    [Header("TimingSettings")] [Space(10)]
    [SerializeField] private float delayBeforeSliding = 3f;
    [SerializeField] private float delayBeforeIdleFart = 4f;
    [SerializeField] private float delayAfterDrippingDownAnimationStarted = 0.4f;

    private bool _stopMove = true;
    private bool _isStanding;
    private bool _isSliding;
    private bool _isPlayerOnBlock;
    private float _moveDistance = 2;

    private Vector2 _startPosition;
    private Vector2 _endPosition;

    private Vector2 _revivePosition;
    private DirectionEnum _reviveDirection;

    private Vector3Int _playerCellCoordinates;
    private Vector3Int _currentPlayerCoordinates;

    private string _currentState;

    private List<ITrigger> _triggers = new();

    private const string MovingUpAnimationState = "MovingUp";
    private const string MovingDownAnimationState = "MovingDown";
    private const string MovingRightLeftAnimationState = "MovingRightLeft";
    private const string LandingGroundAnimationState = "LandGround";
    private const string LandingUpAnimationState = "LandTopWall";
    private const string LandingWallAnimationState = "LandWall";
    private const string IdleFartAnimationState = "IdleFart";
    private const string IdleStandardAnimationState = "StandardGroundIdle";
    private const string DrippingDownAnimationState = "DrippingDown";

    private Coroutine _slidingDelayCoroutine;
    private Coroutine _idleFartDelayCoroutine;

    private WaitForSeconds _waitDurationBeforeIdleFart;
    private WaitForSeconds _waitDurationBeforeSliding;
    private WaitForSeconds _waitDurationAfterDrippingDownAnimationStarted;

    private Tweener slidingSpeedTween;

    private DirectionEnum _previousDirection = DirectionEnum.Down;

    public bool IsPlayerStanding => _isStanding;
    public bool IsPlayerOnBlock
    {
        get => _isPlayerOnBlock;
        set => _isPlayerOnBlock = value;
    }

    public DirectionEnum PlayerStandOnBlockSide { get; set; }

    private readonly Quaternion _leftColliderRotationEnd = Quaternion.Euler(0f, 0f, -90f);
    private readonly Quaternion _rightColliderRotationEnd = Quaternion.Euler(0f, 0f, 90f);
    private readonly Quaternion _upColliderRotationEnd = Quaternion.Euler(0f, 0f, 180f);
    private readonly Quaternion _downColliderRotationEnd = Quaternion.Euler(0f, 0f, 0f);

    private const float DistanceToCheckBlock = 1f;
    private const float DistanceToKill = 2f;
    private const float OffsetFromBlockStart = 0.5f;

    private void Start()
    {
        _waitDurationBeforeIdleFart = new WaitForSeconds(delayBeforeIdleFart);
        _waitDurationBeforeSliding = new WaitForSeconds(delayBeforeSliding);
        _waitDurationAfterDrippingDownAnimationStarted = new WaitForSeconds(delayAfterDrippingDownAnimationStarted);
    }

    private void OnEnable()
    {
#if UNITY_IOS || UNITY_ANDROID
        inputManager.OnSwipeUp += MoveTo;
        inputManager.OnSwipeDown += MoveTo;
        inputManager.OnSwipeLeft += MoveTo;
        inputManager.OnSwipeRight += MoveTo;
#endif

        inputManager.OnUpButtonMove += MoveTo;
        inputManager.OnDownButtonMove += MoveTo;
        inputManager.OnRightButtonMove += MoveTo;
        inputManager.OnLeftButtonMove += MoveTo;

        GameManager.Instance.OnPlayerRespawn += StopMovement;

        playerCollision.WallHit += OnWallHit;
    }

    private void OnDisable()
    {
#if UNITY_IOS || UNITY_ANDROID
        inputManager.OnSwipeUp -= MoveTo;
        inputManager.OnSwipeDown -= MoveTo;
        inputManager.OnSwipeLeft -= MoveTo;
        inputManager.OnSwipeRight -= MoveTo;
#endif

        inputManager.OnUpButtonMove -= MoveTo;
        inputManager.OnDownButtonMove -= MoveTo;
        inputManager.OnRightButtonMove -= MoveTo;
        inputManager.OnLeftButtonMove -= MoveTo;

        GameManager.Instance.OnPlayerRespawn -= StopMovement;

        playerCollision.WallHit -= OnWallHit;
    }

    private void FixedUpdate()
    {
        var direction = Vector3.zero;
        switch (_currentDirection)
        {
            case DirectionEnum.None:
                return;
            case DirectionEnum.Up:
                direction = Vector3.up;
                break;
            case DirectionEnum.Down:
                direction = Vector3.down;
                break;
            case DirectionEnum.Left:
                direction = Vector3.left;
                break;
            case DirectionEnum.Right:
                direction = Vector3.right;
                break;
        }

        var currentSpeed = _isSliding ? currentSlidingSpeed : moveSpeed;
        rigidbody2D.velocity = direction * currentSpeed;
    }

    private void OnWallHit()
    {
        if (_isPlayerOnBlock)
        {
            return;
        }

        ChangeLandingAnimationState(_currentDirection);
        StopCoroutines();

        _isStanding = true;
        _stopMove = true;
        rigidbody2D.position = new Vector2(Mathf.Round(_endPosition.x), Mathf.Round(_endPosition.y));

        if (_previousDirection != DirectionEnum.Down)
        {
            _slidingDelayCoroutine = StartCoroutine(SlidingEffectCoroutine());
        }
        else
        {
            _idleFartDelayCoroutine = StartCoroutine(IdleFartCoroutine());
            player.direction = DirectionEnum.None;
        }

        if (TilemapManager.Instance.MoveCameraImpulse.CanPlay)
        {
            TilemapManager.Instance.MoveCameraImpulse.PlayFeedbacks();
        }

        ResetMovement();
    }

    private void ResetMovement()
    {
        rigidbody2D.velocity = Vector3.zero;
        _currentDirection = DirectionEnum.None;
    }

    #region Movement

    private void MoveTo(DirectionEnum direction)
    {
        if (_previousDirection == direction)
        {
            return;
        }

        StopCoroutines();

        if (_stopMove && player.ableToMove)
        {
            var roundPosition = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0);
            transform.position = roundPosition;

            var raycastHit2D = SaveAllItemsOnTheWay(direction);

            if (raycastHit2D)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.GetRandomMoveSquishAudioType());
                _stopMove = false;
                _isSliding = false;
                player.direction = direction;

                var animationState = MovingRightLeftAnimationState;
                var offset = new Vector2();

                switch (direction)
                {
                    case DirectionEnum.Down:
                        animationState = MovingDownAnimationState;
                        offset.y = boxCollider2D.size.y / 2;
                        break;
                    case DirectionEnum.Up:
                        animationState = MovingUpAnimationState;
                        offset.y = -boxCollider2D.size.y / 2;
                        break;
                    case DirectionEnum.Left:
                        offset.x = boxCollider2D.size.x / 2;
                        break;
                    case DirectionEnum.Right:
                        offset.x = -boxCollider2D.size.x / 2;
                        break;
                    case DirectionEnum.None:
                    default:
                        break;
                }

                RotatePlayer(direction);
                
                SetupMovementValues(raycastHit2D, offset);

                if (_moveDistance > 3)
                {
                    ChangeAnimationState(animationState);
                }

                SaveReviveValues();

                _previousDirection = direction;
                _currentDirection = direction;
            }
        }
    }

    private void SaveReviveValues()
    {
        if (!_isPlayerOnBlock)
        {
            _revivePosition = transform.position;
            _reviveDirection = _previousDirection;
        }
    }

    private RaycastHit2D SaveAllItemsOnTheWay(DirectionEnum direction)
    {
        var directionVector = Utils.Directions[direction];
        var raycastHit2D = Physics2D.Raycast(transform.position, directionVector, Mathf.Infinity,
            1 << LayerMask.NameToLayer("Default"));

        RaycastHit2D[] triggerHits = Physics2D.RaycastAll(transform.position, directionVector,
            raycastHit2D.distance, 1 << LayerMask.NameToLayer("Trigger"));

        _triggers = Array.ConvertAll(triggerHits, hit => hit.transform.gameObject.GetComponent<ITrigger>()).ToList();
        
        return raycastHit2D;
    }

    public void SetPlayerLastPos()
    {
        StopMovement();
        StopCoroutines();

        _isStanding = true;

        player.PlayerCollision.LastEnteredEnemy?.ChangeMovement();

        _revivePosition = new Vector2(Mathf.Round(_revivePosition.x), Mathf.Round(_revivePosition.y));
        
        transform.position = _revivePosition;
        ChangeLandingAnimationState(_reviveDirection);
        RotatePlayer(_reviveDirection);
        _previousDirection = _reviveDirection;
        player.direction = _previousDirection;

        if (_previousDirection != DirectionEnum.Down)
        {
            _slidingDelayCoroutine = StartCoroutine(SlidingEffectCoroutine());
        }
        else
        {
            _idleFartDelayCoroutine = StartCoroutine(IdleFartCoroutine());
        }

        player.PlayerCollision.LastEnteredEnemy = null;
    }

    public void StopMovement()
    {
        StopCoroutines();
        if (!player.IsDead)
        {
            ChangeLandingAnimationState(_previousDirection);
        }

        _stopMove = true;

        ResetMovement();
    }

    #endregion

    #region Rotation

    public void RotatePlayer(DirectionEnum directionEnum)
    {
        switch (directionEnum)
        {
            case DirectionEnum.Left:
                SetRotationData(true);
                break;
            case DirectionEnum.None:
            case DirectionEnum.Up:
            case DirectionEnum.Down:
            case DirectionEnum.Right:
            default:
                SetRotationData(false);
                break;
        }
    }

    private void SetRotationData(bool isFlipped)
    {
        spriteRenderer.flipX = isFlipped;
    }

    public void SetRespawnPlayerMovement()
    {
        SetRotationData(false);
        ChangeAnimationState(IdleStandardAnimationState);
        _previousDirection = DirectionEnum.Down;
    }

    #endregion

    #region Animation

    public void ChangeAnimationState(string newState, bool isAllowedSameState = false)
    {
        if (_currentState == newState && !isAllowedSameState)
        {
            return;
        }

        if (animator)
        {
            animator.Play(newState);
        }

        _currentState = newState;
    }

    public void ChangeLandingAnimationState(DirectionEnum direction, bool isAllowedSameState = false)
    {
        switch (direction)
        {
            case DirectionEnum.Down:
                ChangeAnimationState(LandingGroundAnimationState, isAllowedSameState);
                break;
            case DirectionEnum.Up:
                ChangeAnimationState(LandingUpAnimationState, isAllowedSameState);
                break;
            case DirectionEnum.Left:
            case DirectionEnum.Right:
            case DirectionEnum.None:
            default:
                ChangeAnimationState(LandingWallAnimationState, isAllowedSameState);
                break;
        }
    }

    public void ChangeControllerToDefault(RuntimeAnimatorController animatorController)
    {
        animator.runtimeAnimatorController = animatorController;
        ChangeLandingAnimationState(_reviveDirection, true);
        RotatePlayer(_reviveDirection);
    }

    public void ChangeControllerToShielded(RuntimeAnimatorController animatorController)
    {
        animator.runtimeAnimatorController = animatorController;
    }

    public void SetStartController(RuntimeAnimatorController animatorController)
    {
        animator.runtimeAnimatorController = animatorController;
    }

    #endregion

    #region General

    private Vector3Int _lastCurrentCellCoordinates;

    private void Update()
    {
        _currentPlayerCoordinates = TilemapManager.Instance.CurrentTilemap.Tilemap.WorldToCell(transform.position);

        if (_currentPlayerCoordinates != _playerCellCoordinates)
        {
            _lastCurrentCellCoordinates = _currentPlayerCoordinates;

            if (_isPlayerOnBlock)
            {
                CheckForNextBlocks();
            }

            SetSlimeEffectToBoxes(_playerCellCoordinates);

            CheckHits();
        }
    }

    private void CheckHits()
    {
        var triggers = _triggers.FindAll(trigger =>
            Vector2.Distance(_startPosition, transform.position) + boxCollider2D.size.x >
            Vector2.Distance(_startPosition, trigger.Transform.position));
        foreach (var trigger in triggers)
        {
            trigger.Trigger(player);
            _triggers.Remove(trigger);
        }
    }

    public void ChangeLastDirection(DirectionEnum newDirection)
    {
        _reviveDirection = _previousDirection;
        _revivePosition = transform.position;
        _previousDirection = newDirection;
    }

    private void CheckForNextBlocks()
    {
        var tilemap = TilemapManager.Instance.CurrentTilemap.Tilemap;
        var cellPos = _playerCellCoordinates;

        var leftBox = new Vector3Int(cellPos.x - 1, cellPos.y);
        var rightBox = new Vector3Int(cellPos.x + 1, cellPos.y);
        var upBox = new Vector3Int(cellPos.x, cellPos.y + 1);
        var downBox = new Vector3Int(cellPos.x, cellPos.y - 1);

        var leftTile = tilemap.GetTile(leftBox);
        var rightTile = tilemap.GetTile(rightBox);
        var upTile = tilemap.GetTile(upBox);
        var downTile = tilemap.GetTile(downBox);

        switch (player?.PlayerCollision.LastEnteredEnemy?.GetEnemyDirection())
        {
            case DirectionEnum.Up:
                switch (PlayerStandOnBlockSide)
                {
                    case DirectionEnum.Up:
                        if (upTile)
                        {
                            var worldPosition = tilemap.CellToWorld(upBox);
                            if (Mathf.Abs(worldPosition.y - player.transform.position.y) < DistanceToKill && !player.HasImmunity)
                            {
                                player.OnPlayerDamage();
                            }
                        }

                        break;
                    case DirectionEnum.Down:
                        break;
                    case DirectionEnum.Left:
                    case DirectionEnum.Right:
                        if (upTile)
                        {
                            var worldPosition = tilemap.CellToWorld(upBox);
                            if (Mathf.Abs(worldPosition.y - player.transform.position.y) < DistanceToCheckBlock)
                            {
                                StopMovement();
                                player.PlayerCollision.LastEnteredEnemy?.ChangeMovement();
                                player.transform.position = new Vector3(worldPosition.x + OffsetFromBlockStart,
                                    worldPosition.y - OffsetFromBlockStart);
                                ChangeLandingAnimationState(DirectionEnum.Up);
                                RotatePlayer(DirectionEnum.Up);
                                ChangeLastDirection(DirectionEnum.Up);
                                player.direction = DirectionEnum.Up;
                            }
                        }

                        break;
                    case DirectionEnum.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case DirectionEnum.Down:
                switch (PlayerStandOnBlockSide)
                {
                    case DirectionEnum.Up:
                        break;
                    case DirectionEnum.Down:
                        if (downTile)
                        {
                            var worldPosition = tilemap.CellToWorld(downBox);
                            if (Mathf.Abs(player.transform.position.y - worldPosition.y) < DistanceToKill &&
                                !player.HasImmunity)
                            {
                                player.OnPlayerDamage();
                            }
                        }

                        break;
                    case DirectionEnum.Left:
                    case DirectionEnum.Right:
                        if (downTile)
                        {
                            var worldPosition = tilemap.CellToWorld(downBox);
                            if (Mathf.Abs(player.transform.position.y - worldPosition.y) < DistanceToCheckBlock)
                            {
                                StopMovement();
                                player.PlayerCollision.LastEnteredEnemy?.ChangeMovement();
                                player.transform.position = new Vector3(worldPosition.x + OffsetFromBlockStart,
                                    worldPosition.y + 1.5f);
                                ChangeLandingAnimationState(DirectionEnum.Down);
                                RotatePlayer(DirectionEnum.Down);
                                ChangeLastDirection(DirectionEnum.Down);
                                player.direction = DirectionEnum.Down;
                            }
                        }

                        break;
                    case DirectionEnum.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case DirectionEnum.Left:
                switch (PlayerStandOnBlockSide)
                {
                    case DirectionEnum.Up:
                    case DirectionEnum.Down:
                        if (leftTile)
                        {
                            var worldPosition = tilemap.CellToWorld(leftBox);
                            if (Mathf.Abs(worldPosition.x - player.transform.position.x) < DistanceToCheckBlock)
                            {
                                StopMovement();
                                player.PlayerCollision.LastEnteredEnemy?.ChangeMovement();
                                player.transform.position = new Vector3(worldPosition.x + OffsetFromBlockStart + 1f,
                                    worldPosition.y + OffsetFromBlockStart);
                                ChangeLandingAnimationState(DirectionEnum.Left);
                                RotatePlayer(DirectionEnum.Left);
                                ChangeLastDirection(DirectionEnum.Left);
                                player.direction = DirectionEnum.Left;
                            }
                        }

                        break;
                    case DirectionEnum.Left:
                        if (leftTile)
                        {
                            var worldPosition = tilemap.CellToWorld(leftBox);
                            if (Mathf.Abs(worldPosition.x - player.transform.position.x) < DistanceToKill &&
                                !player.HasImmunity)
                            {
                                player.OnPlayerDamage();
                            }
                        }

                        break;
                    case DirectionEnum.Right:
                        break;
                    case DirectionEnum.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case DirectionEnum.Right:
                switch (PlayerStandOnBlockSide)
                {
                    case DirectionEnum.Up:
                    case DirectionEnum.Down:
                        if (rightTile)
                        {
                            var worldPosition = tilemap.CellToWorld(rightBox);
                            if (Mathf.Abs(worldPosition.x - player.transform.position.x) < DistanceToCheckBlock)
                            {
                                StopMovement();
                                player.PlayerCollision.LastEnteredEnemy?.ChangeMovement();
                                player.transform.position = new Vector3(worldPosition.x - OffsetFromBlockStart,
                                    worldPosition.y + OffsetFromBlockStart);
                                ChangeLandingAnimationState(DirectionEnum.Right);
                                RotatePlayer(DirectionEnum.Right);
                                ChangeLastDirection(DirectionEnum.Right);
                                player.direction = DirectionEnum.Right;
                            }
                        }

                        break;
                    case DirectionEnum.Left:
                        break;
                    case DirectionEnum.Right:
                        if (rightTile)
                        {
                            var worldPosition = tilemap.CellToWorld(rightBox);
                            if (Mathf.Abs(worldPosition.x - player.transform.position.x) < DistanceToKill &&
                                !player.HasImmunity)
                            {
                                player.OnPlayerDamage();
                            }
                        }

                        break;
                    case DirectionEnum.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case DirectionEnum.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Boom(Tilemap tilemap)
    {
        var pos = _playerCellCoordinates;
        var countUpDown = Mathf.Abs(pos.y - _currentPlayerCoordinates.y);
        var countLeftRight = Mathf.Abs(pos.x - _currentPlayerCoordinates.x);

        switch (player.direction)
        {
            case DirectionEnum.Down or DirectionEnum.Up:
            {
                for (var i = 0; i < countUpDown; i++)
                {
                    Vector3Int leftBox;
                    Vector3Int rightBox;

                    if (player.direction == DirectionEnum.Down)
                    {
                        leftBox = new Vector3Int(pos.x - 1, pos.y - 1 - i);
                        rightBox = new Vector3Int(pos.x + 1, pos.y - 1 - i);
                    }
                    else
                    {
                        leftBox = new Vector3Int(pos.x - 1, pos.y + 1 + i);
                        rightBox = new Vector3Int(pos.x + 1, pos.y + 1 + i);
                    }

                    var leftTile = tilemap.GetTile(leftBox);
                    var rightTile = tilemap.GetTile(rightBox);

                    if (leftTile)
                    {
                        var worldPosition = tilemap.CellToWorld(leftBox);
                        var slime = ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject();
                        worldPosition = new Vector3(worldPosition.x + 1.06f, worldPosition.y + 0.5f);
                        var slimeTransform = slime.transform;
                        slimeTransform.position = worldPosition;
                        slimeTransform.rotation = _leftColliderRotationEnd;
                        slime.gameObject.SetActive(true);
                    }

                    if (rightTile)
                    {
                        var worldPosition = tilemap.CellToWorld(rightBox);
                        var slime = ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject();
                        worldPosition = new Vector3(worldPosition.x - 0.06f, worldPosition.y + 0.5f);
                        var slimeTransform = slime.transform;
                        slimeTransform.position = worldPosition;
                        slimeTransform.rotation = _rightColliderRotationEnd;
                        slime.gameObject.SetActive(true);
                    }
                }

                break;
            }
            case DirectionEnum.Left or DirectionEnum.Right:
            {
                for (var i = 0; i < countLeftRight; i++)
                {
                    Vector3Int upBox;
                    Vector3Int downBox;

                    if (player.direction is DirectionEnum.Left)
                    {
                        upBox = new Vector3Int(pos.x - 1 - i, pos.y + 1);
                        downBox = new Vector3Int(pos.x - 1 - i, pos.y - 1);
                    }
                    else
                    {
                        upBox = new Vector3Int(pos.x + 1 + i, pos.y + 1);
                        downBox = new Vector3Int(pos.x + 1 + i, pos.y - 1);
                    }

                    var upTile = tilemap.GetTile(upBox);
                    var downTile = tilemap.GetTile(downBox);

                    if (upTile)
                    {
                        var worldPosition = tilemap.CellToWorld(upBox);
                        var slime = ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject();
                        worldPosition = new Vector3(worldPosition.x + 0.5f, worldPosition.y - 0.06f);
                        slime.transform.position = worldPosition;
                        slime.transform.rotation = _upColliderRotationEnd;
                        slime.gameObject.SetActive(true);
                    }

                    if (downTile)
                    {
                        var worldPosition = tilemap.CellToWorld(downBox);
                        var slime = ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject();
                        worldPosition = new Vector3(worldPosition.x + 0.5f, worldPosition.y + 1.06f);
                        slime.transform.position = worldPosition;
                        slime.transform.rotation = _downColliderRotationEnd;
                        slime.gameObject.SetActive(true);
                    }
                }

                break;
            }
        }

        _playerCellCoordinates = _lastCurrentCellCoordinates;
    }

    private void SetSlimeEffectWhereYouStand(Vector3Int coords)
    {
        var leftBox = new Vector3Int(coords.x - 1, coords.y);
        var rightBox = new Vector3Int(coords.x + 1, coords.y);
        var upBox = new Vector3Int(coords.x, coords.y + 1);
        var downBox = new Vector3Int(coords.x, coords.y - 1);

        var tilemap = TilemapManager.Instance.CurrentTilemap.Tilemap;

        var leftTile = tilemap.GetTile(leftBox);
        var rightTile = tilemap.GetTile(rightBox);
        var upTile = tilemap.GetTile(upBox);
        var downTile = tilemap.GetTile(downBox);

        switch (_previousDirection)
        {
            case DirectionEnum.None:
            default:
                return;
            case DirectionEnum.Up:
            case DirectionEnum.Down:
            case DirectionEnum.Left:
            case DirectionEnum.Right:
                Vector3 worldPosition;

                if (upTile)
                {
                    worldPosition = tilemap.CellToWorld(upBox);
                    var slime = ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject();
                    worldPosition = new Vector3(worldPosition.x + 0.5f, worldPosition.y - 0.06f);
                    slime.transform.position = worldPosition;
                    slime.transform.rotation = _upColliderRotationEnd;
                    slime.gameObject.SetActive(true);
                }

                if (downTile)
                {
                    worldPosition = tilemap.CellToWorld(downBox);
                    var slime = ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject();
                    worldPosition = new Vector3(worldPosition.x + 0.5f, worldPosition.y + 1.06f);
                    slime.transform.position = worldPosition;
                    slime.transform.rotation = _downColliderRotationEnd;
                    slime.gameObject.SetActive(true);
                }

                if (leftTile)
                {
                    worldPosition = tilemap.CellToWorld(leftBox);
                    var slime = ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject();
                    worldPosition = new Vector3(worldPosition.x + 1.06f, worldPosition.y + 0.5f);
                    slime.transform.position = worldPosition;
                    slime.transform.rotation = _leftColliderRotationEnd;
                    slime.gameObject.SetActive(true);
                }

                if (rightTile)
                {
                    worldPosition = tilemap.CellToWorld(rightBox);
                    var slime = ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject();
                    worldPosition = new Vector3(worldPosition.x - 0.06f, worldPosition.y + 0.5f);
                    slime.transform.position = worldPosition;
                    slime.transform.rotation = _rightColliderRotationEnd;
                    slime.gameObject.SetActive(true);
                }

                break;
        }
    }

    private void SetSlimeEffectToBoxes(Vector3Int cellCoords)
    {
        var tilemap = TilemapManager.Instance.CurrentTilemap.Tilemap;
        var cellPos = cellCoords;


        var countUpDown = Mathf.Abs(cellPos.y - _currentPlayerCoordinates.y);
        var countLeftRight = Mathf.Abs(cellPos.x - _currentPlayerCoordinates.x);

        switch (player.direction)
        {
            case DirectionEnum.Down or DirectionEnum.Up:
            {
                for (var i = 0; i < countUpDown; i++)
                {
                    Vector3Int leftBox;
                    Vector3Int rightBox;

                    if (player.direction == DirectionEnum.Down)
                    {
                        leftBox = new Vector3Int(cellPos.x - 1, cellPos.y - 1 - i);
                        rightBox = new Vector3Int(cellPos.x + 1, cellPos.y - 1 - i);
                    }
                    else
                    {
                        leftBox = new Vector3Int(cellPos.x - 1, cellPos.y + 1 + i);
                        rightBox = new Vector3Int(cellPos.x + 1, cellPos.y + 1 + i);
                    }

                    var leftTile = tilemap.GetTile(leftBox);
                    var rightTile = tilemap.GetTile(rightBox);

                    if (leftTile)
                    {
                        var worldPosition = tilemap.CellToWorld(leftBox);
                        var slime = ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject();
                        worldPosition = new Vector3(worldPosition.x + 1.06f, worldPosition.y + 0.5f);
                        var slimeTransform = slime.transform;
                        slimeTransform.position = worldPosition;
                        slimeTransform.rotation = _leftColliderRotationEnd;
                        slime.gameObject.SetActive(true);
                    }

                    if (rightTile)
                    {
                        var worldPosition = tilemap.CellToWorld(rightBox);
                        var slime = ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject();
                        worldPosition = new Vector3(worldPosition.x - 0.06f, worldPosition.y + 0.5f);
                        var slimeTransform = slime.transform;
                        slimeTransform.position = worldPosition;
                        slimeTransform.rotation = _rightColliderRotationEnd;
                        slime.gameObject.SetActive(true);
                    }
                }

                break;
            }
            case DirectionEnum.Left or DirectionEnum.Right:
            {
                for (var i = 0; i < countLeftRight; i++)
                {
                    Vector3Int upBox;
                    Vector3Int downBox;

                    if (player.direction is DirectionEnum.Left)
                    {
                        upBox = new Vector3Int(cellPos.x - 1 - i, cellPos.y + 1);
                        downBox = new Vector3Int(cellPos.x - 1 - i, cellPos.y - 1);
                    }
                    else
                    {
                        upBox = new Vector3Int(cellPos.x + 1 + i, cellPos.y + 1);
                        downBox = new Vector3Int(cellPos.x + 1 + i, cellPos.y - 1);
                    }

                    var upTile = tilemap.GetTile(upBox);
                    var downTile = tilemap.GetTile(downBox);

                    if (upTile)
                    {
                        var worldPosition = tilemap.CellToWorld(upBox);
                        var slime = ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject();
                        worldPosition = new Vector3(worldPosition.x + 0.5f, worldPosition.y - 0.06f);
                        slime.transform.position = worldPosition;
                        slime.transform.rotation = _upColliderRotationEnd;
                        slime.gameObject.SetActive(true);
                    }

                    if (downTile)
                    {
                        var worldPosition = tilemap.CellToWorld(downBox);
                        var slime = ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject();
                        worldPosition = new Vector3(worldPosition.x + 0.5f, worldPosition.y + 1.06f);
                        slime.transform.position = worldPosition;
                        slime.transform.rotation = _downColliderRotationEnd;
                        slime.gameObject.SetActive(true);
                    }
                }

                break;
            }
        }

        _playerCellCoordinates = _lastCurrentCellCoordinates;
    }

    private void StopCoroutines()
    {
        if (_slidingDelayCoroutine != null)
        {
            StopCoroutine(_slidingDelayCoroutine);
            _slidingDelayCoroutine = null;
        }

        if (_idleFartDelayCoroutine != null)
        {
            StopCoroutine(_idleFartDelayCoroutine);
            _idleFartDelayCoroutine = null;
        }
    }

    private IEnumerator IdleFartCoroutine()
    {
        
        while (true)
        {
            yield return _waitDurationBeforeIdleFart;
            ChangeAnimationState(IdleFartAnimationState, true);
            
            if (AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementsTypes.Fart))
            {
                UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh?.Invoke(AchievementsTypes.Fart, 1);
            }
        }
    }

    private IEnumerator SlidingEffectCoroutine()
    {
        yield return _waitDurationBeforeSliding;
        SaveReviveValues();
        _previousDirection = DirectionEnum.Down;

        var raycastHit2D = SaveAllItemsOnTheWay(DirectionEnum.Down);
        switch (player.direction)
        {
            case DirectionEnum.Left:
            case DirectionEnum.Right:
                player.direction = DirectionEnum.Down;
                var currentPos = _currentPlayerCoordinates;
                _isStanding = false;
                yield return new WaitForSeconds(0.5f);
                _currentDirection = DirectionEnum.Down;
                SetSlimeEffectWhereYouStand(currentPos);
                break;
            case DirectionEnum.Up:
                player.direction = DirectionEnum.Down;
                ChangeAnimationState(DrippingDownAnimationState);
                SetSlimeEffectWhereYouStand(_currentPlayerCoordinates);
                yield return _waitDurationAfterDrippingDownAnimationStarted;
                _currentDirection = DirectionEnum.Down;
                _isStanding = false;
                break;
            case DirectionEnum.Down:
                break;
            case DirectionEnum.None:
            default:
                break;
        }

        _isSliding = true;
        slidingSpeedTween?.Kill();
        slidingSpeedTween = DOTween
            .To(value => currentSlidingSpeed = value, startSlidingSpeed, maxSlidingSpeed, timeToReachMaxSlidingSpeed)
            .SetEase(slidingEase);

        var offset = new Vector2 { y = boxCollider2D.size.y / 2 };
        SetupMovementValues(raycastHit2D, offset);
    }

    private void SetupMovementValues(RaycastHit2D raycastHit2D, Vector2 offset)
    {
        var targetPosition = raycastHit2D.point + offset;
        _moveDistance = Vector2.Distance(transform.position, targetPosition);
        SetSlimeEffectWhereYouStand(_currentPlayerCoordinates);
        player.PlayerCollision.LastEnteredEnemy?.ChangeMovement();
        _isStanding = false;
        _startPosition = transform.position;
        _endPosition = targetPosition;
    }

    #endregion
}