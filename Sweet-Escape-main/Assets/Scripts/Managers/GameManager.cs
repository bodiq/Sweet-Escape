using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API;
using Configs;
using Data;
using Enemy;
using Enums;
using Extensions;
using Items;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Camera camera;
    [SerializeField] private GameObject globalVolume;
    [SerializeField] private bool isTestEnvironment;

    public Action OnPlayerDeath;
    public Action OnPlayerRespawn;

    public Action OnStopGame;
    public Action OnResumeGame;

    public Action ResetAnimationInfo;

    public Action<Enums.PowerUps> OnPlayerPowerUpStart;
    public Action<Enums.PowerUps> OnPlayerPowerUpEnd;

    public Action OnGetCoin;

    public Action<Characters, SkinEnum> OnCharacterSkinChange;

    private Characters _selectedCharacter;

    public readonly Dictionary<Characters, CharacterData> CharactersData = new();
    public readonly UserData UserData = new();

    public readonly Dictionary<Enums.PowerUps, PowerUpData> PowerUpData = new();

    public Player player;

    public readonly List<IEnemy> Enemies = new();
    
    public readonly List<Coin> Coins = new();
    public readonly List<Sprinkle> Sprinkles = new();

    public const string ClaimableRewardKey = "ClaimableRewardKey";

    public const string UserNameKey = "UserName";
    public const string CoinsNameKey = "UserCoins";
    public const string UserMusicVolumeKey = "UserMusicVolume";
    public const string UserSoundFXVolumeKey = "UserSoundFXVolume";
    public const string CrtShaderActiveKey = "CRTShaderActive";
    public const string LastScoreKey = "LastScore";
    public const string HighScoreKey = "BestScore";
    public const string TotalScoreKey = "TotalScore";
    
    public const string AddPercentCoinRewardKey = "PercentCoinReward";
    public const string AddPercentPowerUpAppearingKey = "PercentPowerUpAppearing";
    public const string AddEnemyTypeAvoidanceKey = "EnemyTypeAvoidance";
    public const string AccessTokenAPIUserKey = "AccessTokenAPIUser";
    public const string DailyMissionsUsedFreeRerollKey = "DailyMissionsFreereroll";
    public const string AvailableClaimableAchievementsKey = "AvailableClaimableAchievementKey";
    public const string CompletedAchievementsKey = "CompletedAchievementsKey";

    public const string TotalHoursPlayTimeKey = "TotalTimeKey";

    public bool isGoldenSpoonActivate;
    public bool isRemovedAdv = false;

    public int countPowerUpsTakenPerRun = 0;

    private readonly WaitForSeconds _waitForSeconds = new(1);

    private readonly WaitForSeconds _waitForLeaderboardUpdate = new(WaitForLeaderboardUpdate);

    private const int WaitForLeaderboardUpdate = 300;
        
    private Coroutine _refreshLeaderboardTimer;

    public CharacterData SelectedCharacterData => CharactersData[_selectedCharacter];
    public Characters SelectedCharacter => _selectedCharacter;

    public Camera MainCamera => camera;
    public GameObject GlobalVolume => globalVolume;
    public bool IsTestEnvironment => isTestEnvironment;

    public SkinEnum skinEnum = SkinEnum.MintyFresh;

    public DateTime LastUpdatedTime;
    
    public bool canBeRefreshLeaderboard = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        Screen.orientation = ScreenOrientation.Portrait;
    }

    private async void Start()
    {
        InitializeData();
        
        await Ale();
    }

    private async Task Ale()
    {
        Task.Delay(2);
        Debug.LogError("Ale");
    }

    private void InitializeData()
    {
        var powerUps = Enum.GetValues(typeof(Enums.PowerUps)).Cast<Enums.PowerUps>().ToList();
        
        LastUpdatedTime = DateTime.Now;

        if (!PlayerPrefs.HasKey(UserNameKey))
        {
            var randomName = "ChangeNicknameBro" + Random.Range(0, 1000);
            PlayerPrefs.SetString(UserNameKey, randomName);
            UIManager.Instance.MainMenuScreen.ChangeUsername(randomName);
            UserData.UserName = randomName;
        }
        else
        {
            var userName = PlayerPrefs.GetString(UserNameKey);
            UIManager.Instance.MainMenuScreen.ChangeUsername(userName);
            UserData.UserName = userName;
        }

        if (PlayerPrefs.HasKey(ShopCoinBundlesItemType.RemoveAdv.ToString()))
        {
            isRemovedAdv = true;
        }

        foreach (var key in DailyMissionsConfig.Instance.DailyMissionsData.Select(data => data.dailyMissions.ToString() + data.countToDo).Where(key => PlayerPrefs.HasKey(key)))
        {
            DailyBoxManager.SavedDailyTasksKeys.Add(key);
        }

        if (!PlayerPrefs.HasKey("Noob"))
        {
            PlayerPrefs.SetInt("Noob", 1);
        }

        if (!PlayerPrefs.HasKey(SkinEnum.MintyFresh.ToString()))
        {
            PlayerPrefs.SetInt(SkinEnum.MintyFresh.ToString(), 1);
        }

        if (!PlayerPrefs.HasKey(LastScoreKey))
        {
            PlayerPrefs.SetInt(LastScoreKey, 0);
        }

        if (!PlayerPrefs.HasKey(HighScoreKey))
        {
            PlayerPrefs.SetInt(HighScoreKey, 0);
        }

        if (!PlayerPrefs.HasKey(TotalScoreKey))
        {
            PlayerPrefs.SetInt(TotalScoreKey, 0);
        }

        if (!PlayerPrefs.HasKey(TotalHoursPlayTimeKey))
        {
            PlayerPrefs.SetInt(TotalHoursPlayTimeKey, 0);
        }

        if (!PlayerPrefs.HasKey("first_game"))
        {
            PlayerPrefs.SetInt("first_game", 1);
        }

        if (PowerUpConfig.Instance.PowerUpPresets != null)
        {
            foreach (var powerUp in PowerUpConfig.Instance.PowerUpPresets)
            {
                var powerData = new PowerUpData();
                var key = powerUp.Key.ToString();
                var value = powerUp.Value;

                if (!PlayerPrefs.HasKey(key))
                {
                    if (powerUp.Key == Enums.PowerUps.WaffleShield)
                    {
                        PlayerPrefs.SetInt(key, 0);

                        powerData.Level = 0;
                        powerData.Coefficient = value.startCoefficientMultiplier;
                        powerData.DurationTime = value.startDuration;
                        PowerUpData.Add(powerUp.Key, powerData);
                    }
                    else
                    {
                        PlayerPrefs.SetInt(key, 1);
                        powerData.Level = 1;
                        powerData.Coefficient = value.startCoefficientMultiplier;
                        powerData.DurationTime = value.startDuration;
                        PowerUpData.Add(powerUp.Key, powerData);
                    }
                }
                else
                {
                    var level = PlayerPrefs.GetInt(key);

                    if (level >= 5 && powerUp.Key == Enums.PowerUps.Magnet)
                    {
                        powerData.AllowedCoinMagnet = true;
                    }

                    powerData.Level = level;
                    powerData.Coefficient =
                        value.startCoefficientMultiplier + value.perLevelAdditionalMultiplier * (level - 1);
                    powerData.DurationTime = value.startDuration + value.perLevelAdditionalDuration * (level - 1);
                    PowerUpData.Add(powerUp.Key, powerData);
                }
            }
        }
        

        if (!PlayerPrefs.HasKey(CoinsNameKey))
        {
            PlayerPrefs.SetInt(CoinsNameKey, 0);
            UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(0, true);
            UserData.Coins = 0;
        }
        else
        {
            var coins = PlayerPrefs.GetInt(CoinsNameKey);
            UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(coins, true);
            UserData.Coins = coins;
        }

        foreach (var character in CharacterConfig.Instance.CharacterData)
        {
            const int startLevel = 1;
            var currentCharacterLevel = startLevel;

            var characterData = new CharacterData();
            var characterType = character.Key;
            var characterString = characterType.ToString();

            if (!PlayerPrefs.HasKey(characterString))
            {
                PlayerPrefs.SetInt(characterString, 0);
                characterData.Level = startLevel;
            }
            else
            {
                var level = PlayerPrefs.GetInt(characterString);
                characterData.Level = level;
                currentCharacterLevel = level;
            }

            if (PowerUpConfig.Instance.PowerUpAppearance.TryGetValue(character.Key, out var appearanceInfo))
            {
                var levelIndex = characterData.Level - 1;
                foreach (var powers in appearanceInfo.Elements)
                {
                    switch (powers.value)
                    {
                        case Enums.PowerUps.Nothing:
                            powers.chance -= 0.05f * levelIndex;
                            break;
                        case Enums.PowerUps.WaffleShield:
                        case Enums.PowerUps.Magnet:
                        case Enums.PowerUps.ChillBlast:
                        case Enums.PowerUps.GoldSpoon:
                        case Enums.PowerUps.HundAThousands:
                            powers.chance += levelIndex / 100f;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                appearanceInfo.Normalize();
            }

            var characterLevelBuffsStartWorking = currentCharacterLevel - 1;

            if (!PlayerPrefs.HasKey(characterType + AddPercentCoinRewardKey))
            {
                var percentCoinReward = characterLevelBuffsStartWorking *
                                        CharacterBuffConfig.Instance.AdditionalPercentCoinRewardPerLevel;
                PlayerPrefs.SetFloat(characterType + AddPercentCoinRewardKey, percentCoinReward);
                characterData.AdditionalPercentCoinReward = percentCoinReward;
            }
            else
            {
                var percentCoin = PlayerPrefs.GetFloat(characterType + AddPercentCoinRewardKey);
                characterData.AdditionalPercentCoinReward = percentCoin;
            }

            if (PlayerPrefs.HasKey(CrtShaderActiveKey))
            {
                var value = PlayerPrefs.GetInt(CrtShaderActiveKey);

                globalVolume.SetActive(value != 0);
                UIManager.Instance.MainMenuScreen.TurnScanLinesBackground(value != 0);
            }
            else
            {
                PlayerPrefs.SetInt(CrtShaderActiveKey, 0);
                globalVolume.SetActive(false);
                UIManager.Instance.MainMenuScreen.TurnScanLinesBackground(false);
            }

            if (PlayerPrefs.HasKey(AccessTokenAPIUserKey))
            {
                //StartCoroutine(APIManager.CheckAccessToken());
                
                APIManager.Instance.userLoginAPIDataData.access_token = PlayerPrefs.GetString(AccessTokenAPIUserKey);
                StartCoroutine(APIManager.Instance.GetLeaderboardInfo());
                StartCoroutine(APIManager.Instance.GetUserInGameInfo());
            }

            CharactersData.Add(characterType, characterData);
        }
    }

    private IEnumerator LoadSceneAsync()
    {
        yield return _waitForSeconds;
        var asyncLoad = SceneManager.LoadSceneAsync(2);
        while (!asyncLoad.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        StartLevel();
    }

    private void StartLevel()
    {
        UIManager.Instance.DoorTransition.OpenDoor();
        UIManager.Instance.HUDScreen.LoadGameScene();
        InitializePlayer();
    }

    public void LoadScene(Characters character)
    {
        _selectedCharacter = character;
        if (isTestEnvironment)
        {
            StartLevel();
        }
        else
        {
            StartCoroutine(LoadSceneAsync());
        }
    }

    private void InitializePlayer()
    {
        if (CharacterConfig.Instance.CharacterData.TryGetValue(_selectedCharacter, out var characterPreset))
        {
            player = Instantiate(characterPreset.player, TilemapManager.Instance.PlayerSpawnPoint.position,
                Quaternion.identity);
            CameraManager.Instance.SetCameraTarget(player.transform);

            RuntimeAnimatorController controller = null;
            RuntimeAnimatorController shielded = null;

            foreach (var skinData in characterPreset.skinData.Where(skinData => skinData.skinEnum == skinEnum))
            {
                controller = skinData.defaultAnimatorController;
                shielded = skinData.shieldedAnimatorController;
            }

            if (CharactersData.TryGetValue(_selectedCharacter, out var characterData))
            {
                var addShields = PlayerPrefs.GetInt(Enums.PowerUps.WaffleShield.ToString());
                player.Initialize(characterData.Level, characterPreset, _selectedCharacter, addShields, controller,
                    shielded);
            }
        }
    }

    private IEnumerator RefreshLeaderboardTimerCoroutine()
    {
        while (true)
        {
            yield return _waitForLeaderboardUpdate;

            canBeRefreshLeaderboard = true;
            
        }
    }

    public void StartLeaderboardRefreshTimer()
    {
        _refreshLeaderboardTimer = StartCoroutine(RefreshLeaderboardTimerCoroutine());
    }

    public void RefreshSlimePlayer(Tilemap tilemap)
    {
        player.PlayerMovement.Boom(tilemap);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt(TotalHoursPlayTimeKey, Utils.GetTotalHoursPlayed());

        if (_refreshLeaderboardTimer != null)
        {
            StopCoroutine(_refreshLeaderboardTimer);
            _refreshLeaderboardTimer = null;
        }
    }

    public void ResetPoolItems()
    {
        foreach (var coin in Coins)
        {
            coin.gameObject.SetActive(false);
            ManualObjectPool.SharedInstance.pooledCoinsObjects.Add(coin);
        }
        
        Coins.Clear();
        Sprinkles.Clear();
    }
}