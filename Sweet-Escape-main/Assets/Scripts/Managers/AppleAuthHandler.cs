#if UNITY_IOS
using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Extensions;
using Firebase.Auth;
using Managers;
using UnityEngine;

public class AppleAuthHandler : MonoBehaviour
{
    public static AppleAuthHandler Instance { get; private set; }
    
    public IAppleAuthManager AppleAuthManager = null!;
    
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
        AppleAuthManager = new AppleAuthManager(new PayloadDeserializer());
    }

    private void Update()
    {
        AppleAuthManager?.Update();
    }
    
    public void OnIosAuthorize()
    {
        var rawNonce = Utils.GenerateRandomString(32);
        var nonce = Utils.GenerateSHA256Nonce(rawNonce);

        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, nonce);
        AppleAuthManager.LoginWithAppleId(loginArgs, appleCredential =>
        {
            if (appleCredential is not IAppleIDCredential cred)
            {
                Debug.LogError("Apple credential is invalid.");
                return;
            }

            var token = Encoding.UTF8.GetString(cred.IdentityToken, 0, cred.IdentityToken.Length);
            var credential = OAuthProvider.GetCredential("apple.com", token, rawNonce, null);
            FirebaseManager.Instance.HandleAppleSingIn(credential);
        }, Debug.LogError);
    }
}
#endif