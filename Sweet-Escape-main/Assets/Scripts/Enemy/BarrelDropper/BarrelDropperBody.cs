using System;
using UnityEngine;

public class BarrelDropperBody : MonoBehaviour
{
    public event Action BecameInvisible;
    public event Action DropAnimationFinished;
    public event Action FlyAwayAnimationStarted;

    private void OnBecameInvisible()
    {
        BecameInvisible?.Invoke();
    }

    public void OnDropped()
    {
        DropAnimationFinished?.Invoke();
    }

    public void OnFlyAway()
    {
        FlyAwayAnimationStarted?.Invoke();
    }
}