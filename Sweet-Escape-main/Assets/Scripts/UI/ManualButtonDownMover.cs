using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ManualButtonDownMover : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private List<GameObject> objectsToMove;
    [SerializeField] private Button button;
    [SerializeField] private float negativeValueToMove = 7f;
    
    private readonly List<Vector3> _startPositions = new();
    private readonly List<Vector3> _endPositions = new();
    
    private void Awake()
    {
        if (objectsToMove.IsNullOrEmpty()) return;
        
        for (var i = 0; i < objectsToMove.Count; i++)
        {
            var pos = objectsToMove[i].transform.localPosition;

            _startPositions.Add(pos);
            _endPositions.Add(new Vector3(pos.x, pos.y - negativeValueToMove, pos.z));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (objectsToMove.IsNullOrEmpty()) return;
        
        
        for (var i = 0; i < objectsToMove.Count; i++)
        {
            objectsToMove[i].transform.localPosition = _endPositions[i];
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (objectsToMove.IsNullOrEmpty()) return;
        
        for (var i = 0; i < objectsToMove.Count; i++)
        {
            objectsToMove[i].transform.localPosition = _startPositions[i];
        }
    }
}
