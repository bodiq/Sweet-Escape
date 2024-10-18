using Enums;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region Events
    private delegate void StartTouch(Vector2 position);
    
    private event StartTouch OnStartTouch;

    public delegate void LeftSwipeMove(DirectionEnum direction);
    public delegate void RightSwipeMove(DirectionEnum direction);
    public delegate void UpSwipeMove(DirectionEnum direction);
    public delegate void DownSwipeMove(DirectionEnum direction);
    
    public event DownSwipeMove OnSwipeDown;
    public event UpSwipeMove OnSwipeUp;
    public event LeftSwipeMove OnSwipeLeft;
    public event RightSwipeMove OnSwipeRight;
    
    public delegate void LeftButtonMove(DirectionEnum direction);
    public delegate void RightButtonMove(DirectionEnum direction);
    public delegate void UpButtonMove(DirectionEnum direction);
    public delegate void DownButtonMove(DirectionEnum direction);
    
    public event LeftButtonMove OnLeftButtonMove;
    public event RightButtonMove OnRightButtonMove;
    public event UpButtonMove OnUpButtonMove;
    public event DownButtonMove OnDownButtonMove;

    #endregion
    
    [SerializeField, Range(0f, 1f)] private float minimumDistancePercentage = 0.15f;
    [SerializeField, Range(0f, 1f)] private float directionThreshold = 0.9f;

    private Vector2 _startPosition;
    private Vector2 _endPosition;
    
    private PlayerControl _playerControl;
    private PauseMenuScreen _pauseMenuScreen;
    private float minimumDistance;

    private void Awake()
    {
        _playerControl = new PlayerControl();
    }

    private void Start()
    {
        _pauseMenuScreen = UIManager.Instance.GetUIScreen<PauseMenuScreen>();
        minimumDistance = Screen.currentResolution.width * minimumDistancePercentage;

        _pauseMenuScreen.Paused += Deactivate;
        _pauseMenuScreen.Unpaused += Activate;

        _playerControl.Player.PrimaryContact.started += StartTouchPrimary;
        _playerControl.Player.PrimaryPosition.performed += PerformTouchPrimary;

        _playerControl.Player.MoveUp.started += MoveUp;
        _playerControl.Player.MoveDown.started += MoveDown;
        _playerControl.Player.MoveLeft.started += MoveLeft;
        _playerControl.Player.MoveRight.started += MoveRight;
    }

    private void OnEnable()
    {
        _playerControl.Enable();
        OnStartTouch += SwipeStart;
    }

    private void Activate()
    {
        _playerControl.Enable();
    }

    private void Deactivate()
    {
        _playerControl.Disable();
    }

    #region Keyboard

    private void MoveUp(InputAction.CallbackContext ctx)
    {
        OnUpButtonMove?.Invoke(DirectionEnum.Up);
    }
    
    private void MoveDown(InputAction.CallbackContext ctx)
    {
        OnDownButtonMove?.Invoke(DirectionEnum.Down);
    }
    
    private void MoveRight(InputAction.CallbackContext ctx)
    {
        OnRightButtonMove?.Invoke(DirectionEnum.Right);
    }
    
    private void MoveLeft(InputAction.CallbackContext ctx)
    {
        OnLeftButtonMove?.Invoke(DirectionEnum.Left);
    }

    #endregion

    #region TouchScreen

    private void StartTouchPrimary(InputAction.CallbackContext ctx)
    {
        OnStartTouch?.Invoke(ScreenPosition());
    }

    private void PerformTouchPrimary(InputAction.CallbackContext ctx)
    {
        _endPosition = ScreenPosition();
        DetectSwipe();
    }

    private Vector2 ScreenPosition()
    {
        return _playerControl.Player.PrimaryPosition.ReadValue<Vector2>();
    }
    
    private void SwipeStart(Vector2 position)
    {
        _startPosition = position;
    }

    private void DetectSwipe()
    {
        if (Vector3.Distance(_startPosition, _endPosition) >= minimumDistance)
        {
            if (_startPosition != Vector2.zero)
            {
                Vector3 direction = _endPosition - _startPosition;
                Vector2 direction2D = new Vector2(direction.x, direction.y).normalized;
                SwipeDirection(direction2D);
            }
            SwipeStart(_endPosition);
        }
    }
    
    private void SwipeDirection(Vector2 direction)
    {
        if (Vector2.Dot(Vector2.up, direction) > directionThreshold)
        {
            OnSwipeUp?.Invoke(DirectionEnum.Up);
        }
        else if (Vector2.Dot(Vector2.down, direction) > directionThreshold)
        {
            OnSwipeDown?.Invoke(DirectionEnum.Down);
        }
        else if (Vector2.Dot(Vector2.left, direction) > directionThreshold)
        {
            OnSwipeLeft?.Invoke(DirectionEnum.Left);
        }
        else if (Vector2.Dot(Vector2.right, direction) > directionThreshold)
        {
            OnSwipeRight?.Invoke(DirectionEnum.Right);
        }
    }

    #endregion

    private void OnDisable()
    {
        _playerControl.Disable();
        OnStartTouch -= SwipeStart;
    }

    private void OnDestroy()
    {
        _pauseMenuScreen.Paused -= Deactivate;
        _pauseMenuScreen.Unpaused -= Activate;

        _playerControl.Player.PrimaryContact.started -= StartTouchPrimary;
        _playerControl.Player.PrimaryPosition.performed -= PerformTouchPrimary;

        _playerControl.Player.MoveUp.started -= MoveUp;
        _playerControl.Player.MoveDown.started -= MoveDown;
        _playerControl.Player.MoveLeft.started -= MoveLeft;
        _playerControl.Player.MoveRight.started -= MoveRight;
    }
}
