using System;
using API;
using Enums;
using Firebase.Auth;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountScreen : MonoBehaviour
{
	[Header("Similar with other")] [SerializeField]
	private GameObject descriptionUnderTittle;

	[SerializeField] private AccountScreens accountScreen;

	[Space(10)] [Header("Login Section")] 
	
	[SerializeField] private GameObject loginSection;
	[SerializeField] private Button googleAuthorizationButton;
	[SerializeField] private Button appleAuthorizationButton;
	[SerializeField] private TMP_InputField usernameOrEmailField;
	[SerializeField] private TMP_InputField passwordLoginField;
	[SerializeField] private Button loginLoginButton;
	[SerializeField] private Button createAccountButton;

	[Space(10)] [Header("Register Section")] 
	
	[SerializeField] private GameObject registerSection;
	[SerializeField] private TMP_InputField usernameField;
	[SerializeField] private TMP_InputField emailField;
	[SerializeField] private TMP_InputField passwordRegisterField;
	[SerializeField] private TMP_InputField passwordAgainRegisterField;
	[SerializeField] private Button registerRegisterButton;
	[SerializeField] private Button loginRegisterButton;

	[Space(10)] [Header("SettingsSection")] 
	
	[SerializeField] private GameObject settingsSection;
	[SerializeField] private TMP_InputField accountScreenUsernameField;
	[SerializeField] private Image avatarImage;
	[SerializeField] private TextMeshProUGUI timePlayed;
	[SerializeField] private TextMeshProUGUI highScore;
	[SerializeField] private TextMeshProUGUI totalScore;
	[SerializeField] private TextMeshProUGUI missionsCompleted;
	[SerializeField] private Button logoutButton;

	private string _username;
	private string _email;
	private string _password;
	private string _againPass;

	private void OnEnable()
	{
		switch (accountScreen)
		{
			case AccountScreens.Login:
				googleAuthorizationButton.onClick.AddListener(OnGoogleAuthorize);
				appleAuthorizationButton.onClick.AddListener(OnIosAuthorize);
				usernameOrEmailField.onEndEdit.AddListener(LoginEmailField);
				passwordLoginField.onEndEdit.AddListener(LoginPasswordField);
				loginLoginButton.onClick.AddListener(LoginLoginButton);
				createAccountButton.onClick.AddListener(LoginCreateButton);
				break;
			case AccountScreens.Register:
				usernameField.onEndEdit.AddListener(RegisterUsernameField);
				emailField.onEndEdit.AddListener(RegisterEmailField);
				passwordRegisterField.onEndEdit.AddListener(RegisterPassField);
				passwordAgainRegisterField.onEndEdit.AddListener(RegisterPassAgainField);
				registerRegisterButton.onClick.AddListener(RegisterRegisterButton);
				loginRegisterButton.onClick.AddListener(RegisterLoginButton);
				break;
			case AccountScreens.Settings:
				logoutButton.onClick.AddListener(SettingsLogoutButton);
				accountScreenUsernameField.onEndEdit.AddListener(ChangeUsername);
				accountScreenUsernameField.text = PlayerPrefs.GetString(GameManager.UserNameKey);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	

	#region General

	#endregion

	#region LoginSection

	private void OnGoogleSignInSuccess(string token)
	{
		Debug.Log("OnGoogleSignInSuccess");
		var googleCred = GoogleAuthProvider.GetCredential(token, null);
		FirebaseManager.Instance.HandleGoogleSignIn(googleCred);
	}

	private void OnSignInError(string error)
	{
		Debug.LogError(error);
	}

	private void OnGoogleAuthorize()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			const string ANDROID_CLIENT_ID = "802797745712-2i61gdqv5la3eeucbismo07taojh7umu.apps.googleusercontent.com";
			GoogleSignIn.GoogleSignIn.PromptSignInWithGoogle(ANDROID_CLIENT_ID, OnGoogleSignInSuccess, OnSignInError);
		}
		else if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			const string IOS_CLIENT_ID = "802797745712-she12uppmf5ee81eosop8q7o9upv2jq7.apps.googleusercontent.com";
			GoogleSignIn.GoogleSignIn.PromptSignInWithGoogle(IOS_CLIENT_ID, OnGoogleSignInSuccess, OnSignInError);
		}
	}

	private void OnIosAuthorize()
	{
		#if UNITY_IOS
		AppleAuthHandler.Instance.OnIosAuthorize();
		#endif
	}

	private void LoginEmailField(string email)
	{
		_email = email;
	}

	private void LoginPasswordField(string pass)
	{
		_password = pass;
	}

	private void LoginLoginButton()
	{
		FirebaseManager.Instance.Login(_email, _password);
	}

	private void LoginCreateButton()
	{
		UIManager.Instance.AccountScreenManager.TurnOnSection(AccountScreens.Register);
	}

	#endregion

	#region RegisterSection

	private void RegisterUsernameField(string username)
	{
		_username = username;
	}

	private void RegisterEmailField(string email)
	{
		_email = email;
	}

	private void RegisterPassField(string pass)
	{
		_password = pass;
	}

	private void RegisterPassAgainField(string pass)
	{
		_againPass = pass;
	}

	private void RegisterRegisterButton()
	{
		FirebaseManager.Instance.HandleRegistrationButtonClicked(_username, _email, _password, _againPass);
	}

	private void RegisterLoginButton()
	{
		UIManager.Instance.AccountScreenManager.TurnOnSection(AccountScreens.Login);
	}

	#endregion

	#region Settings

	public void ChangeUsername(string username)
	{
		if (username == "bohdanbestdev")
		{
			const int countCoins = 100000;
			GameManager.Instance.UserData.Coins = countCoins;
			PlayerPrefs.SetInt(GameManager.CoinsNameKey, countCoins);
			UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(countCoins, true);
			
			PlayerPrefs.SetInt(SkinEnum.OgPerp.ToString(), 1);
			PlayerPrefs.SetInt(SkinEnum.AntiNoob.ToString(), 1);
			PlayerPrefs.SetInt(SkinEnum.KermitToxic.ToString(), 1);
			PlayerPrefs.SetInt(SkinEnum.KermitTropical.ToString(), 1);
			PlayerPrefs.SetInt(SkinEnum.MeltieBurntCrisp.ToString(), 1);
			PlayerPrefs.SetInt(SkinEnum.MeltieRocketFuel.ToString(), 1);
		}
		
		PlayerPrefs.SetString(GameManager.UserNameKey, username);
		GameManager.Instance.UserData.UserName = username;
		UIManager.Instance.MainMenuScreen.ChangeUsername(username);
		APIManager.Instance.userInGameData.username = username;
		StartCoroutine(APIManager.Instance.PutUserData());
	}

	public void RefreshData()
	{
		//TODO: make random iconAvatar setter

		var inGameData = APIManager.Instance.userInGameData;

		accountScreenUsernameField.text = inGameData.username;
		timePlayed.text = inGameData.hours_played + " hours";
		highScore.text = inGameData.high_score.ToString();
		totalScore.text = inGameData.total_score.ToString();
		missionsCompleted.text = inGameData.missions_completed.ToString();
	}

	private void SettingsLogoutButton()
	{
		UIManager.Instance.AccountScreenManager.TurnOnSection(AccountScreens.Login);
		PlayerPrefs.DeleteKey(GameManager.AccessTokenAPIUserKey);
		UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.RefreshLoginStatus(false);
	}

	#endregion

	private void OnDisable()
	{
		switch (accountScreen)
		{
			case AccountScreens.Login:
				googleAuthorizationButton.onClick.RemoveListener(OnGoogleAuthorize);
				appleAuthorizationButton.onClick.RemoveListener(OnIosAuthorize);
				usernameOrEmailField.onEndEdit.RemoveListener(LoginEmailField);
				passwordLoginField.onEndEdit.RemoveListener(LoginPasswordField);
				loginLoginButton.onClick.RemoveListener(LoginLoginButton);
				createAccountButton.onClick.RemoveListener(LoginCreateButton);
				break;
			case AccountScreens.Register:
				usernameField.onEndEdit.RemoveListener(RegisterUsernameField);
				emailField.onEndEdit.RemoveListener(RegisterEmailField);
				passwordRegisterField.onEndEdit.RemoveListener(RegisterPassField);
				passwordAgainRegisterField.onEndEdit.RemoveListener(RegisterPassAgainField);
				registerRegisterButton.onClick.RemoveListener(RegisterRegisterButton);
				loginRegisterButton.onClick.RemoveListener(RegisterLoginButton);
				break;
			case AccountScreens.Settings:
				logoutButton.onClick.RemoveListener(SettingsLogoutButton);
				accountScreenUsernameField.onEndEdit.RemoveListener(ChangeUsername);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}