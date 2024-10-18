using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using API;
using Configs;
using UnityEngine;
using UnityEngine.UI;

public class RankingBoxManager : MonoBehaviour
{
    [SerializeField] private LeaderboardPlate leaderboardPlateToSpawn;
    
    [SerializeField] private LeaderboardPlate userHighScoreLeaderboardPlate;
    [SerializeField] private LeaderboardPlate userTotalScoreLeaderboardPlate;
    
    [SerializeField] private Button highScoreButton;
    [SerializeField] private Button totalScoreButton;

    [SerializeField] private GameObject highScoreLeaderboardBox;
    [SerializeField] private GameObject totalScoreLeaderboardBox;

    [SerializeField] private Transform highScoreContentParent;
    [SerializeField] private Transform totalScoreContentParent;

    [SerializeField] private GameObject mainLeaderboardsBox;
    [SerializeField] private GameObject signInToAccessLeaderboard;
    [SerializeField] private Button signInButton;

    [SerializeField] private Sprite pressedButtonSprite;
    [SerializeField] private Sprite releasedButtonSprite;

    [SerializeField] private GameObject highScoreText;
    [SerializeField] private GameObject totalScoreText;

    private readonly List<LeaderboardPlate> _highScoreLeaderboardPlates = new();
    private readonly List<LeaderboardPlate> _totalScoreLeaderboardPlates = new();

    private const int MaxLeaderboardPlatesCount = 100;

    private Image _highScoreLeaderboardImage;
    private Image _totalScoreLeaderboardImage;

    private Vector3 _initialHighScoreTextPos;
    private Vector3 _initialTotalScoreTextPos;

    private Vector3 _pressedHighScoreTextPos;
    private Vector3 _pressedTotalScoreTextPos;

    private void Start()
    {
        if (PlayerPrefs.HasKey(GameManager.AccessTokenAPIUserKey))
        {
            mainLeaderboardsBox.SetActive(true);
            signInToAccessLeaderboard.SetActive(false);
            CreateLeaderBoards();
        }
        else
        {
            mainLeaderboardsBox.SetActive(false);
            signInToAccessLeaderboard.SetActive(true);
        }

        _highScoreLeaderboardImage = highScoreButton.GetComponent<Image>();
        _totalScoreLeaderboardImage = totalScoreButton.GetComponent<Image>();

        _initialHighScoreTextPos = highScoreText.transform.localPosition;
        _initialTotalScoreTextPos = totalScoreText.transform.localPosition;

        _pressedHighScoreTextPos = new Vector3(_initialHighScoreTextPos.x, _initialHighScoreTextPos.y - 7f, _initialHighScoreTextPos.z);
        _pressedTotalScoreTextPos = new Vector3(_initialTotalScoreTextPos.x, _initialTotalScoreTextPos.y - 7f, _initialTotalScoreTextPos.z);

        TurnOnHighScoreLeaderboard();

        GameManager.Instance.StartLeaderboardRefreshTimer();
    }

    private void OnEnable()
    {
        highScoreButton.onClick.AddListener(TurnOnHighScoreLeaderboard);
        totalScoreButton.onClick.AddListener(TurnOnTotalScoreLeaderboard);
        
        if (PlayerPrefs.HasKey(GameManager.AccessTokenAPIUserKey))
        {
            mainLeaderboardsBox.SetActive(true);
            signInToAccessLeaderboard.SetActive(false);
        }
        else
        {
            mainLeaderboardsBox.SetActive(false);
            signInToAccessLeaderboard.SetActive(true);
        }
        
        signInButton.onClick.AddListener(GoToAccountLogin);

        if (GameManager.Instance.canBeRefreshLeaderboard)
        {
            StartCoroutine(RefreshLeaderboardsData());
        }
    }

    private IEnumerator RefreshLeaderboardsData()
    {
        GameManager.Instance.canBeRefreshLeaderboard = false;
        
        UIManager.Instance.LoadingBackground.TurnOn();

        yield return StartCoroutine(APIManager.Instance.GetUserInGameInfo());

        UIManager.Instance.LoadingBackground.TurnOff();
        
        FillLeaderboards();
    }

