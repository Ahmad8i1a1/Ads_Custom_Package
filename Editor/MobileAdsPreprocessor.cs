using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.Build;
#endif

namespace MobileAdsPackage.Editor
{
    /// <summary>
    /// Utilities for detecting installed ad SDKs and managing scripting define symbols
    /// ('MOBILE_ADS_ADMOB', 'MOBILE_ADS_UNITY') across build target groups.
    /// </summary>
    [InitializeOnLoad]
    public static class MobileAdsPreprocessor
    {
        public const string DEFINE_ADMOB = "MOBILE_ADS_ADMOB";
        public const string DEFINE_UNITY = "MOBILE_ADS_UNITY";

        static MobileAdsPreprocessor()
        {
            // Auto check on project load / domain reload
            EditorApplication.delayCall += CheckAndPromptDefines;
        }

        public static bool IsAdMobInstalled()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType("GoogleMobileAds.Api.MobileAds") != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsUnityAdsInstalled()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType("UnityEngine.Advertisements.Advertisement") != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasDefineSymbol(string symbol)
        {
            var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = GetDefines(targetGroup);
            return defines.Split(';').Select(d => d.Trim()).Contains(symbol);
        }

        public static void SetDefineSymbol(string symbol, bool enable)
        {
            var targetGroups = new[]
            {
                BuildTargetGroup.Android,
                BuildTargetGroup.iOS,
                BuildTargetGroup.Standalone
            };

            foreach (var group in targetGroups)
            {
                string defines = GetDefines(group);
                var list = defines.Split(';')
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrEmpty(d))
                    .ToList();

                if (enable && !list.Contains(symbol))
                {
                    list.Add(symbol);
                    SetDefines(group, string.Join(";", list));
                    Debug.Log($"<color=#33FF88>[MobileAds] Added define symbol '{symbol}' to {group}.</color>");
                }
                else if (!enable && list.Contains(symbol))
                {
                    list.Remove(symbol);
                    SetDefines(group, string.Join(";", list));
                    Debug.Log($"<color=#FF8833>[MobileAds] Removed define symbol '{symbol}' from {group}.</color>");
                }
            }
        }

        private static string GetDefines(BuildTargetGroup group)
        {
#if UNITY_2021_2_OR_NEWER
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(group);
            return PlayerSettings.GetScriptingDefineSymbols(namedTarget);
#else
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#endif
        }

        private static void SetDefines(BuildTargetGroup group, string defines)
        {
#if UNITY_2021_2_OR_NEWER
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(group);
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, defines);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
#endif
        }

        private static void CheckAndPromptDefines()
        {
            // Auto-activate define if SDK is detected but define is missing
            if (IsAdMobInstalled() && !HasDefineSymbol(DEFINE_ADMOB))
            {
                Debug.Log("[MobileAds] Detected Google Mobile Ads SDK. Enabling 'MOBILE_ADS_ADMOB' define symbol...");
                SetDefineSymbol(DEFINE_ADMOB, true);
            }

            if (IsUnityAdsInstalled() && !HasDefineSymbol(DEFINE_UNITY))
            {
                Debug.Log("[MobileAds] Detected Unity Advertisements package. Enabling 'MOBILE_ADS_UNITY' define symbol...");
                SetDefineSymbol(DEFINE_UNITY, true);
            }
        }
    }
}
