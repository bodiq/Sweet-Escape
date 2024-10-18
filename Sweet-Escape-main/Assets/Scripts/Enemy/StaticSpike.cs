using Enums;
using UnityEngine;

public class StaticSpike : MonoBehaviour, Enemy.IEnemy
{
    private static readonly LayerMask DefaultLayerMask = 2;
    private static readonly LayerMask PowerUpOnLayerMask = 0;

    private void OnEnable()
    {
        GameManager.Instance.OnPlayerPowerUpStart += OnPowerUpOn;
        GameManager.Instance.OnPlayerPowerUpEnd += OnPowerUpOff;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPlayerPowerUpStart -= OnPowerUpOn;
        GameManager.Instance.OnPlayerPowerUpEnd -= OnPowerUpOff;
    }

    private void OnPowerUpOn(Enums.PowerUps powerUp)
    {
        if (powerUp == Enums.PowerUps.ChillBlast)
        {
            gameObject.layer = PowerUpOnLayerMask;
        }
    }

    private void OnPowerUpOff(Enums.PowerUps powerUp)
    {
        if (powerUp == Enums.PowerUps.ChillBlast)
        {
            gameObject.layer = DefaultLayerMask;
        }
    }

    public void OnEnter(Player player)
    {
        if (player.PowerUpsActivated[Enums.PowerUps.ChillBlast])
        {
            return;
        }

        player.OnPlayerDamage();
    }

    public void OnExit()
    {
    }

    public GameObject GameObject => gameObject;

    public void ChangeMovement()
    {
    }

    public void Freeze()
    {
    }

    public void UnFreeze()
    {
    }

    public DirectionEnum GetEnemyDirection()
    {
        return DirectionEnum.None;
    }

    public void TurnCollider(bool isActive)
    {
        return;
    }

    public EnemyType GetEnemyType()
    {
        return EnemyType.StaticSpike;
    }
}