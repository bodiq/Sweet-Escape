using System;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Configs;
using Enums;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using AudioType = Audio.AudioType;
using Random = UnityEngine.Random;

public class DailyBoxManager : MonoBehaviour
{
    [SerializeField] private List<DailyMissionBox> dailyMissionBoxes = new();

    [SerializeField] private Image mainBoxBackground;
    [SerializeField] private TextMeshProUGUI dailyMissionsDescription;
    [SerializeField] private TextMeshProUGUI timeRemaining;
    [SerializeField] private TextMeshProUGUI countRemaining;
    [SerializeField] private TextMeshProUGUI dailyFactTittle;
    [SerializeField] private TextMeshProUGUI dailyFactDescription;

    [SerializeField] private GameObject completedAllTasksBox;
    [SerializeField] private Button darkBackground;
    [SerializeField] private GameObject dailyMissionsNewBoxParent;
    [SerializeField] private GameObject dailyMissionsDefaultBoxParent;

    [SerializeField] private Image topBoxImage;

    public UnityEvent<DailyMissionBox> onSelectedDailyTask;
    public UnityEvent onAdvRerollClicked;

    private int _gameObjectIndex;
    private DailyMissionBox _lastSelectedMissionBox;

    public UnityEvent<DailyMissions, GameObject, int> onDailyMissionsRefresh;

    public static int AvailableDailyMissions = 3;

    public static readonly List<string> SavedDailyTasksKeys = new();
    public static readonly List<DailyMissions> AvailableDailyMissionsTypes = new();

    private const string RefreshMissionsTimeKey = "RefreshMissionsTimeKey";

    private void Start()
    {
        InvokeRepeating(nameof(RefreshTimeRemaining), 2f, 60f);
    }

    private void OnEnable()
    {
        onSelectedDailyTask.AddListener(ChangeDailyBoxToSelectedDailyTask);
        onAdvRerollClicked.AddListener(RerollDailyMission);

        if (AvailableDailyMissions == 0)
        {
            completedAllTasksBox.SetActive(true);
        }

        if (!PlayerPrefs.HasKey(RefreshMissionsTimeKey))
        {
            PlayerPrefs.SetString(RefreshMissionsTimeKey, DateTime.Now.AddHours(24).ToString());
            AvailableDailyMissions = 3;
        }
        else
        {
            RefreshTimeRemaining();
        }

        RefreshAllDailyMissions();
    }
    
    private void RefreshTimeRemaining()
    {
        var unlockTime = DateTime.Parse(PlayerPrefs.GetString(RefreshMissionsTimeKey));
        
        if (unlockTime < DateTime.Now)
        {
            PlayerPrefs.SetString(RefreshMissionsTimeKey, DateTime.Now.AddHours(24).ToString());

            var newUnlockTime = DateTime.Parse(PlayerPrefs.GetString(RefreshMissionsTimeKey));
            var dateTime = newUnlockTime - DateTime.Now;
            var hours = dateTime.Hours + "H";
            var minutes = dateTime.Minutes + "M";
            countRemaining.text = hours + " " + minutes;
            
            PlayerPrefs.DeleteKey(GameManager.DailyMissionsUsedFreeRerollKey);
            
            AvailableDailyMissionsTypes.Clear();
            AvailableDailyMissions = 3;

            foreach (var key in SavedDailyTasksKeys)
            {
                PlayerPrefs.DeleteKey(key);
            }
            
            SavedDailyTasksKeys.Clear();

            RefreshAllDailyMissions();
        }
        else
        {
            var dateTime = unlockTime - DateTime.Now;
            var hours = dateTime.Hours + "H";
            var minutes = dateTime.Minutes + "M";
            countRemaining.text = hours + " " + minutes;
        }
    }

    private void RefreshAllDailyMissions()
    {
        var randomFactIndex = Random.Range(0, DailyMissionsConfig.Instance.DailyFacts.Count);
        dailyFactDescription.text = DailyMissionsConfig.Instance.DailyFacts[randomFactIndex];
        
        if (SavedDailyTasksKeys.IsNullOrEmpty())
        {
            if (AvailableDailyMissions <= 0) return;
            
            AvailableDailyMissionsTypes.Clear();
            SavedDailyTasksKeys.Clear();

            for (var i = 0; i < AvailableDailyMissions; i++)
            {
                foreach (var dailyMissionBox in dailyMissionBoxes.Where(dailyMissionBox => dailyMissionBox.data.countToDo == 0))
                {
                    foreach (var data in DailyMissionsConfig.Instance.DailyMissionsData)
                    {
                        var randomIndex = Random.Range(0, DailyMissionsConfig.Instance.DailyMissionsData.Count);
                        var value = DailyMissionsConfig.Instance.DailyMissionsData[randomIndex];
                        var key = value.dailyMissions.ToString() + value.countToDo;

                        if (SavedDailyTasksKeys.Contains(key))
                        {
                            continue;
                        }
                        PlayerPrefs.SetInt(key, 0);
                        const int currentValue = 0;

                        PlayerPrefs.SetInt(key, 0);
                        SavedDailyTasksKeys.Add(key);
                        AvailableDailyMissionsTypes.Add(value.dailyMissions);
                        dailyMissionBox.Initialize(currentValue, value, i == 2);
                        dailyMissionBox.gameObject.SetActive(true);
                        break;
                    }
                    break;
                }
            }
        }
        else
        {
            foreach (var t in SavedDailyTasksKeys)
            {
                foreach (var dailyMissionBox in dailyMissionBoxes.Where(dailyMissionBox => dailyMissionBox.data.countToDo == 0))
                {
                    foreach (var data in DailyMissionsConfig.Instance.DailyMissionsData)
                    {
                        var key = data.dailyMissions.ToString() + data.countToDo;

                        if (t != key) continue;

                        AvailableDailyMissionsTypes.Add(data.dailyMissions);
                        var currentValue = PlayerPrefs.GetInt(key);
                        dailyMissionBox.Initialize(currentValue, data, t == SavedDailyTasksKeys[^1]);
                        dailyMissionBox.gameObject.SetActive(true);
                        break;
                    }

                    break;
                }
            }
        }
    }

