using System;
using System.Collections;
using UnityEngine;

namespace MobileAdsPackage
{
    /// <summary>
    /// Standalone mock ad provider that simulates ad lifecycles directly in the Unity Editor or desktop players.
    /// Provides zero-dependency testing without requiring mobile devices or ad networks.
    /// </summary>
    public class MockAdProvider : IAdProvider
    {
        public AdNetwork NetworkType => AdNetwork.MockSimulator;
        public bool IsInitialized { get; private set; }
        public bool IsBannerShowing => _simulator != null && _simulator.IsBannerVisible;
        public bool IsInterstitialReady { get; private set; }
        public bool IsRewardedReady { get; private set; }
        public bool IsAppOpenReady { get; private set; }

        public event Action<AdType> OnAdLoaded;
        public event Action<AdErrorInfo> OnAdFailedToLoad;
        public event Action<AdType> OnAdOpened;
        public event Action<AdType> OnAdClosed;
        public event Action<AdReward> OnUserEarnedReward;

        private AdSettings _settings;
        private EditorAdSimulatorUI _simulator;
        private BannerPosition _lastBannerPos = BannerPosition.Bottom;
        private BannerType _lastBannerType = BannerType.Adaptive;

        public void Initialize(AdSettings settings, Action<AdInitResult> onComplete)
        {
            _settings = settings;
            _simulator = EditorAdSimulatorUI.GetOrCreate();
            IsInitialized = true;

            if (_settings != null && _settings.enableDebugLogs)
            {
                Debug.Log("<color=#33B5E5>[MobileAds-Simulator] Initialized successfully.</color>");
            }

            onComplete?.Invoke(new AdInitResult(true, "Editor Ad Simulator initialized successfully.", AdNetwork.MockSimulator));
        }

        public void LoadBanner(BannerPosition position, BannerType type)
        {
            _lastBannerPos = position;
            _lastBannerType = type;

            MobileAdsManager.Instance.StartCoroutine(SimulateDelayRoutine(() =>
            {
                if (_settings != null && _settings.enableDebugLogs)
                {
                    Debug.Log($"<color=#33B5E5>[MobileAds-Simulator] Banner loaded ({type} at {position}).</color>");
                }
                OnAdLoaded?.Invoke(AdType.Banner);
            }));
        }

        public void ShowBanner()
        {
            if (_simulator == null) _simulator = EditorAdSimulatorUI.GetOrCreate();
            _simulator.ShowBanner(_lastBannerPos, _lastBannerType);
            OnAdOpened?.Invoke(AdType.Banner);

            if (_settings != null && _settings.enableDebugLogs)
            {
                Debug.Log("<color=#33B5E5>[MobileAds-Simulator] Banner displayed.</color>");
            }
        }

        public void HideBanner()
        {
            if (_simulator != null)
            {
                _simulator.HideBanner();
            }
            OnAdClosed?.Invoke(AdType.Banner);

            if (_settings != null && _settings.enableDebugLogs)
            {
                Debug.Log("<color=#33B5E5>[MobileAds-Simulator] Banner hidden.</color>");
            }
        }

        public void DestroyBanner()
        {
            HideBanner();
        }

        public void LoadInterstitial()
        {
            MobileAdsManager.Instance.StartCoroutine(SimulateDelayRoutine(() =>
            {
                IsInterstitialReady = true;
                if (_settings != null && _settings.enableDebugLogs)
                {
                    Debug.Log("<color=#33B5E5>[MobileAds-Simulator] Interstitial Ad loaded and ready.</color>");
                }
                OnAdLoaded?.Invoke(AdType.Interstitial);
            }));
        }

