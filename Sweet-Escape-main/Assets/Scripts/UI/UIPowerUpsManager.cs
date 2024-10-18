using System;
using System.Collections.Generic;
using System.Linq;
using PowerUps;
using UnityEngine;

public class UIPowerUpsManager : MonoBehaviour
{
    [SerializeField] private List<UIPowerUpsData> powerUps = new();

    public void InitializePowerUp(Enums.PowerUps powerUp, PowerUp objectPowerUp, int count = 0, float duration = 0f)
    {
        foreach (var powerUpsData in powerUps.Where(powerUpsData => powerUpsData.powerUp == powerUp))
        {
            powerUpsData.uiPowerUp.gameObject.SetActive(true);
            powerUpsData.uiPowerUp.CreateUIPowerUp(objectPowerUp, count, duration);
        }
    }

    public void DecreaseOneShield()
    {
        foreach (var powerUp in powerUps.Where(powerUp => powerUp.powerUp == Enums.PowerUps.WaffleShield))
        {
            powerUp.uiPowerUp.MinusShield();
            break;
        }
    }

    public void TurnAllPowerUpsCount(bool isActive)
    {
        foreach (var powerUp in powerUps.Where(powerUp => powerUp.uiPowerUp.isActiveAndEnabled))
        {
            powerUp.uiPowerUp.PowerUpCountTurn(isActive);
        }
    }

    public void StopAllNumericAnimations()
    {
        foreach (var powerUp in powerUps.Where(powerUp => powerUp.uiPowerUp.isActiveAndEnabled))
        {
            if (powerUp.powerUp is Enums.PowerUps.WaffleShield)
            {
                powerUp.uiPowerUp.StopTwoStateAnimation();
                continue;
            }
            powerUp.uiPowerUp.StopNumericAnimation();
        }
    }

    public void TurnOffAllPreviousPowerUps()
    {
        foreach (var powerUp in powerUps)
        {
            powerUp.uiPowerUp.SetPreviousPowerUpNull();
        }
    }

    public void ResumeAllNumericAnimations()
    {
        foreach (var powerUp in powerUps.Where(powerUp => powerUp.uiPowerUp.isActiveAndEnabled))
        {
            if (powerUp.powerUp is Enums.PowerUps.WaffleShield)
            {
                powerUp.uiPowerUp.ResumeTwoStateAnimation();
                continue;
            }
            powerUp.uiPowerUp.ResumeNumericAnimation();
        }
    }
    
    public void TurnOffAllNumericAnimations()
    {
        foreach (var powerUp in powerUps.Where(powerUp => powerUp.powerUp is not Enums.PowerUps.WaffleShield))
        {
            powerUp.uiPowerUp.TurnOffNumericAnimation();
        }
    }
}

[Serializable]
public struct UIPowerUpsData
{
    public Enums.PowerUps powerUp;
    public UIPowerUp uiPowerUp;
}
