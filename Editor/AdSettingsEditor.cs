using UnityEditor;
using UnityEngine;

namespace MobileAdsPackage.Editor
{
    [CustomEditor(typeof(AdSettings))]
    public class AdSettingsEditor : UnityEditor.Editor
    {
        private AdSettings _settings;
        private int _selectedTab = 0;
        private readonly string[] _tabTitles = new[] { "General", "Google AdMob", "Unity Ads", "Privacy & UMP", "Editor Simulator" };

        private void OnEnable()
        {
            _settings = (AdSettings)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();

            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabTitles, GUILayout.Height(28));
            EditorGUILayout.Space(10);

            switch (_selectedTab)
            {
                case 0:
                    DrawGeneralTab();
                    break;
                case 1:
                    DrawAdMobTab();
                    break;
                case 2:
                    DrawUnityAdsTab();
                    break;
                case 3:
                    DrawPrivacyTab();
                    break;
                case 4:
                    DrawSimulatorTab();
                    break;
            }

            EditorGUILayout.Space(15);
            DrawQuickActions();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Mobile Ads Manager", EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (_settings.testMode)
            {
                var testStyle = new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = new Color(0.2f, 0.8f, 0.4f) } };
                GUILayout.Label("● TEST MODE ACTIVE", testStyle);
            }
            else
            {
                var liveStyle = new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = new Color(1f, 0.4f, 0.3f) } };
                GUILayout.Label("● PRODUCTION MODE", liveStyle);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"Active Network: {_settings.primaryNetwork}  |  Fallback: {(_settings.enableFallback ? _settings.fallbackNetwork.ToString() : "Disabled")}", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawGeneralTab()
        {
            EditorGUILayout.LabelField("Provider & Mediation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("primaryNetwork"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableFallback"));
            if (_settings.enableFallback)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fallbackNetwork"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Operational Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("testMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoInitializeOnStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoLoadAds"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableDebugLogs"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Timers & Retries", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("interstitialCooldownSeconds"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("retryDelayBaseSeconds"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxRetryAttempts"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoShowAppOpenOnResume"));
        }

        private void DrawAdMobTab()
        {
            bool isAdMobDefined = MobileAdsPreprocessor.HasDefineSymbol(MobileAdsPreprocessor.DEFINE_ADMOB);
            if (!isAdMobDefined)
            {
                EditorGUILayout.HelpBox("AdMob define symbol 'MOBILE_ADS_ADMOB' is not enabled. Click below or open Tools > Mobile Ads > Settings to configure.", MessageType.Warning);
                if (GUILayout.Button("Enable MOBILE_ADS_ADMOB Symbol", GUILayout.Height(25)))
                {
                    MobileAdsPreprocessor.SetDefineSymbol(MobileAdsPreprocessor.DEFINE_ADMOB, true);
                }
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.LabelField("Android Ad Unit IDs", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("androidAppId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("androidBannerId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("androidInterstitialId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("androidRewardedId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("androidRewardedInterstitialId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("androidAppOpenId"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("iOS Ad Unit IDs", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iosAppId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iosBannerId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iosInterstitialId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iosRewardedId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iosRewardedInterstitialId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iosAppOpenId"));

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Fill Official Google Test Unit IDs", GUILayout.Height(26)))
            {
                Undo.RecordObject(_settings, "Fill Test IDs");
                _settings.PopulateOfficialTestIDs();
                _settings.testMode = true;
                EditorUtility.SetDirty(_settings);
            }
        }

        private void DrawUnityAdsTab()
        {
            bool isUnityDefined = MobileAdsPreprocessor.HasDefineSymbol(MobileAdsPreprocessor.DEFINE_UNITY);
            if (!isUnityDefined)
            {
                EditorGUILayout.HelpBox("Unity Ads define symbol 'MOBILE_ADS_UNITY' is not enabled. If 'com.unity.ads' is installed, enable it below.", MessageType.Info);
                if (GUILayout.Button("Enable MOBILE_ADS_UNITY Symbol", GUILayout.Height(25)))
                {
                    MobileAdsPreprocessor.SetDefineSymbol(MobileAdsPreprocessor.DEFINE_UNITY, true);
                }
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.LabelField("Game Identifiers", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unityAndroidGameId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unityIosGameId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unityTestMode"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Placement IDs", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unityBannerPlacement"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unityInterstitialPlacement"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unityRewardedPlacement"));
        }

        private void DrawPrivacyTab()
        {
            EditorGUILayout.LabelField("Regulations & Compliance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tagForChildDirectedTreatment"), new GUIContent("COPPA Compliance (Child-Directed)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tagForUnderAgeOfConsent"), new GUIContent("Under Age of Consent"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Google UMP (User Messaging Platform)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoRequestUmpConsent"), new GUIContent("Auto Request Consent on Start"));
            EditorGUILayout.HelpBox("Google UMP displays GDPR/EEA/UK compliant consent dialogs automatically. You can also re-trigger it anytime via MobileAds.ShowConsentForm().", MessageType.Info);
        }

        private void DrawSimulatorTab()
        {
            EditorGUILayout.LabelField("In-Editor Ad Simulator", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useSimulatorInEditor"), new GUIContent("Enable Simulator in Editor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("simulatedLoadDelay"), new GUIContent("Load Delay (seconds)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("simulatedInterstitialCountdown"), new GUIContent("Interstitial Auto-Close (s)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("simulatedRewardedDuration"), new GUIContent("Rewarded Watch Duration (s)"));

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("The Editor Ad Simulator provides interactive banners, interstitials with countdowns, and rewarded videos with claim/skip buttons directly inside Unity Editor Play Mode without requiring native device builds.", MessageType.Info);
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Settings Window", GUILayout.Height(30)))
            {
                MobileAdsSettingsWindow.ShowWindow();
            }
            if (GUILayout.Button("Populate Test IDs", GUILayout.Height(30)))
            {
                MobileAdsMenuItems.PopulateTestIDs();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
