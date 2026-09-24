using System;
using UnityEngine;

#if MOBILE_ADS_ADMOB
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
#endif

namespace MobileAdsPackage
{
    /// <summary>
    /// Google Mobile Ads (AdMob) adapter supporting Google Mobile Ads Unity SDK v8.x and v9.x+.
    /// Enabled via scripting define symbol 'MOBILE_ADS_ADMOB'.
    /// </summary>
    public class AdMobProvider : IAdProvider
    {
        public AdNetwork NetworkType => AdNetwork.GoogleMobileAds;
        public bool IsInitialized { get; private set; }
        public bool IsBannerShowing { get; private set; }

        public bool IsInterstitialReady
        {
            get
            {
#if MOBILE_ADS_ADMOB
                return _interstitialAd != null && _interstitialAd.CanShowAd();
#else
                return false;
#endif
            }
        }

        public bool IsRewardedReady
        {
            get
            {
#if MOBILE_ADS_ADMOB
                return _rewardedAd != null && _rewardedAd.CanShowAd();
#else
                return false;
#endif
            }
        }

        public bool IsAppOpenReady
        {
            get
            {
#if MOBILE_ADS_ADMOB
                return _appOpenAd != null && _appOpenAd.CanShowAd();
#else
                return false;
#endif
            }
        }

        public event Action<AdType> OnAdLoaded;
        public event Action<AdErrorInfo> OnAdFailedToLoad;
        public event Action<AdType> OnAdOpened;
        public event Action<AdType> OnAdClosed;
        public event Action<AdReward> OnUserEarnedReward;

        private AdSettings _settings;

#if MOBILE_ADS_ADMOB
        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;
        private AppOpenAd _appOpenAd;
#endif

        public void Initialize(AdSettings settings, Action<AdInitResult> onComplete)
        {
            _settings = settings;

#if MOBILE_ADS_ADMOB
            try
            {
                // Configure COPPA & Privacy tags
                var requestConfig = new RequestConfiguration();
                if (settings.tagForChildDirectedTreatment)
                {
                    requestConfig.TagForChildDirectedTreatment = TagForChildDirectedTreatment.True;
                }
                if (settings.tagForUnderAgeOfConsent)
                {
                    requestConfig.TagForUnderAgeOfConsent = TagForUnderAgeOfConsent.True;
                }
                MobileAds.SetRequestConfiguration(requestConfig);

                MobileAds.Initialize(initStatus =>
                {
                    IsInitialized = true;
                    if (_settings != null && _settings.enableDebugLogs)
                    {
                        Debug.Log("<color=#44FF77>[MobileAds-AdMob] Initialized successfully.</color>");
                    }

                    // Optional automatic UMP consent gather
                    if (_settings != null && _settings.autoRequestUmpConsent)
                    {
                        RequestConsent((success, msg) =>
                        {
                            onComplete?.Invoke(new AdInitResult(true, "AdMob Initialized & Consent Handled", AdNetwork.GoogleMobileAds));
                        });
                    }
                    else
                    {
                        onComplete?.Invoke(new AdInitResult(true, "AdMob Initialized", AdNetwork.GoogleMobileAds));
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileAds-AdMob] Initialization exception: {ex.Message}");
                onComplete?.Invoke(new AdInitResult(false, ex.Message, AdNetwork.GoogleMobileAds));
            }
#else
            Debug.LogWarning("[MobileAds-AdMob] Google Mobile Ads SDK is not installed or 'MOBILE_ADS_ADMOB' symbol is not defined. Use Tools > Mobile Ads > Settings to enable.");
            onComplete?.Invoke(new AdInitResult(false, "Google Mobile Ads SDK not defined (MOBILE_ADS_ADMOB)", AdNetwork.GoogleMobileAds));
#endif
        }

        #region Banner Ads

        public void LoadBanner(BannerPosition position, BannerType type)
        {
#if MOBILE_ADS_ADMOB
            if (!IsInitialized)
            {
                Debug.LogWarning("[MobileAds-AdMob] Cannot load banner before initialization.");
                return;
            }

            DestroyBanner();

            string adUnitId = _settings.GetAdMobUnitId(AdType.Banner);
            AdSize adSize = ConvertBannerType(type);
            AdPosition adPos = ConvertBannerPosition(position);

            _bannerView = new BannerView(adUnitId, adSize, adPos);

            _bannerView.OnBannerAdLoaded += () =>
            {
                IsBannerShowing = true;
                if (_settings.enableDebugLogs) Debug.Log("[MobileAds-AdMob] Banner loaded successfully.");
                OnAdLoaded?.Invoke(AdType.Banner);
            };

            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                IsBannerShowing = false;
                Debug.LogError($"[MobileAds-AdMob] Banner failed to load: {error.GetMessage()} (Code: {error.GetCode()})");
                OnAdFailedToLoad?.Invoke(new AdErrorInfo(error.GetCode(), error.GetMessage(), AdType.Banner, AdNetwork.GoogleMobileAds));
            };

            _bannerView.OnAdFullScreenContentOpened += () =>
            {
                OnAdOpened?.Invoke(AdType.Banner);
            };

            _bannerView.OnAdFullScreenContentClosed += () =>
            {
                OnAdClosed?.Invoke(AdType.Banner);
            };

            var request = new AdRequest();
            _bannerView.LoadAd(request);
#endif
        }

        public void ShowBanner()
        {
#if MOBILE_ADS_ADMOB
            if (_bannerView != null)
            {
                _bannerView.Show();
                IsBannerShowing = true;
                OnAdOpened?.Invoke(AdType.Banner);
            }
#endif
        }

        public void HideBanner()
        {
#if MOBILE_ADS_ADMOB
            if (_bannerView != null)
            {
                _bannerView.Hide();
                IsBannerShowing = false;
                OnAdClosed?.Invoke(AdType.Banner);
            }
#endif
        }

        public void DestroyBanner()
        {
#if MOBILE_ADS_ADMOB
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
                IsBannerShowing = false;
            }
#endif
        }

