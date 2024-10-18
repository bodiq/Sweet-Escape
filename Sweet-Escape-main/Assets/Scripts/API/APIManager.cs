using System;
using System.Collections;
using Enums;
using Extensions;
using Managers;
using Newtonsoft.Json;
using UI;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace API
{
    public class APIManager : MonoBehaviour
    {
        public static APIManager Instance { get; private set; }
        
        private const string UserAchievementsUrl = "https://api.hundredsthousands.opalstacked.com/user_game_missions";
        private const string LeaderboardUrl = "https://api.hundredsthousands.opalstacked.com/game_leaderboard";
        private const string UserInGameDataUrl = "https://api.hundredsthousands.opalstacked.com/game_user";
        private const string AccessTokenCheckUrl = "https://api.hundredsthousands.opalstacked.com/protected_ping";

        public LoginAPIData userLoginAPIDataData = new();
        public UserAchievementsData userAchievementsData = new();
        public LeaderboardData leaderboardData = new();
        public UserInGameData userInGameData = new();

        private WaitForSeconds _waitForLoadingData = new (DurationDataLoadingWait);
        private const float DurationDataLoadingWait = 3f;

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

        public IEnumerator CheckAccessToken()
        {
            using var webRequest = UnityWebRequest.Get(AccessTokenCheckUrl);
            
            webRequest.SetRequestHeader("Authorization", "Bearer " + userLoginAPIDataData.access_token);

            yield return webRequest.SendWebRequest();

            switch (webRequest.responseCode)
            {
                case 200:
                    Debug.Log(webRequest.responseCode + " AccessToken Refreshed! Welcome");
                    if (string.IsNullOrEmpty(webRequest.downloadHandler.text))
                    {
                        Debug.LogError("WebRequest text is empty");
                        yield break;
                    }

                    var accessToken = JsonConvert.DeserializeObject<RefreshTokenData>(webRequest.downloadHandler.text, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    if (accessToken == null)
                    {
                        Debug.LogError("Error on deserializing Object");
                        yield break;
                    }
            
                    Debug.Log("New refreshed access token: " + accessToken);
                    
                    userLoginAPIDataData.access_token = accessToken.refreshed_accessToken;
                    PlayerPrefs.SetString(GameManager.AccessTokenAPIUserKey, userLoginAPIDataData.access_token);

                    yield return StartCoroutine(GetUserInGameInfo());
                    
                    break;
                case 401:
                    Debug.Log("No refreshing Token, needs to login again");
                    PlayerPrefs.DeleteKey(GameManager.AccessTokenAPIUserKey);
                    break;
                default:
                    if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError("Error: " + webRequest.error);
                    }
                    break;
            }
        }

        #region Put

        public IEnumerator PutUserAchievement(string identifier, string data)
        {
            var url = $"{UserAchievementsUrl}?mission={identifier}";
            using var webRequest = UnityWebRequest.Put(url, data);

            webRequest.SetRequestHeader("Authorization", "Bearer " + userLoginAPIDataData.access_token);

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
                HandleUserAchievementsResponse(webRequest.downloadHandler.text);
            }
        }

        public IEnumerator PutUserData()
        {
            //var data = Utils.GetUserInGameDataToBytes();
            var data = System.Text.Encoding.UTF8.GetBytes("?hours_played=" + userInGameData.hours_played + "&high_score=" + userInGameData.high_score + "&total_score=" + userInGameData.total_score + "&username=" + userInGameData.username);
            using var webRequest = UnityWebRequest.Put(UserInGameDataUrl, data);
            
            webRequest.SetRequestHeader("Authorization", "Bearer " + userLoginAPIDataData.access_token);

            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
            }
        }

        #endregion

        #region Get

        public IEnumerator GetLeaderboardInfo()
        {
            using var webRequest = UnityWebRequest.Get(LeaderboardUrl);
            
            webRequest.SetRequestHeader("Authorization", "Bearer " + userLoginAPIDataData.access_token);

            yield return webRequest.SendWebRequest();
            
            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
                yield return StartCoroutine(HandleLeaderboardResponse(webRequest.downloadHandler.text));
            }
        }

        public IEnumerator GetUserInGameInfo()
        {
            using var webRequest = UnityWebRequest.Get(UserInGameDataUrl);
            
            webRequest.SetRequestHeader("Authorization", "Bearer " + userLoginAPIDataData.access_token);
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
                yield return StartCoroutine(HandleInGameDataResponse(webRequest.downloadHandler.text));
            }
        }

        #endregion

        #region HandleResponce

        private IEnumerator HandleLeaderboardResponse(string responseText)
        {
            if (string.IsNullOrEmpty(responseText)) yield break;

            var userLeaderboardData = JsonConvert.DeserializeObject<LeaderboardData>(responseText, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            if (userLeaderboardData == null) yield break;
            
            Debug.Log("GameAchievements: " + userLeaderboardData);
            leaderboardData = userLeaderboardData;
        }

        private IEnumerator HandleInGameDataResponse(string responseText)
        {
            if (string.IsNullOrEmpty(responseText)) yield break;

            var inGameData = JsonConvert.DeserializeObject<UserInGameData>(responseText, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            if (inGameData == null) yield break;
            
            Debug.Log("userAPIData: " + inGameData);

            if (PlayerPrefs.GetInt(GameManager.TotalScoreKey) > inGameData.total_score)
            {
                RefreshLocalData();
                yield return StartCoroutine(PutUserData());
            }
            else
            {
                userInGameData = inGameData;
                yield return StartCoroutine(FillLocalDataFromServer());
            }
            
            yield return StartCoroutine(GetLeaderboardInfo());
        }

        private void HandleUserAchievementsResponse(string responseText)
        {
            if (string.IsNullOrEmpty(responseText)) return;

            var achievementsData = JsonConvert.DeserializeObject<UserAchievementsData>(responseText, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            if (achievementsData != null)
            {
                Debug.Log("GameAchievements: " + achievementsData);
                userAchievementsData = achievementsData;
            }
        }

        public void HandleAPIError()
        {
            UIManager.Instance.PopupManager.ActivatePopupMessage(PopupTypes.CustomMessage, "API error connection");
            UIManager.Instance.AccountScreenManager.TurnLoadingScreen(false);
        }

        #endregion
        
        private void RefreshLocalData()
        {
            userInGameData.hours_played = Utils.GetTotalHoursPlayed();
            userInGameData.total_score = PlayerPrefs.GetInt(GameManager.TotalScoreKey);
            userInGameData.high_score = PlayerPrefs.GetInt(GameManager.HighScoreKey);
            userInGameData.missions_completed = PlayerPrefs.GetInt(GameManager.CompletedAchievementsKey);
            userInGameData.username = PlayerPrefs.GetString(GameManager.UserNameKey);
            userInGameData.coins = PlayerPrefs.GetInt(GameManager.CoinsNameKey);
            userInGameData.last_score = PlayerPrefs.GetInt(GameManager.LastScoreKey);
                
            userInGameData.noob_lvl = PlayerPrefs.GetInt(Characters.Noob.ToString());
            userInGameData.kermit_lvl = PlayerPrefs.GetInt(Characters.Kermit.ToString());
            userInGameData.meltie_lvl = PlayerPrefs.GetInt(Characters.Meltie.ToString());

            userInGameData.noob_mintyfresh = true;
            userInGameData.noob_ogperp = PlayerPrefs.HasKey(SkinEnum.OgPerp.ToString());
            userInGameData.noob_antinoob = PlayerPrefs.HasKey(SkinEnum.AntiNoob.ToString());

            userInGameData.kermit_seasamegreen = true;
            userInGameData.kermit_tropical = PlayerPrefs.HasKey(SkinEnum.KermitTropical.ToString());
            userInGameData.kermit_toxic = PlayerPrefs.HasKey(SkinEnum.KermitToxic.ToString());

            userInGameData.meltie_hotcream = true;
            userInGameData.meltie_burntcrisp = PlayerPrefs.HasKey(SkinEnum.MeltieBurntCrisp.ToString());
            userInGameData.meltie_rockefuel = PlayerPrefs.HasKey(SkinEnum.MeltieRocketFuel.ToString());

            userInGameData.shields = PlayerPrefs.GetInt(Enums.PowerUps.WaffleShield.ToString());
            userInGameData.magnet_lvl = PlayerPrefs.GetInt(Enums.PowerUps.Magnet.ToString());
            userInGameData.chillblast_lvl = PlayerPrefs.GetInt(Enums.PowerUps.ChillBlast.ToString());
            userInGameData.goldenspoon_lvl = PlayerPrefs.GetInt(Enums.PowerUps.GoldSpoon.ToString());
            userInGameData.hundathous_lvl = PlayerPrefs.GetInt(Enums.PowerUps.HundAThousands.ToString());

            userInGameData.dailyloginday = PlayerPrefs.GetInt(DailyRewardManager.IndexForClaiming);
        }

        private IEnumerator FillLocalDataFromServer()
        {
            UIManager.Instance.LoadingBackground.TurnOn();
            
            PlayerPrefs.SetInt(GameManager.TotalHoursPlayTimeKey, userInGameData.hours_played);
            PlayerPrefs.SetInt(GameManager.TotalScoreKey, userInGameData.total_score);
            PlayerPrefs.SetInt(GameManager.HighScoreKey, userInGameData.high_score);
            PlayerPrefs.SetInt(GameManager.CompletedAchievementsKey, userInGameData.missions_completed);
            PlayerPrefs.SetInt(GameManager.CoinsNameKey, userInGameData.coins);
            PlayerPrefs.SetString(GameManager.UserNameKey, userInGameData.username);

            PlayerPrefs.SetInt(Characters.Noob.ToString(), userInGameData.noob_lvl);
            PlayerPrefs.SetInt(Characters.Kermit.ToString(), userInGameData.kermit_lvl);
            PlayerPrefs.SetInt(Characters.Meltie.ToString(), userInGameData.meltie_lvl);

            if (userInGameData.noob_ogperp)
            {
                PlayerPrefs.SetInt(SkinEnum.OgPerp.ToString(), 1);
            }
            
            if (userInGameData.noob_antinoob)
            {
                PlayerPrefs.SetInt(SkinEnum.AntiNoob.ToString(), 1);
            }
            
            if (userInGameData.kermit_tropical)
            {
                PlayerPrefs.SetInt(SkinEnum.KermitTropical.ToString(), 1);
            }
            
            if (userInGameData.kermit_toxic)
            {
                PlayerPrefs.SetInt(SkinEnum.KermitToxic.ToString(), 1);
            }
            
            if (userInGameData.meltie_burntcrisp)
            {
                PlayerPrefs.SetInt(SkinEnum.MeltieBurntCrisp.ToString(), 1);
            }
            
            if (userInGameData.meltie_rockefuel)
            {
                PlayerPrefs.SetInt(SkinEnum.MeltieRocketFuel.ToString(), 1);
            }

            PlayerPrefs.SetInt(Enums.PowerUps.WaffleShield.ToString(), userInGameData.shields);
            PlayerPrefs.SetInt(Enums.PowerUps.Magnet.ToString(), userInGameData.magnet_lvl);
            PlayerPrefs.SetInt(Enums.PowerUps.ChillBlast.ToString(), userInGameData.chillblast_lvl);
            PlayerPrefs.SetInt(Enums.PowerUps.GoldSpoon.ToString(), userInGameData.goldenspoon_lvl);
            PlayerPrefs.SetInt(Enums.PowerUps.HundAThousands.ToString(), userInGameData.hundathous_lvl);
            
            PlayerPrefs.SetInt(DailyRewardManager.IndexForClaiming, userInGameData.dailyloginday);

            UIManager.Instance.AccountScreenManager.currentAccountScreenScript.RefreshData();
            UIManager.Instance.AccountScreenManager.currentAccountScreenScript.ChangeUsername(userInGameData.username);
            UIManager.Instance.MainMenuScreen.ChangeCoinsAmount(userInGameData.coins);
            UIManager.Instance.GameModeSelection.ResetScore(userInGameData.last_score, userInGameData.high_score);
            CharacterSelectManager.Instance.CharactersUIInfoInitialization();
            UIManager.Instance.CharactersSection.TurnOff();
            UIManager.Instance.ShopSection.TurnOff();
            UIManager.Instance.LeaderboardDailySectionManager.AchievementBoxManager.RefreshAchievements();

            yield return _waitForLoadingData;
            
            UIManager.Instance.LoadingBackground.TurnOff();
        }
    }
}