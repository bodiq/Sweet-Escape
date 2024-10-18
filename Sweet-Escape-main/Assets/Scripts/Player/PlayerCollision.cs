using System;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private Player player;

    public Enemy.IEnemy LastEnteredEnemy;

    private RaycastHit2D _hit2D;
    public GameObject _lastCollision;

    public event Action WallHit;

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        ProcessCollision(collision2D);
    }

    private void OnCollisionStay2D(Collision2D collision2D)
    {
        ProcessCollision(collision2D);
    }

    private void OnCollisionExit2D(Collision2D collision2D)
    {
        if (collision2D.gameObject.CompareTag("Enemy"))
        {
            var enemy = collision2D.gameObject.GetComponent<Enemy.IEnemy>();
            enemy?.OnExit();
        }

        if (collision2D.gameObject == _lastCollision)
        {
            _lastCollision = null;
        }
    }

    private void ProcessCollision(Collision2D collision2D)
    {
        if (collision2D.gameObject.CompareTag("Enemy"))
        {
            var enemy = collision2D.gameObject.GetComponent<Enemy.IEnemy>();
            if (enemy == null)
            {
                enemy = collision2D.gameObject.GetComponentInParent<Enemy.IEnemy>();
            }

            if (_lastCollision != collision2D.gameObject)
            {
                LastEnteredEnemy = enemy;
                enemy.OnEnter(player);
                _lastCollision = collision2D.gameObject;
            }
        }
        else if(!player.PlayerMovement.IsPlayerOnBlock)
        {
            WallHit?.Invoke();
            _lastCollision = collision2D.gameObject;
        }
    }
}