using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileAdsPackage.Samples
{
    /// <summary>
    /// Interactive demonstration script showcasing all features of Mobile Ads Manager.
    /// Can be connected to Canvas UI or rendered via OnGUI if no Canvas is set up.
    /// </summary>
    public class DemoAdsUI : MonoBehaviour
    {
        [Header("Optional Canvas UI Bindings (Leave empty to use OnGUI)")]
        public Button initButton;
        public Button showBannerBottomButton;
        public Button showBannerTopButton;
        public Button hideBannerButton;
        public Button showInterstitialButton;
        public Button showRewardedButton;
        public Button showAppOpenButton;
        public Button toggleNoAdsButton;
        public Button privacyOptionsButton;
        public Text statusText;
        public Text coinsText;

        [Header("Demo State")]
        public int coinBalance = 100;

        private readonly List<string> _eventLogs = new List<string>();
        private Vector2 _logScroll;
        private GUIStyle _headerStyle;
        private GUIStyle _btnStyle;
        private GUIStyle _logStyle;

        private void Start()
        {
            AddLog("Mobile Ads Demo started.");

            // Listen to high-level events
            MobileAds.OnInitialized += result =>
            {
                AddLog($"Initialized: {result.Success} on {result.ActiveNetwork}");
                UpdateStatus();
            };

            MobileAds.OnAdLoaded += type =>
            {
                AddLog($"Ad Loaded: {type}");
                UpdateStatus();
            };

            MobileAds.OnAdFailedToLoad += error =>
            {
                AddLog($"<color=#FF5555>Ad Failed ({error.AdType}): {error.Message}</color>");
                UpdateStatus();
            };

            MobileAds.OnAdOpened += type =>
            {
                AddLog($"Ad Opened: {type}");
            };

            MobileAds.OnAdClosed += type =>
            {
                AddLog($"Ad Closed: {type}");
                UpdateStatus();
            };

            MobileAds.OnUserEarnedReward += reward =>
            {
                coinBalance += (int)reward.Amount;
                AddLog($"<color=#44FF77>User Earned Reward: +{reward.Amount} {reward.Type}! Total Coins: {coinBalance}</color>");
                UpdateCoins();
            };

            // Bind optional canvas buttons if assigned
            if (initButton != null) initButton.onClick.AddListener(() => MobileAds.Initialize());
            if (showBannerBottomButton != null) showBannerBottomButton.onClick.AddListener(() => MobileAds.ShowBanner(BannerPosition.Bottom, BannerType.Adaptive));
            if (showBannerTopButton != null) showBannerTopButton.onClick.AddListener(() => MobileAds.ShowBanner(BannerPosition.Top, BannerType.Adaptive));
            if (hideBannerButton != null) hideBannerButton.onClick.AddListener(() => MobileAds.HideBanner());
            if (showInterstitialButton != null) showInterstitialButton.onClick.AddListener(OnShowInterstitialClicked);
            if (showRewardedButton != null) showRewardedButton.onClick.AddListener(OnShowRewardedClicked);
            if (showAppOpenButton != null) showAppOpenButton.onClick.AddListener(() => MobileAds.ShowAppOpen());
            if (toggleNoAdsButton != null) toggleNoAdsButton.onClick.AddListener(OnToggleNoAdsClicked);
            if (privacyOptionsButton != null) privacyOptionsButton.onClick.AddListener(() => MobileAds.ShowConsentForm());

            UpdateStatus();
            UpdateCoins();
        }

        private void OnShowInterstitialClicked()
        {
            if (MobileAds.CanShowInterstitial)
            {
                AddLog("Showing Interstitial...");
                MobileAds.ShowInterstitial(() => AddLog("Interstitial dismissed."));
            }
            else
            {
                AddLog("Interstitial not ready or currently on cooldown.");
            }
        }

        private void OnShowRewardedClicked()
        {
            if (MobileAds.IsRewardedReady)
            {
                AddLog("Showing Rewarded Ad...");
                MobileAds.ShowRewarded(
                    reward =>
                    {
                        // Reward is also handled in OnUserEarnedReward event
                    },
                    onClosed: () => AddLog("Rewarded ad dismissed.")
                );
            }
            else
            {
                AddLog("Rewarded Ad is not ready yet.");
            }
        }

        private void OnToggleNoAdsClicked()
        {
            bool newState = !MobileAds.IsNoAdsPurchased;
            MobileAds.SetRemoveAds(newState);
            AddLog($"Remove Ads toggled: {(newState ? "PURCHASED (No Banners/Interstitials)" : "INACTIVE")}");
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (statusText != null)
            {
                statusText.text = $"Init: {MobileAds.IsInitialized} | Banner: {MobileAds.IsBannerShowing} | Interstitial: {MobileAds.IsInterstitialReady} | Rewarded: {MobileAds.IsRewardedReady} | NoAds: {MobileAds.IsNoAdsPurchased}";
            }
        }

        private void UpdateCoins()
        {
            if (coinsText != null)
            {
                coinsText.text = $"Coins: {coinBalance}";
            }
        }

        private void AddLog(string msg)
        {
            string time = System.DateTime.Now.ToString("HH:mm:ss");
            string line = $"[{time}] {msg}";
            _eventLogs.Insert(0, line);
            if (_eventLogs.Count > 40) _eventLogs.RemoveAt(_eventLogs.Count - 1);
            Debug.Log(line);
        }

        #region Fallback IMGUI Controls (When No Canvas UI is Bound)

        private void OnGUI()
        {
            // If user did not hook up Canvas buttons, render interactive UI controls automatically!
            if (initButton != null) return;

            InitStyles();

            float panelW = Mathf.Min(340f, Screen.width * 0.45f);
            float panelH = Screen.height - 20f;

            // Left Panel: Controls
            GUILayout.BeginArea(new Rect(10, 10, panelW, panelH), GUI.skin.box);
            GUILayout.Label("Mobile Ads Manager Demo", _headerStyle);
            GUILayout.Space(5);

            GUILayout.Label($"<b>Status:</b> {(MobileAds.IsInitialized ? "<color=#44FF77>Initialized</color>" : "<color=#FF5555>Not Initialized</color>")}", _logStyle);
            GUILayout.Label($"<b>Coins:</b> <color=#FFDD44>{coinBalance}</color> | <b>No Ads:</b> {(MobileAds.IsNoAdsPurchased ? "<color=#44FF77>YES</color>" : "NO")}", _logStyle);
            GUILayout.Label($"<b>Banner:</b> {(MobileAds.IsBannerShowing ? "Showing" : "Hidden")} | <b>Interstitial:</b> {(MobileAds.IsInterstitialReady ? "Ready" : "Loading")}", _logStyle);
            GUILayout.Label($"<b>Rewarded:</b> {(MobileAds.IsRewardedReady ? "<color=#44FF77>Ready</color>" : "Loading")} | <b>App Open:</b> {(MobileAds.IsAppOpenReady ? "Ready" : "Loading")}", _logStyle);

            GUILayout.Space(8);

            if (GUILayout.Button("Initialize Ads", _btnStyle, GUILayout.Height(32)))
            {
                MobileAds.Initialize();
            }

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Show Banner (Bottom)", GUILayout.Height(30)))
            {
                MobileAds.ShowBanner(BannerPosition.Bottom, BannerType.Adaptive);
            }
            if (GUILayout.Button("Show Banner (Top)", GUILayout.Height(30)))
            {
                MobileAds.ShowBanner(BannerPosition.Top, BannerType.Adaptive);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Hide / Destroy Banner", GUILayout.Height(28)))
            {
                MobileAds.HideBanner();
                MobileAds.DestroyBanner();
            }

            GUILayout.Space(6);
            if (GUILayout.Button("Show Interstitial Ad", _btnStyle, GUILayout.Height(34)))
            {
                OnShowInterstitialClicked();
            }

            GUILayout.Space(4);
            if (GUILayout.Button("Show Rewarded Ad (+50 Coins)", _btnStyle, GUILayout.Height(34)))
            {
                OnShowRewardedClicked();
            }

            GUILayout.Space(4);
            if (GUILayout.Button("Show App Open Ad", GUILayout.Height(30)))
            {
                MobileAds.ShowAppOpen(() => AddLog("App Open ad closed."));
            }

            GUILayout.Space(6);
            string noAdsBtnText = MobileAds.IsNoAdsPurchased ? "Turn OFF 'Remove Ads'" : "Buy 'Remove Ads' (IAP Mock)";
            if (GUILayout.Button(noAdsBtnText, GUILayout.Height(30)))
            {
                OnToggleNoAdsClicked();
            }

            GUILayout.Space(4);
            if (GUILayout.Button("Show GDPR / UMP Consent Form", GUILayout.Height(28)))
            {
                MobileAds.ShowConsentForm((success, msg) => AddLog($"Privacy Form: {success} ({msg})"));
            }

            GUILayout.EndArea();

            // Right Panel: Live Event Logs
            float logX = panelW + 20f;
            float logW = Screen.width - logX - 10f;
            if (logW > 180f)
            {
                GUILayout.BeginArea(new Rect(logX, 10, logW, panelH * 0.6f), GUI.skin.box);
                GUILayout.Label("Live Event Log", _headerStyle);
                GUILayout.Space(5);

                _logScroll = GUILayout.BeginScrollView(_logScroll);
                foreach (var log in _eventLogs)
                {
                    GUILayout.Label(log, _logStyle);
                }
                GUILayout.EndScrollView();

                if (GUILayout.Button("Clear Log", GUILayout.Height(24)))
                {
                    _eventLogs.Clear();
                }

                GUILayout.EndArea();
            }
        }

        private void InitStyles()
        {
            if (_headerStyle != null) return;

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _headerStyle.normal.textColor = Color.white;

            _btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold
            };

            _logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                richText = true,
                wordWrap = true
            };
        }

        #endregion
    }
}
