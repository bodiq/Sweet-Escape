using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace API
{
    // LoginAPIData myDeserializedClass = JsonConvert.DeserializeObject<LoginAPIData>(myJsonResponse);
    [Serializable] [CanBeNull]
    public class LoginAPIData
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public UserAPIData userAPIData { get; set; }
    }

    [Serializable] [CanBeNull]
    public class UserAPIData
    {
        public string uid { get; set; }
        public int points { get; set; }
        public string email { get; set; }
        public string image_url { get; set; }
        public DateTime daily_task_ts { get; set; }
        public int daily_task_exp_hours { get; set; }
        public string referred_by { get; set; }
        public int referral_count { get; set; }
        public int second_level_referrals { get; set; }
        public List<object> referrals { get; set; }
        public string x_id { get; set; }
        public string x_username { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public bool x_followed { get; set; }
        public bool X_followed_tom { get; set; }
        public bool X_followed_ht { get; set; }
        public bool X_followed_alex { get; set; }
        public bool discord_booster { get; set; }
        public string discord_id { get; set; }
        public bool discord_joined { get; set; }
        public string discord_roles { get; set; }
        public bool kickstarter_backer { get; set; }
        public int tier { get; set; }
        public float multiplier { get; set; }
        public string wallet_address { get; set; }
    }
    
    [Serializable] [CanBeNull]
    public class UserAchievementsData
    {
        public string user_uid { get; set; }
        public bool fart { get; set; }
        public bool unlock_character { get; set; }
        public bool god_mode { get; set; }
        public bool revive_5 { get; set; }
        public bool level_5 { get; set; }
        public bool score_5000 { get; set; }
        public bool spend_2000 { get; set; }
        public bool spend_500 { get; set; }
        public bool score_1000 { get; set; }
        public bool spend_200 { get; set; }
        public bool score_500 { get; set; }
        public bool score_2000 { get; set; }
        public bool unlock_7th { get; set; }
        public bool collect_5k { get; set; }
        public bool collect_10k { get; set; }
        public bool revive { get; set; }
        public bool level_2 { get; set; }
        public bool collect_1k { get; set; }
        public bool first_game { get; set; }
    }

    [Serializable] [CanBeNull]
    public class RefreshTokenData
    {
        public string refreshed_accessToken { get; set; }
    }
    
        
    [Serializable] [CanBeNull]
    public class CurrentUser
    {
        public int position { get; set; }
        public int high_score { get; set; }
        public int total_score_position { get; set; }
        public int total_score { get; set; }
        public string username { get; set; }
    }

        
    [Serializable] [CanBeNull]
    public class HighScoreLeaderboard
    {
        public int position { get; set; }
        public int high_score { get; set; }
        public int total_score { get; set; }
        public string username { get; set; }
    }

        
    [Serializable] [CanBeNull]
    public class LeaderboardData
    {
        public CurrentUser current_user { get; set; }
        public List<HighScoreLeaderboard> high_score_leaderboard { get; set; }
        public List<TotalScoreLeaderboard> total_score_leaderboard { get; set; }
    }

        
    [Serializable] [CanBeNull]
    public class TotalScoreLeaderboard
    {
        public int position { get; set; }
        public int high_score { get; set; }
        public int total_score { get; set; }
        public string username { get; set; }
    }
    
    [Serializable] [CanBeNull]
    public class UserInGameData
    {
        public string user_uid { get; set; }
        public int hours_played { get; set; }
        public int high_score { get; set; }
        public int last_score { get; set; }
        public int total_score { get; set; }
        public string username { get; set; }
        public int missions_completed { get; set; }
        public DateTime last_played { get; set; }
        public int coins { get; set; }
        
        public int noob_lvl { get; set; }
        public int kermit_lvl { get; set; }
        public int meltie_lvl { get; set; }
        
        public bool noob_mintyfresh { get; set; }
        public bool noob_ogperp { get; set; }
        public bool noob_antinoob { get; set; }
        
        public bool kermit_seasamegreen { get; set; }
        public bool kermit_tropical { get; set; }
        public bool kermit_toxic { get; set; }
        
        public bool meltie_hotcream { get; set; }
        public bool meltie_burntcrisp { get; set; }
        public bool meltie_rockefuel { get; set; }
        
        public int shields { get; set; }
        public int magnet_lvl { get; set; }
        public int chillblast_lvl { get; set; }
        public int goldenspoon_lvl { get; set; }
        public int hundathous_lvl { get; set; }
        
        public int dailyloginday { get; set; }
    }
}