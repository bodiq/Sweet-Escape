using System.Collections.Generic;
using Extensions;
using Items;
using PowerUps;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapRoom : MonoBehaviour
{
    [SerializeField] private Transform enterRoomPosition;
    [SerializeField] private Transform exitRoomPosition;

    [SerializeField, ReadOnly] private Tilemap tilemap;
    [SerializeField, ReadOnly] private Transform sprinklesParent;
    [SerializeField, ReadOnly] private Transform coinsParent;
    [SerializeField, ReadOnly] private Transform powerUpParent;

    [SerializeField, ReadOnly] private RoomInfo _roomInfo;

    private readonly Dictionary<Vector3, Sprinkle> sprinkles = new();
    private readonly Dictionary<Vector3, Coin> coins = new();
    private readonly Dictionary<Vector3, PowerUpManager> powerUps = new();

    public Transform EnterRoomPos => enterRoomPosition;
    public Transform ExitRoomPos => exitRoomPosition;
    public Tilemap Tilemap
    {
        get => tilemap;
        set => tilemap = value;
    }
    public RoomInfo RoomInfo => _roomInfo;

    private void Awake()
    {
        _roomInfo = gameObject.name.ParseRoomInfo();

        if (sprinklesParent)
        {
            foreach (var sprinkle in sprinklesParent.GetComponentsInChildren<Sprinkle>())
            {
                if (!sprinkles.TryAdd(sprinkle.transform.localPosition, sprinkle))
                {
                    Destroy(sprinkle.gameObject);
                }
            }
        }

        if (coinsParent)
        {
            foreach (var coin in coinsParent.GetComponentsInChildren<Coin>())
            {
                if (!coins.TryAdd(coin.transform.localPosition, coin))
                {
                    Destroy(coin.gameObject);
                }
            }
        }

        if (powerUpParent)
        {
            foreach (var powerUp in powerUpParent.GetComponentsInChildren<PowerUpManager>())
            {
                if (!powerUps.TryAdd(powerUp.transform.localPosition, powerUp))
                {
                    Destroy(powerUp);
                }
            }
        }
    }

    private void OnEnable()
    {
        if (sprinkles.Count != 0)
        {
            foreach (var sprinkle in sprinkles.Values)
            {
                sprinkle.Activate();
            }
        }

        if (coins.Count != 0)
        {
            foreach (var coin in coins.Values)
            {
                coin.Activate();
            }
        }
    }

    private void OnValidate()
    {
        sprinklesParent = transform.FindChildRecursively(Constants.SprinkleParentNameInLevel)?.transform;
        coinsParent = transform.FindChildRecursively(Constants.CoinParentNameInLevel)?.transform;
        powerUpParent = transform.FindChildRecursively(Constants.PowerUpParentNameInLevel)?.transform;
        tilemap = transform.FindChildRecursively(Constants.TilemapNameInLevel)?.GetComponent<Tilemap>();
    }

    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        TilemapRoomPool.Instance.ReturnRoom(this);
    }
}