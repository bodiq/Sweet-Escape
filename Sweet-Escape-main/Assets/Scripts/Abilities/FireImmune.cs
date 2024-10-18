using Enums;

namespace Abilities
{
    public class FireImmune : ICharacterAbility
    {
        private Player _player;
        public override void InitializeAbility(Player player, int level)
        {
            _player = player;

            switch (level)
            {
                case 1:
                    _player.SetFireImmune(1);
                    break;
                case 3:
                    _player.SetFireImmune(3);
                    break;
                case 5:
                    _player.SetFireImmune(0, true);
                    break;
            }
        }
        
    }
}