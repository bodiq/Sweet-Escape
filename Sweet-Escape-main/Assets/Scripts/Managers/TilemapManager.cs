using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : SerializedMonoBehaviour
{
    public static TilemapManager Instance;

    [SerializeField] private TilemapRoom startTileMap;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private PersistantHazard persistantHazard;
    [SerializeField] private CinemachineVirtualCamera camera;
    [SerializeField] private MMF_Player moveCameraImpulse;
    [SerializeField] private MMF_Player slamBlockImpulse;
    [SerializeField] private MMF_Player hazardCloseShake;
    [SerializeField] private MMF_Player magnetCameraImpulse;

    [SerializeField] private CanvasGroup defaultBackground;
    [SerializeField] private CanvasGroup startRoomBackground;

    [SerializeField] private Parallax parallaxEffect;

    [SerializeField] private Canvas canvas;

    [SerializeField, ReadOnly] private List<TilemapRoom> _allTilemapRooms = new();

    private Queue<TilemapRoom> _activeTilemapRooms;

    private int _currentTilemapIndex;

    public TilemapRoom CurrentTilemap => _allTilemapRooms[_currentTilemapIndex];
    public Transform PlayerSpawnPoint => playerSpawnPoint;

    public MMF_Player MoveCameraImpulse => moveCameraImpulse;
    public MMF_Player SlamBlockImpulse => slamBlockImpulse;
    public MMF_Player HazardCloseShake => hazardCloseShake;
    public MMF_Player MagnetCameraImpulse => magnetCameraImpulse;
    public CinemachineVirtualCamera Camera => camera;
    public PersistantHazard PersistantHazard => persistantHazard;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        _activeTilemapRooms = new Queue<TilemapRoom>(TilemapManagerConfig.Instance.ActiveTilemapRoomsAmount);
    }

    private void Start()
    {
        if (!GameManager.Instance.IsTestEnvironment)
        {
            SpawnStartRoom();
        }
    }

    private void OnEnable()
    {
        canvas.worldCamera = GameManager.Instance.MainCamera;

        GameManager.Instance.OnPlayerRespawn += OnPlayerRespawned;
    }

    private void SpawnStartRoom()
    {
        startTileMap.gameObject.SetActive(true);
        _allTilemapRooms.Add(startTileMap);
        SpawnRoom();
    }

    private void OnPlayerRespawned()
    {
        ResetMap();
        RefreshPersistantMovement();
        ResetParallaxData();
    }

    public void ResetParallaxData()
    {
        parallaxEffect.ResetData();
    }

    private void UpdatePersistantHazard()
    {
        persistantHazard.MovementSpeed += 0.1f;
        persistantHazard.NormalizeHazardPosition();
    }

    private void UpdateParallaxEffect()
    {
        parallaxEffect.ResetOrder(GameManager.Instance.player.transform);
    }

    private void RefreshPersistantMovement()
    {
        persistantHazard.MovementSpeed = 3f;
    }

    private void SpawnRoom()
    {
        var roomInfo = RoomInfoManager.Instance.GetRoomInfo(_allTilemapRooms.Count - 1);
        var room = TilemapRoomPool.Instance.GetRoom(roomInfo);
        var previousRoom = _activeTilemapRooms.Count == 0 ? startTileMap : _activeTilemapRooms.Last();
        room.transform.position = previousRoom.ExitRoomPos.position - room.EnterRoomPos.localPosition;
        room.gameObject.SetActive(true);
        _activeTilemapRooms.Enqueue(room);
        _allTilemapRooms.Add(room);
    }

    private void ChangeBackground(bool isRespawn = false)
    {
        if (isRespawn)
        {
            defaultBackground.DOFade(0f, 0.4f);
            startRoomBackground.DOFade(1f, 0.3f);
        }
        else
        {
            defaultBackground.DOFade(1f, 0.3f);
            startRoomBackground.DOFade(0f, 0.4f);
        }
    }

    public void OnRoomChanged()
    {
        if (_currentTilemapIndex == 0)
        {
            ChangeBackground();
        }

        SpawnRoom();

        if (_currentTilemapIndex >= TilemapManagerConfig.Instance.ActiveTilemapRoomsAmount - 2)
        {
            var tilemapRoom = _activeTilemapRooms.Dequeue();
            tilemapRoom.ReturnToPool();
        }

        _currentTilemapIndex += 1;

        UpdatePersistantHazard();
        UpdateParallaxEffect();
    }

    private void ResetMap()
    {
        ChangeBackground(true);

        foreach (var tilemapRoom in _activeTilemapRooms)
        {
            tilemapRoom.ReturnToPool();
        }

        _allTilemapRooms.Clear();
        _activeTilemapRooms.Clear();
        _currentTilemapIndex = 0;

        SpawnStartRoom();
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPlayerRespawn -= OnPlayerRespawned;
    }
}