    private void FillLeaderboards()
    {
        Sprite leaderboardSprite = null;
        
        for (var i = 0; i < APIManager.Instance.leaderboardData.high_score_leaderboard.Count; i++)
        {
            var leaderboardData = APIManager.Instance.leaderboardData.high_score_leaderboard[i];
            
            leaderboardSprite = leaderboardData.position switch
            {
                1 => LeaderboardConfig.Instance.GoldLeaderboardPlate,
                2 => LeaderboardConfig.Instance.SilverLeaderboardPlate,
                _ => LeaderboardConfig.Instance.DefaultLeaderboardPlate
            };
            
            _highScoreLeaderboardPlates[i].SetData(leaderboardSprite, leaderboardData.position, leaderboardData.high_score, leaderboardData.username);
        }
        
        for (var i = 0; i < APIManager.Instance.leaderboardData.total_score_leaderboard.Count; i++)
        {
            var leaderboardData = APIManager.Instance.leaderboardData.total_score_leaderboard[i];
            
            leaderboardSprite = leaderboardData.position switch
            {
                1 => LeaderboardConfig.Instance.GoldLeaderboardPlate,
                2 => LeaderboardConfig.Instance.SilverLeaderboardPlate,
                _ => LeaderboardConfig.Instance.DefaultLeaderboardPlate
            };
            
            _totalScoreLeaderboardPlates[i].SetData(leaderboardSprite, leaderboardData.position, leaderboardData.total_score, leaderboardData.username);
        }
        
        var currentUserLeaderboard = APIManager.Instance.leaderboardData.current_user;

        leaderboardSprite = currentUserLeaderboard.position switch
        {
            1 => LeaderboardConfig.Instance.GoldLeaderboardPlate,
            2 => LeaderboardConfig.Instance.SilverLeaderboardPlate,
            _ => LeaderboardConfig.Instance.DefaultLeaderboardPlate
        };
        userHighScoreLeaderboardPlate.SetData(leaderboardSprite, currentUserLeaderboard.position, currentUserLeaderboard.high_score, currentUserLeaderboard.username);
        
        leaderboardSprite = currentUserLeaderboard.total_score_position switch
        {
            1 => LeaderboardConfig.Instance.GoldLeaderboardPlate,
            2 => LeaderboardConfig.Instance.SilverLeaderboardPlate,
            _ => LeaderboardConfig.Instance.DefaultLeaderboardPlate
        };
        userTotalScoreLeaderboardPlate.SetData(leaderboardSprite, currentUserLeaderboard.total_score_position, currentUserLeaderboard.total_score, currentUserLeaderboard.username);
    }

    private void CreateLeaderBoards()
    {
        foreach (var plate in APIManager.Instance.leaderboardData.high_score_leaderboard.Select(highScoreUser => Instantiate(leaderboardPlateToSpawn, highScoreContentParent)))
        {
            _highScoreLeaderboardPlates.Add(plate);
        }

        foreach (var plate in APIManager.Instance.leaderboardData.total_score_leaderboard.Select(totalScoreUser => Instantiate(leaderboardPlateToSpawn, totalScoreContentParent)))
        {
            _totalScoreLeaderboardPlates.Add(plate);
        }

        FillLeaderboards();
    }

    private void GoToAccountLogin()
    {
        UIManager.Instance.AccountScreenManager.TurnOn();
    }

    private void TurnOnHighScoreLeaderboard()
    {
        _highScoreLeaderboardImage.sprite = pressedButtonSprite;
        _totalScoreLeaderboardImage.sprite = releasedButtonSprite;

        highScoreText.transform.localPosition = _pressedHighScoreTextPos;
        totalScoreText.transform.localPosition = _initialTotalScoreTextPos;
        
        highScoreButton.interactable = false;
        totalScoreButton.interactable = true;
        
        highScoreLeaderboardBox.SetActive(true);
        totalScoreLeaderboardBox.SetActive(false);
        
        userHighScoreLeaderboardPlate.gameObject.SetActive(true);
        userTotalScoreLeaderboardPlate.gameObject.SetActive(false);
    }

    private void TurnOnTotalScoreLeaderboard()
    {
        _highScoreLeaderboardImage.sprite = releasedButtonSprite;
        _totalScoreLeaderboardImage.sprite = pressedButtonSprite;
        
        highScoreText.transform.localPosition = _initialHighScoreTextPos;
        totalScoreText.transform.localPosition = _pressedTotalScoreTextPos;
        
        totalScoreButton.interactable = false;
        highScoreButton.interactable = true;
        
        highScoreLeaderboardBox.SetActive(false);
        totalScoreLeaderboardBox.SetActive(true);
        
        userHighScoreLeaderboardPlate.gameObject.SetActive(false);
        userTotalScoreLeaderboardPlate.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        highScoreButton.onClick.RemoveListener(TurnOnHighScoreLeaderboard);
        totalScoreButton.onClick.RemoveListener(TurnOnTotalScoreLeaderboard);
        
        signInButton.onClick.RemoveListener(GoToAccountLogin);
    }
}
