using System;
using UnityEngine;

namespace MobileAdsPackage
{
    /// <summary>
    /// Runtime in-game UI overlay for simulating Mobile Ads directly inside the Unity Editor.
    /// Provides interactive visual banners, full-screen interstitials, rewarded videos with progress bars, and app open ads.
    /// </summary>
    public class EditorAdSimulatorUI : MonoBehaviour
    {
        private static EditorAdSimulatorUI _instance;

        public static EditorAdSimulatorUI GetOrCreate()
        {
            if (_instance == null)
            {
                var go = new GameObject("[MobileAds_EditorSimulator]");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<EditorAdSimulatorUI>();
            }
            return _instance;
        }

        // Banner state
        public bool IsBannerVisible { get; private set; }
        public BannerPosition CurrentBannerPosition { get; private set; } = BannerPosition.Bottom;
        public BannerType CurrentBannerType { get; private set; } = BannerType.Adaptive;

        // Interstitial state
        private bool _isShowingInterstitial;
        private Action _onInterstitialClosed;
        private float _interstitialTimer;
        private float _interstitialDuration = 3f;

        // Rewarded state
        private bool _isShowingRewarded;
        private Action<AdReward> _onRewardedSuccess;
        private Action _onRewardedClosed;
        private float _rewardedTimer;
        private float _rewardedDuration = 5f;
        private bool _rewardGranted;

        // App Open state
        private bool _isShowingAppOpen;
        private Action _onAppOpenClosed;

        // GUI Styles and Textures
        private GUIStyle _cardStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _subTitleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _cancelButtonStyle;
        private GUIStyle _bannerStyle;
        private Texture2D _darkOverlayTex;
        private Texture2D _cardBgTex;
        private Texture2D _bannerBgTex;
        private Texture2D _progressBarBgTex;
        private Texture2D _progressBarFillTex;
        private bool _stylesInitialized;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_isShowingInterstitial)
            {
                _interstitialTimer -= Time.unscaledDeltaTime;
                if (_interstitialTimer <= 0f)
                {
                    CloseInterstitial();
                }
            }

