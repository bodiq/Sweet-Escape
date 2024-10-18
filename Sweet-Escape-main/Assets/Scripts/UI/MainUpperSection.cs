using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace UI
{
    public class MainUpperSection : UIScreen
    {
        [SerializeField] private TextMeshProUGUI userNameField;
        [SerializeField] private Button avatarButton;
        [SerializeField] private TextMeshProUGUI coinsCountField;
        [SerializeField] private Button moreCoins;
        [SerializeField] private CoinUIAnimation coinUIAnimation;

        private int _lastCountAmount;

        private void OnEnable()
        {
            moreCoins.onClick.AddListener(GoToShop);
            avatarButton.onClick.AddListener(MoveToAccountScreen);
        }

        private void GoToShop()
        {
            UIManager.Instance.MainMenuScreen.OpenShopSection();
        }

        private void MoveToAccountScreen()
        {
            UIManager.Instance.AccountScreenManager.TurnOn();
        }

        private void TestAddCoins()
        {
            var total = _lastCountAmount + 50;
            coinsCountField.text = total.ToString();
            GameManager.Instance.UserData.Coins = total;
            PlayerPrefs.SetInt(GameManager.CoinsNameKey, total);
            _lastCountAmount = total;
        }

        public void SetUsername(string userName)
        {
            userNameField.text = userName;
        }

        public void ChangeCoinsCount(int count)
        {
            coinsCountField.text = count.ToString();
            _lastCountAmount = count;
        }

        private void OnDisable()
        {
            moreCoins.onClick.RemoveListener(GoToShop);
            avatarButton.onClick.RemoveListener(MoveToAccountScreen);
        }
    }
}
