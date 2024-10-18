using System;
using UnityEngine;

public class BarrelAttackZone : MonoBehaviour, ITrigger
{
    public event Action PlayerCollided;

    public Transform Transform => transform;

    public void Trigger(Player player)
    {
        PlayerCollided?.Invoke();
    }
}