# Changelog

All notable changes to the **Mobile Ads Manager** package will be documented in this file.

## [1.0.0] - 2026-09-25

### Added
- **Core Architecture**:
  - `MobileAds` unified static facade API matching Gley Mobile Ads patterns.
  - `MobileAdsManager` persistent singleton with auto-retry and cooldown mechanics.
  - `AdSettings` ScriptableObject for centralized configuration in `Resources/MobileAdsSettings`.
  - Full support for Banner, Interstitial, Rewarded, Rewarded Interstitial, and App Open ads.
- **Provider Adapters**:
  - `AdMobProvider`: Full Google Mobile Ads SDK (v8.x/v9.x+) integration with adaptive banners and Google UMP consent.
  - `UnityAdsProvider`: Unity Advertisements integration with banner, interstitial, and rewarded handlers.
  - `MockAdProvider`: Standalone in-editor ad simulator with zero external SDK dependencies.
- **Editor Ad Simulator**:
  - Realistic IMGUI overlay for banners, countdown interstitials, and rewarded videos with interactive claim/skip controls.
- **Editor Tooling**:
  - `Tools > Mobile Ads > Settings Window`: Central configuration hub with SDK detection, tabbed views, and code cheatsheets.
  - `AdSettingsEditor`: Custom Inspector for `AdSettings` with status banners and one-click test ID population.
  - `MobileAdsPreprocessor`: Automated management of `MOBILE_ADS_ADMOB` and `MOBILE_ADS_UNITY` compilation symbols.
- **Privacy & Compliance**:
  - Native Google UMP (User Messaging Platform) integration for GDPR / EEA / UK compliance.
  - COPPA (Child-Directed) and Under Age of Consent configuration.
- **Samples & Documentation**:
  - Interactive `DemoAdsUI` sample script supporting both OnGUI and Canvas UI bindings.
  - Comprehensive documentation in `Documentation~/MobileAdsGuide.md`.