    private void RerollDailyMission()
    {
        foreach (var data in DailyMissionsConfig.Instance.DailyMissionsData)
        {
            var randomIndex = Random.Range(0, DailyMissionsConfig.Instance.DailyMissionsData.Count);
            var value = DailyMissionsConfig.Instance.DailyMissionsData[randomIndex];
            var newKey = value.dailyMissions.ToString() + value.countToDo;

            if (SavedDailyTasksKeys.Contains(newKey))
            {
                continue;
            }

            var lastObject = _lastSelectedMissionBox._isLastObject;
            
            _lastSelectedMissionBox.DeleteData();
            PlayerPrefs.SetInt(newKey, 0);
            SavedDailyTasksKeys.Add(newKey);
            _lastSelectedMissionBox.Initialize(0, value, lastObject);
            break;
        }

        ChangeDailyBoxToDefaultDailyTask();
    }

    private void ChangeDailyBoxToSelectedDailyTask(DailyMissionBox dailyMissionBox)
    {
        AudioManager.Instance.PlaySFX(AudioType.Popup);
        mainBoxBackground.sprite = DailyMissionsConfig.Instance.SelectedDailyMissionsBoxSprite;
        dailyMissionsDescription.enabled = false;
        timeRemaining.color = DailyMissionsConfig.Instance.WhiteColor;
        countRemaining.color = DailyMissionsConfig.Instance.BrownColor;
        dailyFactTittle.color = DailyMissionsConfig.Instance.BrownColor;
        dailyFactDescription.color = DailyMissionsConfig.Instance.BrownColor;
        topBoxImage.enabled = true;

        _lastSelectedMissionBox = dailyMissionBox;
        _gameObjectIndex = _lastSelectedMissionBox.gameObject.transform.GetSiblingIndex();
        _lastSelectedMissionBox.transform.SetParent(dailyMissionsNewBoxParent.transform);
        darkBackground.gameObject.SetActive(true);
        darkBackground.onClick.AddListener(CloseSelectedDailyMission);
    }

    public void ChangeDailyBoxToDefaultDailyTask()
    {
        mainBoxBackground.sprite = DailyMissionsConfig.Instance.DefaultDailyMissionsBoxSprite;
        dailyMissionsDescription.enabled = true;
        timeRemaining.color = DailyMissionsConfig.Instance.WaffleColor;
        countRemaining.color = DailyMissionsConfig.Instance.WhiteColor;
        dailyFactTittle.color = DailyMissionsConfig.Instance.WaffleColor;
        dailyFactDescription.color = DailyMissionsConfig.Instance.WaffleColor;
        topBoxImage.enabled = false;

        _lastSelectedMissionBox.transform.SetParent(dailyMissionsDefaultBoxParent.transform);
        _lastSelectedMissionBox.transform.SetSiblingIndex(_gameObjectIndex);
        _lastSelectedMissionBox.ChangeToUncompletedMission();
        darkBackground.onClick.RemoveListener(CloseSelectedDailyMission);
        darkBackground.gameObject.SetActive(false);
        _lastSelectedMissionBox = null;
    }

    private void CloseSelectedDailyMission()
    {
        _lastSelectedMissionBox.ChangeToUncompletedMission();
        ChangeDailyBoxToDefaultDailyTask();
    }

    private void OnDisable()
    {
        onSelectedDailyTask.RemoveListener(ChangeDailyBoxToSelectedDailyTask);
        onAdvRerollClicked.RemoveListener(RerollDailyMission);
    }
}

[Serializable]
public struct DailyMissionData
{
    public DailyMissions dailyMissions;
    public int rewardCount;
    public DailyMissionsRewardType rewardType;
    public Sprite rewardIcon;
    public int countToDo;
    public string dailyMissionDescription;
}