using System;

namespace MobileAdsPackage
{
    /// <summary>
    /// Position of the banner ad on the screen.
    /// </summary>
    public enum BannerPosition
    {
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }

    /// <summary>
    /// Supported banner sizes and types.
    /// </summary>
    public enum BannerType
    {
        /// <summary>
        /// Standard phone banner (typically 320x50).
        /// </summary>
        Standard,

        /// <summary>
        /// Adaptive banner that automatically fits the full width of the screen.
        /// </summary>
        Adaptive,

        /// <summary>
        /// Medium rectangle (300x250), also known as MREC.
        /// </summary>
        MediumRectangle,

        /// <summary>
        /// Large banner (320x100).
        /// </summary>
        LargeBanner,

        /// <summary>
        /// Tablet leaderboard banner (728x90).
        /// </summary>
        Leaderboard,

        /// <summary>
        /// Smart banner dynamically sized for older formats.
        /// </summary>
        SmartBanner
    }

    /// <summary>
    /// Supported ad networks / provider types.
    /// </summary>
    public enum AdNetwork
    {
        /// <summary>
        /// In-Editor visual ad simulator (allows testing without building to device).
        /// </summary>
        MockSimulator,

        /// <summary>
        /// Google Mobile Ads (AdMob).
        /// </summary>
        GoogleMobileAds,

        /// <summary>
        /// Unity Advertisements.
        /// </summary>
        UnityAds
    }

    /// <summary>
    /// Ad unit categories.
    /// </summary>
    public enum AdType
    {
        Banner,
        Interstitial,
        Rewarded,
        RewardedInterstitial,
        AppOpen
    }

    /// <summary>
    /// Current loading state of an ad unit.
    /// </summary>
    public enum AdLoadState
    {
        NotLoaded,
        Loading,
        Loaded,
        FailedToLoad
    }

    /// <summary>
    /// GDPR / UMP Consent Status.
    /// </summary>
    public enum UserConsentStatus
    {
        Unknown,
        Required,
        NotRequired,
        Obtained
    }
}
