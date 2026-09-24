using System;

namespace MobileAdsPackage
{
    /// <summary>
    /// Contract required for any mobile ad network provider adapter.
    /// </summary>
    public interface IAdProvider
    {
        AdNetwork NetworkType { get; }
        bool IsInitialized { get; }
        bool IsBannerShowing { get; }
        bool IsInterstitialReady { get; }
        bool IsRewardedReady { get; }
        bool IsAppOpenReady { get; }

        // Events
        event Action<AdType> OnAdLoaded;
        event Action<AdErrorInfo> OnAdFailedToLoad;
        event Action<AdType> OnAdOpened;
        event Action<AdType> OnAdClosed;
        event Action<AdReward> OnUserEarnedReward;

        // Initialization
        void Initialize(AdSettings settings, Action<AdInitResult> onComplete);

        // Banner Ads
        void LoadBanner(BannerPosition position, BannerType type);
        void ShowBanner();
        void HideBanner();
        void DestroyBanner();

        // Interstitial Ads
        void LoadInterstitial();
        void ShowInterstitial(Action onClosed);

        // Rewarded Ads
        void LoadRewarded();
        void ShowRewarded(Action<AdReward> onRewarded, Action onClosed);

        // App Open Ads
        void LoadAppOpen();
        void ShowAppOpen(Action onClosed);

        // Consent (GDPR / UMP)
        void RequestConsent(Action<bool, string> onComplete);
        void ShowConsentForm(Action<bool, string> onComplete);
    }
}
