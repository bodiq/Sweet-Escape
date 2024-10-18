using System;

using UnityEngine;

public class TongueGrab : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Player player;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CircleCollider2D collider2D;

    private const string CoinTag = "Coin";
    private bool _readyToGrab = true;
    
    private static readonly int Grab = Animator.StringToHash(TongueGrabAnimationState);

    private const string TongueGrabAnimationState = "TongueGrab";

    private Coin _coin;

    private float _defaultRadius = 0f;

    private void Awake()
    {
        _defaultRadius = collider2D.radius;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(CoinTag) && _readyToGrab && !GameManager.Instance.player.magnetSprinkles && _coin?.gameObject != other.gameObject)
        {
            _readyToGrab = false;
            animator.SetTrigger(Grab);
            _coin = other.gameObject.GetComponent<Coin>();
        }

        if (other.CompareTag(CoinTag) && _coin != null)
        {
            var distance = Vector3.Distance(other.gameObject.transform.position, transform.position);
            var size = new Vector2(distance, spriteRenderer.size.y);
            spriteRenderer.size = size;

            var offset = Quaternion.Euler(0, -90, 0);
            var forward = other.transform.position - transform.position;
            var rotation = Quaternion.LookRotation(forward);
            transform.rotation = rotation * offset;
        }
    }

    public void GrabCoin()
    {
        _coin?.GrabCoinWithTongue(player);
    }

    public void ReadyToGrab()
    {
        _readyToGrab = true;
        _coin = null;
    }

    public void IncreaseRadius(int increasedRadius)
    {
        var radius = _defaultRadius;
        var newRadius = radius + radius / 100 * increasedRadius;

        collider2D.radius = newRadius;
    }
}
