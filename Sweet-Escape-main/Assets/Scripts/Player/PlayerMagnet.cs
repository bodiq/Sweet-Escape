using Items;
using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
    [SerializeField] private Player player;

    private bool _isCoinAllowedOnMagnet = false;
    private bool _isMagnetTurn = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Coin"))
        {
            if (_isMagnetTurn)
            {
                if (!_isCoinAllowedOnMagnet) return;
                var coin = col.gameObject.GetComponent<Coin>();
                
                coin?.Trigger(player);
            }
            else
            {
                var coin = col.gameObject.GetComponent<Coin>();
                coin?.Trigger(player);
            }
        }
        else if(col.CompareTag("Sprinkle"))
        {
            col.gameObject.GetComponent<Sprinkle>().OnEnter(player);
        }
    }

    public void MagnetTurn(bool isActive, bool isCoinsAllowed = false)
    {
        if (isActive)
        {
            _isCoinAllowedOnMagnet = isCoinsAllowed;
            transform.localScale = Vector3.one * 35; 
        }
        else
        {
            transform.localScale = Vector3.one;
        }
        
        _isMagnetTurn = isActive;
        _isCoinAllowedOnMagnet = isCoinsAllowed;
    }
}
