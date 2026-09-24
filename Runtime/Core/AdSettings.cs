using System;
using UnityEngine;

namespace MobileAdsPackage
{
    /// <summary>
    /// Global configuration asset for Mobile Ads Manager.
    /// Loaded automatically from Resources/MobileAdsSettings.
    /// </summary>
    [CreateAssetMenu(fileName = "MobileAdsSettings", menuName = "Mobile Ads/Settings Asset", order = 1)]
    public class AdSettings : ScriptableObject
    {
        public const string RESOURCE_PATH = "MobileAdsSettings";

        private static AdSettings _instance;
        public static AdSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<AdSettings>(RESOURCE_PATH);
                    if (_instance == null)
                    {
                        _instance = CreateInstance<AdSettings>();
#if UNITY_EDITOR
                        Debug.LogWarning("[MobileAds] No MobileAdsSettings asset found in Resources. Using default in-memory settings. Use Tools > Mobile Ads > Settings to generate one.");
#endif
                    }
                }
                return _instance;
            }
        }

        [Header("General Settings")]
        [Tooltip("Primary ad network provider.")]
        public AdNetwork primaryNetwork = AdNetwork.GoogleMobileAds;

        [Tooltip("If primary network fails to load or is unavailable, use this fallback network.")]
        public bool enableFallback = true;

        [Tooltip("Fallback ad network provider.")]
        public AdNetwork fallbackNetwork = AdNetwork.MockSimulator;

        [Tooltip("Enable Test Mode to use Google/Unity official test ad unit IDs.")]
        public bool testMode = true;

        [Tooltip("Automatically initialize Mobile Ads when MobileAdsManager starts.")]
        public bool autoInitializeOnStart = true;

        [Tooltip("Automatically request ad loads upon successful initialization.")]
        public bool autoLoadAds = true;

        [Tooltip("Display detailed internal debug logs in Unity Console.")]
        public bool enableDebugLogs = true;

        [Tooltip("Minimum time (in seconds) that must pass between showing interstitial ads.")]
        [Range(0f, 300f)]
        public float interstitialCooldownSeconds = 30f;

        [Tooltip("Base delay in seconds for exponential backoff retry on failed ad loads.")]
        [Range(1f, 60f)]
        public float retryDelayBaseSeconds = 3f;

        [Tooltip("Maximum retry attempts before pausing reload until manual call.")]
        [Range(1, 10)]
        public int maxRetryAttempts = 5;

        [Tooltip("Automatically show App Open Ad when the game returns from background pause.")]
        public bool autoShowAppOpenOnResume = false;

        [Header("Google Mobile Ads (AdMob) - Android")]
        public string androidAppId = "ca-app-pub-3940256099942544~3347511713";
        public string androidBannerId = "ca-app-pub-3940256099942544/6300978111";
        public string androidInterstitialId = "ca-app-pub-3940256099942544/1033173712";
        public string androidRewardedId = "ca-app-pub-3940256099942544/5224354917";
        public string androidRewardedInterstitialId = "ca-app-pub-3940256099942544/5354046379";
        public string androidAppOpenId = "ca-app-pub-3940256099942544/9257390721";

        [Header("Google Mobile Ads (AdMob) - iOS")]
        public string iosAppId = "ca-app-pub-3940256099942544~1458602516";
        public string iosBannerId = "ca-app-pub-3940256099942544/2934735716";
        public string iosInterstitialId = "ca-app-pub-3940256099942544/4411468910";
        public string iosRewardedId = "ca-app-pub-3940256099942544/1712485313";
        public string iosRewardedInterstitialId = "ca-app-pub-3940256099942544/6978759866";
        public string iosAppOpenId = "ca-app-pub-3940256099942544/5575463023";

        [Header("Unity Ads Settings")]
        public string unityAndroidGameId = "1234567";
        public string unityIosGameId = "1234568";
        public string unityBannerPlacement = "Banner_Android";
        public string unityInterstitialPlacement = "Interstitial_Android";
        public string unityRewardedPlacement = "Rewarded_Android";
        public bool unityTestMode = true;

        [Header("Privacy & Compliance (GDPR, COPPA, CCPA)")]
        [Tooltip("Tag for child-directed treatment (COPPA compliance).")]
        public bool tagForChildDirectedTreatment = false;

        [Tooltip("Tag for users under the age of consent.")]
        public bool tagForUnderAgeOfConsent = false;

        [Tooltip("Automatically request Google UMP consent form upon initialization.")]
        public bool autoRequestUmpConsent = true;

        [Header("Editor Ad Simulator")]
        [Tooltip("Use the visual interactive in-Editor simulator when running inside the Unity Editor.")]
        public bool useSimulatorInEditor = true;

        [Tooltip("Simulated load delay in seconds for mock ad responses.")]
        [Range(0.1f, 5f)]
        public float simulatedLoadDelay = 0.5f;

        [Tooltip("Countdown duration for simulated interstitial auto-close button.")]
        [Range(1f, 10f)]
        public float simulatedInterstitialCountdown = 3f;

        [Tooltip("Duration in seconds required to watch simulated rewarded ad.")]
        [Range(1f, 15f)]
        public float simulatedRewardedDuration = 5f;

        #region Official Test IDs Constants
        public const string TEST_ANDROID_BANNER = "ca-app-pub-3940256099942544/6300978111";
        public const string TEST_ANDROID_INTERSTITIAL = "ca-app-pub-3940256099942544/1033173712";
        public const string TEST_ANDROID_REWARDED = "ca-app-pub-3940256099942544/5224354917";
        public const string TEST_ANDROID_REWARDED_INTERSTITIAL = "ca-app-pub-3940256099942544/5354046379";
        public const string TEST_ANDROID_APP_OPEN = "ca-app-pub-3940256099942544/9257390721";

        public const string TEST_IOS_BANNER = "ca-app-pub-3940256099942544/2934735716";
        public const string TEST_IOS_INTERSTITIAL = "ca-app-pub-3940256099942544/4411468910";
        public const string TEST_IOS_REWARDED = "ca-app-pub-3940256099942544/1712485313";
        public const string TEST_IOS_REWARDED_INTERSTITIAL = "ca-app-pub-3940256099942544/6978759866";
        public const string TEST_IOS_APP_OPEN = "ca-app-pub-3940256099942544/5575463023";
        #endregion

        /// <summary>
        /// Retrieves the correct AdMob Ad Unit ID for the current platform and test mode status.
        /// </summary>
        public string GetAdMobUnitId(AdType adType)
        {
#if UNITY_IOS
            if (testMode)
            {
                switch (adType)
                {
                    case AdType.Banner: return TEST_IOS_BANNER;
                    case AdType.Interstitial: return TEST_IOS_INTERSTITIAL;
                    case AdType.Rewarded: return TEST_IOS_REWARDED;
                    case AdType.RewardedInterstitial: return TEST_IOS_REWARDED_INTERSTITIAL;
                    case AdType.AppOpen: return TEST_IOS_APP_OPEN;
                }
            }
            switch (adType)
            {
                case AdType.Banner: return iosBannerId;
                case AdType.Interstitial: return iosInterstitialId;
                case AdType.Rewarded: return iosRewardedId;
                case AdType.RewardedInterstitial: return iosRewardedInterstitialId;
                case AdType.AppOpen: return iosAppOpenId;
            }
#else
            // Default to Android / Editor
            if (testMode)
            {
                switch (adType)
                {
                    case AdType.Banner: return TEST_ANDROID_BANNER;
                    case AdType.Interstitial: return TEST_ANDROID_INTERSTITIAL;
                    case AdType.Rewarded: return TEST_ANDROID_REWARDED;
                    case AdType.RewardedInterstitial: return TEST_ANDROID_REWARDED_INTERSTITIAL;
                    case AdType.AppOpen: return TEST_ANDROID_APP_OPEN;
                }
            }
            switch (adType)
            {
                case AdType.Banner: return androidBannerId;
                case AdType.Interstitial: return androidInterstitialId;
                case AdType.Rewarded: return androidRewardedId;
                case AdType.RewardedInterstitial: return androidRewardedInterstitialId;
                case AdType.AppOpen: return androidAppOpenId;
            }
#endif
            return string.Empty;
        }

        /// <summary>
        /// Helper to fill in Google's official test IDs.
        /// </summary>
        public void PopulateOfficialTestIDs()
        {
            androidBannerId = TEST_ANDROID_BANNER;
            androidInterstitialId = TEST_ANDROID_INTERSTITIAL;
            androidRewardedId = TEST_ANDROID_REWARDED;
            androidRewardedInterstitialId = TEST_ANDROID_REWARDED_INTERSTITIAL;
            androidAppOpenId = TEST_ANDROID_APP_OPEN;

            iosBannerId = TEST_IOS_BANNER;
            iosInterstitialId = TEST_IOS_INTERSTITIAL;
            iosRewardedId = TEST_IOS_REWARDED;
            iosRewardedInterstitialId = TEST_IOS_REWARDED_INTERSTITIAL;
            iosAppOpenId = TEST_IOS_APP_OPEN;
        }
    }
}
