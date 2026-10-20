#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using HexaSort.UI;

namespace HexaSort.Editor
{
    public static class MainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/Asset/Scenes/UI scene.unity";

        [MenuItem("HexaSort/Open UI Scene")]
        public static void OpenScene()
        {
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("HexaSort/Build UI Scene")]
        public static void BuildScene()
        {
            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Asset/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/Asset", "Scenes");
            }

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Setup Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.08f, 0.23f, 1f); // Rich dark purple
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.AddComponent<AudioListener>();

            // 2. Setup EventSystem
            var eventGo = new GameObject("EventSystem");
            eventGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // 3. Setup Canvas for 20:9 Aspect Ratio (1080 x 2400 reference)
            var canvasGo = new GameObject("MainMenuCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2400); // 20:9 aspect ratio standard
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f; // Width-based 1080: perfect responsive layout across 20:9 modern phones

            canvasGo.AddComponent<GraphicRaycaster>();

            // Add MainMenuUIEffects to Canvas
            var effects = canvasGo.AddComponent<MainMenuUIEffects>();

            // Load Fonts & Sprites
            var fontAsset = LoadAsset<TMP_FontAsset>("Assets/Asset/Art/Fonts/Coolvetica SDF.asset");
            var square10 = LoadAsset<Sprite>("Assets/Asset/Art/Sprites/Mine/Square_10.png");
            var square20 = LoadAsset<Sprite>("Assets/Asset/Art/Sprites/Mine/Square_20.png");
            var square50 = LoadAsset<Sprite>("Assets/Asset/Art/Sprites/Mine/Square_50.png");
            var circle = LoadAsset<Sprite>("Assets/Asset/Art/Sprites/Mine/Circle.png");
            var circleOutline = LoadAsset<Sprite>("Assets/Asset/Art/Sprites/Mine/Circle_Outline.png");
            var starSprite = LoadAsset<Sprite>("Assets/Asset/Art/Sprites/Mine/Star.png");
            var triangleSprite = LoadAsset<Sprite>("Assets/Asset/Art/Sprites/Mine/Triangle.png");
            var pinSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Spin-Pin_0.asset");

            // Hero Island
            var lighthouseSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Texture2D/1_PleasantLighthouse 1.png");
            var bubbleSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Texture2D/NewBubble.png");

            // Play Button
            var playButtonSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/PlayButtonNew_0.asset");
            if (playButtonSprite == null)
                playButtonSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Texture2D/PlayButtonNew.png");

            // Hexagon Buttons
            var hexaSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Quest-Btn-Bg_0.asset");
            if (hexaSprite == null)
                hexaSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Texture2D/Achievemennt-Btn-Hexa.png");

            var mailboxSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Texture2D/Mailbox 1.png");
            var clipboardSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Texture2D/clipboard.png");

            // Top Bar
            var avatarSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Texture2D/hxs_maleavatar1.png");
            var coinSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Atlas-Coin.asset");
            var addBtnSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Atlas-Button-Add-B.asset");
            var heartSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Texture2D/0_Red_Hearts.png");
            var settingsSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Atlas-Button-Settings.asset");

            // Side Buttons Sprites
            var lockSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Atlas-Lock-A.asset");
            var coinStackSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/CoinStack3 1_0.asset");
            var colorWheelSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Spin-Icon_0.asset");
            var piggySprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/PiggyBank_0.asset");
            var alertSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Alert_0.asset");
            var noAdsSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Texture2D/AD-Board.png");
            if (noAdsSprite == null)
                noAdsSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/No-Ads-Small.asset");

            var pumpkinSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/54_WitchsWhimsy_S.asset");
            var volcanoSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/18_MartianVista.asset");
            var airplaneSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/11_AeroHub.asset");
            var storybookSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/20_StorybookGrove.asset");

            // Nav Sprites
            var navMapSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Icon_Event_Flat.asset");
            var navShopSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Icon_Shop_Flat.asset");
            var navHomeSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Icon_Home_Export.asset");
            if (navHomeSprite == null)
                navHomeSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Texture2D/HomeIcon.png");
            var navLeaderSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Icon_LeaderBoard_Flat_1.asset");
            var navCardSprite = LoadAsset<Sprite>("Assets/Asset/Art/New Folder/Sprite/Icon_Collection_Flat_0.asset");

            // -------------------------------------------------------------
            // 4. Background Hierarchy
            // -------------------------------------------------------------
            var bgGo = CreateUIObject("Background", canvasGo.transform);
            SetStretch(bgGo);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.08f, 0.23f, 1f); // Deep rich dark purple
            bgImg.raycastTarget = false; // Never block UI raycasts

            // Background Sparkles (spatially distributed for 20:9 canvas height 2400)
            var sparklesGo = CreateUIObject("Sparkles", bgGo.transform);
            SetStretch(sparklesGo);
            var starList = new List<RectTransform>();
            Vector2[] starPositions = new Vector2[]
            {
                new Vector2(-380, 950), new Vector2(-150, 1100), new Vector2(250, 1020),
                new Vector2(380, 800), new Vector2(-80, 650), new Vector2(220, 620),
                new Vector2(-300, 250), new Vector2(330, 280), new Vector2(-350, -180),
                new Vector2(280, -360), new Vector2(-120, -580), new Vector2(360, -750),
                new Vector2(-260, -920), new Vector2(160, -1020)
            };

            for (int i = 0; i < starPositions.Length; i++)
            {
                var star = CreateUIObject($"Star_{i}", sparklesGo.transform);
                var rt = star.GetComponent<RectTransform>();
                rt.anchoredPosition = starPositions[i];
                float s = Random.Range(14f, 28f);
                rt.sizeDelta = new Vector2(s, s);
                var img = star.AddComponent<Image>();
                img.sprite = starSprite != null ? starSprite : circle;
                img.color = new Color(1f, 1f, 1f, Random.Range(0.45f, 0.9f));
                img.raycastTarget = false; // Never block UI clicks
                star.AddComponent<CanvasGroup>();
                starList.Add(rt);
            }

            // -------------------------------------------------------------
            // 5. Hero Island (Center - Proportionally scaled for 20:9)
            // -------------------------------------------------------------
            var islandRoot = CreateUIObject("HeroIsland", canvasGo.transform);
            var islandRt = islandRoot.GetComponent<RectTransform>();
            islandRt.anchorMin = new Vector2(0.5f, 0.5f);
            islandRt.anchorMax = new Vector2(0.5f, 0.5f);
            islandRt.pivot = new Vector2(0.5f, 0.5f);
            islandRt.anchoredPosition = new Vector2(0, 95f); // Placed proportionally in 20:9 vertical space
            islandRt.sizeDelta = new Vector2(920, 920);

            var islandImg = islandRoot.AddComponent<Image>();
            islandImg.sprite = lighthouseSprite;
            islandImg.preserveAspect = true;
            islandImg.raycastTarget = false; // CRITICAL: Never block clicks to buttons below!

            // Pier Progress Badge ("63%")
            var badgeGo = CreateUIObject("PierProgressBadge", islandRoot.transform);
            var badgeRt = badgeGo.GetComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(0.5f, 0.5f);
            badgeRt.anchorMax = new Vector2(0.5f, 0.5f);
            badgeRt.pivot = new Vector2(0.5f, 0.5f);
            badgeRt.anchoredPosition = new Vector2(-200, -150);
            badgeRt.sizeDelta = new Vector2(150, 74);

            var badgeImg = badgeGo.AddComponent<Image>();
            badgeImg.sprite = bubbleSprite != null ? bubbleSprite : square20;
            badgeImg.raycastTarget = true;
            var badgeBtn = badgeGo.AddComponent<Button>();
            badgeBtn.targetGraphic = badgeImg;
            badgeGo.AddComponent<JuicyButton>();

            // Text "63%"
            var text63Go = CreateUIObject("Text", badgeGo.transform);
            var text63Rt = text63Go.GetComponent<RectTransform>();
            text63Rt.anchorMin = Vector2.zero;
            text63Rt.anchorMax = Vector2.one;
            text63Rt.offsetMin = new Vector2(8, 0);
            text63Rt.offsetMax = new Vector2(-38, 0);
            var text63 = text63Go.AddComponent<TextMeshProUGUI>();
            text63.font = fontAsset;
            text63.text = "63%";
            text63.fontSize = 32;
            text63.fontStyle = FontStyles.Bold;
            text63.color = new Color(0.12f, 0.22f, 0.45f, 1f);
            text63.alignment = TextAlignmentOptions.Center;
            text63.raycastTarget = false; // Clicks pass through to badgeBtn

            // Triangle Play Icon
            var arrowGo = CreateUIObject("ArrowIcon", badgeGo.transform);
            var arrowRt = arrowGo.GetComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(1f, 0.5f);
            arrowRt.anchorMax = new Vector2(1f, 0.5f);
            arrowRt.pivot = new Vector2(0.5f, 0.5f);
            arrowRt.anchoredPosition = new Vector2(-26, 0);
            arrowRt.sizeDelta = new Vector2(22, 22);
            arrowRt.localEulerAngles = new Vector3(0, 0, -90); // Pointing right
            var arrowImg = arrowGo.AddComponent<Image>();
            arrowImg.sprite = triangleSprite != null ? triangleSprite : circle;
            arrowImg.color = new Color(0.12f, 0.22f, 0.45f, 1f);
            arrowImg.preserveAspect = true;
            arrowImg.raycastTarget = false;

            // Seagulls
            var seagullList = new List<RectTransform>();
            Vector2[] seagullPositions = new Vector2[] { new Vector2(-75, 450), new Vector2(210, 460) };
            for (int i = 0; i < seagullPositions.Length; i++)
            {
                var gullGo = CreateUIObject($"Seagull_{i}", islandRoot.transform);
                var rt = gullGo.GetComponent<RectTransform>();
                rt.anchoredPosition = seagullPositions[i];
                rt.sizeDelta = new Vector2(40, 28);
                var gullText = gullGo.AddComponent<TextMeshProUGUI>();
                gullText.font = fontAsset;
                gullText.text = "v";
                gullText.fontSize = 26;
                gullText.fontStyle = FontStyles.Bold;
                gullText.color = new Color(0.95f, 0.95f, 1f, 0.9f);
                gullText.alignment = TextAlignmentOptions.Center;
                gullText.raycastTarget = false;
                seagullList.Add(rt);
            }

            // -------------------------------------------------------------
            // 6. Top Bar (Anchored Top for 20:9 Notch Safe Area)
            // -------------------------------------------------------------
            var topBarGo = CreateUIObject("TopBar", canvasGo.transform);
            var topBarRt = topBarGo.GetComponent<RectTransform>();
            topBarRt.anchorMin = new Vector2(0, 1);
            topBarRt.anchorMax = new Vector2(1, 1);
            topBarRt.pivot = new Vector2(0.5f, 1);
            topBarRt.anchoredPosition = new Vector2(0, -60); // 60px safe area top margin
            topBarRt.sizeDelta = new Vector2(0, 130);

            // 6a. Avatar Button (Left)
            var avatarGo = CreateUIObject("AvatarButton", topBarGo.transform);
            var avatarRt = avatarGo.GetComponent<RectTransform>();
            avatarRt.anchorMin = new Vector2(0, 0.5f);
            avatarRt.anchorMax = new Vector2(0, 0.5f);
            avatarRt.pivot = new Vector2(0.5f, 0.5f);
            avatarRt.anchoredPosition = new Vector2(105, 0);
            avatarRt.sizeDelta = new Vector2(120, 120);

            var avatarBg = avatarGo.AddComponent<Image>();
            avatarBg.sprite = square20;
            avatarBg.color = new Color(0.38f, 0.65f, 0.98f, 1f); // Sky blue border
            avatarBg.raycastTarget = true;
            var avatarBtn = avatarGo.AddComponent<Button>();
            avatarBtn.targetGraphic = avatarBg;
            avatarGo.AddComponent<JuicyButton>();

            // Avatar inner image
            var avatarInnerGo = CreateUIObject("AvatarInner", avatarGo.transform);
            var avatarInnerRt = avatarInnerGo.GetComponent<RectTransform>();
            avatarInnerRt.anchorMin = Vector2.zero;
            avatarInnerRt.anchorMax = Vector2.one;
            avatarInnerRt.offsetMin = new Vector2(6, 6);
            avatarInnerRt.offsetMax = new Vector2(-6, -6);
            var avatarInnerImg = avatarInnerGo.AddComponent<Image>();
            avatarInnerImg.sprite = avatarSprite;
            avatarInnerImg.preserveAspect = true;
            avatarInnerImg.raycastTarget = false;

            // "NEW" Ribbon
            var newTagGo = CreateUIObject("NewTag", avatarGo.transform);
            var newTagRt = newTagGo.GetComponent<RectTransform>();
            newTagRt.anchorMin = new Vector2(0, 1);
            newTagRt.anchorMax = new Vector2(0, 1);
            newTagRt.pivot = new Vector2(0.5f, 0.5f);
            newTagRt.anchoredPosition = new Vector2(12, -12);
            newTagRt.sizeDelta = new Vector2(62, 28);
            newTagRt.localEulerAngles = new Vector3(0, 0, 25);
            var newTagImg = newTagGo.AddComponent<Image>();
            newTagImg.sprite = square10;
            newTagImg.color = new Color(0.96f, 0.2f, 0.4f, 1f);
            newTagImg.raycastTarget = false;

            var newTagTextGo = CreateUIObject("Text", newTagGo.transform);
            SetStretch(newTagTextGo);
            var newTagText = newTagTextGo.AddComponent<TextMeshProUGUI>();
            newTagText.font = fontAsset;
            newTagText.text = "NEW";
            newTagText.fontSize = 16;
            newTagText.fontStyle = FontStyles.Bold;
            newTagText.color = Color.white;
            newTagText.alignment = TextAlignmentOptions.Center;
            newTagText.raycastTarget = false;

            // 6b. Coins Counter (Center-Left) - STATIC DISPLAY WIDGET (NO BUTTON, NO INTERACTION)
            var coinBarGo = CreateUIObject("CoinsCounter", topBarGo.transform);
            var coinBarRt = coinBarGo.GetComponent<RectTransform>();
            coinBarRt.anchorMin = new Vector2(0.5f, 0.5f);
            coinBarRt.anchorMax = new Vector2(0.5f, 0.5f);
            coinBarRt.pivot = new Vector2(0.5f, 0.5f);
            coinBarRt.anchoredPosition = new Vector2(-125, 0);
            coinBarRt.sizeDelta = new Vector2(220, 72);

            var coinBarImg = coinBarGo.AddComponent<Image>();
            coinBarImg.sprite = square50;
            coinBarImg.color = new Color(0.10f, 0.05f, 0.18f, 0.95f);
            coinBarImg.raycastTarget = false; // Pure display widget

            // Coin Icon
            var coinIconGo = CreateUIObject("CoinIcon", coinBarGo.transform);
            var coinIconRt = coinIconGo.GetComponent<RectTransform>();
            coinIconRt.anchorMin = new Vector2(0, 0.5f);
            coinIconRt.anchorMax = new Vector2(0, 0.5f);
            coinIconRt.pivot = new Vector2(0.5f, 0.5f);
            coinIconRt.anchoredPosition = new Vector2(14, 0);
            coinIconRt.sizeDelta = new Vector2(78, 78);
            var coinIconImg = coinIconGo.AddComponent<Image>();
            coinIconImg.sprite = coinSprite;
            coinIconImg.preserveAspect = true;
            coinIconImg.raycastTarget = false;

            // Add (+) Badge
            if (addBtnSprite != null)
            {
                var addBadgeGo = CreateUIObject("AddBadge", coinIconGo.transform);
                var addBadgeRt = addBadgeGo.GetComponent<RectTransform>();
                addBadgeRt.anchorMin = new Vector2(1, 0);
                addBadgeRt.anchorMax = new Vector2(1, 0);
                addBadgeRt.pivot = new Vector2(0.5f, 0.5f);
                addBadgeRt.anchoredPosition = new Vector2(-8, 12);
                addBadgeRt.sizeDelta = new Vector2(32, 32);
                var addBadgeImg = addBadgeGo.AddComponent<Image>();
                addBadgeImg.sprite = addBtnSprite;
                addBadgeImg.preserveAspect = true;
                addBadgeImg.raycastTarget = false;
            }

            // Text "125"
            var coinTextGo = CreateUIObject("Text", coinBarGo.transform);
            var coinTextRt = coinTextGo.GetComponent<RectTransform>();
            coinTextRt.anchorMin = new Vector2(0, 0);
            coinTextRt.anchorMax = new Vector2(1, 1);
            coinTextRt.offsetMin = new Vector2(62, 0);
            coinTextRt.offsetMax = new Vector2(-12, 0);
            var coinText = coinTextGo.AddComponent<TextMeshProUGUI>();
            coinText.font = fontAsset;
            coinText.text = "125";
            coinText.fontSize = 36;
            coinText.fontStyle = FontStyles.Bold;
            coinText.color = Color.white;
            coinText.alignment = TextAlignmentOptions.Center;
            coinText.raycastTarget = false;

            // 6c. Lives / Hearts Counter (Center-Right) - STATIC DISPLAY WIDGET (NO BUTTON, NO INTERACTION)
            var heartBarGo = CreateUIObject("HeartsCounter", topBarGo.transform);
            var heartBarRt = heartBarGo.GetComponent<RectTransform>();
            heartBarRt.anchorMin = new Vector2(0.5f, 0.5f);
            heartBarRt.anchorMax = new Vector2(0.5f, 0.5f);
            heartBarRt.pivot = new Vector2(0.5f, 0.5f);
            heartBarRt.anchoredPosition = new Vector2(140, 0);
            heartBarRt.sizeDelta = new Vector2(250, 72);

            var heartBarImg = heartBarGo.AddComponent<Image>();
            heartBarImg.sprite = square50;
            heartBarImg.color = new Color(0.10f, 0.05f, 0.18f, 0.95f);
            heartBarImg.raycastTarget = false; // Pure display widget

            // Heart Icon
            var heartIconGo = CreateUIObject("HeartIcon", heartBarGo.transform);
            var heartIconRt = heartIconGo.GetComponent<RectTransform>();
            heartIconRt.anchorMin = new Vector2(0, 0.5f);
            heartIconRt.anchorMax = new Vector2(0, 0.5f);
            heartIconRt.pivot = new Vector2(0.5f, 0.5f);
            heartIconRt.anchoredPosition = new Vector2(20, 0);
            heartIconRt.sizeDelta = new Vector2(76, 76);
            var heartIconImg = heartIconGo.AddComponent<Image>();
            heartIconImg.sprite = heartSprite;
            heartIconImg.preserveAspect = true;
            heartIconImg.raycastTarget = false;

            var heartCountGo = CreateUIObject("HeartCount", heartIconGo.transform);
            SetStretch(heartCountGo);
            var heartCount = heartCountGo.AddComponent<TextMeshProUGUI>();
            heartCount.font = fontAsset;
            heartCount.text = "5";
            heartCount.fontSize = 32;
            heartCount.fontStyle = FontStyles.Bold;
            heartCount.color = Color.white;
            heartCount.alignment = TextAlignmentOptions.Center;
            heartCount.raycastTarget = false;

            // Text "FULL"
            var fullTextGo = CreateUIObject("TextFull", heartBarGo.transform);
            var fullTextRt = fullTextGo.GetComponent<RectTransform>();
            fullTextRt.anchorMin = new Vector2(0, 0);
            fullTextRt.anchorMax = new Vector2(1, 1);
            fullTextRt.offsetMin = new Vector2(66, 0);
            fullTextRt.offsetMax = new Vector2(-14, 0);
            var fullText = fullTextGo.AddComponent<TextMeshProUGUI>();
            fullText.font = fontAsset;
            fullText.text = "FULL";
            fullText.fontSize = 34;
            fullText.fontStyle = FontStyles.Bold;
            fullText.color = Color.white;
            fullText.alignment = TextAlignmentOptions.Center;
            fullText.raycastTarget = false;

            // 6d. Settings Button (Right)
            var settingsGo = CreateUIObject("SettingsButton", topBarGo.transform);
            var settingsRt = settingsGo.GetComponent<RectTransform>();
            settingsRt.anchorMin = new Vector2(1, 0.5f);
            settingsRt.anchorMax = new Vector2(1, 0.5f);
            settingsRt.pivot = new Vector2(0.5f, 0.5f);
            settingsRt.anchoredPosition = new Vector2(-105, 0);
            settingsRt.sizeDelta = new Vector2(96, 96);

            var settingsImg = settingsGo.AddComponent<Image>();
            settingsImg.sprite = settingsSprite;
            settingsImg.preserveAspect = true;
            settingsImg.raycastTarget = true;
            var settingsBtn = settingsGo.AddComponent<Button>();
            settingsBtn.targetGraphic = settingsImg;
            settingsGo.AddComponent<JuicyButton>();

            // -------------------------------------------------------------
            // 7. Left Side Action Buttons (20:9 Vertical Layout)
            // -------------------------------------------------------------
            var leftSideGo = CreateUIObject("LeftSideButtons", canvasGo.transform);
            var leftSideRt = leftSideGo.GetComponent<RectTransform>();
            leftSideRt.anchorMin = new Vector2(0, 1);
            leftSideRt.anchorMax = new Vector2(0, 1);
            leftSideRt.pivot = new Vector2(0, 1);
            leftSideRt.anchoredPosition = new Vector2(105, 0);
            leftSideRt.sizeDelta = new Vector2(140, 1200);

            var sideButtonsList = new List<Button>();
            var notifDotList = new List<RectTransform>();
            var lockBadgesList = new List<RectTransform>();

            // Left 1: Trophy / Lv 7
            var left1 = CreateSideBadgeButton("Btn_Lv7", leftSideGo.transform, new Vector2(0, -350),
                coinStackSprite, "Lv 7", lockSprite, false, fontAsset, square50, circle, circleOutline);
            sideButtonsList.Add(left1.GetComponent<Button>());
            var lock1 = left1.transform.Find("LockBadge")?.GetComponent<RectTransform>();
            if (lock1 != null) lockBadgesList.Add(lock1);

            // Left 2: Color Wheel / 1/5
            var left2 = CreateSideBadgeButton("Btn_ColorWheel", leftSideGo.transform, new Vector2(0, -560),
                colorWheelSprite, "1/5", null, false, fontAsset, square50, circle, circleOutline, true, false, pinSprite, triangleSprite);
            sideButtonsList.Add(left2.GetComponent<Button>());
            var wheelTransform = left2.transform.Find("Icon").GetComponent<RectTransform>();
            var needleTransform = left2.transform.Find("Needle").GetComponent<RectTransform>();

            // Left 3: Piggy Bank / 75/700
            var left3 = CreateSideBadgeButton("Btn_PiggyBank", leftSideGo.transform, new Vector2(0, -770),
                piggySprite, "75/700", null, true, fontAsset, square50, circle, circleOutline, false, true);
            sideButtonsList.Add(left3.GetComponent<Button>());
            var piggyIconRt = left3.transform.Find("Icon").GetComponent<RectTransform>();
            var notifDotPiggy = left3.transform.Find("NotifDot").GetComponent<RectTransform>();
            notifDotList.Add(notifDotPiggy);

            // Left 4: Remove Ads
            var left4 = CreateSideBadgeButton("Btn_RemoveAds", leftSideGo.transform, new Vector2(0, -980),
                noAdsSprite, "REMOVE ADS", null, false, fontAsset, square50, circle, circleOutline);
            sideButtonsList.Add(left4.GetComponent<Button>());

            // -------------------------------------------------------------
            // 8. Right Side Action Buttons (20:9 Vertical Layout)
            // -------------------------------------------------------------
            var rightSideGo = CreateUIObject("RightSideButtons", canvasGo.transform);
            var rightSideRt = rightSideGo.GetComponent<RectTransform>();
            rightSideRt.anchorMin = new Vector2(1, 1);
            rightSideRt.anchorMax = new Vector2(1, 1);
            rightSideRt.pivot = new Vector2(1, 1);
            rightSideRt.anchoredPosition = new Vector2(-105, 0);
            rightSideRt.sizeDelta = new Vector2(140, 1200);

            // Right 1: Halloween / Lv 15
            var right1 = CreateSideBadgeButton("Btn_Lv15", rightSideGo.transform, new Vector2(0, -350),
                pumpkinSprite, "Lv 15", lockSprite, false, fontAsset, square50, circle, circleOutline);
            sideButtonsList.Add(right1.GetComponent<Button>());
            var lock2 = right1.transform.Find("LockBadge")?.GetComponent<RectTransform>();
            if (lock2 != null) lockBadgesList.Add(lock2);

            // Right 2: Volcano / Lv 50
            var right2 = CreateSideBadgeButton("Btn_Lv50", rightSideGo.transform, new Vector2(0, -560),
                volcanoSprite, "Lv 50", lockSprite, false, fontAsset, square50, circle, circleOutline);
            sideButtonsList.Add(right2.GetComponent<Button>());
            var lock3 = right2.transform.Find("LockBadge")?.GetComponent<RectTransform>();
            if (lock3 != null) lockBadgesList.Add(lock3);

            // Right 3: Airplane / Lv 11
            var right3 = CreateSideBadgeButton("Btn_Lv11", rightSideGo.transform, new Vector2(0, -770),
                airplaneSprite, "Lv 11", lockSprite, false, fontAsset, square50, circle, circleOutline);
            sideButtonsList.Add(right3.GetComponent<Button>());
            var airplaneIconRt = right3.transform.Find("Icon").GetComponent<RectTransform>();
            var lock4 = right3.transform.Find("LockBadge")?.GetComponent<RectTransform>();
            if (lock4 != null) lockBadgesList.Add(lock4);

            // Right 4: Storybook / Lv 72
            var right4 = CreateSideBadgeButton("Btn_Lv72", rightSideGo.transform, new Vector2(0, -980),
                storybookSprite, "Lv 72", lockSprite, false, fontAsset, square50, circle, circleOutline);
            sideButtonsList.Add(right4.GetComponent<Button>());
            var lock5 = right4.transform.Find("LockBadge")?.GetComponent<RectTransform>();
            if (lock5 != null) lockBadgesList.Add(lock5);

            // -------------------------------------------------------------
            // 9. Bottom Action Area (Mailbox, LEVEL 6 Button, Checklist)
            // -------------------------------------------------------------
            var bottomActionGo = CreateUIObject("BottomActionArea", canvasGo.transform);
            var bottomActionRt = bottomActionGo.GetComponent<RectTransform>();
            bottomActionRt.anchorMin = new Vector2(0.5f, 0);
            bottomActionRt.anchorMax = new Vector2(0.5f, 0);
            bottomActionRt.pivot = new Vector2(0.5f, 0);
            bottomActionRt.anchoredPosition = new Vector2(0, 360); // 360px above bottom in 20:9, comfortably above the 185px nav bar
            bottomActionRt.sizeDelta = new Vector2(1080, 170);

            // 9a. Mailbox Button (Left) - Hexagon Button
            var mailboxGo = CreateUIObject("MailboxButton", bottomActionGo.transform);
            var mailboxRt = mailboxGo.GetComponent<RectTransform>();
            mailboxRt.anchoredPosition = new Vector2(-290, 0);
            mailboxRt.sizeDelta = new Vector2(142, 142);
            var mailboxHexaImg = mailboxGo.AddComponent<Image>();
            mailboxHexaImg.sprite = hexaSprite != null ? hexaSprite : square20;
            mailboxHexaImg.preserveAspect = true;
            mailboxHexaImg.raycastTarget = true; // Primary hit target
            var mailboxBtn = mailboxGo.AddComponent<Button>();
            mailboxBtn.targetGraphic = mailboxHexaImg;
            mailboxGo.AddComponent<JuicyButton>();

            var mailboxIconGo = CreateUIObject("Icon", mailboxGo.transform);
            var mailboxIconRt = mailboxIconGo.GetComponent<RectTransform>();
            mailboxIconRt.anchoredPosition = Vector2.zero;
            mailboxIconRt.sizeDelta = new Vector2(96, 96);
            var mailboxIconImg = mailboxIconGo.AddComponent<Image>();
            mailboxIconImg.sprite = mailboxSprite;
            mailboxIconImg.preserveAspect = true;
            mailboxIconImg.raycastTarget = false; // Don't block clicks to button!

            // 9b. Big LEVEL 6 Play Button (Center)
            var playBtnGo = CreateUIObject("PlayButton_Level6", bottomActionGo.transform);
            var playBtnRt = playBtnGo.GetComponent<RectTransform>();
            playBtnRt.anchoredPosition = new Vector2(0, 0);
            playBtnRt.sizeDelta = new Vector2(470, 150); // Generous size for 20:9

            var playBtnImg = playBtnGo.AddComponent<Image>();
            playBtnImg.sprite = playButtonSprite;
            playBtnImg.type = Image.Type.Sliced;
            playBtnImg.raycastTarget = true; // Primary hit target
            var playBtn = playBtnGo.AddComponent<Button>();
            playBtn.targetGraphic = playBtnImg;
            playBtnGo.AddComponent<RectMask2D>(); // Clip child shine bar!

            // Metallic Shine Bar Effect
            var shineGo = CreateUIObject("ShineBar", playBtnGo.transform);
            var shineRt = shineGo.GetComponent<RectTransform>();
            shineRt.anchoredPosition = new Vector2(-360, 0);
            shineRt.sizeDelta = new Vector2(90, 280);
            shineRt.localEulerAngles = new Vector3(0, 0, -25);
            var shineImg = shineGo.AddComponent<Image>();
            shineImg.color = new Color(1f, 1f, 1f, 0.35f);
            shineImg.raycastTarget = false; // Don't block clicks to button!

            var juicyPlay = playBtnGo.AddComponent<JuicyButton>();
            var juicySo = new SerializedObject(juicyPlay);
            juicySo.FindProperty("shineBar").objectReferenceValue = shineRt;
            juicySo.FindProperty("idlePulse").boolValue = false; // MainMenuUIEffects will manage the idle pulse cleanly
            juicySo.FindProperty("useClickPunch").boolValue = false; // MainMenuUIEffects will manage click punch
            juicySo.ApplyModifiedProperties();

            var playTextGo = CreateUIObject("Text", playBtnGo.transform);
            SetStretch(playTextGo);
            var playText = playTextGo.AddComponent<TextMeshProUGUI>();
            playText.font = fontAsset;
            playText.text = "LEVEL 6";
            playText.fontSize = 56;
            playText.fontStyle = FontStyles.Bold;
            playText.color = Color.white;
            playText.alignment = TextAlignmentOptions.Center;
            playText.raycastTarget = false; // CRITICAL: Never let label block clicks!

            // 9c. Checklist Button (Right) - Hexagon Button
            var checklistGo = CreateUIObject("ChecklistButton", bottomActionGo.transform);
            var checklistRt = checklistGo.GetComponent<RectTransform>();
            checklistRt.anchoredPosition = new Vector2(290, 0);
            checklistRt.sizeDelta = new Vector2(142, 142);
            var checklistHexaImg = checklistGo.AddComponent<Image>();
            checklistHexaImg.sprite = hexaSprite != null ? hexaSprite : square20;
            checklistHexaImg.preserveAspect = true;
            checklistHexaImg.raycastTarget = true; // Primary hit target
            var checklistBtn = checklistGo.AddComponent<Button>();
            checklistBtn.targetGraphic = checklistHexaImg;
            checklistGo.AddComponent<JuicyButton>();

            var checklistIconGo = CreateUIObject("Icon", checklistGo.transform);
            var checklistIconRt = checklistIconGo.GetComponent<RectTransform>();
            checklistIconRt.anchoredPosition = Vector2.zero;
            checklistIconRt.sizeDelta = new Vector2(94, 94);
            var checklistIconImg = checklistIconGo.AddComponent<Image>();
            checklistIconImg.sprite = clipboardSprite;
            checklistIconImg.preserveAspect = true;
            checklistIconImg.raycastTarget = false; // Don't block clicks to button!

            // Notification Dot on Checklist
            var checklistNotif = CreateUIObject("NotifDot", checklistGo.transform);
            var checklistNotifRt = checklistNotif.GetComponent<RectTransform>();
            checklistNotifRt.anchorMin = new Vector2(0, 1);
            checklistNotifRt.anchorMax = new Vector2(0, 1);
            checklistNotifRt.pivot = new Vector2(0.5f, 0.5f);
            checklistNotifRt.anchoredPosition = new Vector2(15, -15);
            checklistNotifRt.sizeDelta = new Vector2(44, 44);
            var notifImg2 = checklistNotif.AddComponent<Image>();
            notifImg2.sprite = alertSprite != null ? alertSprite : circle;
            notifImg2.preserveAspect = true;
            notifImg2.raycastTarget = false; // Don't block clicks to button!
            notifDotList.Add(checklistNotifRt);

            // -------------------------------------------------------------
            // 10. Bottom Navigation Bar (Height 185 for 20:9 Safe Area)
            // -------------------------------------------------------------
            var navBarGo = CreateUIObject("NavigationBar", canvasGo.transform);
            var navBarRt = navBarGo.GetComponent<RectTransform>();
            navBarRt.anchorMin = new Vector2(0, 0);
            navBarRt.anchorMax = new Vector2(1, 0);
            navBarRt.pivot = new Vector2(0.5f, 0);
            navBarRt.anchoredPosition = Vector2.zero;
            navBarRt.sizeDelta = new Vector2(0, 185); // 185px includes bottom gesture bar safe margin

            var navBarBg = navBarGo.AddComponent<Image>();
            navBarBg.color = new Color(0.13f, 0.10f, 0.29f, 1f);
            navBarBg.raycastTarget = false; // Background shouldn't swallow clicks from tabs

            var navDividerGo = CreateUIObject("TopDivider", navBarGo.transform);
            var navDivRt = navDividerGo.GetComponent<RectTransform>();
            navDivRt.anchorMin = new Vector2(0, 1);
            navDivRt.anchorMax = new Vector2(1, 1);
            navDivRt.pivot = new Vector2(0.5f, 1);
            navDivRt.anchoredPosition = Vector2.zero;
            navDivRt.sizeDelta = new Vector2(0, 3);
            var navDivImg = navDividerGo.AddComponent<Image>();
            navDivImg.color = new Color(0.28f, 0.22f, 0.58f, 1f);
            navDivImg.raycastTarget = false;

            var navButtons = new List<Button>();
            var navRoots = new List<RectTransform>();

            string[] tabNames = new string[] { "Map", "Shop", "Home", "Leaderboard", "Cards" };
            string[] tabLabels = new string[] { "MAP", "SHOP", "HOME", "RANKS", "CARDS" };
            Sprite[] tabSprites = new Sprite[] { navMapSprite, navShopSprite, navHomeSprite, navLeaderSprite, navCardSprite };
            float[] tabXPositions = new float[] { -432, -216, 0, 216, 432 };

            for (int i = 0; i < tabNames.Length; i++)
            {
                var tabGo = CreateUIObject($"Tab_{tabNames[i]}", navBarGo.transform);
                var tabRt = tabGo.GetComponent<RectTransform>();
                tabRt.anchorMin = new Vector2(0.5f, 0.5f);
                tabRt.anchorMax = new Vector2(0.5f, 0.5f);
                tabRt.pivot = new Vector2(0.5f, 0.5f);
                tabRt.anchoredPosition = new Vector2(tabXPositions[i], 0);
                tabRt.sizeDelta = new Vector2(216, 185); // Full column hit area!

                // Full column hit target graphic
                var tabHitImg = tabGo.AddComponent<Image>();
                tabHitImg.color = Color.clear;
                tabHitImg.raycastTarget = true;

                var tabBtn = tabGo.AddComponent<Button>();
                tabBtn.targetGraphic = tabHitImg;
                tabGo.AddComponent<JuicyButton>();
                navButtons.Add(tabBtn);
                navRoots.Add(tabRt);

                // Active Plate
                var activePlateGo = CreateUIObject("ActivePlate", tabGo.transform);
                var activePlateRt = activePlateGo.GetComponent<RectTransform>();
                activePlateRt.anchorMin = new Vector2(0.5f, 0.5f);
                activePlateRt.anchorMax = new Vector2(0.5f, 0.5f);
                activePlateRt.pivot = new Vector2(0.5f, 0.5f);
                activePlateRt.anchoredPosition = new Vector2(0, 14);
                activePlateRt.sizeDelta = new Vector2(150, 136);
                var activePlateImg = activePlateGo.AddComponent<Image>();
                activePlateImg.sprite = square20;
                activePlateImg.color = new Color(0.20f, 0.38f, 0.82f, 1f);
                activePlateImg.raycastTarget = false;
                activePlateGo.SetActive(i == 2);

                // Icon
                var iconGo = CreateUIObject("Icon", tabGo.transform);
                var iconRt = iconGo.GetComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0.5f, 0.5f);
                iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                iconRt.pivot = new Vector2(0.5f, 0.5f);
                iconRt.anchoredPosition = new Vector2(0, i == 2 ? 22 : 6);
                iconRt.sizeDelta = new Vector2(96, 84);
                var iconImg = iconGo.AddComponent<Image>();
                iconImg.sprite = tabSprites[i];
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
                iconImg.color = (i == 2) ? Color.white : new Color(0.42f, 0.48f, 0.68f, 1f);

                // Label for all tabs (active only for selected)
                var labelGo = CreateUIObject("Label", tabGo.transform);
                var labelRt = labelGo.GetComponent<RectTransform>();
                labelRt.anchorMin = new Vector2(0.5f, 0);
                labelRt.anchorMax = new Vector2(0.5f, 0);
                labelRt.pivot = new Vector2(0.5f, 0);
                labelRt.anchoredPosition = new Vector2(0, 14);
                labelRt.sizeDelta = new Vector2(180, 30);
                var label = labelGo.AddComponent<TextMeshProUGUI>();
                label.font = fontAsset;
                label.text = tabLabels[i];
                label.fontSize = 22;
                label.fontStyle = FontStyles.Bold;
                label.color = Color.white;
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
                labelGo.SetActive(i == 2);
            }

            // -------------------------------------------------------------
            // 11. Wire Up MainMenuUIEffects Properties
            // -------------------------------------------------------------
            var so = new SerializedObject(effects);
            so.FindProperty("islandTransform").objectReferenceValue = islandRt;
            so.FindProperty("progressBadgeTransform").objectReferenceValue = badgeRt;

            var seagullsProp = so.FindProperty("seagulls");
            seagullsProp.arraySize = seagullList.Count;
            for (int i = 0; i < seagullList.Count; i++)
                seagullsProp.GetArrayElementAtIndex(i).objectReferenceValue = seagullList[i];

            so.FindProperty("playButtonTransform").objectReferenceValue = playBtnRt;
            so.FindProperty("playButton").objectReferenceValue = playBtn;
            so.FindProperty("playButtonShine").objectReferenceValue = shineRt;

            so.FindProperty("avatarButton").objectReferenceValue = avatarBtn;
            so.FindProperty("settingsButton").objectReferenceValue = settingsBtn;

            var sideButtonsProp = so.FindProperty("sideButtons");
            sideButtonsProp.arraySize = sideButtonsList.Count;
            for (int i = 0; i < sideButtonsList.Count; i++)
                sideButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = sideButtonsList[i];

            so.FindProperty("colorWheelTransform").objectReferenceValue = wheelTransform;
            so.FindProperty("colorWheelNeedle").objectReferenceValue = needleTransform;
            so.FindProperty("piggyTransform").objectReferenceValue = piggyIconRt;
            so.FindProperty("airplaneTransform").objectReferenceValue = airplaneIconRt;

            var locksProp = so.FindProperty("lockBadges");
            locksProp.arraySize = lockBadgesList.Count;
            for (int i = 0; i < lockBadgesList.Count; i++)
                locksProp.GetArrayElementAtIndex(i).objectReferenceValue = lockBadgesList[i];

            var notifDotsProp = so.FindProperty("notificationDots");
            notifDotsProp.arraySize = notifDotList.Count;
            for (int i = 0; i < notifDotList.Count; i++)
                notifDotsProp.GetArrayElementAtIndex(i).objectReferenceValue = notifDotList[i];

            so.FindProperty("mailboxButton").objectReferenceValue = mailboxBtn;
            so.FindProperty("mailboxIcon").objectReferenceValue = mailboxIconRt;
            so.FindProperty("checklistButton").objectReferenceValue = checklistBtn;
            so.FindProperty("checklistIcon").objectReferenceValue = checklistIconRt;

            var navTabsProp = so.FindProperty("navTabButtons");
            navTabsProp.arraySize = navButtons.Count;
            for (int i = 0; i < navButtons.Count; i++)
                navTabsProp.GetArrayElementAtIndex(i).objectReferenceValue = navButtons[i];

            var navRootsProp = so.FindProperty("navTabRoots");
            navRootsProp.arraySize = navRoots.Count;
            for (int i = 0; i < navRoots.Count; i++)
                navRootsProp.GetArrayElementAtIndex(i).objectReferenceValue = navRoots[i];

            var starsProp = so.FindProperty("backgroundStars");
            starsProp.arraySize = starList.Count;
            for (int i = 0; i < starList.Count; i++)
                starsProp.GetArrayElementAtIndex(i).objectReferenceValue = starList[i];

            so.ApplyModifiedProperties();

            // -------------------------------------------------------------
            // 12. Save Scene & Add to Build Settings
            // -------------------------------------------------------------
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"<color=green>[MainMenuSceneBuilder] Successfully rebuilt UI scene for 20:9 at: {ScenePath}</color>");

            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject CreateSideBadgeButton(
            string name, Transform parent, Vector2 pos,
            Sprite iconSprite, string labelText, Sprite lockSprite,
            bool hasNotifDot, TMP_FontAsset font, Sprite square50, Sprite circle, Sprite circleOutline,
            bool isColorWheel = false, bool isPiggy = false, Sprite pinSprite = null, Sprite triangleSprite = null)
        {
            var root = CreateUIObject(name, parent);
            var rt = root.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(135, 135);

            // Add full-bounds invisible hit target graphic so clicks anywhere on the button work 100% reliably
            var hitImg = root.AddComponent<Image>();
            hitImg.color = Color.clear;
            hitImg.raycastTarget = true;

            var btn = root.AddComponent<Button>();
            btn.targetGraphic = hitImg;
            root.AddComponent<JuicyButton>();

            // Circular Base Background
            var circleBase = CreateUIObject("CircleBase", root.transform);
            var circleBaseRt = circleBase.GetComponent<RectTransform>();
            circleBaseRt.anchoredPosition = Vector2.zero;
            circleBaseRt.sizeDelta = new Vector2(122, 122);
            var circleBaseImg = circleBase.AddComponent<Image>();
            circleBaseImg.sprite = circle;
            circleBaseImg.color = new Color(0.18f, 0.12f, 0.35f, 0.95f);
            circleBaseImg.raycastTarget = false; // Don't block raycasts

            // Circular Outline
            if (circleOutline != null)
            {
                var circleOutGo = CreateUIObject("CircleOutline", root.transform);
                var circleOutRt = circleOutGo.GetComponent<RectTransform>();
                circleOutRt.anchoredPosition = Vector2.zero;
                circleOutRt.sizeDelta = new Vector2(122, 122);
                var circleOutImg = circleOutGo.AddComponent<Image>();
                circleOutImg.sprite = circleOutline;
                circleOutImg.color = new Color(0.35f, 0.28f, 0.60f, 0.85f);
                circleOutImg.raycastTarget = false;
            }

            // Main Icon
            var iconGo = CreateUIObject("Icon", root.transform);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(96, 96);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Needle for color wheel
            if (isColorWheel)
            {
                var needleGo = CreateUIObject("Needle", root.transform);
                var needleRt = needleGo.GetComponent<RectTransform>();
                needleRt.anchoredPosition = new Vector2(0, 54);
                needleRt.sizeDelta = new Vector2(34, 40);
                var needleImg = needleGo.AddComponent<Image>();
                needleImg.sprite = pinSprite != null ? pinSprite : (triangleSprite != null ? triangleSprite : circle);
                if (pinSprite == null && triangleSprite != null)
                {
                    needleRt.localEulerAngles = new Vector3(0, 0, 180);
                    needleImg.color = new Color(0.35f, 0.75f, 1f, 1f);
                }
                needleImg.preserveAspect = true;
                needleImg.raycastTarget = false;
            }

            // Padlock Badge (if locked)
            if (lockSprite != null)
            {
                var lockGo = CreateUIObject("LockBadge", root.transform);
                var lockRt = lockGo.GetComponent<RectTransform>();
                lockRt.anchorMin = new Vector2(1, 0.5f);
                lockRt.anchorMax = new Vector2(1, 0.5f);
                lockRt.pivot = new Vector2(0.5f, 0.5f);
                lockRt.anchoredPosition = new Vector2(-12, 12);
                lockRt.sizeDelta = new Vector2(40, 40);
                var lockImg = lockGo.AddComponent<Image>();
                lockImg.sprite = lockSprite;
                lockImg.preserveAspect = true;
                lockImg.raycastTarget = false;
            }

            // Red Notification Dot
            if (hasNotifDot)
            {
                var notifGo = CreateUIObject("NotifDot", root.transform);
                var notifRt = notifGo.GetComponent<RectTransform>();
                notifRt.anchorMin = new Vector2(1, 1);
                notifRt.anchorMax = new Vector2(1, 1);
                notifRt.pivot = new Vector2(0.5f, 0.5f);
                notifRt.anchoredPosition = new Vector2(-12, -12);
                notifRt.sizeDelta = new Vector2(40, 40);
                var notifImg = notifGo.AddComponent<Image>();
                notifImg.sprite = circle;
                notifImg.color = new Color(0.96f, 0.22f, 0.32f, 1f);
                notifImg.raycastTarget = false;

                var exclamGo = CreateUIObject("Exclamation", notifGo.transform);
                SetStretch(exclamGo);
                var exclamText = exclamGo.AddComponent<TextMeshProUGUI>();
                exclamText.font = font;
                exclamText.text = "!";
                exclamText.fontSize = 24;
                exclamText.fontStyle = FontStyles.Bold;
                exclamText.color = Color.white;
                exclamText.alignment = TextAlignmentOptions.Center;
                exclamText.raycastTarget = false;
            }

            // Bottom Label Pill
            var pillGo = CreateUIObject("LabelPill", root.transform);
            var pillRt = pillGo.GetComponent<RectTransform>();
            pillRt.anchoredPosition = new Vector2(0, -58);
            pillRt.sizeDelta = new Vector2(isColorWheel ? 96 : (isPiggy ? 124 : 106), 38);
            var pillImg = pillGo.AddComponent<Image>();
            pillImg.sprite = square50;

            if (isColorWheel)
                pillImg.color = new Color(0.08f, 0.75f, 0.34f, 1f); // Vibrant green
            else if (isPiggy)
                pillImg.color = new Color(0.08f, 0.45f, 0.25f, 0.95f); // Dark green progress pill
            else
                pillImg.color = new Color(0.12f, 0.07f, 0.22f, 0.95f); // Dark capsule

            pillImg.raycastTarget = false;

            var pillTextGo = CreateUIObject("Text", pillGo.transform);
            SetStretch(pillTextGo);
            var pillText = pillTextGo.AddComponent<TextMeshProUGUI>();
            pillText.font = font;
            pillText.text = labelText;
            pillText.fontSize = labelText.Length > 6 ? 16 : 22;
            pillText.fontStyle = FontStyles.Bold;
            pillText.color = Color.white;
            pillText.alignment = TextAlignmentOptions.Center;
            pillText.raycastTarget = false;

            return root;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetStretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                string[] guids = AssetDatabase.FindAssets(fileName);
                foreach (var g in guids)
                {
                    string p = AssetDatabase.GUIDToAssetPath(g);
                    asset = AssetDatabase.LoadAssetAtPath<T>(p);
                    if (asset != null) break;
                }
            }
            return asset;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var currentScenes = EditorBuildSettings.scenes;
            foreach (var s in currentScenes)
            {
                if (s.path == scenePath) return;
            }

            var newScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];
            currentScenes.CopyTo(newScenes, 0);
            newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
#endif
