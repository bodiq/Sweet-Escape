
namespace Abilities
{
    public class Bubble : ICharacterAbility
    {
        private Player _player;
        public override void InitializeAbility(Player player, int level)
        {
            _player = player;
            switch (level)
            {
                case 2:
                    GiveShield(1);
                    break;
                case 4:
                    GiveShield(1);
                    break;
            }
        }

        private void GiveShield(int count)
        {
            _player.SetShields(count);
        }
    }
}