namespace Abilities
{
    public class CoinGrabbingTongue : ICharacterAbility
    {
        private Player _player;
        
        public override void InitializeAbility(Player player, int level)
        {
            _player = player;
            switch (level)
            {
                case 1:
                    _player?.TongueGrab.gameObject.SetActive(true);
                    break;
            }
        }
    }
}