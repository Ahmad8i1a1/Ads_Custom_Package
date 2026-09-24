using System.IO;
using UnityEditor;
using UnityEngine;

namespace MobileAdsPackage.Editor
{
    public static class MobileAdsMenuItems
    {
        [MenuItem("Tools/Mobile Ads/Settings Window", false, 0)]
        [MenuItem("Window/Mobile Ads/Settings Window", false, 100)]
        public static void OpenSettingsWindow()
        {
            MobileAdsSettingsWindow.ShowWindow();
        }

        [MenuItem("Tools/Mobile Ads/Select or Create Settings Asset", false, 1)]
        public static void SelectOrCreateSettings()
        {
            var asset = GetOrCreateSettingsAsset();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        [MenuItem("Tools/Mobile Ads/Populate Test IDs", false, 20)]
        public static void PopulateTestIDs()
        {
            var asset = GetOrCreateSettingsAsset();
            Undo.RecordObject(asset, "Populate Test IDs");
            asset.PopulateOfficialTestIDs();
            asset.testMode = true;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=#33FF88>[MobileAds] Populated official Google test IDs in MobileAdsSettings.</color>");
        }

        public static AdSettings GetOrCreateSettingsAsset()
        {
            var asset = Resources.Load<AdSettings>(AdSettings.RESOURCE_PATH);
            if (asset == null)
            {
                // Ensure Resources folder exists
                string resourcesFolder = "Assets/Resources";
                if (!AssetDatabase.IsValidFolder(resourcesFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                string assetPath = $"{resourcesFolder}/{AdSettings.RESOURCE_PATH}.asset";
                asset = ScriptableObject.CreateInstance<AdSettings>();
                asset.PopulateOfficialTestIDs();

                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"<color=#33FF88>[MobileAds] Created MobileAdsSettings at {assetPath}</color>");
            }
            return asset;
        }
    }
}
