using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectUIPool : MonoBehaviour
{
    public static ObjectUIPool Instance;

    [SerializeField] private FeedbackPurchase feedbackPopup;
    [SerializeField] private int countToSpawnFeedbackPopups;

    private List<FeedbackPurchase> _pooledFeedbackPopups = new();

    private const float FeedbackPopupDurationAnimation = 0.7f;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } 
    }

    private void Start()
    {
        for (var i = 0; i < countToSpawnFeedbackPopups; i++)
        {
            var spawnedPopup = Instantiate(feedbackPopup, transform);
            spawnedPopup.gameObject.SetActive(false);
            _pooledFeedbackPopups.Add(spawnedPopup);
        }
    }

    public FeedbackPurchase GetPooledFeedbackPopup()
    {
        foreach (var popup in _pooledFeedbackPopups.Where(popup => !popup.gameObject.activeSelf))
        {
            return popup;
        }

        return null;
    }
}
