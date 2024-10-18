using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonHandlerManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField] private List<UnityEvent> onPointerDownActions;
    [SerializeField] private List<UnityEvent> onPointerUpActions;
    [SerializeField] private List<UnityEvent> onPointerClickActions;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (onPointerDownActions.IsNullOrEmpty()) return;
        
        foreach (var action in onPointerDownActions)
        {
            action.Invoke();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (onPointerUpActions.IsNullOrEmpty()) return;
        
        foreach (var action in onPointerUpActions)
        {
            action.Invoke();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onPointerClickActions.IsNullOrEmpty()) return;
        
        foreach (var action in onPointerClickActions)
        {
            action.Invoke();
        }
    }
}
