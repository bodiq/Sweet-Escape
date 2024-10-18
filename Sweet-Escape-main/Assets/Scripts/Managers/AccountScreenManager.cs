using System;
using Enums;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class AccountScreenManager : UIScreen
{
    [SerializeField] private Button backButton;
    [SerializeField] private AccountScreen loginScreen;
    [SerializeField] private AccountScreen registerScreen;
    [SerializeField] private AccountScreen settingsScreen;

    public AccountScreens currentAccountScreenEnum;
    public AccountScreen currentAccountScreenScript;

    private void OnEnable()
    {
        backButton.onClick.AddListener(OnBackButton);
    }

    private void OnBackButton()
    {
        switch (currentAccountScreenEnum)
        {
            case AccountScreens.Login:
            case AccountScreens.Settings:
                currentAccountScreenScript.gameObject.SetActive(false);
                TurnOff();
                UIManager.Instance.MainMenuScreen.TurnOnGameModeSelection();
                break;
            case AccountScreens.Register:
                currentAccountScreenScript.gameObject.SetActive(false);
                loginScreen.gameObject.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void TurnOn()
    {
        base.TurnOn();
        
        if (!PlayerPrefs.HasKey(GameManager.AccessTokenAPIUserKey))
        {
            TurnOnSection(AccountScreens.Login);
        }
        else
        {
            TurnOnSection(AccountScreens.Settings);
            settingsScreen.RefreshData();
        }
    }

    public void TurnLoadingScreen(bool isActive)
    {
        if (isActive)
        {
            UIManager.Instance.LoadingBackground.TurnOn();
        }
        else
        {
            UIManager.Instance.LoadingBackground.TurnOff();
        }
    }

    public void TurnOnSection(AccountScreens accountScreen)
    {
        if (currentAccountScreenScript != null)
        {
            currentAccountScreenScript.gameObject.SetActive(false);
        }
        
        switch (accountScreen)
        {
            case AccountScreens.Login:
                loginScreen.gameObject.SetActive(true);
                currentAccountScreenEnum = AccountScreens.Login;
                currentAccountScreenScript = loginScreen;
                settingsScreen.gameObject.SetActive(false);
                registerScreen.gameObject.SetActive(false);
                break;
            case AccountScreens.Register:
                registerScreen.gameObject.SetActive(true);
                currentAccountScreenEnum = AccountScreens.Register;
                currentAccountScreenScript = registerScreen;
                settingsScreen.gameObject.SetActive(false);
                loginScreen.gameObject.SetActive(false);
                break;
            case AccountScreens.Settings:
                settingsScreen.gameObject.SetActive(true);
                currentAccountScreenEnum = AccountScreens.Settings;
                currentAccountScreenScript = settingsScreen;
                registerScreen.gameObject.SetActive(false);
                loginScreen.gameObject.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(accountScreen), accountScreen, null);
        }
    }
}
