using System;

namespace MobileAdsPackage
{
    /// <summary>
    /// Unified static facade API for Mobile Ads Manager.
    /// Provides one-line access to initialize ads, show banners, trigger interstitials,
    /// rewarded videos, app open ads, consent management, and Remove-Ads handling.
    /// </summary>
    public static class MobileAds
    {
        #region Public Properties

        /// <summary>
        /// True if the ad provider has completed initialization.
        /// </summary>
        public static bool IsInitialized => MobileAdsManager.Instance.IsInitialized;

        /// <summary>
        /// True if the user has purchased 'Remove Ads'. Interstitial and banner ads are disabled when true.
        /// </summary>
        public static bool IsNoAdsPurchased => MobileAdsManager.Instance.IsNoAdsPurchased;

        /// <summary>
        /// True if a banner is currently displayed on screen.
        /// </summary>
        public static bool IsBannerShowing => MobileAdsManager.Instance.IsBannerShowing();

        /// <summary>
        /// True if an interstitial ad is loaded and ready.
        /// </summary>
        public static bool IsInterstitialReady => MobileAdsManager.Instance.IsInterstitialReady();

        /// <summary>
        /// True if an interstitial ad is ready AND the cooldown timer has elapsed.
        /// </summary>
        public static bool CanShowInterstitial => MobileAdsManager.Instance.CanShowInterstitial();

        /// <summary>
        /// True if a rewarded ad is loaded and ready to play.
        /// </summary>
        public static bool IsRewardedReady => MobileAdsManager.Instance.IsRewardedReady();

        /// <summary>
        /// True if an App Open ad is loaded and ready.
        /// </summary>
        public static bool IsAppOpenReady => MobileAdsManager.Instance.IsAppOpenReady();

        #endregion

        #region Events

        /// <summary>
        /// Triggered when the ad provider finishes initialization.
        /// </summary>
        public static event Action<AdInitResult> OnInitialized
        {
            add => MobileAdsManager.Instance.OnInitialized += value;
            remove => MobileAdsManager.Instance.OnInitialized -= value;
        }

        /// <summary>
        /// Triggered whenever an ad successfully finishes loading.
        /// </summary>
        public static event Action<AdType> OnAdLoaded
        {
            add => MobileAdsManager.Instance.OnAdLoaded += value;
            remove => MobileAdsManager.Instance.OnAdLoaded -= value;
        }

        /// <summary>
        /// Triggered whenever an ad fails to load.
        /// </summary>
        public static event Action<AdErrorInfo> OnAdFailedToLoad
        {
            add => MobileAdsManager.Instance.OnAdFailedToLoad += value;
            remove => MobileAdsManager.Instance.OnAdFailedToLoad -= value;
        }

        /// <summary>
        /// Triggered whenever an ad opens and covers the screen.
        /// </summary>
        public static event Action<AdType> OnAdOpened
        {
            add => MobileAdsManager.Instance.OnAdOpened += value;
            remove => MobileAdsManager.Instance.OnAdOpened -= value;
        }

        /// <summary>
        /// Triggered whenever an ad is closed and gameplay resumes.
        /// </summary>
        public static event Action<AdType> OnAdClosed
        {
            add => MobileAdsManager.Instance.OnAdClosed += value;
            remove => MobileAdsManager.Instance.OnAdClosed -= value;
        }

