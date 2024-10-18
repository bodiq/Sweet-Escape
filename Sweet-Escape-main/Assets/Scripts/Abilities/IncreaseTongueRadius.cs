using Enums;

namespace Abilities
{
    public class IncreaseTongueRadius : ICharacterAbility
    {
        private Player _player;
        
        public override void InitializeAbility(Player player, int level)
        {
            _player = player;
            switch (level)
            {
                case 3:
                    IncreaseTongueRadiusGrabbing(50);
                    break;
                case 5:
                    IncreaseTongueRadiusGrabbing(100);
                    break;
            }
        }

        private void IncreaseTongueRadiusGrabbing(int addCount)
        {
            _player.TongueGrab.IncreaseRadius(addCount);
        }
    }
}

