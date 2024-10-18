using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Configs;
using DG.Tweening;
using Enums;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AudioType = Audio.AudioType;

namespace Managers
{
    public class DailyRewardManager : UIScreen, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private List<DailyRewardUI> dailyRewards;
        [SerializeField] private Image popupRewardIcon;
        [SerializeField] private TextMeshProUGUI popupRewardText;
        [SerializeField] private GameObject rewardPopup;
        [SerializeField] private GameObject iceCreamGameObject;
        [SerializeField] private Image rewardPopupBackground;
        [SerializeField] private Button claimRewardButton;

        [SerializeField] private Transform dailyLoginTittle;
        [SerializeField] private Transform tapToLickText;
        [SerializeField] private GameObject feedbackPopup;
 
        private int _day = 1;
        private int _lastClaimedIndex = 0;

        public const string UnlockTimeKey = "UnlockTimeKey";
        public const string IndexForClaiming = "LastIndexClaimed";

        private const string DayKey = "DayKey";

        private Vector3 _initialIceCreamPos;
        private Vector3 _initialRewardPopupPos;
        private Vector3 _initialTapToLickPos;

        private const float EndRewardPopupAlpha = 0.4f;
        private const float HalfSecondDuration = 0.5f;
        private const float SecondDuration = 1f;
        private const float QuarterSecondDuration = 0.25f;
        
        private readonly Vector3 _endIceCreamScale = new (0.95f, 0.95f, 0.95f);

        private readonly WaitForSeconds _waitForClosingDoor = new (HalfSecondDuration);
        private readonly WaitForSeconds _waitForOpeningDoor = new(SecondDuration);
        private readonly WaitForSeconds _waitForDailyLoginShowStarts = new(HalfSecondDuration);
        private readonly WaitForSeconds _waitForDailyLoginTittleAppears = new(QuarterSecondDuration);

        private Tween _iceCreamObjectTween;
        private Tween _dailyLoginTittleTween;
        private Tween _tapToLickTween;
        private Tween _rewardBackgroundTween;
        private Tween _rewardPopupTween;
        private Tween _iceCreamScaleTween;
    
        private void OnEnable()
        {
            claimRewardButton.onClick.AddListener(GetDailyReward);
        }

        private void GetDailyReward()
        {
            dailyRewards[_lastClaimedIndex].ClaimReward();
            claimRewardButton.onClick.RemoveListener(GetDailyReward);

            StartCoroutine(MoveToGameModeSelectionSection());
        }

        private IEnumerator MoveToGameModeSelectionSection()
        {
            yield return _waitForClosingDoor;
            
            UIManager.Instance.DoorTransition.TurnOn();
            UIManager.Instance.DoorTransition.CloseDoor();
            
            yield return _waitForOpeningDoor;
        
            UIManager.Instance.DoorTransition.OpenDoor();
            UIManager.Instance.MainMenuScreen.TurnOnGameModeSelection();
        }

        public void OnFeedbackPopupCall()
        {
            feedbackPopup.SetActive(true);
        }
    
        public void StartUIShow()
        {
            StartCoroutine(DailyLoginUIShow());
        }
        
        private IEnumerator DailyLoginUIShow()
        {
            yield return _waitForDailyLoginShowStarts;

            _iceCreamObjectTween = iceCreamGameObject.transform.DOLocalMove(_initialIceCreamPos, 0.75f).SetEase(Ease.OutBack);
            AudioManager.Instance.PlaySFX(AudioType.DailyLoginEnter);

            yield return _waitForDailyLoginTittleAppears;

            _dailyLoginTittleTween = dailyLoginTittle.DOScale(Vector3.one, 1f).OnComplete((() =>
            {
                tapToLickText.gameObject.SetActive(true);
                _tapToLickTween = tapToLickText.DOLocalMove(_initialTapToLickPos, 0.25f);
            }));
        }

