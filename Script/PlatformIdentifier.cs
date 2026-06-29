using UnityEngine;
using System;
using System.Threading.Tasks;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Game
{
    public static class PlatformIdentifier
    {
        private const string CACHED_IDFV_KEY = "cached_idfv";
        private const string CACHED_APPSETID_KEY = "cached_appsetid";

        /// <summary>
        /// Get IDFV on iOS. Returns null on other platforms.
        /// Resets when all apps from the vendor are uninstalled.
        /// </summary>
        public static string GetIDFV()
        {
#if UNITY_IOS && !UNITY_EDITOR
            string idfv = Device.vendorIdentifier;
            if (!string.IsNullOrEmpty(idfv) && idfv != "00000000-0000-0000-0000-000000000000")
            {
                PlayerPrefs.SetString(CACHED_IDFV_KEY, idfv);
                PlayerPrefs.Save();
                return idfv;
            }
            string cached = PlayerPrefs.GetString(CACHED_IDFV_KEY, null);
            return string.IsNullOrEmpty(cached) ? null : cached;
#else
            return null;
#endif
        }

        /// <summary>
        /// Get App Set ID on Android. Returns null on other platforms.
        /// Delegates to AppSetIdService and caches in PlayerPrefs.
        /// </summary>
        public static async Task<string> GetAppSetIdAsync()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                string appSetId = await AppSetIdService.GetAppSetIdAsync();
                if (!string.IsNullOrEmpty(appSetId))
                {
                    PlayerPrefs.SetString(CACHED_APPSETID_KEY, appSetId);
                    PlayerPrefs.Save();
                    return appSetId;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to get App Set ID: {e.Message}");
            }

            string cached = PlayerPrefs.GetString(CACHED_APPSETID_KEY, null);
            return string.IsNullOrEmpty(cached) ? null : cached;
#else
            await Task.CompletedTask;
            return null;
#endif
        }

        /// <summary>
        /// Get cached App Set ID synchronously.
        /// </summary>
        public static string GetCachedAppSetId()
        {
#if UNITY_ANDROID
            string cached = PlayerPrefs.GetString(CACHED_APPSETID_KEY, null);
            return string.IsNullOrEmpty(cached) ? null : cached;
#else
            return null;
#endif
        }
    }
}
