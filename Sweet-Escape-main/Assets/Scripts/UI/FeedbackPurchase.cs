using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackPurchase : MonoBehaviour
{
    private const float AnimationFeedbackDuration = 0.7f;
    private static readonly WaitForSeconds WaitForDestroy = new (AnimationFeedbackDuration);

    private Coroutine _coroutine;
    
    public void StartSelfDestroyCoroutine()
    {
        _coroutine = StartCoroutine(DestroyCoroutine());
    }

    private IEnumerator DestroyCoroutine()
    {
        yield return WaitForDestroy;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }
}
