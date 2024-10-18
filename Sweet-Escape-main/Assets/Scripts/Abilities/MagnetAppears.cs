using System.Linq;
using Configs;
using Enums;

namespace Abilities
{
    public class MagnetAppears : ICharacterAbility
    {
        public override void InitializeAbility(Player player, int level)
        {
            switch (level)
            {
                case 2:
                case 4:
                    IncreaseMagnetAppearing(10);
                    break;
            }
        }

        private void IncreaseMagnetAppearing(int addCount)
        {
            if (!PowerUpConfig.Instance.PowerUpAppearance.TryGetValue(Characters.Meltie, out var powerUps)) return;
            
            foreach (var power in powerUps.Elements.Where(power => power.value == Enums.PowerUps.Magnet))
            {
                power.chance += addCount / 100f;
            }
            powerUps.Normalize();
        }
    }
}