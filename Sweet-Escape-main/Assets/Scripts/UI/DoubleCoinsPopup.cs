using Audio;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DoubleCoinsPopup : UIScreen
{
    [SerializeField] private TextMeshProUGUI coinEarnedCountText;
    [SerializeField] private Button doubleUpButton;
    [SerializeField] private Button claimButton;

    private int _coinsEarned;

    private Tween scalingTween;

    private const float DurationPopupAppears = 0.75f;
    
    private void OnEnable()
    {
        doubleUpButton.onClick.AddListener(WatchAdvDoublePoints);
        claimButton.onClick.AddListener(ClaimReward);

        coinEarnedCountText.text = GameManager.Instance.player.coinsPerGame.ToString();

        scalingTween = transform.DOScale(Vector3.one, DurationPopupAppears).SetEase(Ease.OutBack).OnComplete(() =>
        {
            AudioManager.Instance.PlaySFX(Audio.AudioType.Popup);
        });
    }
    
    private void ClaimReward()
    {
        var newCount = GameManager.Instance.UserData.Coins + GameManager.Instance.player.coinsPerGame;

        GameManager.Instance.player.coinsPerGame = 0;
        GameManager.Instance.UserData.Coins = newCount;
        PlayerPrefs.SetInt(GameManager.CoinsNameKey, newCount);
        UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(newCount);
        
        var pooledFeedbackPopup = ObjectUIPool.Instance.GetPooledFeedbackPopup();
        if (pooledFeedbackPopup)
        {
            pooledFeedbackPopup.transform.position = claimButton.transform.position;
            pooledFeedbackPopup.gameObject.SetActive(true);
            pooledFeedbackPopup.StartSelfDestroyCoroutine();
        }
        
        AudioManager.Instance.PlaySFX(Audio.AudioType.CollectCoinUI);
        
        TurnOff();
    }

    public void ClaimDoubledReward()
    {
        var newCount = GameManager.Instance.UserData.Coins + (GameManager.Instance.player.coinsPerGame * 2);
        GameManager.Instance.player.coinsPerGame = 0;
        GameManager.Instance.UserData.Coins = newCount;
        PlayerPrefs.SetInt(GameManager.CoinsNameKey, newCount);
        UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(newCount);
        
        var pooledFeedbackPopup = ObjectUIPool.Instance.GetPooledFeedbackPopup();
        if (pooledFeedbackPopup)
        {
            pooledFeedbackPopup.transform.position = doubleUpButton.transform.position;
            pooledFeedbackPopup.gameObject.SetActive(true);
            pooledFeedbackPopup.StartSelfDestroyCoroutine();
        }
        
        AudioManager.Instance.PlaySFX(Audio.AudioType.CollectCoinUI);
        
        TurnOff();
    }

    private void WatchAdvDoublePoints()
    {
        if (!GameManager.Instance.isRemovedAdv)
        {
            AdsManager.Instance.ShowRewardedAd(Constants.AdPlacementDoubleCoins);
        }
    }

    private void OnDisable()
    {
        doubleUpButton.onClick.RemoveListener(WatchAdvDoublePoints);
        claimButton.onClick.RemoveListener(ClaimReward);
        
        scalingTween?.Kill();
        transform.localScale = Vector3.zero;
    }
}
