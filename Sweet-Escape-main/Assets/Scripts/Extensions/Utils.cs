using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using API;
using Enums;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace Extensions
{
    public static class Utils
    {
        public static readonly Dictionary<DirectionEnum, Vector2> Directions = new()
        {
            { DirectionEnum.Down, Vector2.down },
            { DirectionEnum.Up, Vector2.up },
            { DirectionEnum.Left, Vector2.left },
            { DirectionEnum.Right, Vector2.right }
        };

        public static Enum GetRandomEnumValue(this Type t)
        {
            return Enum.GetValues(t) // get values from Type provided
                .OfType<Enum>() // casts to Enum
                .OrderBy(e => Guid.NewGuid()) // mess with order of results
                .FirstOrDefault(); // take first item in result
        }

        public static void SetupKeys<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            IReadOnlyCollection<TKey> keys, TValue value = default)
        {
            foreach (var key in dictionary.Keys.Except(keys).ToArray())
            {
                dictionary.Remove(key);
            }

            foreach (var key in keys.Except(dictionary.Keys))
            {
                dictionary.Add(key, value);
            }
        }

        public static int GetTotalHoursPlayed()
        {
            var hoursPlayed = (DateTime.Now - GameManager.Instance.LastUpdatedTime).Minutes;
            GameManager.Instance.LastUpdatedTime = DateTime.Now;
            
            hoursPlayed += PlayerPrefs.GetInt(GameManager.TotalHoursPlayTimeKey);
            PlayerPrefs.SetInt(GameManager.TotalHoursPlayTimeKey, hoursPlayed);
            return hoursPlayed;
        }

        public static string GenerateRandomString(int length)
        {
            if (length <= 0)
                throw new Exception("Expected nonce to have positive length");

            const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
            var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
            var result = string.Empty;
            var remainingLength = length;

            var randomNumberHolder = new byte[1];
            while (remainingLength > 0)
            {
                var randomNumbers = new List<int>(16);
                for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
                {
                    cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                    randomNumbers.Add(randomNumberHolder[0]);
                }

                for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++)
                {
                    if (remainingLength == 0)
                    {
                        break;
                    }

                    var randomNumber = randomNumbers[randomNumberIndex];
                    if (randomNumber < charset.Length)
                    {
                        result += charset[randomNumber];
                        remainingLength--;
                    }
                }
            }

            return result;
        }
        
        public static string GenerateSHA256Nonce(string rawNonce)
        {
            var sha = new SHA256Managed();
            var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
            var hash = sha.ComputeHash(utf8RawNonce);

            var result = string.Empty;
            for (var i = 0; i < hash.Length; i++)
            {
                result += hash[i].ToString("x2");
            }

            return result;
        }

        private static Vector2 CalculateFocusedScrollPosition(this ScrollRect scrollView, Vector2 focusPoint)
        {
            var contentSize = scrollView.content.rect.size;
            var viewportSize = ((RectTransform)scrollView.content.parent).rect.size;
            var contentScale = scrollView.content.localScale;

            contentSize.Scale(contentScale);
            focusPoint.Scale(contentScale);

            var scrollPosition = scrollView.normalizedPosition;

            if (scrollView.horizontal && contentSize.x > viewportSize.x)
            {
                scrollPosition.x =
                    Mathf.Clamp01((focusPoint.x - viewportSize.x * 0.5f) / (contentSize.x - viewportSize.x));
            }

            if (scrollView.vertical && contentSize.y > viewportSize.y)
            {
                scrollPosition.y =
                    Mathf.Clamp01((focusPoint.y - viewportSize.y * 0.5f) / (contentSize.y - viewportSize.y));
            }

            return scrollPosition;
        }

        private static Vector2 CalculateFocusedScrollPosition(this ScrollRect scrollView, RectTransform item)
        {
            var pos = new Vector2(item.rect.center.x, item.rect.yMax - 500f);
            Vector2 itemFocusPoint = scrollView.content.InverseTransformPoint(item.TransformPoint(pos));

            var contentSizeOffset = scrollView.content.rect.size;
            contentSizeOffset.Scale(scrollView.content.pivot);

            return scrollView.CalculateFocusedScrollPosition(itemFocusPoint + contentSizeOffset);
        }

        public static byte[] GetUserInGameDataToBytes()
        {
            var userLocalInGameData = APIManager.Instance.userInGameData;
            var listString = "?hours_played=" + userLocalInGameData.hours_played +
                             "&high_score=" + userLocalInGameData.high_score +
                             "&last_score=" + userLocalInGameData.last_score +
                             "&total_score=" + userLocalInGameData.total_score +
                             "&username=" + userLocalInGameData.username +
                             "&missions_complete" + userLocalInGameData.missions_completed +
                             "&coins" + userLocalInGameData.coins +
                             "&noob_lvl" + userLocalInGameData.noob_lvl +
                             "&kermit_lvl" + userLocalInGameData.kermit_lvl +
                             "&meltie_lvl" + userLocalInGameData.meltie_lvl +
                             "&noob_mintyfresh" + userLocalInGameData.noob_mintyfresh +
                             "&noob_ogperp" + userLocalInGameData.noob_ogperp +
                             "&noob_antinoob" + userLocalInGameData.noob_antinoob +
                             "&kermit_seasamegreen" + userLocalInGameData.kermit_seasamegreen +
                             "&kermit_tropical" + userLocalInGameData.kermit_tropical +
                             "&kermit_toxic" + userLocalInGameData.kermit_toxic +
                             "&meltie_hotcream" + userLocalInGameData.meltie_hotcream +
                             "&meltie_burntcrisp" + userLocalInGameData.meltie_burntcrisp +
                             "&meltie_rockefuel" + userLocalInGameData.meltie_rockefuel +
                             "&shields" + userLocalInGameData.shields +
                             "&magnet_lvl" + userLocalInGameData.magnet_lvl +
                             "&chillblast_lvl" + userLocalInGameData.chillblast_lvl +
                             "&goldenspoon_lvl" + userLocalInGameData.goldenspoon_lvl +
                             "&hundathous_lvl" + userLocalInGameData.hundathous_lvl +
                             "&dailyloginday" + userLocalInGameData.dailyloginday;
            
            return Encoding.UTF8.GetBytes(listString);
        }

        public static void FocusOnItem(this ScrollRect scrollRect, RectTransform item)
        {
            scrollRect.normalizedPosition = scrollRect.CalculateFocusedScrollPosition(item);
        }

        public static Transform FindChildRecursively(this Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }

                var found = FindChildRecursively(child, childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public static void AddEmptyElements<T>(this List<T> list, int amount) where T : new()
        {
            for (int i = 0; i < amount; i++)
            {
                list.Add(new T());
            }
        }

        public static RoomInfo ParseRoomInfo(this string assetPath)
        {
            var roomInfo = new RoomInfo { Difficulty = LevelDifficultyEnum.None, Biome = BiomeEnum.None };

            var allDifficulties = (LevelDifficultyEnum[])Enum.GetValues(typeof(LevelDifficultyEnum));
            foreach (var difficulty in allDifficulties)
            {
                if (assetPath.Contains(difficulty.ToString()))
                {
                    roomInfo.Difficulty = difficulty;
                }
            }

            var allBiomes = (BiomeEnum[])Enum.GetValues(typeof(BiomeEnum));
            foreach (var biome in allBiomes)
            {
                if (assetPath.Contains(biome.ToString()))
                {
                    roomInfo.Biome = biome;
                }
            }

            return roomInfo;
        }
    }
}