        /// <summary>
        /// Triggered whenever a rewarded ad completes and the user earns their reward.
        /// </summary>
        public static event Action<AdReward> OnUserEarnedReward
        {
            add => MobileAdsManager.Instance.OnUserEarnedReward += value;
            remove => MobileAdsManager.Instance.OnUserEarnedReward -= value;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the ad network and begins loading configured ads.
        /// </summary>
        /// <param name="onComplete">Callback invoked when initialization finishes.</param>
        public static void Initialize(Action<AdInitResult> onComplete = null)
        {
            MobileAdsManager.Instance.Initialize(onComplete);
        }

        #endregion

        #region Banner Ads

        /// <summary>
        /// Loads and immediately shows a banner ad at the specified position and size.
        /// </summary>
        public static void ShowBanner(BannerPosition position = BannerPosition.Bottom, BannerType type = BannerType.Adaptive)
        {
            MobileAdsManager.Instance.LoadBanner(position, type);
            MobileAdsManager.Instance.ShowBanner();
        }

        /// <summary>
        /// Loads a banner in the background without immediately showing it.
        /// </summary>
        public static void LoadBanner(BannerPosition position = BannerPosition.Bottom, BannerType type = BannerType.Adaptive)
        {
            MobileAdsManager.Instance.LoadBanner(position, type);
        }

        /// <summary>
        /// Hides the currently visible banner without destroying the view.
        /// </summary>
        public static void HideBanner()
        {
            MobileAdsManager.Instance.HideBanner();
        }

        /// <summary>
        /// Destroys and cleans up the banner view.
        /// </summary>
        public static void DestroyBanner()
        {
            MobileAdsManager.Instance.DestroyBanner();
        }

        #endregion

        #region Interstitial Ads

        /// <summary>
        /// Shows a full-screen interstitial ad if ready and cooldown has passed.
        /// Automatically schedules reloading upon completion.
        /// </summary>
        /// <param name="onClosed">Invoked when the interstitial ad closes or cannot show.</param>
        public static void ShowInterstitial(Action onClosed = null)
        {
            MobileAdsManager.Instance.ShowInterstitial(onClosed);
        }

        /// <summary>
        /// Manually triggers loading of an interstitial ad.
        /// </summary>
        public static void LoadInterstitial()
        {
            MobileAdsManager.Instance.LoadInterstitial();
        }

        #endregion

        #region Rewarded Ads

        /// <summary>
        /// Displays a rewarded video ad. Invokes onRewarded when the player completes the video.
        /// </summary>
        /// <param name="onRewarded">Invoked when the player earns the reward.</param>
        /// <param name="onClosed">Invoked when the rewarded ad is closed.</param>
        public static void ShowRewarded(Action<AdReward> onRewarded, Action onClosed = null)
        {
            MobileAdsManager.Instance.ShowRewarded(onRewarded, onClosed);
        }

        /// <summary>
        /// Simplified overload for rewarded ad when reward details are not required.
        /// </summary>
        public static void ShowRewarded(Action onRewardEarned, Action onClosed = null)
        {
            MobileAdsManager.Instance.ShowRewarded(reward => onRewardEarned?.Invoke(), onClosed);
        }

        /// <summary>
        /// Manually triggers loading of a rewarded ad.
        /// </summary>
        public static void LoadRewarded()
        {
            MobileAdsManager.Instance.LoadRewarded();
        }

        #endregion

        #region App Open Ads

        /// <summary>
        /// Displays an App Open ad (useful on launch or returning from pause).
        /// </summary>
        /// <param name="onClosed">Invoked when the ad closes.</param>
        public static void ShowAppOpen(Action onClosed = null)
        {
            MobileAdsManager.Instance.ShowAppOpen(onClosed);
        }

        /// <summary>
        /// Manually triggers loading of an App Open ad.
        /// </summary>
        public static void LoadAppOpen()
        {
            MobileAdsManager.Instance.LoadAppOpen();
        }

        #endregion

        #region Remove Ads (IAP)

        /// <summary>
        /// Enables or disables 'Remove Ads'.
        /// When true, banners are permanently hidden and interstitials will not show.
        /// Rewarded ads continue working normally for opt-in rewards.
        /// </summary>
        public static void SetRemoveAds(bool removeAds)
        {
            MobileAdsManager.Instance.SetRemoveAds(removeAds);
        }

        #endregion

        #region Consent Management (GDPR / UMP)

        /// <summary>
        /// Requests user consent information and displays the consent form if required (e.g., EEA / UK).
        /// </summary>
        public static void RequestConsent(Action<bool, string> onComplete = null)
        {
            MobileAdsManager.Instance.RequestConsent(onComplete);
        }

        /// <summary>
        /// Shows the privacy options form allowing users to update their consent preferences anytime (e.g. from an in-game settings menu).
        /// </summary>
        public static void ShowConsentForm(Action<bool, string> onComplete = null)
        {
            MobileAdsManager.Instance.ShowConsentForm(onComplete);
        }

        #endregion
    }
}
