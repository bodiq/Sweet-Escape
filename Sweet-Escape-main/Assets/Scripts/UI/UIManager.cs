using System.Collections.Generic;
using UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private List<UIScreen> screens;
    [SerializeField] private RectTransform rectTransform;

    private MainMenuScreen _mainMenuScreen;
    private HUDScreen _hudScreen;
    private PopupManager _popupManager;
    private LostScreenUI _lostScreenUI;
    private DoorTransition _doorTransition;
    private CharactersSection _charactersSection;
    private ShopSection _shopSection;
    private GameModeSelection _gameModeSelection;
    private AccountScreenManager _accountScreenManager;
    private LeaderboardDailySectionManager _leaderboardDailySectionManager;
    private MainDownSection _mainDownSection;
    private LoadingBackground _loadingBackground;
    
    public static UIManager Instance { get; private set; }

    public MainMenuScreen MainMenuScreen => _mainMenuScreen;
    public HUDScreen HUDScreen => _hudScreen;
    public PopupManager PopupManager => _popupManager;
    public LostScreenUI LostScreenUI => _lostScreenUI;
    public DoorTransition DoorTransition => _doorTransition;
    public CharactersSection CharactersSection => _charactersSection;
    public RectTransform RectTransform => rectTransform;
    public ShopSection ShopSection => _shopSection;
    public GameModeSelection GameModeSelection => _gameModeSelection;
    public AccountScreenManager AccountScreenManager => _accountScreenManager;
    public LeaderboardDailySectionManager LeaderboardDailySectionManager => _leaderboardDailySectionManager;
    public MainDownSection MainDownSection => _mainDownSection;
    public LoadingBackground LoadingBackground => _loadingBackground;
    
    private void Awake()
    {
        InitializeSingleton();
        GetScreens();
    }

    private void GetScreens()
    {
        _mainMenuScreen = GetUIScreen<MainMenuScreen>();
        _hudScreen = GetUIScreen<HUDScreen>();
        _popupManager = GetUIScreen<PopupManager>();
        _lostScreenUI = GetUIScreen<LostScreenUI>();
        _doorTransition = GetUIScreen<DoorTransition>();
        _charactersSection = GetUIScreen<CharactersSection>();
        _shopSection = GetUIScreen<ShopSection>();
        _gameModeSelection = GetUIScreen<GameModeSelection>();
        _accountScreenManager = GetUIScreen<AccountScreenManager>();
        _leaderboardDailySectionManager = GetUIScreen<LeaderboardDailySectionManager>();
        _mainDownSection = GetUIScreen<MainDownSection>();
        _loadingBackground = GetUIScreen<LoadingBackground>();
    }

    private void Start()
    {
        _mainMenuScreen.TurnOn();
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

    public TUIScreen GetUIScreen<TUIScreen>() where TUIScreen : UIScreen
    {
        return screens.Find(screen => screen is TUIScreen) as TUIScreen;
    }
}