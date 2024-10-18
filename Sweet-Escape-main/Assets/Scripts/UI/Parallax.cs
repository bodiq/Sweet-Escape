using System;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

public class Parallax : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    [SerializeField] private float parallaxEffect;
    [SerializeField] private List<SpriteRenderer> backgroundListImages;

    private float lenght;
    private float startPosY;
    private float startPosX;

    private Vector3 _initialPosition;

    private Player _player;

    private void Awake()
    {
        startPosY = transform.position.y;
        startPosX = transform.position.x;
        lenght = GetComponent<SpriteRenderer>().bounds.size.y;
        _initialPosition = transform.position;
    }

    private void Start()
    {
        _player = GameManager.Instance.player;
    }

    private void OnEnable()
    {
        GameManager.Instance.OnPlayerRespawn += ResetPos;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPlayerRespawn -= ResetPos;
    }

    private void FixedUpdate()
    {
        if (_player == null)
        {
            _player = GameManager.Instance.player;
        }
        var temp = camera.transform.position.y * (1 - parallaxEffect);

        if (temp > startPosY + lenght) startPosY += lenght;
        else if (temp < startPosY - lenght) startPosY -= lenght;
    }

    public void ResetData()
    {
        GameManager.Instance.MainCamera.backgroundColor = MapVariations.Instance.factoryColorBackground;
        foreach (var image in backgroundListImages)
        {
            image.sprite = MapVariations.Instance.factorySpriteBackground;
        }
    }

    public void ResetOrder(Transform posToCenter)
    {
        var temp = camera.transform.position.y * (1 - parallaxEffect);
        var dist = camera.transform.position.y * parallaxEffect;

        transform.position = new Vector3(posToCenter.position.x, startPosY + dist, transform.position.z);

        switch (TilemapManager.Instance.CurrentTilemap.RoomInfo.Biome)
        {
            case BiomeEnum.None:
                break;
            case BiomeEnum.Factory:
                GameManager.Instance.MainCamera.backgroundColor = MapVariations.Instance.factoryColorBackground;
                foreach (var image in backgroundListImages)
                {
                    image.sprite = MapVariations.Instance.factorySpriteBackground;
                }
                break;
            case BiomeEnum.Sewers:
                GameManager.Instance.MainCamera.backgroundColor = MapVariations.Instance.severColorBackground;
                foreach (var image in backgroundListImages)
                {
                    image.sprite = MapVariations.Instance.severSpriteBackground;
                }
                break;
            case BiomeEnum.Radiation:
                GameManager.Instance.MainCamera.backgroundColor = MapVariations.Instance.radiationColorBackground;
                foreach (var image in backgroundListImages)
                {
                    image.sprite = MapVariations.Instance.radiationSpriteBackground;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ResetPos()
    {
        transform.position = _initialPosition;
    }
}