        private void OpenRewardPopup()
        {
            var dailyRewardInfo = DailyRewardConfig.Instance.dailyRewards[_lastClaimedIndex];

            popupRewardIcon.sprite = dailyRewardInfo.dailyRewards[0].rewardIconOnPopup;
            popupRewardText.text = dailyRewardInfo.dailyRewards[0].rewardTextOnPopup;
        
            _rewardBackgroundTween = rewardPopupBackground.DOFade(EndRewardPopupAlpha, 1f);
            _rewardPopupTween = rewardPopup.transform.DOLocalMove(_initialRewardPopupPos, 0.75f).SetEase(Ease.OutBack);
        }

        private void Start()
        {
            var rect = UIManager.Instance.RectTransform.rect;
        
            _initialIceCreamPos = iceCreamGameObject.transform.localPosition;
            _initialRewardPopupPos = rewardPopup.transform.localPosition;
            _initialTapToLickPos = tapToLickText.localPosition;
        
            dailyLoginTittle.localScale = Vector3.zero;
            tapToLickText.gameObject.SetActive(false);
        
            tapToLickText.localPosition = new Vector3(_initialTapToLickPos.x, 0f, _initialTapToLickPos.z);
            iceCreamGameObject.transform.localPosition = new Vector3(_initialIceCreamPos.x, _initialIceCreamPos.y - rect.height, _initialIceCreamPos.z);
            rewardPopup.transform.localPosition = new Vector3(_initialRewardPopupPos.x, _initialRewardPopupPos.y - rect.height, _initialRewardPopupPos.z);
        
            if (PlayerPrefs.HasKey(IndexForClaiming))
            {
                var lastIndexClaiming = PlayerPrefs.GetInt(IndexForClaiming);

                if (lastIndexClaiming >= dailyRewards.Count)
                {
                    PlayerPrefs.DeleteKey(UnlockTimeKey);
                    PlayerPrefs.DeleteKey(IndexForClaiming);
                    PlayerPrefs.DeleteKey(DayKey);
                }
            }
        
            if (!PlayerPrefs.HasKey(DayKey))
            {
                _day = 1;
                PlayerPrefs.SetInt(DayKey, _day);
            }
            else
            {
                _day = PlayerPrefs.GetInt(DayKey);
            }

            if (PlayerPrefs.HasKey(IndexForClaiming))
            {
                _lastClaimedIndex = PlayerPrefs.GetInt(IndexForClaiming);
            }

            if (PlayerPrefs.HasKey(UnlockTimeKey))
            {
                var unlockTime = DateTime.Parse(PlayerPrefs.GetString(UnlockTimeKey));
                if (unlockTime < DateTime.Now)
                {
                    _day++;
                    PlayerPrefs.DeleteKey(UnlockTimeKey);
                }
            }
        
            var dailyRewardConfig = DailyRewardConfig.Instance.dailyRewards;
            var index = _day - 1;

            for (var i = 0; i < dailyRewardConfig.Count; i++)
            {
                var state = DailyRewardState.Claimed;
                if (index < i)
                {
                    state = DailyRewardState.NotClaimableYet;
                }
                else if (_lastClaimedIndex == i && index == _lastClaimedIndex)
                {
                    state = DailyRewardState.Claimable;
                    PlayerPrefs.SetInt(GameManager.ClaimableRewardKey, 1);
                }
                else if(index > i)
                {
                    state = DailyRewardState.Claimed;
                }
            
                dailyRewards[i].Initialize(dailyRewardConfig[i].dailyRewards, state);
            }
        
            PlayerPrefs.SetInt(DayKey, _day);
        }

        private void OnDisable()
        {
            PlayerPrefs.SetInt(GameManager.ClaimableRewardKey, 0);
            claimRewardButton.onClick.RemoveListener(GetDailyReward);
            _iceCreamObjectTween.Kill();
            _dailyLoginTittleTween.Kill();
            _tapToLickTween.Kill();
            _rewardBackgroundTween.Kill();
            _rewardPopupTween.Kill();
             _iceCreamScaleTween.Kill();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _iceCreamScaleTween = iceCreamGameObject.transform.DOScale(_endIceCreamScale, 0.1f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _iceCreamScaleTween = iceCreamGameObject.transform.DOScale(Vector3.one, 0.1f);
            AudioManager.Instance.PlaySFX(AudioType.IceCreamLick);
            OpenRewardPopup();
        }
    }
}