            if (_isShowingRewarded)
            {
                _rewardedTimer += Time.unscaledDeltaTime;
                if (_rewardedTimer >= _rewardedDuration && !_rewardGranted)
                {
                    _rewardGranted = true;
                }
            }
        }

        #region Public Show/Hide Triggers

        public void ShowBanner(BannerPosition position, BannerType type)
        {
            CurrentBannerPosition = position;
            CurrentBannerType = type;
            IsBannerVisible = true;
        }

        public void HideBanner()
        {
            IsBannerVisible = false;
        }

        public void ShowInterstitial(float autoCloseDuration, Action onClosed)
        {
            _isShowingInterstitial = true;
            _interstitialDuration = Mathf.Max(1f, autoCloseDuration);
            _interstitialTimer = _interstitialDuration;
            _onInterstitialClosed = onClosed;
        }

        public void ShowRewarded(float duration, Action<AdReward> onRewarded, Action onClosed)
        {
            _isShowingRewarded = true;
            _rewardedDuration = Mathf.Max(1f, duration);
            _rewardedTimer = 0f;
            _rewardGranted = false;
            _onRewardedSuccess = onRewarded;
            _onRewardedClosed = onClosed;
        }

        public void ShowAppOpen(Action onClosed)
        {
            _isShowingAppOpen = true;
            _onAppOpenClosed = onClosed;
        }

        private void CloseInterstitial()
        {
            _isShowingInterstitial = false;
            var callback = _onInterstitialClosed;
            _onInterstitialClosed = null;
            callback?.Invoke();
        }

        private void CompleteRewarded()
        {
            _isShowingRewarded = false;
            var rewardCallback = _onRewardedSuccess;
            var closeCallback = _onRewardedClosed;
            _onRewardedSuccess = null;
            _onRewardedClosed = null;

            rewardCallback?.Invoke(new AdReward("Simulated Coins", 50));
            closeCallback?.Invoke();
        }

        private void CancelRewarded()
        {
            _isShowingRewarded = false;
            var closeCallback = _onRewardedClosed;
            _onRewardedSuccess = null;
            _onRewardedClosed = null;

            closeCallback?.Invoke();
        }

        private void CloseAppOpen()
        {
            _isShowingAppOpen = false;
            var callback = _onAppOpenClosed;
            _onAppOpenClosed = null;
            callback?.Invoke();
        }

        #endregion

        #region IMGUI Rendering

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            _darkOverlayTex = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.85f));
            _cardBgTex = MakeTex(2, 2, new Color(0.12f, 0.14f, 0.18f, 0.98f));
            _bannerBgTex = MakeTex(2, 2, new Color(0.08f, 0.10f, 0.14f, 0.95f));
            _progressBarBgTex = MakeTex(2, 2, new Color(0.2f, 0.22f, 0.26f, 1f));
            _progressBarFillTex = MakeTex(2, 2, new Color(0.2f, 0.75f, 0.35f, 1f));

            _cardStyle = new GUIStyle();
            _cardStyle.normal.background = _cardBgTex;
            _cardStyle.padding = new RectOffset(25, 25, 20, 20);

            _titleStyle = new GUIStyle();
            _titleStyle.fontSize = 20;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.normal.textColor = Color.white;

            _subTitleStyle = new GUIStyle();
            _subTitleStyle.fontSize = 13;
            _subTitleStyle.alignment = TextAnchor.MiddleCenter;
            _subTitleStyle.normal.textColor = new Color(0.75f, 0.8f, 0.85f);
            _subTitleStyle.wordWrap = true;

            _buttonStyle = new GUIStyle();
            _buttonStyle.normal.background = MakeTex(2, 2, new Color(0.18f, 0.52f, 0.92f));
            _buttonStyle.hover.background = MakeTex(2, 2, new Color(0.28f, 0.62f, 1.0f));
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.fontSize = 14;
            _buttonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.alignment = TextAnchor.MiddleCenter;

            _cancelButtonStyle = new GUIStyle();
            _cancelButtonStyle.normal.background = MakeTex(2, 2, new Color(0.8f, 0.25f, 0.25f));
            _cancelButtonStyle.hover.background = MakeTex(2, 2, new Color(0.9f, 0.35f, 0.35f));
            _cancelButtonStyle.normal.textColor = Color.white;
            _cancelButtonStyle.fontSize = 13;
            _cancelButtonStyle.fontStyle = FontStyle.Bold;
            _cancelButtonStyle.alignment = TextAnchor.MiddleCenter;

            _bannerStyle = new GUIStyle();
            _bannerStyle.normal.background = _bannerBgTex;
            _bannerStyle.alignment = TextAnchor.MiddleCenter;
            _bannerStyle.fontSize = 12;
            _bannerStyle.fontStyle = FontStyle.Bold;
            _bannerStyle.normal.textColor = new Color(0.3f, 0.85f, 0.4f);

            _stylesInitialized = true;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void OnGUI()
        {
            InitStyles();

            // Render Banner if active
            if (IsBannerVisible)
            {
                DrawBanner();
            }

            // Render Fullscreen Dialogs
            if (_isShowingInterstitial)
            {
                DrawInterstitialModal();
            }
            else if (_isShowingRewarded)
            {
                DrawRewardedModal();
            }
            else if (_isShowingAppOpen)
            {
                DrawAppOpenModal();
            }
        }

        private void DrawBanner()
        {
            float bannerWidth = Screen.width;
            float bannerHeight = 50f;

            if (CurrentBannerType == BannerType.MediumRectangle)
            {
                bannerWidth = Mathf.Min(300f, Screen.width);
                bannerHeight = 250f;
            }
            else if (CurrentBannerType == BannerType.LargeBanner)
            {
                bannerHeight = 100f;
            }
            else if (CurrentBannerType == BannerType.Leaderboard)
            {
                bannerWidth = Mathf.Min(728f, Screen.width);
                bannerHeight = 90f;
            }

            float x = (Screen.width - bannerWidth) * 0.5f;
            float y = Screen.height - bannerHeight; // Default Bottom

            switch (CurrentBannerPosition)
            {
                case BannerPosition.Top:
                    y = 0;
                    break;
                case BannerPosition.Bottom:
                    y = Screen.height - bannerHeight;
                    break;
                case BannerPosition.TopLeft:
                    x = 0;
                    y = 0;
                    break;
                case BannerPosition.TopRight:
                    x = Screen.width - bannerWidth;
                    y = 0;
                    break;
                case BannerPosition.BottomLeft:
                    x = 0;
                    y = Screen.height - bannerHeight;
                    break;
                case BannerPosition.BottomRight:
                    x = Screen.width - bannerWidth;
                    y = Screen.height - bannerHeight;
                    break;
                case BannerPosition.Center:
                    x = (Screen.width - bannerWidth) * 0.5f;
                    y = (Screen.height - bannerHeight) * 0.5f;
                    break;
            }

            Rect bannerRect = new Rect(x, y, bannerWidth, bannerHeight);
            GUI.Box(bannerRect, $"[SIMULATED {CurrentBannerType.ToString().ToUpper()} BANNER - {bannerWidth:0}x{bannerHeight:0}]", _bannerStyle);

            // Small hide button on the corner of the banner
            Rect closeBtnRect = new Rect(x + bannerWidth - 25f, y + 2f, 22f, 20f);
            if (GUI.Button(closeBtnRect, "X", _cancelButtonStyle))
            {
                HideBanner();
            }
        }

        private void DrawInterstitialModal()
        {
            // Dark Backdrop
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _darkOverlayTex);

            float cardW = Mathf.Min(460f, Screen.width * 0.9f);
            float cardH = 260f;
            float cardX = (Screen.width - cardW) * 0.5f;
            float cardY = (Screen.height - cardH) * 0.5f;

            GUILayout.BeginArea(new Rect(cardX, cardY, cardW, cardH), _cardStyle);
            GUILayout.Label("[ SIMULATED INTERSTITIAL AD ]", _titleStyle);
            GUILayout.Space(10);
            GUILayout.Label("Testing Ad integration in Unity Editor.\nThis full-screen interstitial simulates a live ad view.", _subTitleStyle);
            GUILayout.Space(15);

            int secRemaining = Mathf.CeilToInt(_interstitialTimer);
            GUILayout.Label($"Auto-closing in {secRemaining} second{(secRemaining == 1 ? "" : "s")}...", _subTitleStyle);
            GUILayout.Space(20);

            if (GUILayout.Button("SKIP / CLOSE AD", _buttonStyle, GUILayout.Height(45)))
            {
                CloseInterstitial();
            }
            GUILayout.EndArea();
        }

        private void DrawRewardedModal()
        {
            // Dark Backdrop
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _darkOverlayTex);

            float cardW = Mathf.Min(480f, Screen.width * 0.9f);
            float cardH = 310f;
            float cardX = (Screen.width - cardW) * 0.5f;
            float cardY = (Screen.height - cardH) * 0.5f;

            float progress = Mathf.Clamp01(_rewardedTimer / _rewardedDuration);
            int secRemaining = Mathf.CeilToInt(Mathf.Max(0f, _rewardedDuration - _rewardedTimer));

            GUILayout.BeginArea(new Rect(cardX, cardY, cardW, cardH), _cardStyle);
            GUILayout.Label("[ SIMULATED REWARDED AD ]", _titleStyle);
            GUILayout.Space(8);
            GUILayout.Label("Simulating video playback to grant in-game player rewards.", _subTitleStyle);
            GUILayout.Space(12);

            // Progress Bar
            Rect pBarRect = GUILayoutUtility.GetRect(cardW - 50, 18);
            GUI.DrawTexture(pBarRect, _progressBarBgTex);
            Rect pBarFillRect = new Rect(pBarRect.x, pBarRect.y, pBarRect.width * progress, pBarRect.height);
            GUI.DrawTexture(pBarFillRect, _progressBarFillTex);

            GUILayout.Space(8);
            if (!_rewardGranted)
            {
                GUILayout.Label($"Watching video... {secRemaining}s left ({Mathf.RoundToInt(progress * 100)}%)", _subTitleStyle);
            }
            else
            {
                GUILayout.Label("<color=#44FF77>✓ Reward Ready to Claim!</color>", _subTitleStyle);
            }
            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_rewardGranted ? "CLAIM REWARD" : "SKIP TO REWARD", _buttonStyle, GUILayout.Height(45)))
            {
                CompleteRewarded();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("CANCEL (NO REWARD)", _cancelButtonStyle, GUILayout.Height(45)))
            {
                CancelRewarded();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void DrawAppOpenModal()
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _darkOverlayTex);

            float cardW = Mathf.Min(440f, Screen.width * 0.9f);
            float cardH = 240f;
            float cardX = (Screen.width - cardW) * 0.5f;
            float cardY = (Screen.height - cardH) * 0.5f;

            GUILayout.BeginArea(new Rect(cardX, cardY, cardW, cardH), _cardStyle);
            GUILayout.Label("[ SIMULATED APP OPEN AD ]", _titleStyle);
            GUILayout.Space(10);
            GUILayout.Label("App Open Ad shown on game start or app resume from background.", _subTitleStyle);
            GUILayout.Space(25);

            if (GUILayout.Button("CONTINUE TO GAME", _buttonStyle, GUILayout.Height(45)))
            {
                CloseAppOpen();
            }
            GUILayout.EndArea();
        }

        #endregion
    }
}
