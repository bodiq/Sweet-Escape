using Enums;
using UnityEngine;

namespace Enemy
{
    public interface IEnemy
    {
        public void OnEnter(Player player);
        public void OnExit();
        public GameObject GameObject { get; }
        public void ChangeMovement();
        public void Freeze();
        public void UnFreeze();
        public DirectionEnum GetEnemyDirection();
        public void TurnCollider(bool isActive);
        public EnemyType GetEnemyType();
    }
}
