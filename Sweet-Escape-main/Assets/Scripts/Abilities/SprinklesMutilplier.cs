using Enums;

namespace Abilities
{
    public class SprinklesMultiplier : ICharacterAbility
    {
        private Player _player;
        public override void InitializeAbility(Player player, int level)
        {
            _player = player;

            switch (level)
            {
                case 3:
                    GiveSprinkleMultiplier(1.5f);
                    break;
                case 5:
                    GiveSprinkleMultiplier(2f);
                    break;
            }
        }

        private void GiveSprinkleMultiplier(float multiplier)
        {
            _player.multiplierSprinkle = multiplier;
        }
    }
}