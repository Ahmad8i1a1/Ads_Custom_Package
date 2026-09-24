using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MobileAdsPackage
{
    /// <summary>
    /// Persistent Singleton engine managing ad provider lifecycles, mediation fallback,
    /// cooldown timers, auto-reloading with exponential backoff, and Remove-Ads persistence.
    /// </summary>
    public class MobileAdsManager : MonoBehaviour
    {
        public const string PREFS_NO_ADS_KEY = "MobileAds_NoAdsPurchased";

        private static MobileAdsManager _instance;
        public static MobileAdsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var existing = FindObjectOfType<MobileAdsManager>();
                    if (existing != null)
                    {
                        _instance = existing;
                    }
                    else
                    {
                        var go = new GameObject("[MobileAds_Manager]");
                        _instance = go.AddComponent<MobileAdsManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [SerializeField]
        private AdSettings _settings;
        public AdSettings Settings => _settings != null ? _settings : AdSettings.Instance;

        // Providers
        public IAdProvider PrimaryProvider { get; private set; }
        public IAdProvider FallbackProvider { get; private set; }
        public IAdProvider ActiveProvider => PrimaryProvider;

        // State
        public bool IsInitialized { get; private set; }
        public bool IsNoAdsPurchased { get; private set; }
        private float _lastInterstitialTime = -9999f;
        private readonly Dictionary<AdType, int> _retryCounts = new Dictionary<AdType, int>();

        // High-level Events
        public event Action<AdInitResult> OnInitialized;
        public event Action<AdType> OnAdLoaded;
        public event Action<AdErrorInfo> OnAdFailedToLoad;
        public event Action<AdType> OnAdOpened;
        public event Action<AdType> OnAdClosed;
        public event Action<AdReward> OnUserEarnedReward;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (_settings == null)
            {
                _settings = AdSettings.Instance;
            }

            // Load No-Ads state
            IsNoAdsPurchased = PlayerPrefs.GetInt(PREFS_NO_ADS_KEY, 0) == 1;

            if (_settings.autoInitializeOnStart)
            {
                Initialize();
            }
        }

        #region Initialization

        /// <summary>
        /// Initializes the ad system using current settings.
        /// </summary>
        public void Initialize(Action<AdInitResult> onComplete = null)
        {
            if (IsInitialized)
            {
                onComplete?.Invoke(new AdInitResult(true, "Already initialized.", PrimaryProvider?.NetworkType ?? AdNetwork.MockSimulator));
                return;
            }

            // In Editor, use Simulator if configured
            if (Application.isEditor && Settings.useSimulatorInEditor)
            {
                PrimaryProvider = new MockAdProvider();
            }
            else
            {
                PrimaryProvider = CreateProvider(Settings.primaryNetwork);
            }

            if (Settings.enableFallback)
            {
                FallbackProvider = CreateProvider(Settings.fallbackNetwork);
            }

            RegisterProviderEvents(PrimaryProvider);

            PrimaryProvider.Initialize(Settings, result =>
            {
                IsInitialized = result.Success;

                if (result.Success)
                {
                    if (Settings.enableDebugLogs)
                    {
                        Debug.Log($"<color=#33FF88>[MobileAds] Initialization Successful ({result.ActiveNetwork})</color>");
                    }

                    if (Settings.autoLoadAds)
                    {
                        AutoLoadConfiguredAds();
                    }
                }
                else
                {
                    Debug.LogWarning($"[MobileAds] Primary provider failed to initialize: {result.Message}");

                    // Try fallback provider if configured
                    if (FallbackProvider != null)
                    {
                        Debug.Log("[MobileAds] Attempting Fallback provider initialization...");
                        RegisterProviderEvents(FallbackProvider);
                        PrimaryProvider = FallbackProvider;
                        PrimaryProvider.Initialize(Settings, fallbackResult =>
                        {
                            IsInitialized = fallbackResult.Success;
                            if (fallbackResult.Success && Settings.autoLoadAds)
                            {
                                AutoLoadConfiguredAds();
                            }
                            OnInitialized?.Invoke(fallbackResult);
                            onComplete?.Invoke(fallbackResult);
                        });
                        return;
                    }
                }

                OnInitialized?.Invoke(result);
                onComplete?.Invoke(result);
            });
        }

        private IAdProvider CreateProvider(AdNetwork network)
        {
            switch (network)
            {
                case AdNetwork.GoogleMobileAds:
                    return new AdMobProvider();
                case AdNetwork.UnityAds:
                    return new UnityAdsProvider();
                case AdNetwork.MockSimulator:
                default:
                    return new MockAdProvider();
            }
        }

        private void RegisterProviderEvents(IAdProvider provider)
        {
            if (provider == null) return;

            provider.OnAdLoaded += type =>
            {
                _retryCounts[type] = 0; // reset retry counter on success
                OnAdLoaded?.Invoke(type);
            };

            provider.OnAdFailedToLoad += error =>
            {
                HandleAdFailedToLoad(error);
                OnAdFailedToLoad?.Invoke(error);
            };

            provider.OnAdOpened += type =>
            {
                if (type == AdType.Interstitial)
                {
                    _lastInterstitialTime = Time.realtimeSinceStartup;
                }
                OnAdOpened?.Invoke(type);
            };

            provider.OnAdClosed += type =>
            {
                OnAdClosed?.Invoke(type);
            };

            provider.OnUserEarnedReward += reward =>
            {
                OnUserEarnedReward?.Invoke(reward);
            };
        }

        private void AutoLoadConfiguredAds()
        {
            if (!IsNoAdsPurchased)
            {
                LoadBanner(BannerPosition.Bottom, BannerType.Adaptive);
                LoadInterstitial();
            }
            LoadRewarded();
            LoadAppOpen();
        }

        #endregion

        #region Banner Ads

        public void LoadBanner(BannerPosition position = BannerPosition.Bottom, BannerType type = BannerType.Adaptive)
        {
            if (IsNoAdsPurchased) return;
            PrimaryProvider?.LoadBanner(position, type);
        }

        public void ShowBanner()
        {
            if (IsNoAdsPurchased)
            {
                HideBanner();
                return;
            }
            PrimaryProvider?.ShowBanner();
        }

        public void HideBanner()
        {
            PrimaryProvider?.HideBanner();
        }

        public void DestroyBanner()
        {
            PrimaryProvider?.DestroyBanner();
        }

        public bool IsBannerShowing()
        {
            return PrimaryProvider != null && PrimaryProvider.IsBannerShowing;
        }

        #endregion

        #region Interstitial Ads

        public void LoadInterstitial()
        {
            if (IsNoAdsPurchased) return;
            PrimaryProvider?.LoadInterstitial();
        }

        public bool IsInterstitialReady()
        {
            if (IsNoAdsPurchased) return false;
            return PrimaryProvider != null && PrimaryProvider.IsInterstitialReady;
        }

        public bool CanShowInterstitial()
        {
            if (IsNoAdsPurchased) return false;
            if (!IsInterstitialReady()) return false;
            float elapsed = Time.realtimeSinceStartup - _lastInterstitialTime;
            return elapsed >= Settings.interstitialCooldownSeconds;
        }

        public void ShowInterstitial(Action onClosed = null)
        {
            if (IsNoAdsPurchased)
            {
                if (Settings.enableDebugLogs) Debug.Log("[MobileAds] Interstitial skipped: 'Remove Ads' is active.");
                onClosed?.Invoke();
                return;
            }

            if (!CanShowInterstitial())
            {
                float remaining = Settings.interstitialCooldownSeconds - (Time.realtimeSinceStartup - _lastInterstitialTime);
                if (remaining > 0f)
                {
                    Debug.LogWarning($"[MobileAds] Interstitial on cooldown. {remaining:0.0}s remaining.");
                }
                else
                {
                    Debug.LogWarning("[MobileAds] Interstitial is not loaded or ready.");
                }
                onClosed?.Invoke();
                return;
            }

            _lastInterstitialTime = Time.realtimeSinceStartup;
            PrimaryProvider.ShowInterstitial(onClosed);
        }

        #endregion

        #region Rewarded Ads

        public void LoadRewarded()
        {
            PrimaryProvider?.LoadRewarded();
        }

        public bool IsRewardedReady()
        {
            return PrimaryProvider != null && PrimaryProvider.IsRewardedReady;
        }

        public void ShowRewarded(Action<AdReward> onRewarded, Action onClosed = null)
        {
            if (!IsRewardedReady())
            {
                Debug.LogWarning("[MobileAds] Rewarded ad not ready.");
                onClosed?.Invoke();
                return;
            }

            PrimaryProvider.ShowRewarded(onRewarded, onClosed);
        }

        #endregion

        #region App Open Ads

        public void LoadAppOpen()
        {
            if (IsNoAdsPurchased) return;
            PrimaryProvider?.LoadAppOpen();
        }

        public bool IsAppOpenReady()
        {
            if (IsNoAdsPurchased) return false;
            return PrimaryProvider != null && PrimaryProvider.IsAppOpenReady;
        }

        public void ShowAppOpen(Action onClosed = null)
        {
            if (IsNoAdsPurchased)
            {
                onClosed?.Invoke();
                return;
            }

            if (!IsAppOpenReady())
            {
                onClosed?.Invoke();
                return;
            }

            PrimaryProvider.ShowAppOpen(onClosed);
        }

        #endregion

        #region Consent & GDPR

        public void RequestConsent(Action<bool, string> onComplete = null)
        {
            PrimaryProvider?.RequestConsent(onComplete);
        }

        public void ShowConsentForm(Action<bool, string> onComplete = null)
        {
            PrimaryProvider?.ShowConsentForm(onComplete);
        }

        #endregion

        #region Remove Ads (IAP Support)

        /// <summary>
        /// Sets the Remove-Ads status. When enabled, banners and interstitials are permanently disabled.
        /// Rewarded ads remain accessible for user opt-in bonuses.
        /// </summary>
        public void SetRemoveAds(bool remove)
        {
            IsNoAdsPurchased = remove;
            PlayerPrefs.SetInt(PREFS_NO_ADS_KEY, remove ? 1 : 0);
            PlayerPrefs.Save();

            if (remove)
            {
                HideBanner();
                DestroyBanner();
            }

            if (Settings.enableDebugLogs)
            {
                Debug.Log($"<color=#33FF88>[MobileAds] Remove Ads status set to: {remove}</color>");
            }
        }

        #endregion

        #region Auto-Reload & Exponential Backoff

        private void HandleAdFailedToLoad(AdErrorInfo error)
        {
            if (!_retryCounts.ContainsKey(error.AdType))
            {
                _retryCounts[error.AdType] = 0;
            }

            int count = _retryCounts[error.AdType];
            if (count < Settings.maxRetryAttempts)
            {
                _retryCounts[error.AdType] = count + 1;
                float delay = Settings.retryDelayBaseSeconds * Mathf.Pow(1.8f, count);
                if (Settings.enableDebugLogs)
                {
                    Debug.Log($"[MobileAds] Scheduling retry for {error.AdType} in {delay:0.0}s (Attempt {count + 1}/{Settings.maxRetryAttempts})");
                }
                StartCoroutine(RetryLoadRoutine(error.AdType, delay));
            }
            else
            {
                if (Settings.enableDebugLogs)
                {
                    Debug.LogWarning($"[MobileAds] Max retry attempts ({Settings.maxRetryAttempts}) reached for {error.AdType}. Pausing reloads until requested.");
                }
            }
        }

        private IEnumerator RetryLoadRoutine(AdType type, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            switch (type)
            {
                case AdType.Banner:
                    if (!IsNoAdsPurchased) LoadBanner();
                    break;
                case AdType.Interstitial:
                    if (!IsNoAdsPurchased) LoadInterstitial();
                    break;
                case AdType.Rewarded:
                    LoadRewarded();
                    break;
                case AdType.AppOpen:
                    if (!IsNoAdsPurchased) LoadAppOpen();
                    break;
            }
        }

        #endregion

        #region Application Pause & Resume (App Open)

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                // Returning to foreground
                if (Settings.autoShowAppOpenOnResume && !IsNoAdsPurchased && IsAppOpenReady())
                {
                    ShowAppOpen();
                }
            }
        }

        #endregion
    }
}