        #endregion

        #region Interstitial Ads

        public void LoadInterstitial()
        {
#if MOBILE_ADS_ADMOB
            if (!IsInitialized) return;

            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            string adUnitId = _settings.GetAdMobUnitId(AdType.Interstitial);
            var request = new AdRequest();

            InterstitialAd.Load(adUnitId, request, (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"[MobileAds-AdMob] Interstitial failed to load: {error?.GetMessage()}");
                    OnAdFailedToLoad?.Invoke(new AdErrorInfo(error != null ? error.GetCode() : -1, error != null ? error.GetMessage() : "Null Ad", AdType.Interstitial, AdNetwork.GoogleMobileAds));
                    return;
                }

                _interstitialAd = ad;
                if (_settings.enableDebugLogs) Debug.Log("[MobileAds-AdMob] Interstitial loaded.");
                OnAdLoaded?.Invoke(AdType.Interstitial);
            });
#endif
        }

        public void ShowInterstitial(Action onClosed)
        {
#if MOBILE_ADS_ADMOB
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _interstitialAd.OnAdFullScreenContentOpened += () =>
                {
                    OnAdOpened?.Invoke(AdType.Interstitial);
                };

                _interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    OnAdClosed?.Invoke(AdType.Interstitial);
                    onClosed?.Invoke();
                    LoadInterstitial(); // Auto reload
                };

                _interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    Debug.LogError($"[MobileAds-AdMob] Interstitial presentation failed: {error.GetMessage()}");
                    OnAdClosed?.Invoke(AdType.Interstitial);
                    onClosed?.Invoke();
                    LoadInterstitial();
                };

                _interstitialAd.Show();
            }
            else
            {
                Debug.LogWarning("[MobileAds-AdMob] Interstitial ad not ready to show.");
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
#if MOBILE_ADS_ADMOB
            if (!IsInitialized) return;

            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            string adUnitId = _settings.GetAdMobUnitId(AdType.Rewarded);
            var request = new AdRequest();

            RewardedAd.Load(adUnitId, request, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"[MobileAds-AdMob] Rewarded ad failed to load: {error?.GetMessage()}");
                    OnAdFailedToLoad?.Invoke(new AdErrorInfo(error != null ? error.GetCode() : -1, error != null ? error.GetMessage() : "Null Ad", AdType.Rewarded, AdNetwork.GoogleMobileAds));
                    return;
                }

                _rewardedAd = ad;
                if (_settings.enableDebugLogs) Debug.Log("[MobileAds-AdMob] Rewarded ad loaded.");
                OnAdLoaded?.Invoke(AdType.Rewarded);
            });
#endif
        }

        public void ShowRewarded(Action<AdReward> onRewarded, Action onClosed)
        {
#if MOBILE_ADS_ADMOB
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _rewardedAd.OnAdFullScreenContentOpened += () =>
                {
                    OnAdOpened?.Invoke(AdType.Rewarded);
                };

                _rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    OnAdClosed?.Invoke(AdType.Rewarded);
                    onClosed?.Invoke();
                    LoadRewarded(); // Auto reload
                };

                _rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    Debug.LogError($"[MobileAds-AdMob] Rewarded ad presentation failed: {error.GetMessage()}");
                    OnAdClosed?.Invoke(AdType.Rewarded);
                    onClosed?.Invoke();
                    LoadRewarded();
                };

                _rewardedAd.Show(reward =>
                {
                    var adReward = new AdReward(reward.Type, reward.Amount);
                    if (_settings.enableDebugLogs) Debug.Log($"[MobileAds-AdMob] User earned reward: {adReward}");
                    OnUserEarnedReward?.Invoke(adReward);
                    onRewarded?.Invoke(adReward);
                });
            }
            else
            {
                Debug.LogWarning("[MobileAds-AdMob] Rewarded ad not ready to show.");
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
#if MOBILE_ADS_ADMOB
            if (!IsInitialized) return;

            if (_appOpenAd != null)
            {
                _appOpenAd.Destroy();
                _appOpenAd = null;
            }

            string adUnitId = _settings.GetAdMobUnitId(AdType.AppOpen);
            var request = new AdRequest();

            AppOpenAd.Load(adUnitId, request, (AppOpenAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"[MobileAds-AdMob] App Open ad failed to load: {error?.GetMessage()}");
                    OnAdFailedToLoad?.Invoke(new AdErrorInfo(error != null ? error.GetCode() : -1, error != null ? error.GetMessage() : "Null Ad", AdType.AppOpen, AdNetwork.GoogleMobileAds));
                    return;
                }

                _appOpenAd = ad;
                if (_settings.enableDebugLogs) Debug.Log("[MobileAds-AdMob] App Open ad loaded.");
                OnAdLoaded?.Invoke(AdType.AppOpen);
            });
