using Audio;
using Enums;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
#if UNITY_ANDROID 
    private readonly string appKey = "1de5371fd";
#elif UNITY_IOS
    private readonly string appKey = "1e1cd9b45";
#else
    private readonly string appKey = "";
#endif

    public static AdsManager Instance { get; private set; }

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        IronSource.Agent.validateIntegration();
        IronSource.Agent.init(appKey);
    }

    private void OnEnable()
    {
        IronSourceEvents.onSdkInitializationCompletedEvent += OnSDKInitialized;

        //Add AdInfo Rewarded Video Events
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
    }

    private void OnDisable()
    {
        IronSourceEvents.onSdkInitializationCompletedEvent -= OnSDKInitialized;

        //Add AdInfo Rewarded Video Events
        IronSourceRewardedVideoEvents.onAdOpenedEvent -= RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent -= RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdAvailableEvent -= RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent -= RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent -= RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent -= RewardedVideoOnAdClickedEvent;
    }

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnSDKInitialized()
    {
        Debug.Log("IronSource SDK is initialized");
    }

    private void OnApplicationPause(bool pause)
    {
        IronSource.Agent.onApplicationPause(pause);
    }

    #region rewarded

    public void LoadRewardedAd()
    {
        IronSource.Agent.loadRewardedVideo();
    }

    public void ShowRewardedAd(string placementName = "")
    {
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            if (placementName == "")
            {
                IronSource.Agent.showRewardedVideo();
            }
            else
            {
                IronSource.Agent.showRewardedVideo(placementName);
            }
        }
        else
        {
            Debug.Log("Reward is not loaded");
        }
    }

    /************* RewardedVideo AdInfo Delegates *************/
    // Indicates that there’s an available ad.
    // The adInfo object includes information about the ad that was loaded successfully
    // This replaces the RewardedVideoAvailabilityChangedEvent(true) event
    void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
    {
    }

    // Indicates that no ads are available to be displayed
    // This replaces the RewardedVideoAvailabilityChangedEvent(false) event
    void RewardedVideoOnAdUnavailable()
    {
    }

    // The Rewarded Video ad view has opened. Your activity will loose focus.
    void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
    {
    }

    // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
    void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
    {
    }

    // The user completed to watch the video, and should be rewarded.
    // The placement parameter will include the reward data.
    // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
    void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
        if (placement.getPlacementName() == Constants.AdPlacementShopCoins)
        {
            var coins = placement.getRewardAmount();
            GameManager.Instance.UserData.Coins += coins;
            UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(GameManager.Instance.UserData.Coins);
            PlayerPrefs.SetInt(GameManager.CoinsNameKey, GameManager.Instance.UserData.Coins);
            AudioManager.Instance.mainBackgroundAudioSource.Play();
        }
        else if (placement.getPlacementName() == Constants.AdPlacementRevive)
        {
            UIManager.Instance.GetUIScreen<LostScreenUI>().TurnOffAdvReward();
            GameManager.Instance.player.ReviveAtThePosition();
            UIManager.Instance.GetUIScreen<LostScreenUI>().StartCountDownCoroutine();
            AudioManager.Instance.mainBackgroundAudioSource.Play();
        }
        else if (placement.getPlacementName() == Constants.AdPlacementDoubleCoins)
        {
            UIManager.Instance.MainMenuScreen.DoubleCoinsPopup.ClaimDoubledReward();
            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.ReviveByWatchingAdv))
            {
                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.ReviveByWatchingAdv, null, 1);
            }
        }
        else if (placement.getPlacementName() == Constants.AdPlacementRerollDailyMission)
        {
            UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onAdvRerollClicked?.Invoke();
        }
    }

    // The rewarded video ad was failed to show.
    void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
    {
    }

    // Invoked when the video ad was clicked.
    // This callback is not supported by all networks, and we recommend using it only if
    // it’s supported by all networks you included in your build.
    void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
    }

    #endregion
}