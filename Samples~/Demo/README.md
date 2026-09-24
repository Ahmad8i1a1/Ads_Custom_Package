# Mobile Ads Manager Demo Sample

This sample demonstrates complete integration of the **Mobile Ads Manager** package, similar to Gley Mobile Ads.

## Quick Start in Unity Editor

1. Open any empty scene or create a new GameObject in your scene named `DemoAds`.
2. Attach the `DemoAdsUI` script:
   - Select `DemoAds` GameObject -> Add Component -> `DemoAdsUI`.
3. Press **Play** in the Unity Editor.
4. You will immediately see interactive on-screen controls:
   - **Initialize Ads**: Initializes the selected ad provider (in-Editor simulator by default).
   - **Show Banner (Bottom/Top)**: Displays a simulated banner bar.
   - **Show Interstitial**: Triggers a simulated full-screen interstitial ad with countdown and skip button.
   - **Show Rewarded Ad**: Triggers a rewarded video simulation with progress bar and reward callback granting +50 coins.
   - **Show App Open Ad**: Triggers a simulated app resume ad.
   - **Buy 'Remove Ads'**: Tests the IAP remove-ads feature. Notice that banners are immediately hidden and interstitials are skipped, while rewarded ads still work for bonus coins!
   - **GDPR / UMP Consent**: Tests consent form flows.

## Connecting to Canvas UI

If you prefer using Unity Canvas UI buttons instead of the automatic OnGUI controls, simply assign your buttons and text fields to the Inspector slots on `DemoAdsUI`:
- `Init Button`
- `Show Banner Bottom Button`
- `Show Interstitial Button`
- `Show Rewarded Button`
- `Status Text`
- `Coins Text`
