# Mobile Ads Manager - Comprehensive Guide

## Table of Contents
1. [Overview & Architecture](#overview--architecture)
2. [Comparison & Migration from Gley Mobile Ads](#comparison--migration-from-gley-mobile-ads)
3. [Installation](#installation)
4. [Editor Setup & Configuration](#editor-setup--configuration)
5. [In-Editor Ad Simulator](#in-editor-ad-simulator)
6. [AdMob (Google Mobile Ads) Integration](#admob-google-mobile-ads-integration)
7. [Unity Advertisements Integration](#unity-advertisements-integration)
8. [GDPR, CCPA & Google UMP Consent](#gdpr-ccpa--google-ump-consent)
9. [Remove Ads (IAP Integration)](#remove-ads-iap-integration)
10. [Full API Reference & Code Examples](#full-api-reference--code-examples)

---

## 1. Overview & Architecture

**Mobile Ads Manager** is a lightweight, decoupled, production-ready ad mediation and management package for Unity (supporting Unity 2021.3 LTS, 2022.3 LTS, 2023 LTS, and Unity 6+).

### Key Architectural Highlights
- **Gley-style Static Facade (`MobileAds`)**: One-line access across your entire project without needing `FindObjectOfType` or Inspector drag-and-drops.
- **Multi-Network Mediation & Fallback**: Support for Google Mobile Ads (AdMob v8/v9+), Unity Ads, and fallback cascaded ad networks.
- **In-Editor Realistic Simulator**: Zero external dependencies required to test banners, countdown interstitials, and video rewarded ads directly in the Unity Editor.
- **Smart Cooldown & Exponential Backoff**: Built-in protection against interstitial spamming and intelligent retry logic when devices have weak network connectivity.
- **Persistent Remove-Ads**: Seamlessly respects IAP purchases by hiding banners and bypassing interstitials while preserving opt-in rewarded ads.
- **Clean Decoupling with Assembly Definitions (`.asmdef`)**: Zero compile errors even when native SDKs are not yet installed in a project.

---

## 2. Comparison & Migration from Gley Mobile Ads

If you have used **Gley Mobile Ads** from the Unity Asset Store, the API is intentionally familiar and straightforward:

| Gley Mobile Ads API | Mobile Ads Manager API | Notes |
| :--- | :--- | :--- |
| `Gley.MobileAds.API.Initialize()` | `MobileAds.Initialize(callback)` | Initializes active provider & auto-loads |
| `Gley.MobileAds.API.ShowBanner(pos, type)` | `MobileAds.ShowBanner(pos, type)` | Supports Adaptive, Standard, MREC, etc. |
| `Gley.MobileAds.API.HideBanner()` | `MobileAds.HideBanner()` | Temporarily hides banner |
| `Gley.MobileAds.API.IsInterstitialAvailable()` | `MobileAds.CanShowInterstitial` | Checks readiness **and** cooldown timer |
| `Gley.MobileAds.API.ShowInterstitial(callback)` | `MobileAds.ShowInterstitial(callback)` | Auto-reloads next interstitial on close |
| `Gley.MobileAds.API.IsRewardedVideoAvailable()` | `MobileAds.IsRewardedReady` | Checks if video is cached |
| `Gley.MobileAds.API.ShowRewardedVideo(callback)` | `MobileAds.ShowRewarded(onReward, onClosed)` | Fires reward callback on completion |
| `Gley.MobileAds.API.ShowAppOpen()` | `MobileAds.ShowAppOpen(callback)` | Full App Open ad support |
| `Gley.MobileAds.API.SetRemoveAds(true)` | `MobileAds.SetRemoveAds(true)` | Persists in PlayerPrefs |

---

## 3. Installation

### Option A: Install via Unity Package Manager (Git URL)
1. In Unity, open **Window > Package Manager**.
2. Click the `+` button in the top-left corner.
3. Select **Add package from git URL...**
4. Enter:
   ```
   https://github.com/Ahmad8i1a1/Ads_Custom_Package.git
   ```

### Option B: Local Disk Package
1. Clone or download this repository to your computer.
2. In **Window > Package Manager**, click `+` -> **Add package from disk...**
3. Select the `package.json` file in this directory.

---

## 4. Editor Setup & Configuration

Open the settings panel using:
`Tools > Mobile Ads > Settings Window` or `Window > Mobile Ads > Settings Window`.

### Settings Overview
- **Primary Network**: Choose `GoogleMobileAds`, `UnityAds`, or `MockSimulator`.
- **Test Mode**: Toggle to automatically use official Google AdMob test IDs (`ca-app-pub-3940256099942544/...`).
- **Auto Initialize On Start**: Initializes the ad manager automatically on scene launch.
- **Auto Load Ads**: Pre-caches banners, interstitials, and rewarded ads as soon as initialization succeeds.
- **Interstitial Cooldown (Seconds)**: Minimum interval required between consecutive interstitials (default: 30 seconds).
- **Auto Show App Open on Resume**: Automatically triggers an App Open ad when players return to the game after backgrounding the app.

---

## 5. In-Editor Ad Simulator

Testing mobile ads usually requires building an APK/AAB or iOS Xcode project. The **In-Editor Ad Simulator** solves this:
- **Simulated Banner**: Displays a realistic banner bar at the requested screen position (e.g. `Bottom`, `Top`, `Center`) with a quick dismiss button.
- **Simulated Interstitial**: Renders a dark fullscreen overlay with an ad card, countdown timer, and a "SKIP / CLOSE AD" button.
- **Simulated Rewarded Ad**: Displays video playback simulation with a real-time progress bar, a "CLAIM REWARD" button, and an "ABANDON (NO REWARD)" button to test both outcomes!
- Runs entirely in Unity's runtime GUI without requiring prefabs or TextMeshPro.

---

## 6. AdMob (Google Mobile Ads) Integration

1. Import the **Google Mobile Ads Unity Plugin** (v8.x or v9.x+ - e.g., the included `Unity Ads.unitypackage`).
2. Open `Tools > Mobile Ads > Settings Window`.
3. Under the **SDK Status & Defines** tab, verify that Google Mobile Ads is detected, and ensure `MOBILE_ADS_ADMOB` is enabled.
4. Set your **Android App ID** and **iOS App ID** in `Assets > Google Mobile Ads > Settings`.
5. Enter your production Ad Unit IDs for Banner, Interstitial, Rewarded, and App Open.
6. Enable **Test Mode** during development to avoid policy violations.

---

## 7. Unity Advertisements Integration

1. Install the `com.unity.ads` package via the Unity Package Manager.
2. In `Tools > Mobile Ads > Settings Window`, enable the `MOBILE_ADS_UNITY` compilation symbol.
3. Enter your **Unity Game IDs** (Android & iOS) and placement IDs.

---

## 8. GDPR, CCPA & Google UMP Consent

Mobile Ads Manager natively integrates with Google's **User Messaging Platform (UMP)**:

### Automatic Consent on Launch
Enable `autoRequestUmpConsent = true` in `AdSettings`. When `MobileAds.Initialize()` is called, it will automatically query Google UMP and display the European EEA/UK consent form if legally required.

### Manual Privacy Options Form
Google requires apps to give users an option to change their privacy consent choices at any time (e.g., from an in-game settings menu):

```csharp
public void OnPrivacySettingsButtonClicked()
{
    MobileAds.ShowConsentForm((success, message) =>
    {
        Debug.Log($"Privacy consent status updated: {success}");
    });
}
```

---

## 9. Remove Ads (IAP Integration)

When a player purchases your in-app purchase product for removing ads:

```csharp
// Call this upon verified IAP purchase
MobileAds.SetRemoveAds(true);
```

**Effects of `SetRemoveAds(true)`:**
- Current banners are immediately hidden and destroyed.
- Subsequent calls to `MobileAds.ShowBanner()` and `MobileAds.ShowInterstitial()` are automatically silenced and bypassed.
- `MobileAds.ShowRewarded()` continues to function normally so players can still choose to watch rewarded videos for bonuses.
- Status is automatically persisted in `PlayerPrefs` across game restarts.

---

## 10. Full API Reference & Code Examples

### Initializing Ads
```csharp
using MobileAdsPackage;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private void Start()
    {
        MobileAds.Initialize(result =>
        {
            if (result.Success)
            {
                Debug.Log($"Ads initialized using {result.ActiveNetwork}");
            }
        });
    }
}
```

### Banner Ads
```csharp
// Show bottom adaptive banner
MobileAds.ShowBanner(BannerPosition.Bottom, BannerType.Adaptive);

// Show top standard banner
MobileAds.ShowBanner(BannerPosition.Top, BannerType.Standard);

// Hide banner without destroying
MobileAds.HideBanner();

// Destroy banner
MobileAds.DestroyBanner();
```

### Interstitial Ads
```csharp
public void OnLevelCompleted()
{
    if (MobileAds.CanShowInterstitial)
    {
        MobileAds.ShowInterstitial(onClosed: () =>
        {
            LoadNextLevel();
        });
    }
    else
    {
        LoadNextLevel();
    }
}
```

### Rewarded Ads
```csharp
public void OnWatchRewardAdButtonClicked()
{
    if (MobileAds.IsRewardedReady)
    {
        MobileAds.ShowRewarded(
            onRewarded: reward =>
            {
                AddCoins(50);
                Debug.Log($"Rewarded player with {reward.Amount} {reward.Type}");
            },
            onClosed: () =>
            {
                Debug.Log("Rewarded ad finished.");
            }
        );
    }
    else
    {
        Debug.Log("Rewarded ad is still caching...");
    }
}
```

### Event Subscriptions
```csharp
void OnEnable()
{
    MobileAds.OnAdLoaded += HandleAdLoaded;
    MobileAds.OnAdFailedToLoad += HandleAdFailed;
    MobileAds.OnUserEarnedReward += HandleReward;
}

void OnDisable()
{
    MobileAds.OnAdLoaded -= HandleAdLoaded;
    MobileAds.OnAdFailedToLoad -= HandleAdFailed;
    MobileAds.OnUserEarnedReward -= HandleReward;
}

private void HandleAdLoaded(AdType adType) => Debug.Log($"Loaded: {adType}");
private void HandleAdFailed(AdErrorInfo error) => Debug.LogError(error.ToString());
private void HandleReward(AdReward reward) => Debug.Log($"Reward earned: {reward}");
```
