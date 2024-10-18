using Enums;
using UnityEngine;

public class PlayerSlimeCollider : MonoBehaviour
{
    [SerializeField] private DirectionEnum colliderDirection;
    
    private readonly Quaternion _leftColliderRotationEnd = Quaternion.Euler(0f, 0f, -90f);
    private readonly Quaternion _rightColliderRotationEnd = Quaternion.Euler(0f, 0f, 90f);
    private readonly Quaternion _upColliderRotationEnd = Quaternion.Euler(0f, 0f, 180f);
    private readonly Quaternion _downColliderRotationEnd = Quaternion.Euler(0f, 0f, 0f);

    private Quaternion _slimeRotationDueToDirection;

    private void Start()
    {
        switch (colliderDirection)
        {
            case DirectionEnum.Up:
                _slimeRotationDueToDirection = _upColliderRotationEnd;
                break;
            case DirectionEnum.Down:
                _slimeRotationDueToDirection = _downColliderRotationEnd;
                break;
            case DirectionEnum.Left:
                _slimeRotationDueToDirection = _leftColliderRotationEnd;
                break;
            case DirectionEnum.Right:
                _slimeRotationDueToDirection = _rightColliderRotationEnd;
                break;
            case DirectionEnum.None:
            default:
                _slimeRotationDueToDirection = Quaternion.identity;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.CompareTag("Enemy")) return;
        var slimeObject= ManualObjectPool.SharedInstance.GetPooledSlimeTrailObject().GetComponent<SlimeTrail>();

        var wallPosition = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0f);
        
        slimeObject.transform.position = wallPosition;
        slimeObject.transform.rotation = _slimeRotationDueToDirection;
        slimeObject.gameObject.SetActive(true);
    }
}
