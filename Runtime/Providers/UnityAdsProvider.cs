using System;
using UnityEngine;

#if MOBILE_ADS_UNITY
using UnityEngine.Advertisements;
#endif

namespace MobileAdsPackage
{
    /// <summary>
    /// Unity Advertisements adapter.
    /// Enabled via scripting define symbol 'MOBILE_ADS_UNITY' or by installing 'com.unity.ads' package.
    /// </summary>
    public class UnityAdsProvider : IAdProvider
#if MOBILE_ADS_UNITY
        , IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
#endif
    {
        public AdNetwork NetworkType => AdNetwork.UnityAds;
        public bool IsInitialized { get; private set; }
        public bool IsBannerShowing { get; private set; }
        public bool IsInterstitialReady { get; private set; }
        public bool IsRewardedReady { get; private set; }
        public bool IsAppOpenReady => false; // Unity Ads does not have native App Open format

        public event Action<AdType> OnAdLoaded;
        public event Action<AdErrorInfo> OnAdFailedToLoad;
        public event Action<AdType> OnAdOpened;
        public event Action<AdType> OnAdClosed;
        public event Action<AdReward> OnUserEarnedReward;

        private AdSettings _settings;
        private Action<AdInitResult> _initCallback;
        private Action _interstitialCloseCallback;
        private Action<AdReward> _rewardSuccessCallback;
        private Action _rewardCloseCallback;

        public void Initialize(AdSettings settings, Action<AdInitResult> onComplete)
        {
            _settings = settings;
            _initCallback = onComplete;

#if MOBILE_ADS_UNITY
            string gameId = "";
#if UNITY_IOS
            gameId = settings.unityIosGameId;
#else
            gameId = settings.unityAndroidGameId;
#endif

            if (string.IsNullOrEmpty(gameId))
            {
                Debug.LogError("[MobileAds-UnityAds] Game ID is empty.");
                onComplete?.Invoke(new AdInitResult(false, "Unity Game ID empty", AdNetwork.UnityAds));
                return;
            }

            Advertisement.Initialize(gameId, settings.unityTestMode, this);
#else
            Debug.LogWarning("[MobileAds-UnityAds] Unity Ads SDK is not installed or 'MOBILE_ADS_UNITY' is not defined.");
            onComplete?.Invoke(new AdInitResult(false, "Unity Ads SDK not installed", AdNetwork.UnityAds));
#endif
        }

        #region Banner Ads

        public void LoadBanner(BannerPosition position, BannerType type)
        {
#if MOBILE_ADS_UNITY
            if (!IsInitialized) return;

            Advertisement.Banner.SetPosition(ConvertBannerPosition(position));
            var options = new BannerLoadOptions
            {
                loadCallback = () =>
                {
                    if (_settings.enableDebugLogs) Debug.Log("[MobileAds-UnityAds] Banner loaded.");
                    OnAdLoaded?.Invoke(AdType.Banner);
                    ShowBanner();
                },
                errorCallback = (msg) =>
                {
                    Debug.LogError($"[MobileAds-UnityAds] Banner load failed: {msg}");
                    OnAdFailedToLoad?.Invoke(new AdErrorInfo(-1, msg, AdType.Banner, AdNetwork.UnityAds));
                }
            };

            Advertisement.Banner.Load(_settings.unityBannerPlacement, options);
#endif
        }

        public void ShowBanner()
        {
#if MOBILE_ADS_UNITY
            var options = new BannerOptions
            {
                showCallback = () =>
                {
                    IsBannerShowing = true;
                    OnAdOpened?.Invoke(AdType.Banner);
                },
                hideCallback = () =>
                {
                    IsBannerShowing = false;
                    OnAdClosed?.Invoke(AdType.Banner);
                }
            };
            Advertisement.Banner.Show(_settings.unityBannerPlacement, options);
#endif
        }

        public void HideBanner()
        {
#if MOBILE_ADS_UNITY
            Advertisement.Banner.Hide(false);
            IsBannerShowing = false;
            OnAdClosed?.Invoke(AdType.Banner);
#endif
        }

        public void DestroyBanner()
        {
#if MOBILE_ADS_UNITY
            Advertisement.Banner.Hide(true);
            IsBannerShowing = false;
#endif
        }

        #endregion

        #region Interstitial Ads

        public void LoadInterstitial()
        {
#if MOBILE_ADS_UNITY
            if (!IsInitialized) return;
            Advertisement.Load(_settings.unityInterstitialPlacement, this);
#endif
        }

        public void ShowInterstitial(Action onClosed)
        {
#if MOBILE_ADS_UNITY
            if (IsInterstitialReady)
            {
                _interstitialCloseCallback = onClosed;
                IsInterstitialReady = false;
                Advertisement.Show(_settings.unityInterstitialPlacement, this);
            }
            else
            {
                Debug.LogWarning("[MobileAds-UnityAds] Interstitial not ready.");
                onClosed?.Invoke();
            }
#else
            onClosed?.Invoke();
#endif
        }

        #endregion

        #region Rewarded Ads

        public void LoadRewarded()
        {
#if MOBILE_ADS_UNITY
            if (!IsInitialized) return;
            Advertisement.Load(_settings.unityRewardedPlacement, this);
#endif
        }

        public void ShowRewarded(Action<AdReward> onRewarded, Action onClosed)
        {
#if MOBILE_ADS_UNITY
            if (IsRewardedReady)
            {
                _rewardSuccessCallback = onRewarded;
                _rewardCloseCallback = onClosed;
                IsRewardedReady = false;
                Advertisement.Show(_settings.unityRewardedPlacement, this);
            }
            else
            {
                Debug.LogWarning("[MobileAds-UnityAds] Rewarded ad not ready.");
                onClosed?.Invoke();
            }
#else
            onClosed?.Invoke();
#endif
        }

        #endregion

        #region App Open Ads

        public void LoadAppOpen()
        {
            // Not supported in Unity Ads
        }

        public void ShowAppOpen(Action onClosed)
        {
            onClosed?.Invoke();
        }

        #endregion

        #region Consent

        public void RequestConsent(Action<bool, string> onComplete)
        {
            onComplete?.Invoke(true, "Unity Ads uses built-in or network consent.");
        }

        public void ShowConsentForm(Action<bool, string> onComplete)
        {
            onComplete?.Invoke(true, "Unity Ads consent form not applicable.");
        }

        #endregion

#if MOBILE_ADS_UNITY
        #region Unity Ads Callbacks

        public void OnInitializationComplete()
        {
            IsInitialized = true;
            if (_settings.enableDebugLogs) Debug.Log("<color=#44FF77>[MobileAds-UnityAds] Initialization complete.</color>");
            _initCallback?.Invoke(new AdInitResult(true, "Unity Ads initialized", AdNetwork.UnityAds));
            _initCallback = null;
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            IsInitialized = false;
            Debug.LogError($"[MobileAds-UnityAds] Initialization failed: {error} - {message}");
            _initCallback?.Invoke(new AdInitResult(false, message, AdNetwork.UnityAds));
            _initCallback = null;
        }

        public void OnUnityAdsAdLoaded(string placementId)
        {
            if (placementId == _settings.unityInterstitialPlacement)
            {
                IsInterstitialReady = true;
                OnAdLoaded?.Invoke(AdType.Interstitial);
            }
            else if (placementId == _settings.unityRewardedPlacement)
            {
                IsRewardedReady = true;
                OnAdLoaded?.Invoke(AdType.Rewarded);
            }
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            var adType = placementId == _settings.unityRewardedPlacement ? AdType.Rewarded : AdType.Interstitial;
            Debug.LogError($"[MobileAds-UnityAds] Failed to load {placementId}: {error} - {message}");
            OnAdFailedToLoad?.Invoke(new AdErrorInfo((int)error, message, adType, AdNetwork.UnityAds));
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            var adType = placementId == _settings.unityRewardedPlacement ? AdType.Rewarded : AdType.Interstitial;
            Debug.LogError($"[MobileAds-UnityAds] Failed to show {placementId}: {error} - {message}");
            OnAdClosed?.Invoke(adType);

            if (placementId == _settings.unityRewardedPlacement)
            {
                var cb = _rewardCloseCallback;
                _rewardCloseCallback = null;
                _rewardSuccessCallback = null;
                cb?.Invoke();
                LoadRewarded();
            }
            else
            {
                var cb = _interstitialCloseCallback;
                _interstitialCloseCallback = null;
                cb?.Invoke();
                LoadInterstitial();
            }
        }

        public void OnUnityAdsShowStart(string placementId)
        {
            var adType = placementId == _settings.unityRewardedPlacement ? AdType.Rewarded : AdType.Interstitial;
            OnAdOpened?.Invoke(adType);
        }

        public void OnUnityAdsShowClick(string placementId)
        {
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            var adType = placementId == _settings.unityRewardedPlacement ? AdType.Rewarded : AdType.Interstitial;
            OnAdClosed?.Invoke(adType);

            if (placementId == _settings.unityRewardedPlacement)
            {
                if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
                {
                    var reward = new AdReward("Reward", 1);
                    OnUserEarnedReward?.Invoke(reward);
                    _rewardSuccessCallback?.Invoke(reward);
                }

                var cb = _rewardCloseCallback;
                _rewardCloseCallback = null;
                _rewardSuccessCallback = null;
                cb?.Invoke();
                LoadRewarded();
            }
            else
            {
                var cb = _interstitialCloseCallback;
                _interstitialCloseCallback = null;
                cb?.Invoke();
                LoadInterstitial();
            }
        }

#if MOBILE_ADS_UNITY
        private UnityEngine.Advertisements.BannerPosition ConvertBannerPosition(BannerPosition position)
        {
            switch (position)
            {
                case BannerPosition.Top: return UnityEngine.Advertisements.BannerPosition.TOP_CENTER;
                case BannerPosition.Bottom: return UnityEngine.Advertisements.BannerPosition.BOTTOM_CENTER;
                case BannerPosition.TopLeft: return UnityEngine.Advertisements.BannerPosition.TOP_LEFT;
                case BannerPosition.TopRight: return UnityEngine.Advertisements.BannerPosition.TOP_RIGHT;
                case BannerPosition.BottomLeft: return UnityEngine.Advertisements.BannerPosition.BOTTOM_LEFT;
                case BannerPosition.BottomRight: return UnityEngine.Advertisements.BannerPosition.BOTTOM_RIGHT;
                case BannerPosition.Center: return UnityEngine.Advertisements.BannerPosition.CENTER;
                default: return UnityEngine.Advertisements.BannerPosition.BOTTOM_CENTER;
            }
        }
#endif

        #endregion
#endif
    }
}
