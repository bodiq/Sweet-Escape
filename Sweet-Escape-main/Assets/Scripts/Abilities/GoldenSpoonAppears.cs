using System.Linq;
using Configs;
using Enums;

namespace Abilities
{
    public class GoldenSpoonAppears : ICharacterAbility
    {
        public override void InitializeAbility(Player player, int level)
        {
            switch (level)
            {
                case 2:
                    IncreaseGoldenSpoonAppearing(15);
                    break;
                case 4:
                    IncreaseGoldenSpoonAppearing(10);
                    break;
            }
        }

        private void IncreaseGoldenSpoonAppearing(int addCount)
        {
            if (!PowerUpConfig.Instance.PowerUpAppearance.TryGetValue(Characters.Kermit, out var powerUps)) return;
            
            foreach (var power in powerUps.Elements.Where(power => power.value == Enums.PowerUps.GoldSpoon))
            {
                power.chance += addCount / 100f;
            }
            powerUps.Normalize();
        }
    }
}