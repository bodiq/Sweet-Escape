using System;
using System.Collections.Generic;
using System.Security.Claims;
using Audio;
using Enums;
using Managers;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AudioType = UnityEngine.AudioType;

namespace UI
{
    public class DailyRewardUI : MonoBehaviour
    {
        [SerializeField] private Image rewardSpriteRenderer;
        
        private List<DailyRewardData> _dailyRewardLists = new();

        private readonly Color _disabledColor = new(1f, 1f, 1f, 0.5f);
        private readonly Color _enabledColor = new(1f, 1f, 1f, 1f);

        public void Initialize(List<DailyRewardData> dailyRewardLists, DailyRewardState dailyRewardState)
        {
            _dailyRewardLists = dailyRewardLists;

            if (_dailyRewardLists.Count == 0) return;
            
            var reward = _dailyRewardLists[0];
            rewardSpriteRenderer.sprite = reward.rewardIconOnIceCream;
            switch (dailyRewardState)
            {
                case DailyRewardState.Claimed:
                    RewardClaimed();
                    break;
                case DailyRewardState.Claimable:
                    RewardUnClaimed();
                    break;
                case DailyRewardState.NotClaimableYet:
                    RewardUnClaimed();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dailyRewardState), dailyRewardState, null);
            }
        }

        public void ClaimReward()
        {
            PlayerPrefs.SetString(DailyRewardManager.UnlockTimeKey, DateTime.Now.AddHours(24).ToString());
            
            var index = gameObject.transform.GetSiblingIndex();
            
            if (index == 7)
            {
                if(AchievementBoxManager.AvailableAchievementsTypes.Contains(AchievementsTypes.UnlockAllDailyRewards))
                {
                    UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.onAchievementRefresh?.Invoke(AchievementsTypes.UnlockAllDailyRewards, 1);
                }
            }
            
            index++;
            PlayerPrefs.SetInt(DailyRewardManager.IndexForClaiming, index);
            

            foreach (var dailyRewardList in _dailyRewardLists)
            {
                switch (dailyRewardList.dailyRewardType)
                {
                    case DailyReward.Coins:
                        CoinReward(dailyRewardList.count);
                        break;
                    case DailyReward.Bubble:
                        BubbleReward(dailyRewardList.count);
                        break;
                    case DailyReward.Surprise:
                        SurpriseReward();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void CoinReward(int count)
        {
            GameManager.Instance.UserData.Coins += count;
            UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(GameManager.Instance.UserData.Coins);
            PlayerPrefs.SetInt(GameManager.CoinsNameKey, GameManager.Instance.UserData.Coins);
            AudioManager.Instance.PlaySFX(Audio.AudioType.CollectCoinUI);
            RewardClaimed();
        }

        private void BubbleReward(int count)
        {
            var countAdditionalShields = PlayerPrefs.GetInt(Enums.PowerUps.WaffleShield.ToString());

            var newCount = countAdditionalShields + count;
            
            PlayerPrefs.SetInt(Enums.PowerUps.WaffleShield.ToString(), newCount);
            
            RewardClaimed();
        }

        private void SurpriseReward()
        {
            if (!PlayerPrefs.HasKey(SkinEnum.AntiNoob.ToString()))
            {
                PlayerPrefs.SetInt(SkinEnum.AntiNoob.ToString(), 1);
            }
        }

        private void RewardClaimed()
        {
            rewardSpriteRenderer.color = _disabledColor;
        }

        private void RewardUnClaimed()
        {
            rewardSpriteRenderer.color = _enabledColor;
        }
    }
}