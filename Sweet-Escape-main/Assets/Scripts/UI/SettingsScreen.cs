using UI;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : UIScreen
{
    [SerializeField] private UIToggle musicToggle;
    [SerializeField] private UIToggle sfxToggle;
    [SerializeField] private Button returnButton;
    [SerializeField] private Button privacyPolicyButton;

    private MainMenuScreen _mainMenuScreen;

    private void Start()
    {
        _mainMenuScreen = UIManager.Instance.GetUIScreen<MainMenuScreen>();
    }

    private void OnEnable()
    {
        returnButton.onClick.AddListener(OpenMainMenuScreen);
        privacyPolicyButton.onClick.AddListener(OpenPrivacyPolicy);
        musicToggle.Changed += TurnMusic;
        sfxToggle.Changed += TurnSFX;
    }

    private void OnDisable()
    {
        returnButton.onClick.RemoveListener(OpenMainMenuScreen);
        privacyPolicyButton.onClick.RemoveListener(OpenPrivacyPolicy);
        musicToggle.Changed -= TurnMusic;
        sfxToggle.Changed -= TurnSFX;
    }

    private void OpenMainMenuScreen()
    {
        TurnOff();
        _mainMenuScreen.TurnOn();
    }

    private void OpenPrivacyPolicy()
    {
        Application.OpenURL(Constants.PrivacyPolicy);
    }

    private void TurnMusic(bool isEnabled)
    {
        
    }

    private void TurnSFX(bool isEnabled)
    {
        
    }
}