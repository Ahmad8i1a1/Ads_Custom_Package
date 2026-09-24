using UnityEditor;
using UnityEngine;

namespace MobileAdsPackage.Editor
{
    /// <summary>
    /// Dedicated Editor Window for Mobile Ads Manager (Tools > Mobile Ads > Settings Window).
    /// Provides central configuration, SDK status diagnostics, define symbols management, and quick code cheat sheets.
    /// </summary>
    public class MobileAdsSettingsWindow : EditorWindow
    {
        private AdSettings _settings;
        private UnityEditor.Editor _cachedEditor;
        private Vector2 _scrollPos;
        private int _tab = 0;
        private readonly string[] _tabs = new[] { "Settings", "SDK Status & Defines", "Code Cheatsheet", "About" };

        [MenuItem("Tools/Mobile Ads/Settings Window", false, 0)]
        [MenuItem("Window/Mobile Ads/Settings Window", false, 100)]
        public static void ShowWindow()
        {
            var win = GetWindow<MobileAdsSettingsWindow>("Mobile Ads Settings");
            win.minSize = new Vector2(520, 580);
            win.Show();
        }

        private void OnEnable()
        {
            _settings = MobileAdsMenuItems.GetOrCreateSettingsAsset();
        }

        private void OnGUI()
        {
            if (_settings == null)
            {
                _settings = MobileAdsMenuItems.GetOrCreateSettingsAsset();
            }

            DrawTitleBanner();

            _tab = GUILayout.Toolbar(_tab, _tabs, GUILayout.Height(30));
            EditorGUILayout.Space(10);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_tab)
            {
                case 0:
                    DrawSettingsTab();
                    break;
                case 1:
                    DrawSdkStatusTab();
                    break;
                case 2:
                    DrawCheatsheetTab();
                    break;
                case 3:
                    DrawAboutTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawTitleBanner()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("MOBILE ADS MANAGER", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (_settings != null && _settings.testMode)
            {
                var testStyle = new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = new Color(0.2f, 0.85f, 0.4f) } };
                GUILayout.Label("● TEST MODE ACTIVE", testStyle);
            }
            else
            {
                var liveStyle = new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = new Color(1f, 0.4f, 0.3f) } };
                GUILayout.Label("● PRODUCTION MODE", liveStyle);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Gley-style unified mobile advertising solution for Unity", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawSettingsTab()
        {
            if (_settings == null) return;

            if (_cachedEditor == null || _cachedEditor.target != _settings)
            {
                _cachedEditor = UnityEditor.Editor.CreateEditor(_settings);
            }

            _cachedEditor.OnInspectorGUI();
        }

        private void DrawSdkStatusTab()
        {
            EditorGUILayout.LabelField("Installed SDKs & Preprocessor Symbols", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this panel to verify whether the native mobile ad SDKs are recognized by Unity and ensure the compilation symbols are active.", MessageType.Info);
            EditorGUILayout.Space(10);

            // AdMob Section
            bool admobInstalled = MobileAdsPreprocessor.IsAdMobInstalled();
            bool admobDefined = MobileAdsPreprocessor.HasDefineSymbol(MobileAdsPreprocessor.DEFINE_ADMOB);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Google Mobile Ads (AdMob)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"SDK Detected: {(admobInstalled ? "✓ Yes" : "✕ Not Found (import unitypackage or UPM)")}");
            EditorGUILayout.LabelField($"Define Symbol ({MobileAdsPreprocessor.DEFINE_ADMOB}): {(admobDefined ? "✓ Enabled" : "✕ Disabled")}");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(admobDefined ? "Disable Symbol" : "Enable Symbol", GUILayout.Height(26)))
            {
                MobileAdsPreprocessor.SetDefineSymbol(MobileAdsPreprocessor.DEFINE_ADMOB, !admobDefined);
            }
            if (GUILayout.Button("Import / Open AdMob UnityPackage", GUILayout.Height(26)))
            {
                EditorUtility.DisplayDialog("AdMob Import", "To install Google Mobile Ads, import the official GoogleMobileAds.unitypackage or use EDM4U.", "OK");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Unity Ads Section
            bool unityInstalled = MobileAdsPreprocessor.IsUnityAdsInstalled();
            bool unityDefined = MobileAdsPreprocessor.HasDefineSymbol(MobileAdsPreprocessor.DEFINE_UNITY);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Unity Advertisements (com.unity.ads)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"SDK Detected: {(unityInstalled ? "✓ Yes" : "✕ Not Found")}");
            EditorGUILayout.LabelField($"Define Symbol ({MobileAdsPreprocessor.DEFINE_UNITY}): {(unityDefined ? "✓ Enabled" : "✕ Disabled")}");