#endif
        }

        public void ShowAppOpen(Action onClosed)
        {
#if MOBILE_ADS_ADMOB
            if (_appOpenAd != null && _appOpenAd.CanShowAd())
            {
                _appOpenAd.OnAdFullScreenContentOpened += () =>
                {
                    OnAdOpened?.Invoke(AdType.AppOpen);
                };

                _appOpenAd.OnAdFullScreenContentClosed += () =>
                {
                    OnAdClosed?.Invoke(AdType.AppOpen);
                    onClosed?.Invoke();
                    LoadAppOpen(); // Auto reload
                };

                _appOpenAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    Debug.LogError($"[MobileAds-AdMob] App Open ad presentation failed: {error.GetMessage()}");
                    OnAdClosed?.Invoke(AdType.AppOpen);
                    onClosed?.Invoke();
                    LoadAppOpen();
                };

                _appOpenAd.Show();
            }
            else
            {
                Debug.LogWarning("[MobileAds-AdMob] App Open ad not ready to show.");
                onClosed?.Invoke();
            }
#else
            onClosed?.Invoke();
#endif
        }

        #endregion

        #region Consent & UMP

        public void RequestConsent(Action<bool, string> onComplete)
        {
#if MOBILE_ADS_ADMOB
            var requestParameters = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = _settings != null && _settings.tagForUnderAgeOfConsent
            };

            ConsentInformation.Update(requestParameters, (FormError error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning($"[MobileAds-AdMob] Consent information update error: {error.GetMessage()}");
                    onComplete?.Invoke(false, error.GetMessage());
                    return;
                }

                ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
                {
                    if (formError != null)
                    {
                        Debug.LogWarning($"[MobileAds-AdMob] Consent form presentation error: {formError.GetMessage()}");
                        onComplete?.Invoke(false, formError.GetMessage());
                    }
                    else
                    {
                        if (_settings.enableDebugLogs) Debug.Log("[MobileAds-AdMob] Consent form process completed.");
                        onComplete?.Invoke(true, "Consent gathered / not required");
                    }
                });
            });
#else
            onComplete?.Invoke(false, "SDK not installed");
#endif
        }

        public void ShowConsentForm(Action<bool, string> onComplete)
        {
#if MOBILE_ADS_ADMOB
            ConsentForm.ShowPrivacyOptionsForm((FormError error) =>
            {
                if (error != null)
                {
                    Debug.LogError($"[MobileAds-AdMob] Privacy options form error: {error.GetMessage()}");
                    onComplete?.Invoke(false, error.GetMessage());
                }
                else
                {
                    if (_settings.enableDebugLogs) Debug.Log("[MobileAds-AdMob] Privacy options updated.");
                    onComplete?.Invoke(true, "Privacy options updated");
                }
            });
#else
            onComplete?.Invoke(false, "SDK not installed");
#endif
        }

        #endregion

        #region Conversion Helpers

#if MOBILE_ADS_ADMOB
        private AdSize ConvertBannerType(BannerType type)
        {
            switch (type)
            {
                case BannerType.Adaptive:
                    return AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                case BannerType.MediumRectangle:
                    return AdSize.MediumRectangle;
                case BannerType.LargeBanner:
                    return AdSize.IABBanner;
                case BannerType.Leaderboard:
                    return AdSize.Leaderboard;
                case BannerType.SmartBanner:
                    return AdSize.SmartBanner;
                default:
                    return AdSize.Banner;
            }
        }

        private AdPosition ConvertBannerPosition(BannerPosition position)
        {
            switch (position)
            {
                case BannerPosition.Top: return AdPosition.Top;
                case BannerPosition.Bottom: return AdPosition.Bottom;
                case BannerPosition.TopLeft: return AdPosition.TopLeft;
                case BannerPosition.TopRight: return AdPosition.TopRight;
                case BannerPosition.BottomLeft: return AdPosition.BottomLeft;
                case BannerPosition.BottomRight: return AdPosition.BottomRight;
                case BannerPosition.Center: return AdPosition.Center;
                default: return AdPosition.Bottom;
            }
        }
#endif

        #endregion
    }
}