        public void ShowInterstitial(Action onClosed)
        {
            if (!IsInterstitialReady)
            {
                Debug.LogWarning("[MobileAds-Simulator] Interstitial is not loaded yet.");
                onClosed?.Invoke();
                return;
            }

            IsInterstitialReady = false;
            OnAdOpened?.Invoke(AdType.Interstitial);

            if (_simulator == null) _simulator = EditorAdSimulatorUI.GetOrCreate();
            float countdown = _settings != null ? _settings.simulatedInterstitialCountdown : 3f;

            _simulator.ShowInterstitial(countdown, () =>
            {
                OnAdClosed?.Invoke(AdType.Interstitial);
                onClosed?.Invoke();
                // Auto reload
                LoadInterstitial();
            });
        }

        public void LoadRewarded()
        {
            MobileAdsManager.Instance.StartCoroutine(SimulateDelayRoutine(() =>
            {
                IsRewardedReady = true;
                if (_settings != null && _settings.enableDebugLogs)
                {
                    Debug.Log("<color=#33B5E5>[MobileAds-Simulator] Rewarded Ad loaded and ready.</color>");
                }
                OnAdLoaded?.Invoke(AdType.Rewarded);
            }));
        }

        public void ShowRewarded(Action<AdReward> onRewarded, Action onClosed)
        {
            if (!IsRewardedReady)
            {
                Debug.LogWarning("[MobileAds-Simulator] Rewarded Ad is not loaded yet.");
                onClosed?.Invoke();
                return;
            }

            IsRewardedReady = false;
            OnAdOpened?.Invoke(AdType.Rewarded);

            if (_simulator == null) _simulator = EditorAdSimulatorUI.GetOrCreate();
            float duration = _settings != null ? _settings.simulatedRewardedDuration : 5f;

            _simulator.ShowRewarded(duration,
                reward =>
                {
                    OnUserEarnedReward?.Invoke(reward);
                    onRewarded?.Invoke(reward);
                },
                () =>
                {
                    OnAdClosed?.Invoke(AdType.Rewarded);
                    onClosed?.Invoke();
                    // Auto reload
                    LoadRewarded();
                });
        }

        public void LoadAppOpen()
        {
            MobileAdsManager.Instance.StartCoroutine(SimulateDelayRoutine(() =>
            {
                IsAppOpenReady = true;
                if (_settings != null && _settings.enableDebugLogs)
                {
                    Debug.Log("<color=#33B5E5>[MobileAds-Simulator] App Open Ad loaded and ready.</color>");
                }
                OnAdLoaded?.Invoke(AdType.AppOpen);
            }));
        }

        public void ShowAppOpen(Action onClosed)
        {
            if (!IsAppOpenReady)
            {
                Debug.LogWarning("[MobileAds-Simulator] App Open Ad is not loaded yet.");
                onClosed?.Invoke();
                return;
            }

            IsAppOpenReady = false;
            OnAdOpened?.Invoke(AdType.AppOpen);

            if (_simulator == null) _simulator = EditorAdSimulatorUI.GetOrCreate();
            _simulator.ShowAppOpen(() =>
            {
                OnAdClosed?.Invoke(AdType.AppOpen);
                onClosed?.Invoke();
                LoadAppOpen();
            });
        }

        public void RequestConsent(Action<bool, string> onComplete)
        {
            if (_settings != null && _settings.enableDebugLogs)
            {
                Debug.Log("<color=#33B5E5>[MobileAds-Simulator] Simulated GDPR / UMP Consent requested and granted.</color>");
            }
            onComplete?.Invoke(true, "Consent granted (Simulated)");
        }

        public void ShowConsentForm(Action<bool, string> onComplete)
        {
            if (_settings != null && _settings.enableDebugLogs)
            {
                Debug.Log("<color=#33B5E5>[MobileAds-Simulator] Simulated Consent form closed.</color>");
            }
            onComplete?.Invoke(true, "Consent form completed (Simulated)");
        }

        private IEnumerator SimulateDelayRoutine(Action action)
        {
            float delay = _settings != null ? _settings.simulatedLoadDelay : 0.5f;
            yield return new WaitForSecondsRealtime(delay);
            action?.Invoke();
        }
    }
}
