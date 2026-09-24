# Mobile Ads Manager for Unity

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B%20%7C%202022.3%20%7C%202023%20%7C%206-blue.svg)](#)
[![Package Format](https://img.shields.io/badge/UPM-Package-green.svg)](#)
[![Mediation](https://img.shields.io/badge/Supports-AdMob%20%7C%20Unity%20Ads%20%7C%20Simulator-brightgreen.svg)](#)
[![License](https://img.shields.io/badge/License-MIT-orange.svg)](#)

A high-performance, developer-friendly **Mobile Ads Management Package** for Unity, inspired by the architecture and ease of **Gley Mobile Ads**.

Features a clean static API (`MobileAds.ShowBanner()`, `MobileAds.ShowInterstitial()`, `MobileAds.ShowRewarded()`), full Google Mobile Ads (AdMob v8/v9+) and Unity Ads support, Google UMP / GDPR consent handling, intelligent auto-reload with retry backoff, Remove-Ads persistence, and an interactive **In-Editor Ad Simulator**.

---

## ✨ Features

- 🚀 **1-Line Static API**: Call `MobileAds.ShowInterstitial()` or `MobileAds.ShowRewarded()` from anywhere in your codebase.
- 📱 **Google Mobile Ads (AdMob v8/v9+) & Unity Ads**: Pre-configured adapter architecture supporting banners, interstitials, rewarded videos, and app open ads.
- 🎮 **In-Editor Ad Simulator**: Zero device-building needed for gameplay testing! Displays visual mock banners, countdown interstitials, and simulated rewarded videos with interactive claim/skip controls.
- 🛡️ **Built-in Interstitial Cooldown**: Configurable cooldown interval (e.g. 30s) prevents frustrating accidental back-to-back interstitials.
- 🔄 **Smart Auto-Reload & Exponential Backoff**: Automatically schedules reloads when ads close or fail to load with network backoff delays.
- 🚫 **Remove-Ads (IAP Integration)**: One call (`MobileAds.SetRemoveAds(true)`) automatically hides active banners and bypasses interstitials while keeping rewarded ads active. Persists across game sessions in `PlayerPrefs`.
- 🇪🇺 **Google UMP / GDPR Consent**: Built-in User Messaging Platform support for European EEA/UK compliance with privacy options dialogs.
- ⚙️ **Dedicated Settings Window**: Manage Ad Unit IDs, test mode, COPPA settings, and preprocessor defines from `Tools > Mobile Ads > Settings Window`.
- 📦 **UPM Compliant**: Clean Assembly Definitions (`.asmdef`) ensure no compilation errors even when native SDKs are not yet installed.

---

## 📦 Installation

### Option 1: Install via Git URL (Recommended)
1. In Unity, navigate to **Window > Package Manager**.
2. Click the **`+`** icon in the upper-left corner and select **Add package from git URL...**
3. Paste the repository URL:
   ```text
   https://github.com/Ahmad8i1a1/Ads_Custom_Package.git
   ```
4. Click **Add**. Unity will download and import the package automatically.

### Option 2: Install via Local Disk
1. Clone or download this repository.
2. In **Window > Package Manager**, click **`+` > Add package from disk...**
3. Select `package.json` in the root folder.

---

## ⚡ Quick Start

### 1. Configure Settings
Open the settings panel via **Tools > Mobile Ads > Settings Window** or click **Tools > Mobile Ads > Populate Test IDs** to instantly load Google's official test ad units.

```csharp
using MobileAdsPackage;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // 1. Initialize Ads (Auto-loads banners, interstitials, and rewarded ads)
        MobileAds.Initialize(result =>
        {
            Debug.Log($"Ads Initialized: {result.Success} on {result.ActiveNetwork}");
        });
    }

    // 2. Banner Ads
    public void ShowBanner()
    {
        MobileAds.ShowBanner(BannerPosition.Bottom, BannerType.Adaptive);
    }

    public void HideBanner()
    {
        MobileAds.HideBanner();
    }

    // 3. Interstitial Ads
    public void OnGameOver()
    {
        if (MobileAds.CanShowInterstitial)
        {
            MobileAds.ShowInterstitial(onClosed: () =>
            {
                RestartLevel();
            });
        }
        else
        {
            RestartLevel();
        }
    }

    // 4. Rewarded Ads
    public void OnWatchRewardAd()
    {
        if (MobileAds.IsRewardedReady)
        {
            MobileAds.ShowRewarded(
                onRewarded: reward =>
                {
                    GivePlayerCoins(50);
                },
                onClosed: () =>
                {
                    Debug.Log("Rewarded ad closed.");
                }
            );
        }
    }

    // 5. Remove Ads (Call after IAP purchase)
    public void OnNoAdsPurchased()
    {
        MobileAds.SetRemoveAds(true);
    }
}
```

---

## 🆚 Comparison with Gley Mobile Ads

| Gley Mobile Ads API | Mobile Ads Manager API | Description |
| :--- | :--- | :--- |
| `Gley.MobileAds.API.Initialize()` | `MobileAds.Initialize()` | Initializes provider and caches ads |
| `Gley.MobileAds.API.ShowBanner(...)` | `MobileAds.ShowBanner(pos, type)` | Shows banner at chosen position & size |
| `Gley.MobileAds.API.HideBanner()` | `MobileAds.HideBanner()` | Hides currently visible banner |
| `Gley.MobileAds.API.IsInterstitialAvailable()` | `MobileAds.CanShowInterstitial` | Checks if ad is loaded and cooldown passed |
| `Gley.MobileAds.API.ShowInterstitial(cb)` | `MobileAds.ShowInterstitial(cb)` | Shows interstitial and auto-reloads |
| `Gley.MobileAds.API.IsRewardedVideoAvailable()` | `MobileAds.IsRewardedReady` | Checks rewarded video readiness |
| `Gley.MobileAds.API.ShowRewardedVideo(cb)` | `MobileAds.ShowRewarded(onReward, cb)` | Shows rewarded video and grants reward |
| `Gley.MobileAds.API.ShowAppOpen()` | `MobileAds.ShowAppOpen(cb)` | Shows app resume / splash ad |
| `Gley.MobileAds.API.SetRemoveAds(true)` | `MobileAds.SetRemoveAds(true)` | Disables banners/interstitials across sessions |

---

## 🕹️ In-Editor Ad Simulator

When running inside the Unity Editor (Play Mode), the **In-Editor Ad Simulator** automatically activates:
- **Simulated Banner**: Renders a customizable banner box at the specified screen position with a test dismiss button.
- **Simulated Interstitial**: Displays a modal overlay with an automatic countdown and a "Skip / Close" button.
- **Simulated Rewarded Video**: Renders video progress with real-time playback bar, a "Claim Reward" button, and an "Abandon (No Reward)" button to test all user branches.

---

## 📜 License

MIT License. Feel free to use in personal and commercial Unity projects.