            if (GUILayout.Button(unityDefined ? "Disable Symbol" : "Enable Symbol", GUILayout.Height(26)))
            {
                MobileAdsPreprocessor.SetDefineSymbol(MobileAdsPreprocessor.DEFINE_UNITY, !unityDefined);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Editor Simulator
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("In-Editor Ad Simulator", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Status: ✓ Always Available (Zero external dependencies)");
            EditorGUILayout.HelpBox("Provides interactive visual banners, full-screen interstitials with countdowns, and rewarded videos with claim/skip buttons directly inside Unity Editor Play Mode without requiring native device builds.", MessageType.None);
            EditorGUILayout.EndVertical();
        }

        private void DrawCheatsheetTab()
        {
            EditorGUILayout.LabelField("Quick Code Cheatsheet", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Reference examples for integrating Mobile Ads into your scripts.", MessageType.Info);
            EditorGUILayout.Space(8);

            DrawCodeSnippet("1. Initialize Mobile Ads",
@"using MobileAdsPackage;

void Start()
{
    // Auto-initializes if enabled in settings, or call manually:
    MobileAds.Initialize(result => {
        Debug.Log($""Ads Initialized: {result.Success} on {result.ActiveNetwork}"");
    });
}");

            DrawCodeSnippet("2. Banner Ads",
@"// Show adaptive banner at bottom
MobileAds.ShowBanner(BannerPosition.Bottom, BannerType.Adaptive);

// Hide or Destroy
MobileAds.HideBanner();
MobileAds.DestroyBanner();");

            DrawCodeSnippet("3. Interstitial Ads",
@"// Check availability & show
if (MobileAds.CanShowInterstitial)
{
    MobileAds.ShowInterstitial(() => {
        Debug.Log(""Interstitial closed! Resume game logic."");
    });
}");

            DrawCodeSnippet("4. Rewarded Ads",
@"// Show rewarded video and grant bonus
if (MobileAds.IsRewardedReady)
{
    MobileAds.ShowRewarded(
        onRewarded: reward => {
            Debug.Log($""Player earned: {reward.Amount} {reward.Type}"");
            AddCoins(100);
        },
        onClosed: () => {
            Debug.Log(""Rewarded video closed."");
        }
    );
}");

            DrawCodeSnippet("5. Remove Ads (IAP Integration)",
@"// Call this when player purchases 'No Ads'
MobileAds.SetRemoveAds(true);

// Check anywhere
if (MobileAds.IsNoAdsPurchased) { ... }");

            DrawCodeSnippet("6. Privacy & Consent (GDPR / UMP)",
@"// Show privacy options form (e.g. from in-game Settings button)
MobileAds.ShowConsentForm((success, message) => {
    Debug.Log($""Consent updated: {success}"");
});");
        }

        private void DrawCodeSnippet(string title, string code)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.TextArea(code, EditorStyles.textArea);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6);
        }

        private void DrawAboutTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("About Mobile Ads Manager", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Version: 1.0.0");
            EditorGUILayout.LabelField("Architecture: Gley-style Unified Mediation & Static Facade");
            EditorGUILayout.LabelField("Supported Networks: Google Mobile Ads (AdMob v8/v9+), Unity Ads, In-Editor Simulator");
            EditorGUILayout.LabelField("Author: Ahmad Bilal");
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("This package is structured according to the Unity Package Manager (UPM) standard and can be added via Git URL or local disk.", MessageType.Info);
            EditorGUILayout.EndVertical();
        }
    }
}
