using System;
using System.Collections;
using System.Threading.Tasks;
using API;
using Enums;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using Firebase.Extensions;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine.Events;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Managers
{
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager Instance { get; private set; }

        private FirebaseUser _user;
        private FirebaseAuth _auth;
        private DependencyStatus _dependencyStatus;

        public UnityEvent onFirebaseInitialized = new();

        private Coroutine _registrationCoroutine;
        private Coroutine _loginCoroutine;
        private Coroutine _apiLoginCoroutine;

        public UnityEvent<Exception> onUserRegistrationFail;
        public UnityEvent<FirebaseUser> onUserRegistrationSuccess;

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
        }

        private void Start()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.

                    InitializeFirebase();

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                }
                else
                {
                    UnityEngine.Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    // Firebase Unity SDK is not safe to use here.
                }
            });
        }

        private void InitializeFirebase()
        {
            _auth = FirebaseAuth.DefaultInstance;
            _auth.StateChanged += AuthStateChanged;

            AuthStateChanged(this, null);
        }

        private void AuthStateChanged(object sender, System.EventArgs eventArgs)
        {
            if (_auth.CurrentUser != _user)
            {
                var signedIn = _user != _auth.CurrentUser && _auth.CurrentUser != null;

                if (!signedIn && _user != null)
                {
                    Debug.LogError("Signed out " + _user.UserId);
                }

                _user = _auth.CurrentUser;

                if (signedIn)
                {
                    Debug.LogError("Signed in " + _user.UserId);
                }
            }
        }

        public void Login(string email, string pass)
        {
            _loginCoroutine = StartCoroutine(LoginAsync(email, pass));
        }

        private IEnumerator LoginAsync(string email, string pass)
        {
            var loginTask = _auth.SignInWithEmailAndPasswordAsync(email, pass);

            UIManager.Instance.AccountScreenManager.TurnLoadingScreen(true);
            
            yield return new WaitUntil(() => loginTask.IsCompleted);

            if (loginTask.Exception != null)
            {
                Debug.LogError(loginTask.Exception);

                var firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
                if (firebaseException == null) yield break;

                var authError = (AuthError)firebaseException.ErrorCode;

                var failedMessage = "Login Failed! Because: ";

                var authErrorMessage = "";

                failedMessage += authError switch
                {
                    AuthError.InvalidEmail => authErrorMessage = "Email is invalid",
                    AuthError.WrongPassword => authErrorMessage = "Wrong Password",
                    AuthError.MissingEmail => authErrorMessage = "Email is missing",
                    AuthError.MissingPassword => authErrorMessage = "Password is missing",
                    _ => authErrorMessage = "Login Failed"
                };

                UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
                UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.CustomMessage, authErrorMessage);
                Debug.LogError(failedMessage);
            }
            else
            {
                _user = loginTask.Result.User;

                var task = _user.TokenAsync(true);

                yield return new WaitUntil(() => task.IsCompleted);

                if (task.IsCanceled)
                {
                    Debug.LogError("TokenAsync was canceled.");
                    UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
                    yield break;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("TokenAsync encountered an error: " + task.Exception);
                    UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
                    yield break;
                }

                if (task.IsCompletedSuccessfully)
                {
                    idToken = task.Result;
                    if (_user.IsEmailVerified)
                    {
                        StartAPILogin(AccountScreens.Settings);
                        Debug.LogError("{0} You are successfully logged in, " + _user.DisplayName);
                    }
                    else
                    {
                        var error = "You need to verify your email ";
                        Debug.LogError(error + _user.DisplayName);
                        UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.CustomMessage, error);
                        UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
                    }
                }
            }
        }

        public void HandleRegistrationButtonClicked(string username, string email, string pass, string againPass)
        {
            _registrationCoroutine = StartCoroutine(RegisterUser(username, email, pass, againPass));
        }

        public void HandleGoogleSignIn(Credential credential)
        {
            Debug.Log("HandleGoogleSignIn");
            StartCoroutine(HandleGoogleSignInCoroutine(credential));
        }

        public void HandleAppleSingIn(Credential credential)
        {
            Debug.Log("HandleAppleSignIn");
            StartCoroutine(HandleAppleSignInCoroutine(credential));
        }
        
        private IEnumerator HandleAppleSignInCoroutine(Credential credential)
        {
            Debug.Log("HandleGoogleSignInCoroutine");
            var signInTask = _auth.SignInAndRetrieveDataWithCredentialAsync(credential);
            
            UIManager.Instance.AccountScreenManager.TurnLoadingScreen(true);
            yield return new WaitUntil(() => signInTask.IsCompleted);
            
            if (signInTask.IsCanceled) 
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                yield break;
            }
            
            if (!signInTask.IsCompletedSuccessfully)
            {
                Debug.LogException(signInTask.Exception);
                UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
                yield break;
            }
            
            Debug.Log("signInTask.Result.TokenAsync");
            
            var tokenTask = signInTask.Result.User.TokenAsync(true);
            
            yield return new WaitUntil(() => tokenTask.IsCompleted);
            
            if (tokenTask.IsCompletedSuccessfully)
            {
                Debug.Log("tokenTask.IsCompletedSuccessfully");
                idToken = tokenTask.Result;
                StartAPILogin(AccountScreens.Settings);
            }
            else
            {
                Debug.LogException(tokenTask.Exception);
            }
        }

        private IEnumerator HandleGoogleSignInCoroutine(Credential credential)
        {
            Debug.Log("HandleGoogleSignInCoroutine");
            var signInTask = _auth.SignInWithCredentialAsync(credential);
            UIManager.Instance.AccountScreenManager.TurnLoadingScreen(true);
            yield return new WaitUntil(() => signInTask.IsCompleted);
            if (!signInTask.IsCompletedSuccessfully)
            {
                Debug.LogException(signInTask.Exception);
                UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
                yield break;
            }
            
            Debug.Log("signInTask.Result.TokenAsync");
            var tokenTask = signInTask.Result.TokenAsync(false);
            yield return new WaitUntil(() => tokenTask.IsCompleted);
            
            if (tokenTask.IsCompletedSuccessfully)
            {
                Debug.Log("tokenTask.IsCompletedSuccessfully");
                idToken = tokenTask.Result;
                StartAPILogin(AccountScreens.Settings);
            }
            else
            {
                Debug.LogException(tokenTask.Exception);
            }
        }

        IEnumerator Upload(string url, AccountScreens redirectTo)
        {
            yield return new WaitForSeconds(1);

            var json = $"{{\"idtoken\":\"{idToken}\", \"referred_by\":\"\"}}";
            Debug.LogError(json);

            using var request = new UnityWebRequest(url, "POST");
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"{request.error}: {request.downloadHandler.text}");
                APIManager.Instance.HandleAPIError();
            }
            else
            {
                HandleAPIConnectSuccess(request.downloadHandler.text, redirectTo);
            }
        }


        private void HandleAPIConnectSuccess(string responseText, AccountScreens redirectTo)
        {
            Debug.Log($"Response: {responseText}");
            UIManager.Instance.AccountScreenManager.TurnOnSection(redirectTo);
            UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
            UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.RefreshLoginStatus(true);

            if (!string.IsNullOrEmpty(responseText))
            {
                var apiStorageData = JsonConvert.DeserializeObject<LoginAPIData>(responseText, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                if (apiStorageData != null)
                {
                    Debug.Log("Saved response text for access_token: " + apiStorageData.access_token);
                    PlayerPrefs.SetString(GameManager.AccessTokenAPIUserKey, apiStorageData.access_token);
                    APIManager.Instance.userLoginAPIDataData = apiStorageData;

                    StartCoroutine(APIManager.Instance.GetLeaderboardInfo());
                    //StartCoroutine(APIManager.Instance.GetUserInGameInfo());
                }
                else
                {
                    Debug.LogError("Failed to deserialize the response text to API.LoginAPIData");
                }
            }
            else
            {
                Debug.LogError("Response text is null or empty");
            }
        }

        string responce = String.Empty;
        
        string idToken = String.Empty;

        private void StartAPILogin(AccountScreens redirectTo)
        {
            var url = "https://api.hundredsthousands.opalstacked.com/firebase_login";
            _apiLoginCoroutine = StartCoroutine(Upload(url, redirectTo));
        }

        private IEnumerator RegisterUser(string username, string email, string pass, string againPass)
        {
            if (username.IsNullOrWhitespace())
            {
                var error = "User Name is empty";
                UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.CustomMessage, error);
            }
            else if (email.IsNullOrWhitespace())
            {
                var error = "Email is empty";
                UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.CustomMessage, error);
            }
            else if (pass != againPass)
            {
                var error = "Password doesnt match";
                UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.CustomMessage, error);
            }
            else
            {
                var registerTask = _auth.CreateUserWithEmailAndPasswordAsync(email, pass);

                UIManager.Instance.AccountScreenManager.TurnLoadingScreen(true);
                
                yield return new WaitUntil(() => registerTask.IsCompleted);

                yield return HandleUserCreation(registerTask);
            }

            _registrationCoroutine = null;
        }

        private IEnumerator HandleUserCreation(Task<AuthResult> registerTask)
        {
            if (registerTask.Exception != null)
            {
                Debug.LogWarning($"Failed to register task with {registerTask.Exception}");

                var firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                var authError = (AuthError)firebaseException.ErrorCode;

                var failedMessage = "Register Failed! Because: ";

                var authErrorMessage = "";
                
                failedMessage += authError switch
                {
                    AuthError.InvalidEmail => authErrorMessage = "Email is invalid",
                    AuthError.WrongPassword => authErrorMessage = "Wrong Password",
                    AuthError.MissingEmail => authErrorMessage = "Email is missing",
                    AuthError.MissingPassword => authErrorMessage = "Password is missing",
                    _ => authErrorMessage = "Register Profile Failed"
                };

                Debug.LogError(failedMessage);
                
                UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.CustomMessage, authErrorMessage);
                UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
            }
            else
            {
                _user = registerTask.Result.User;

                var userProfile = new UserProfile { DisplayName = name };
                var updateProfileTask = _user.UpdateUserProfileAsync(userProfile);

                yield return new WaitUntil(() => updateProfileTask.IsCompleted);

                if (updateProfileTask.Exception != null)
                {
                    _user.DeleteAsync();

                    Debug.LogError(updateProfileTask.Exception);

                    var firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                    var authError = (AuthError)firebaseException.ErrorCode;

                    var failedMessage = "Profile update Failed! Because ";

                    failedMessage += authError switch
                    {
                        AuthError.InvalidEmail => "Email is invalid",
                        AuthError.WrongPassword => "Wrong Password",
                        AuthError.MissingEmail => "Email is missing",
                        AuthError.MissingPassword => "Password is missing",
                        _ => "Profile Update Failed"
                    };

                    Debug.LogError(failedMessage);
                    UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.CustomMessage, authError.ToString());
                    UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
                }
                else
                {
                    Debug.LogError("Registration Successful Welcome " + _user.DisplayName);
                    var user = _auth.CurrentUser;

                    var sentEmail = user.SendEmailVerificationAsync();

                    yield return new WaitUntil((() => sentEmail.IsCompleted));

                    if (sentEmail.IsCompletedSuccessfully)
                    {
                        var message = "Verification email sent ";
                        Debug.Log(message + user.Email);
                        UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.CustomMessage, message);
                        UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
                        yield break;
                    }

                    if (sentEmail.IsCanceled)
                    {
                        var message = "Verification email was canceled";
                        UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.CustomMessage, message);
                        UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
                        yield break;
                    }

                    if (sentEmail.IsFaulted)
                    {
                        Debug.LogError("Verification email encountered an error: " + sentEmail.Exception);
                        var message = "Verification email encountered an error";
                        UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.CustomMessage, message);
                        UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
                    }
                }
            }
        }
    }
}