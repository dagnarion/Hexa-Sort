#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using HexaSort.UI;

namespace HexaSort.Editor
{
    public static class ShopSceneBuilder
    {
        private static TMP_FontAsset s_Font;
        private static Material s_OutlineMaterial;

        [MenuItem("HexaSort/Build Shop UI")]
        public static void BuildShopUI()
        {
            // 1. Ensure single EventSystem
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }

            // 2. Load font & outline material
            s_Font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Asset/Art/Fonts/Coolvetica SDF.asset");
            if (s_Font == null)
            {
                s_Font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
            s_OutlineMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Asset/Art/ShopGenerated/Coolvetica SDF - BlackOutline.mat");

            // 3. Remove existing ShopCanvas if any
            var existing = GameObject.Find("ShopCanvas");
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            // 4. Create ShopCanvas root
            var canvasGO = new GameObject("ShopCanvas");
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create ShopCanvas");

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2400); // 20:9 reference ratio
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f; // Width match for portrait mobile

            canvasGO.AddComponent<GraphicRaycaster>();
            var shopController = canvasGO.AddComponent<ShopUIController>();

            // Fullscreen Background with subtle dark purple vignette
            var bgGO = CreateUIObject("Background", canvasGO.transform);
            SetStretchFull(bgGO.GetComponent<RectTransform>());
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.086f, 0.055f, 0.165f, 0.94f); // Rich dark purple/navy (#160E2A)
            bgImg.raycastTarget = false;

            // 5. Scroll View for product list
            var scrollViewGO = CreateUIObject("ProductScrollView", canvasGO.transform);
            var scrollRectTransform = scrollViewGO.GetComponent<RectTransform>();
            SetStretchFull(scrollRectTransform);
            // Offset from bottom navigation bar (185px) and top header awning (210px)
            scrollRectTransform.offsetMin = new Vector2(0, 185);
            scrollRectTransform.offsetMax = new Vector2(0, -210);

            var scrollRect = scrollViewGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 35f;

            // Viewport
            var viewportGO = CreateUIObject("Viewport", scrollViewGO.transform);
            var viewportRect = viewportGO.GetComponent<RectTransform>();
            SetStretchFull(viewportRect);
            viewportGO.AddComponent<RectMask2D>();
            var vpImg = viewportGO.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true; // Drag & scroll anywhere

            scrollRect.viewport = viewportRect;

            // Content
            var contentGO = CreateUIObject("Content", viewportGO.transform);
            var contentRect = contentGO.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(32, 32, 28, 48);
            vlg.spacing = 30;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;

            // 6. Build Products into Content
            // 6.1 Special Offer (Two-Tone Purple Card with Gold Border)
            BuildPurpleCard(contentGO.transform,
                id: "special_offer",
                title: "Special Offer",
                price: "VND 26000",
                badgeType: BadgeType.Extra800,
                badgeText: "800%\nEXTRA",
                mainIconPath: "Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack4.png",
                mainIconCount: "300",
                boosters: new (string, string)[] {
                    ("Assets/Asset/Art/New Folder/Texture2D/UseBooster_Hammer.png", "x2"),
                    ("Assets/Asset/Art/New Folder/Texture2D/Booster-Hand-no-border.png", "x2"),
                    ("Assets/Asset/Art/ShopGenerated/Shop_Booster_Swap.png", "x2")
                });

            // 6.2 No Ads Bundle (Two-Tone Purple Card with Gold Border)
            BuildNoAdsCard(contentGO.transform,
                id: "no_ads_bundle",
                title: "No Ads Bundle",
                price: "VND 210000",
                boosters: new (string, string)[] {
                    ("Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack1.png", "x200"),
                    ("Assets/Asset/Art/New Folder/Texture2D/UseBooster_Hammer.png", "x2"),
                    ("Assets/Asset/Art/New Folder/Texture2D/Booster-Hand-no-border.png", "x2"),
                    ("Assets/Asset/Art/ShopGenerated/Shop_Booster_Swap.png", "x2")
                });

            // 6.3 Tiny Hex Pack (Two-Tone Blue Card)
            BuildBlueCard(contentGO.transform,
                id: "tiny_hex_pack",
                title: "Tiny Hex Pack",
                price: "VND 254000",
                badgeType: BadgeType.None,
                badgeText: null,
                mainIconPath: "Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack1.png",
                mainIconCount: "500",
                boosters: new (string, string)[] {
                    ("Assets/Asset/Art/New Folder/Texture2D/UseBooster_Hammer.png", "x2"),
                    ("Assets/Asset/Art/New Folder/Texture2D/Booster-Hand-no-border.png", "x2"),
                    ("Assets/Asset/Art/ShopGenerated/Shop_Booster_Swap.png", "x2"),
                    ("Assets/Asset/Art/ShopGenerated/InfiniteLives_Icon.png", "30M")
                });

            // 6.4 Mini Hex Pack (Two-Tone Blue Card) - POPULAR
            BuildBlueCard(contentGO.transform,
                id: "mini_hex_pack",
                title: "Mini Hex Pack",
                price: "VND 509000",
                badgeType: BadgeType.PinkRibbon,
                badgeText: "POPULAR",
                mainIconPath: "Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack2.png",
                mainIconCount: "2000",
                boosters: new (string, string)[] {
                    ("Assets/Asset/Art/New Folder/Texture2D/UseBooster_Hammer.png", "x2"),
                    ("Assets/Asset/Art/New Folder/Texture2D/Booster-Hand-no-border.png", "x2"),
                    ("Assets/Asset/Art/ShopGenerated/Shop_Booster_Swap.png", "x2"),
                    ("Assets/Asset/Art/ShopGenerated/InfiniteLives_Icon.png", "1H")
                });

            // 6.5 Hex Vault Pack (Two-Tone Blue Card)
            BuildBlueCard(contentGO.transform,
                id: "hex_vault_pack",
                title: "Hex Vault Pack",
                price: "VND 1000000",
                badgeType: BadgeType.None,
                badgeText: null,
                mainIconPath: "Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack3.png",
                mainIconCount: "5000",
                boosters: new (string, string)[] {
                    ("Assets/Asset/Art/New Folder/Texture2D/UseBooster_Hammer.png", "x4"),
                    ("Assets/Asset/Art/New Folder/Texture2D/Booster-Hand-no-border.png", "x4"),
                    ("Assets/Asset/Art/ShopGenerated/Shop_Booster_Swap.png", "x4"),
                    ("Assets/Asset/Art/ShopGenerated/InfiniteLives_Icon.png", "2H")
                });

            // 6.6 Grand Hex Pack (Two-Tone Blue Card)
            BuildBlueCard(contentGO.transform,
                id: "grand_hex_pack",
                title: "Grand Hex Pack",
                price: "VND 2050000",
                badgeType: BadgeType.None,
                badgeText: null,
                mainIconPath: "Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack4.png",
                mainIconCount: "12000",
                boosters: new (string, string)[] {
                    ("Assets/Asset/Art/New Folder/Texture2D/UseBooster_Hammer.png", "x10"),
                    ("Assets/Asset/Art/New Folder/Texture2D/Booster-Hand-no-border.png", "x10"),
                    ("Assets/Asset/Art/ShopGenerated/Shop_Booster_Swap.png", "x10"),
                    ("Assets/Asset/Art/ShopGenerated/InfiniteLives_Icon.png", "4H")
                });

            // 6.7 Mega Hex Pack (Two-Tone Blue Card) - BEST VALUE
            BuildBlueCard(contentGO.transform,
                id: "mega_hex_pack",
                title: "Mega Hex Pack",
                price: "VND 2550000",
                badgeType: BadgeType.PinkRibbon,
                badgeText: "BEST\nVALUE",
                mainIconPath: "Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack5.png",
                mainIconCount: "20000",
                boosters: new (string, string)[] {
                    ("Assets/Asset/Art/New Folder/Texture2D/UseBooster_Hammer.png", "x15"),
                    ("Assets/Asset/Art/New Folder/Texture2D/Booster-Hand-no-border.png", "x15"),
                    ("Assets/Asset/Art/ShopGenerated/Shop_Booster_Swap.png", "x15"),
                    ("Assets/Asset/Art/ShopGenerated/InfiniteLives_Icon.png", "6H")
                });

            // 6.8 Coin Packs Grid (6 small packs)
            var coinGridSection = BuildCoinGridSection(contentGO.transform);

            // 6.9 HEXA BOOSTS Promo Code Card
            BuildHexaBoostsCard(contentGO.transform);

            // 7. Top Header (6-Stripe Scalloped Awning & Coin Counter Pill)
            var headerGO = CreateUIObject("TopHeader", canvasGO.transform);
            var headerRect = headerGO.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0, 280);

            // 6-Stripe Scalloped Awning
            var awningGO = CreateUIObject("Awning", headerGO.transform);
            var awningRect = awningGO.GetComponent<RectTransform>();
            awningRect.anchorMin = new Vector2(0, 1);
            awningRect.anchorMax = new Vector2(1, 1);
            awningRect.pivot = new Vector2(0.5f, 1);
            awningRect.anchoredPosition = Vector2.zero;
            awningRect.sizeDelta = new Vector2(0, 280);
            var awningImg = awningGO.AddComponent<Image>();
            awningImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/Shop_Awning_6Stripes.png");
            awningImg.raycastTarget = false;

            // Coin Balance Pill (Top Left, under phone status bar)
            var coinPillGO = CreateUIObject("CoinPill", headerGO.transform);
            var coinPillRect = coinPillGO.GetComponent<RectTransform>();
            coinPillRect.anchorMin = new Vector2(0, 1);
            coinPillRect.anchorMax = new Vector2(0, 1);
            coinPillRect.pivot = new Vector2(0, 1);
            coinPillRect.anchoredPosition = new Vector2(36, -46);
            coinPillRect.sizeDelta = new Vector2(240, 78);

            var pillBg = coinPillGO.AddComponent<Image>();
            pillBg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/Shop_CoinPill_Bg.png");
            pillBg.type = Image.Type.Sliced;
            var pillBtn = coinPillGO.AddComponent<Button>();
            coinPillGO.AddComponent<JuicyButton>();

            // Official Gold Hexagon Coin Icon on Left (Hud-Coin.png from Art assets)
            var coinIconGO = CreateUIObject("CoinIcon", coinPillGO.transform);
            var coinIconRect = coinIconGO.GetComponent<RectTransform>();
            coinIconRect.anchorMin = new Vector2(0, 0.5f);
            coinIconRect.anchorMax = new Vector2(0, 0.5f);
            coinIconRect.pivot = new Vector2(0.5f, 0.5f);
            coinIconRect.anchoredPosition = new Vector2(22, 0);
            coinIconRect.sizeDelta = new Vector2(92, 92);
            var coinIconImg = coinIconGO.AddComponent<Image>();
            coinIconImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/Hud-Coin.png");
            coinIconImg.preserveAspect = true;
            coinIconImg.raycastTarget = false;

            // Balance Text "219" with crisp black outline
            var balanceTmp = CreateTMPText("BalanceText", coinPillGO.transform, "219", 48, Color.white, TextAlignmentOptions.MidlineLeft);
            var balanceTextRect = balanceTmp.rectTransform;
            balanceTextRect.anchorMin = new Vector2(0, 0);
            balanceTextRect.anchorMax = new Vector2(1, 1);
            balanceTextRect.offsetMin = new Vector2(78, 0);
            balanceTextRect.offsetMax = new Vector2(-16, 0);

            // 8. Bottom Navigation Bar
            var navTabsList = new List<ShopNavTab>();
            var navBarGO = BuildBottomNavigationBar(canvasGO.transform, navTabsList);

            // 9. Purchase Cancelled Popup
            var popup = BuildPurchaseCancelledPopup(canvasGO.transform);

            // 10. Link controller references
            var so = new SerializedObject(shopController);
            so.FindProperty("shopCanvas").objectReferenceValue = canvas;
            so.FindProperty("scrollRect").objectReferenceValue = scrollRect;
            so.FindProperty("contentRect").objectReferenceValue = contentRect;
            so.FindProperty("coinGridSection").objectReferenceValue = coinGridSection;
            so.FindProperty("coinBalanceText").objectReferenceValue = balanceTmp;
            so.FindProperty("coinPlusButton").objectReferenceValue = pillBtn; // Clicking coin pill scrolls to coins!
            so.FindProperty("coinPillTransform").objectReferenceValue = coinPillRect;
            so.FindProperty("purchasePopup").objectReferenceValue = popup;

            var navTabsProp = so.FindProperty("navTabs");
            navTabsProp.arraySize = navTabsList.Count;
            for (int i = 0; i < navTabsList.Count; i++)
            {
                navTabsProp.GetArrayElementAtIndex(i).objectReferenceValue = navTabsList[i];
            }
            so.FindProperty("defaultSelectedTabIndex").intValue = 1; // Shop Tab selected
            so.ApplyModifiedProperties();

            // 11. Save as Prefab
            string prefabPath = "Assets/Asset/Prefabs/UI/ShopCanvas.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGO, prefabPath, InteractionMode.UserAction);

            // 12. Mark scene dirty & save
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=green><b>[ShopSceneBuilder]</b> Successfully rebuilt ShopCanvas with refined pixel-perfect assets!</color>");
        }

        private enum BadgeType { None, PinkRibbon, Extra800 }

        // =========================================================================
        // PURPLE CARD BUILDER (Special Offer - Two-Tone Design)
        // =========================================================================
        private static GameObject BuildPurpleCard(Transform parent, string id, string title, string price,
            BadgeType badgeType, string badgeText, string mainIconPath, string mainIconCount, (string path, string count)[] boosters)
        {
            var cardGO = CreateUIObject("Card_" + id, parent);
            var cardRect = cardGO.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(1016, 520);

            var le = cardGO.AddComponent<LayoutElement>();
            le.minHeight = 520;
            le.preferredHeight = 520;
            le.flexibleHeight = 0;

            // Outer Base: Deep Dark Purple Container with Gold Glowing Rim
            var cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/ShopCard_Purple_Base.png");
            cardImg.type = Image.Type.Sliced;
            cardImg.raycastTarget = false;

            var productCard = cardGO.AddComponent<ShopProductCard>();

            // Top Inset: Vibrant Lighter Purple / Violet Panel
            var insetGO = CreateUIObject("TopInset", cardGO.transform);
            var insetRect = insetGO.GetComponent<RectTransform>();
            insetRect.anchorMin = new Vector2(0, 0.28f);
            insetRect.anchorMax = new Vector2(1, 1);
            insetRect.offsetMin = new Vector2(14, 0);
            insetRect.offsetMax = new Vector2(-14, -14);

            var insetImg = insetGO.AddComponent<Image>();
            insetImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/ShopCard_Purple_Inset.png");
            insetImg.type = Image.Type.Sliced;
            insetImg.raycastTarget = false;

            // Main Icon & Count on Left (Huge Wallet of Coins)
            var mainIconGO = CreateUIObject("MainIconGroup", insetGO.transform);
            var mainIconGroupRect = mainIconGO.GetComponent<RectTransform>();
            mainIconGroupRect.anchorMin = new Vector2(0, 0);
            mainIconGroupRect.anchorMax = new Vector2(0.35f, 1);
            mainIconGroupRect.offsetMin = Vector2.zero;
            mainIconGroupRect.offsetMax = Vector2.zero;

            var iconImgGO = CreateUIObject("Icon", mainIconGO.transform);
            var iconImgRect = iconImgGO.GetComponent<RectTransform>();
            iconImgRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconImgRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconImgRect.pivot = new Vector2(0.5f, 0.5f);
            iconImgRect.anchoredPosition = new Vector2(0, 24);
            iconImgRect.sizeDelta = new Vector2(260, 200); // Huge wallet!
            var iconImg = iconImgGO.AddComponent<Image>();
            iconImg.sprite = LoadSprite(mainIconPath);
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            var countTmp = CreateTMPText("CountText", mainIconGO.transform, mainIconCount, 60, Color.white);
            var countRect = countTmp.rectTransform;
            countRect.anchorMin = new Vector2(0.5f, 0.5f);
            countRect.anchorMax = new Vector2(0.5f, 0.5f);
            countRect.pivot = new Vector2(0.5f, 0.5f);
            countRect.anchoredPosition = new Vector2(0, -90);
            countRect.sizeDelta = new Vector2(300, 60);

            // Boosters Row on Right (Hammer, Hand, Swap - Large Icons)
            BuildBoostersRow(insetGO.transform, boosters, isThreeItems: true);

            // Badge at Top Right (800% EXTRA flower badge)
            BuildBadge(cardGO.transform, badgeType, badgeText);

            // Bottom Area: Title in Golden Yellow + Green Buy Button on Dark Purple Base
            var (titleTmp, priceTmp, buyBtn) = BuildBottomBar(cardGO.transform, title, price, isGoldTitle: true);

            var pCardSo = new SerializedObject(productCard);
            pCardSo.FindProperty("productId").stringValue = id;
            pCardSo.FindProperty("productName").stringValue = title;
            pCardSo.FindProperty("priceText").stringValue = price;
            pCardSo.FindProperty("buyButton").objectReferenceValue = buyBtn;
            pCardSo.FindProperty("titleLabel").objectReferenceValue = titleTmp;
            pCardSo.FindProperty("priceLabel").objectReferenceValue = priceTmp;
            pCardSo.ApplyModifiedProperties();

            return cardGO;
        }

        // =========================================================================
        // NO ADS CARD BUILDER (Two-Tone Purple Design)
        // =========================================================================
        private static GameObject BuildNoAdsCard(Transform parent, string id, string title, string price, (string path, string count)[] boosters)
        {
            var cardGO = CreateUIObject("Card_" + id, parent);
            var cardRect = cardGO.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(1016, 520);

            var le = cardGO.AddComponent<LayoutElement>();
            le.minHeight = 520;
            le.preferredHeight = 520;
            le.flexibleHeight = 0;

            var cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/ShopCard_Purple_Base.png");
            cardImg.type = Image.Type.Sliced;
            cardImg.raycastTarget = false;

            var productCard = cardGO.AddComponent<ShopProductCard>();

            // Top Inset: Vibrant Lighter Purple / Violet Panel
            var insetGO = CreateUIObject("TopInset", cardGO.transform);
            var insetRect = insetGO.GetComponent<RectTransform>();
            insetRect.anchorMin = new Vector2(0, 0.28f);
            insetRect.anchorMax = new Vector2(1, 1);
            insetRect.offsetMin = new Vector2(14, 0);
            insetRect.offsetMax = new Vector2(-14, -14);

            var insetImg = insetGO.AddComponent<Image>();
            insetImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/ShopCard_Purple_Inset.png");
            insetImg.type = Image.Type.Sliced;
            insetImg.raycastTarget = false;

            // Left: No Ads Icon + 3-line description
            var leftGroupGO = CreateUIObject("LeftGroup", insetGO.transform);
            var leftRect = leftGroupGO.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0, 0);
            leftRect.anchorMax = new Vector2(0.35f, 1);
            leftRect.offsetMin = Vector2.zero;
            leftRect.offsetMax = Vector2.zero;

            var noAdsIconGO = CreateUIObject("NoAdsIcon", leftGroupGO.transform);
            var noAdsRect = noAdsIconGO.GetComponent<RectTransform>();
            noAdsRect.anchorMin = new Vector2(0.5f, 0.5f);
            noAdsRect.anchorMax = new Vector2(0.5f, 0.5f);
            noAdsRect.pivot = new Vector2(0.5f, 0.5f);
            noAdsRect.anchoredPosition = new Vector2(0, 40);
            noAdsRect.sizeDelta = new Vector2(165, 165); // Large icon!
            var noAdsImg = noAdsIconGO.AddComponent<Image>();
            noAdsImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Texture2D/RemoveAds-Icon.png");
            noAdsImg.preserveAspect = true;
            noAdsImg.raycastTarget = false;

            var descTmp = CreateTMPText("DescText", leftGroupGO.transform,
                "Remove <color=#FFE345>forced ADS</color>\ninterruptions. Rewarded\nvideo will still be available",
                22, Color.white);
            descTmp.lineSpacing = -10;
            var descRect = descTmp.rectTransform;
            descRect.anchorMin = new Vector2(0.5f, 0.5f);
            descRect.anchorMax = new Vector2(0.5f, 0.5f);
            descRect.pivot = new Vector2(0.5f, 0.5f);
            descRect.anchoredPosition = new Vector2(0, -80);
            descRect.sizeDelta = new Vector2(330, 80);

            // Right: Boosters (4 items)
            BuildBoostersRow(insetGO.transform, boosters, isThreeItems: false);

            // Bottom Area: Title in Golden Yellow + Green Buy Button on Dark Purple Base
            var (titleTmp, priceTmp, buyBtn) = BuildBottomBar(cardGO.transform, title, price, isGoldTitle: true);

            var pCardSo = new SerializedObject(productCard);
            pCardSo.FindProperty("productId").stringValue = id;
            pCardSo.FindProperty("productName").stringValue = title;
            pCardSo.FindProperty("priceText").stringValue = price;
            pCardSo.FindProperty("buyButton").objectReferenceValue = buyBtn;
            pCardSo.FindProperty("titleLabel").objectReferenceValue = titleTmp;
            pCardSo.FindProperty("priceLabel").objectReferenceValue = priceTmp;
            pCardSo.ApplyModifiedProperties();

            return cardGO;
        }

        // =========================================================================
        // TWO-TONE BLUE CARD BUILDER (Tiny, Mini, Vault, Grand, Mega)
        // =========================================================================
        private static GameObject BuildBlueCard(Transform parent, string id, string title, string price,
            BadgeType badgeType, string badgeText, string mainIconPath, string mainIconCount, (string path, string count)[] boosters)
        {
            var cardGO = CreateUIObject("Card_" + id, parent);
            var cardRect = cardGO.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(1016, 520);

            var le = cardGO.AddComponent<LayoutElement>();
            le.minHeight = 520;
            le.preferredHeight = 520;
            le.flexibleHeight = 0;

            // Outer Base: Deep Royal Blue container
            var baseImg = cardGO.AddComponent<Image>();
            baseImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/ShopCard_Blue_Base.png");
            baseImg.type = Image.Type.Sliced;
            baseImg.raycastTarget = false;

            var productCard = cardGO.AddComponent<ShopProductCard>();

            // Top Inset: Light Sky-Blue Panel
            var insetGO = CreateUIObject("TopInset", cardGO.transform);
            var insetRect = insetGO.GetComponent<RectTransform>();
            insetRect.anchorMin = new Vector2(0, 0.28f);
            insetRect.anchorMax = new Vector2(1, 1);
            insetRect.offsetMin = new Vector2(14, 0);
            insetRect.offsetMax = new Vector2(-14, -14);

            var insetImg = insetGO.AddComponent<Image>();
            insetImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/ShopCard_Blue_Inset.png");
            insetImg.type = Image.Type.Sliced;
            insetImg.raycastTarget = false;

            // Main Icon & Count on Left
            var mainIconGO = CreateUIObject("MainIconGroup", insetGO.transform);
            var mainIconGroupRect = mainIconGO.GetComponent<RectTransform>();
            mainIconGroupRect.anchorMin = new Vector2(0, 0);
            mainIconGroupRect.anchorMax = new Vector2(0.35f, 1);
            mainIconGroupRect.offsetMin = Vector2.zero;
            mainIconGroupRect.offsetMax = Vector2.zero;

            var iconImgGO = CreateUIObject("Icon", mainIconGO.transform);
            var iconImgRect = iconImgGO.GetComponent<RectTransform>();
            iconImgRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconImgRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconImgRect.pivot = new Vector2(0.5f, 0.5f);
            iconImgRect.anchoredPosition = new Vector2(0, 24);
            iconImgRect.sizeDelta = new Vector2(250, 190);
            var iconImg = iconImgGO.AddComponent<Image>();
            iconImg.sprite = LoadSprite(mainIconPath);
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            var countTmp = CreateTMPText("CountText", mainIconGO.transform, mainIconCount, 60, Color.white);
            var countRect = countTmp.rectTransform;
            countRect.anchorMin = new Vector2(0.5f, 0.5f);
            countRect.anchorMax = new Vector2(0.5f, 0.5f);
            countRect.pivot = new Vector2(0.5f, 0.5f);
            countRect.anchoredPosition = new Vector2(0, -90);
            countRect.sizeDelta = new Vector2(300, 60);

            // Boosters Row on Right (4 items)
            BuildBoostersRow(insetGO.transform, boosters, isThreeItems: false);

            // Badge
            BuildBadge(cardGO.transform, badgeType, badgeText);

            // Bottom Area: Title in crisp bold white + Green buy button on Dark Blue Base
            var (titleTmp, priceTmp, buyBtn) = BuildBottomBar(cardGO.transform, title, price, isGoldTitle: false);

            var pCardSo = new SerializedObject(productCard);
            pCardSo.FindProperty("productId").stringValue = id;
            pCardSo.FindProperty("productName").stringValue = title;
            pCardSo.FindProperty("priceText").stringValue = price;
            pCardSo.FindProperty("buyButton").objectReferenceValue = buyBtn;
            pCardSo.FindProperty("titleLabel").objectReferenceValue = titleTmp;
            pCardSo.FindProperty("priceLabel").objectReferenceValue = priceTmp;
            pCardSo.ApplyModifiedProperties();

            return cardGO;
        }

        // =========================================================================
        // COIN GRID SECTION
        // =========================================================================
        private static RectTransform BuildCoinGridSection(Transform parent)
        {
            var sectionGO = CreateUIObject("CoinPacksSection", parent);
            var sectionRect = sectionGO.GetComponent<RectTransform>();
            sectionRect.sizeDelta = new Vector2(1016, 780);

            var le = sectionGO.AddComponent<LayoutElement>();
            le.minHeight = 780;
            le.preferredHeight = 780;
            le.flexibleHeight = 0;

            var glg = sectionGO.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(320, 365);
            glg.spacing = new Vector2(28, 28);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            var packs = new (string id, string coins, string extra, string price, string icon)[] {
                ("coin_150", "150", null, "VND 48000", "Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack1.png"),
                ("coin_700", "700", "15% Extra", "VND 195000", "Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack1.png"),
                ("coin_1600", "1600", "40% Extra", "VND 365000", "Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack2.png"),
                ("coin_4000", "4000", "75% Extra", "VND 731000", "Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack3.png"),
                ("coin_7000", "7000", "85% Extra", "VND 1200000", "Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack4.png"),
                ("coin_15000", "15000", "100% Extra", "VND 2450000", "Assets/Asset/Art/New Folder/Texture2D/Shop-CoinPack5.png"),
            };

            foreach (var p in packs)
            {
                var cardGO = CreateUIObject("Card_" + p.id, sectionGO.transform);
                var cardRect = cardGO.GetComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(320, 365);

                var cardImg = cardGO.AddComponent<Image>();
                cardImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/ShopCard_Blue_Base.png");
                cardImg.type = Image.Type.Sliced;
                cardImg.raycastTarget = false;

                var productCard = cardGO.AddComponent<ShopProductCard>();

                // Inset
                var miniInsetGO = CreateUIObject("Inset", cardGO.transform);
                var miniInsetRect = miniInsetGO.GetComponent<RectTransform>();
                miniInsetRect.anchorMin = new Vector2(0, 0.28f);
                miniInsetRect.anchorMax = new Vector2(1, 1);
                miniInsetRect.offsetMin = new Vector2(10, 0);
                miniInsetRect.offsetMax = new Vector2(-10, -10);

                var miniInsetImg = miniInsetGO.AddComponent<Image>();
                miniInsetImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/ShopCard_Blue_Inset.png");
                miniInsetImg.type = Image.Type.Sliced;
                miniInsetImg.raycastTarget = false;

                // Coin Stack Icon
                var iconGO = CreateUIObject("Icon", miniInsetGO.transform);
                var iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.68f);
                iconRect.anchorMax = new Vector2(0.5f, 0.68f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = new Vector2(150, 115);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.sprite = LoadSprite(p.icon);
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;

                // Amount Text with black outline
                var amountTmp = CreateTMPText("AmountText", miniInsetGO.transform, p.coins, 52, Color.white);
                var amountRect = amountTmp.rectTransform;
                amountRect.anchorMin = new Vector2(0, 0.24f);
                amountRect.anchorMax = new Vector2(1, 0.44f);
                amountRect.offsetMin = Vector2.zero;
                amountRect.offsetMax = Vector2.zero;

                // Extra Tag
                if (!string.IsNullOrEmpty(p.extra))
                {
                    var extraTmp = CreateTMPText("ExtraText", miniInsetGO.transform, p.extra, 24, new Color(0.24f, 0.88f, 1f, 1f));
                    var extraRect = extraTmp.rectTransform;
                    extraRect.anchorMin = new Vector2(0, 0.05f);
                    extraRect.anchorMax = new Vector2(1, 0.24f);
                    extraRect.offsetMin = Vector2.zero;
                    extraRect.offsetMax = Vector2.zero;
                }

                // Buy Button at bottom
                var buyBtnGO = CreateUIObject("BuyButton", cardGO.transform);
                var buyBtnRect = buyBtnGO.GetComponent<RectTransform>();
                buyBtnRect.anchorMin = new Vector2(0.5f, 0.14f);
                buyBtnRect.anchorMax = new Vector2(0.5f, 0.14f);
                buyBtnRect.pivot = new Vector2(0.5f, 0.5f);
                buyBtnRect.sizeDelta = new Vector2(275, 78);

                var buyBtnImg = buyBtnGO.AddComponent<Image>();
                buyBtnImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/Shop_Green_Button.png");
                buyBtnImg.type = Image.Type.Sliced;
                var buyBtn = buyBtnGO.AddComponent<Button>();
                buyBtnGO.AddComponent<JuicyButton>();

                var priceTmp = CreateTMPText("PriceText", buyBtnGO.transform, p.price, 32, Color.white);
                SetStretchFull(priceTmp.rectTransform);

                var pCardSo = new SerializedObject(productCard);
                pCardSo.FindProperty("productId").stringValue = p.id;
                pCardSo.FindProperty("productName").stringValue = p.coins + " Coins";
                pCardSo.FindProperty("priceText").stringValue = p.price;
                pCardSo.FindProperty("buyButton").objectReferenceValue = buyBtn;
                pCardSo.FindProperty("titleLabel").objectReferenceValue = amountTmp;
                pCardSo.FindProperty("priceLabel").objectReferenceValue = priceTmp;
                pCardSo.ApplyModifiedProperties();
            }

            return sectionRect;
        }

        // =========================================================================
        // HEXA BOOSTS PROMO CARD
        // =========================================================================
        private static GameObject BuildHexaBoostsCard(Transform parent)
        {
            var cardGO = CreateUIObject("Card_HexaBoosts", parent);
            var cardRect = cardGO.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(1016, 460);

            var le = cardGO.AddComponent<LayoutElement>();
            le.minHeight = 460;
            le.preferredHeight = 460;
            le.flexibleHeight = 0;

            var cardImg = cardGO.AddComponent<Image>();
            cardImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/ShopCard_Purple_Base.png");
            cardImg.type = Image.Type.Sliced;
            cardImg.raycastTarget = false;

            var productCard = cardGO.AddComponent<ShopProductCard>();

            // Avatar on Top Center (peeking above card)
            var avatarGO = CreateUIObject("Avatar", cardGO.transform);
            var avatarRect = avatarGO.GetComponent<RectTransform>();
            avatarRect.anchorMin = new Vector2(0.5f, 1f);
            avatarRect.anchorMax = new Vector2(0.5f, 1f);
            avatarRect.pivot = new Vector2(0.5f, 0.45f);
            avatarRect.anchoredPosition = new Vector2(0, 14);
            avatarRect.sizeDelta = new Vector2(145, 145);
            var avatarImg = avatarGO.AddComponent<Image>();
            avatarImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Texture2D/Shop-ContectCreatorIcon.png");
            avatarImg.preserveAspect = true;
            avatarImg.raycastTarget = false;

            // 3D Title Banner
            var titleBannerGO = CreateUIObject("TitleBanner", cardGO.transform);
            var bannerRect = titleBannerGO.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 0.83f);
            bannerRect.anchorMax = new Vector2(0.5f, 0.83f);
            bannerRect.pivot = new Vector2(0.5f, 0.5f);
            bannerRect.anchoredPosition = new Vector2(0, -6);
            bannerRect.sizeDelta = new Vector2(560, 110);
            var bannerImg = titleBannerGO.AddComponent<Image>();
            bannerImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Sprite/Shop-HexaBoosts_0.asset");
            bannerImg.preserveAspect = true;
            bannerImg.raycastTarget = false;

            // Middle Section
            var middleGO = CreateUIObject("MiddleContent", cardGO.transform);
            var middleRect = middleGO.GetComponent<RectTransform>();
            middleRect.anchorMin = new Vector2(0, 0.28f);
            middleRect.anchorMax = new Vector2(1, 0.72f);
            middleRect.offsetMin = new Vector2(30, 0);
            middleRect.offsetMax = new Vector2(-30, 0);

            // Left graphic (Chest with rewards)
            var rewardsGO = CreateUIObject("RewardsGraphic", middleGO.transform);
            var rewardsRect = rewardsGO.GetComponent<RectTransform>();
            rewardsRect.anchorMin = new Vector2(0.04f, 0.5f);
            rewardsRect.anchorMax = new Vector2(0.04f, 0.5f);
            rewardsRect.pivot = new Vector2(0f, 0.5f);
            rewardsRect.sizeDelta = new Vector2(310, 175);
            var rewardsImg = rewardsGO.AddComponent<Image>();
            rewardsImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/Shop-CombinedRewards.png");
            rewardsImg.preserveAspect = true;
            rewardsImg.raycastTarget = false;

            // Right message with black outline
            var msgTmp = CreateTMPText("MessageText", middleGO.transform, "Get a Big Boost by\nentering a Bonus Code!", 32, Color.white, TextAlignmentOptions.MidlineLeft);
            var msgRect = msgTmp.rectTransform;
            msgRect.anchorMin = new Vector2(0.42f, 0);
            msgRect.anchorMax = new Vector2(0.96f, 1);
            msgRect.offsetMin = Vector2.zero;
            msgRect.offsetMax = Vector2.zero;

            // Long Green Button
            var promoBtnGO = CreateUIObject("PromoButton", cardGO.transform);
            var promoBtnRect = promoBtnGO.GetComponent<RectTransform>();
            promoBtnRect.anchorMin = new Vector2(0.5f, 0.14f);
            promoBtnRect.anchorMax = new Vector2(0.5f, 0.14f);
            promoBtnRect.pivot = new Vector2(0.5f, 0.5f);
            promoBtnRect.sizeDelta = new Vector2(640, 95);

            var promoBtnImg = promoBtnGO.AddComponent<Image>();
            promoBtnImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/Shop_Green_Button.png");
            promoBtnImg.type = Image.Type.Sliced;
            var promoBtn = promoBtnGO.AddComponent<Button>();
            promoBtnGO.AddComponent<JuicyButton>();

            var promoTmp = CreateTMPText("PromoText", promoBtnGO.transform, "USE PROMO CODE", 38, Color.white);
            SetStretchFull(promoTmp.rectTransform);

            var pCardSo = new SerializedObject(productCard);
            pCardSo.FindProperty("productId").stringValue = "promo_code";
            pCardSo.FindProperty("productName").stringValue = "Promo Code";
            pCardSo.FindProperty("priceText").stringValue = "FREE";
            pCardSo.FindProperty("buyButton").objectReferenceValue = promoBtn;
            pCardSo.FindProperty("priceLabel").objectReferenceValue = promoTmp;
            pCardSo.ApplyModifiedProperties();

            return cardGO;
        }

        // =========================================================================
        // BOTTOM NAVIGATION BAR
        // =========================================================================
        private static GameObject BuildBottomNavigationBar(Transform parent, List<ShopNavTab> navTabsList)
        {
            var navBarGO = CreateUIObject("BottomNavigationBar", parent);
            var navRect = navBarGO.GetComponent<RectTransform>();
            navRect.anchorMin = new Vector2(0, 0);
            navRect.anchorMax = new Vector2(1, 0);
            navRect.pivot = new Vector2(0.5f, 0);
            navRect.anchoredPosition = Vector2.zero;
            navRect.sizeDelta = new Vector2(0, 185);

            var navBg = navBarGO.AddComponent<Image>();
            navBg.color = new Color(0.09f, 0.075f, 0.19f, 0.98f); // Deep navy purple
            navBg.raycastTarget = false;

            var hlg = navBarGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 0;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            for (int i = 0; i < 5; i++)
            {
                var tabGO = CreateUIObject("Tab_" + i, navBarGO.transform);
                var tabBtn = tabGO.AddComponent<Button>();
                tabGO.AddComponent<JuicyButton>();
                var navTab = tabGO.AddComponent<ShopNavTab>();
                navTabsList.Add(navTab);

                Image iconImg = null;
                RectTransform iconRect = null;
                TextMeshProUGUI labelTmp = null;
                GameObject activeIndicator = null;

                if (i == 0) // Map Tab: Base + Pin
                {
                    tabGO.name = "Tab_Map";
                    var baseGO = CreateUIObject("Base", tabGO.transform);
                    var bRect = baseGO.GetComponent<RectTransform>();
                    bRect.anchorMin = new Vector2(0.5f, 0.38f);
                    bRect.anchorMax = new Vector2(0.5f, 0.38f);
                    bRect.pivot = new Vector2(0.5f, 0.5f);
                    bRect.sizeDelta = new Vector2(90, 52);
                    var bImg = baseGO.AddComponent<Image>();
                    bImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Sprite/WorldsBase.asset");
                    bImg.preserveAspect = true;
                    bImg.raycastTarget = false;

                    var iconGO = CreateUIObject("Icon", tabGO.transform);
                    iconRect = iconGO.GetComponent<RectTransform>();
                    iconRect.anchorMin = new Vector2(0.5f, 0.60f);
                    iconRect.anchorMax = new Vector2(0.5f, 0.60f);
                    iconRect.pivot = new Vector2(0.5f, 0.5f);
                    iconRect.sizeDelta = new Vector2(68, 68);
                    iconImg = iconGO.AddComponent<Image>();
                    iconImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Sprite/WorldsPin.asset");
                    iconImg.preserveAspect = true;
                    iconImg.raycastTarget = false;
                }
                else if (i == 1) // Shop Tab: Elevated highlighted stall + SHOP label
                {
                    tabGO.name = "Tab_Shop";
                    activeIndicator = CreateUIObject("ActiveGlow", tabGO.transform);
                    var indRect = activeIndicator.GetComponent<RectTransform>();
                    indRect.anchorMin = new Vector2(0.5f, 0.5f);
                    indRect.anchorMax = new Vector2(0.5f, 0.5f);
                    indRect.pivot = new Vector2(0.5f, 0.5f);
                    indRect.sizeDelta = new Vector2(170, 150);
                    indRect.anchoredPosition = new Vector2(0, 18);
                    var indImg = activeIndicator.AddComponent<Image>();
                    indImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Sprite/Shop_selected_1.asset");
                    indImg.preserveAspect = true;
                    indImg.raycastTarget = false;

                    var iconGO = CreateUIObject("Icon", tabGO.transform);
                    iconRect = iconGO.GetComponent<RectTransform>();
                    iconRect.anchorMin = new Vector2(0.5f, 0.65f);
                    iconRect.anchorMax = new Vector2(0.5f, 0.65f);
                    iconRect.pivot = new Vector2(0.5f, 0.5f);
                    iconRect.sizeDelta = new Vector2(98, 98);
                    iconRect.anchoredPosition = new Vector2(0, 8);
                    iconImg = iconGO.AddComponent<Image>();
                    iconImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Sprite/Icon_Shop_Flat.asset");
                    iconImg.preserveAspect = true;
                    iconImg.raycastTarget = false;

                    labelTmp = CreateTMPText("Label", tabGO.transform, "SHOP", 28, Color.white);
                    var labelRect = labelTmp.rectTransform;
                    labelRect.anchorMin = new Vector2(0, 0);
                    labelRect.anchorMax = new Vector2(1, 0.28f);
                    labelRect.offsetMin = Vector2.zero;
                    labelRect.offsetMax = Vector2.zero;
                }
                else if (i == 2) // Home Tab
                {
                    tabGO.name = "Tab_Home";
                    var iconGO = CreateUIObject("Icon", tabGO.transform);
                    iconRect = iconGO.GetComponent<RectTransform>();
                    iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                    iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                    iconRect.pivot = new Vector2(0.5f, 0.5f);
                    iconRect.sizeDelta = new Vector2(84, 84);
                    iconImg = iconGO.AddComponent<Image>();
                    iconImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Sprite/HomeIcon.asset");
                    iconImg.preserveAspect = true;
                    iconImg.raycastTarget = false;
                }
                else if (i == 3) // Leaderboard / Level Tab: 3-column chart
                {
                    tabGO.name = "Tab_Leaderboard";
                    var iconGO = CreateUIObject("Icon", tabGO.transform);
                    iconRect = iconGO.GetComponent<RectTransform>();
                    iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                    iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                    iconRect.pivot = new Vector2(0.5f, 0.5f);
                    iconRect.sizeDelta = new Vector2(84, 84);
                    iconImg = iconGO.AddComponent<Image>();
                    iconImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Sprite/Icon_LeaderBoard_Flat_1.asset");
                    iconImg.preserveAspect = true;
                    iconImg.raycastTarget = false;
                }
                else if (i == 4) // Collection / Cards Tab with Red "!" Badge
                {
                    tabGO.name = "Tab_Cards";
                    var iconGO = CreateUIObject("Icon", tabGO.transform);
                    iconRect = iconGO.GetComponent<RectTransform>();
                    iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                    iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                    iconRect.pivot = new Vector2(0.5f, 0.5f);
                    iconRect.sizeDelta = new Vector2(84, 84);
                    iconImg = iconGO.AddComponent<Image>();
                    iconImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Sprite/Icon_Collection_Flat_0.asset");
                    iconImg.preserveAspect = true;
                    iconImg.raycastTarget = false;

                    var badgeGO = CreateUIObject("NotifBadge", tabGO.transform);
                    var badgeRect = badgeGO.GetComponent<RectTransform>();
                    badgeRect.anchorMin = new Vector2(0.5f, 0.5f);
                    badgeRect.anchorMax = new Vector2(0.5f, 0.5f);
                    badgeRect.pivot = new Vector2(0.5f, 0.5f);
                    badgeRect.anchoredPosition = new Vector2(36, 32);
                    badgeRect.sizeDelta = new Vector2(34, 34);
                    var badgeImg = badgeGO.AddComponent<Image>();
                    badgeImg.color = new Color(0.96f, 0.22f, 0.22f, 1f); // Vibrant red circle
                    badgeImg.raycastTarget = false;

                    var excTmp = CreateTMPText("Exclamation", badgeGO.transform, "!", 26, Color.white, useShadow: false);
                    SetStretchFull(excTmp.rectTransform);
                }

                var tabSo = new SerializedObject(navTab);
                tabSo.FindProperty("tabButton").objectReferenceValue = tabBtn;
                tabSo.FindProperty("iconTransform").objectReferenceValue = iconRect;
                tabSo.FindProperty("iconImage").objectReferenceValue = iconImg;
                tabSo.FindProperty("tabLabel").objectReferenceValue = labelTmp;
                tabSo.FindProperty("activeIndicator").objectReferenceValue = activeIndicator;
                tabSo.FindProperty("tabIndex").intValue = i;
                tabSo.FindProperty("isSelected").boolValue = (i == 1);
                tabSo.ApplyModifiedProperties();
            }

            return navBarGO;
        }

        // =========================================================================
        // PURCHASE CANCELLED POPUP
        // =========================================================================
        private static ShopPurchasePopup BuildPurchaseCancelledPopup(Transform parent)
        {
            var popupRootGO = CreateUIObject("PurchaseCancelledPopup", parent);
            var rootRect = popupRootGO.GetComponent<RectTransform>();
            SetStretchFull(rootRect);

            var cg = popupRootGO.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;

            var popupScript = popupRootGO.AddComponent<ShopPurchasePopup>();

            // Dimmed backdrop
            var backdropGO = CreateUIObject("Backdrop", popupRootGO.transform);
            SetStretchFull(backdropGO.GetComponent<RectTransform>());
            var bdImg = backdropGO.AddComponent<Image>();
            bdImg.color = new Color(0, 0, 0, 0.75f);
            var bdBtn = backdropGO.AddComponent<Button>();

            // Centered Window
            var windowGO = CreateUIObject("Window", popupRootGO.transform);
            var winRect = windowGO.GetComponent<RectTransform>();
            winRect.anchorMin = new Vector2(0.5f, 0.5f);
            winRect.anchorMax = new Vector2(0.5f, 0.5f);
            winRect.pivot = new Vector2(0.5f, 0.5f);
            winRect.sizeDelta = new Vector2(820, 680);

            var winImg = windowGO.AddComponent<Image>();
            winImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/Shop_Popup_Window.png");
            winImg.type = Image.Type.Sliced;
            winImg.raycastTarget = true;

            // Title: OOPS! with black outline
            var titleTmp = CreateTMPText("Title", windowGO.transform, "OOPS!", 72, Color.white);
            var titleRect = titleTmp.rectTransform;
            titleRect.anchorMin = new Vector2(0, 0.76f);
            titleRect.anchorMax = new Vector2(1, 0.94f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Cancel Icon in Center
            var iconGO = CreateUIObject("CancelIcon", windowGO.transform);
            var iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.52f);
            iconRect.anchorMax = new Vector2(0.5f, 0.52f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(180, 180);
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Texture2D/CancelIcon.png") ??
                             LoadSprite("Assets/Asset/Art/New Folder/Sprite/CancelIcon.asset");
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Message: PURCHASE WAS CANCELLED with black outline
            var msgTmp = CreateTMPText("MessageText", windowGO.transform, "PURCHASE WAS CANCELLED", 38, Color.white);
            var msgRect = msgTmp.rectTransform;
            msgRect.anchorMin = new Vector2(0.05f, 0.22f);
            msgRect.anchorMax = new Vector2(0.95f, 0.38f);
            msgRect.offsetMin = Vector2.zero;
            msgRect.offsetMax = Vector2.zero;

            // Close Button in Top-Right
            var closeBtnGO = CreateUIObject("CloseButton", windowGO.transform);
            var closeBtnRect = closeBtnGO.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(1, 1);
            closeBtnRect.anchorMax = new Vector2(1, 1);
            closeBtnRect.pivot = new Vector2(1, 1);
            closeBtnRect.anchoredPosition = new Vector2(-22, -22);
            closeBtnRect.sizeDelta = new Vector2(76, 76);

            var closeBtnImg = closeBtnGO.AddComponent<Image>();
            closeBtnImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Sprite/Cross-Generic.asset") ??
                                 LoadSprite("Assets/Asset/Art/New Folder/Texture2D/Close.png");
            closeBtnImg.preserveAspect = true;
            var closeBtn = closeBtnGO.AddComponent<Button>();
            closeBtnGO.AddComponent<JuicyButton>();

            // Setup popup references
            var popupSo = new SerializedObject(popupScript);
            popupSo.FindProperty("canvasGroup").objectReferenceValue = cg;
            popupSo.FindProperty("windowTransform").objectReferenceValue = winRect;
            popupSo.FindProperty("closeButton").objectReferenceValue = closeBtn;
            popupSo.FindProperty("backdropButton").objectReferenceValue = bdBtn;
            popupSo.ApplyModifiedProperties();

            popupRootGO.SetActive(false);

            return popupScript;
        }

        // =========================================================================
        // REUSABLE HELPER METHODS (PROMINENT LARGE ICONS, BLACK OUTLINE FONTS)
        // =========================================================================
        private static void BuildBoostersRow(Transform parent, (string path, string count)[] boosters, bool isThreeItems)
        {
            if (boosters == null || boosters.Length == 0) return;

            var boostersRowGO = CreateUIObject("BoostersRow", parent);
            var rowRect = boostersRowGO.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.35f, 0);
            rowRect.anchorMax = new Vector2(isThreeItems ? 0.88f : 0.98f, 1);
            rowRect.offsetMin = Vector2.zero;
            rowRect.offsetMax = Vector2.zero;

            var hlg = boostersRowGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = isThreeItems ? 22 : 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;

            float itemWidth = isThreeItems ? 140f : 110f;
            float iconSize = isThreeItems ? 135f : 108f;

            foreach (var (bPath, bCount) in boosters)
            {
                var itemGO = CreateUIObject("BoosterItem", boostersRowGO.transform);
                var itemRect = itemGO.GetComponent<RectTransform>();
                itemRect.sizeDelta = new Vector2(itemWidth, 220);

                // Icon centered slightly above middle (Prominent & Juicy)
                var bIconGO = CreateUIObject("Icon", itemGO.transform);
                var bIconRect = bIconGO.GetComponent<RectTransform>();
                bIconRect.anchorMin = new Vector2(0.5f, 0.5f);
                bIconRect.anchorMax = new Vector2(0.5f, 0.5f);
                bIconRect.pivot = new Vector2(0.5f, 0.5f);
                bIconRect.anchoredPosition = new Vector2(0, 26);
                bIconRect.sizeDelta = new Vector2(iconSize, iconSize);
                var bImg = bIconGO.AddComponent<Image>();
                bImg.sprite = LoadSprite(bPath);
                bImg.preserveAspect = true;
                bImg.raycastTarget = false;

                // Text sitting cleanly right below the icon with crisp black outline
                var bTmp = CreateTMPText("Text", itemGO.transform, bCount, isThreeItems ? 42 : 38, Color.white);
                var bTextRect = bTmp.rectTransform;
                bTextRect.anchorMin = new Vector2(0.5f, 0.5f);
                bTextRect.anchorMax = new Vector2(0.5f, 0.5f);
                bTextRect.pivot = new Vector2(0.5f, 0.5f);
                bTextRect.anchoredPosition = new Vector2(0, -56);
                bTextRect.sizeDelta = new Vector2(itemWidth, 44);
            }
        }

        private static void BuildBadge(Transform parent, BadgeType badgeType, string text)
        {
            if (badgeType == BadgeType.None || string.IsNullOrEmpty(text)) return;

            var badgeGO = CreateUIObject("Badge", parent);
            var badgeRect = badgeGO.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1, 1);
            badgeRect.anchorMax = new Vector2(1, 1);
            badgeRect.pivot = new Vector2(1, 1);
            badgeRect.anchoredPosition = new Vector2(-10, -10);

            var badgeImg = badgeGO.AddComponent<Image>();
            badgeImg.preserveAspect = true;
            badgeImg.raycastTarget = false;

            if (badgeType == BadgeType.Extra800)
            {
                badgeRect.sizeDelta = new Vector2(145, 145);
                badgeImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/Shop_Tag_Extra800.png");
            }
            else
            {
                badgeRect.sizeDelta = new Vector2(175, 100);
                badgeImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/Shop_Tag_Pink.png");
            }

            var badgeTmp = CreateTMPText("Label", badgeGO.transform, text, (badgeType == BadgeType.Extra800) ? 26 : 28, Color.white);
            badgeTmp.lineSpacing = -18;
            SetStretchFull(badgeTmp.rectTransform);
        }

        private static (TextMeshProUGUI title, TextMeshProUGUI price, Button button) BuildBottomBar(Transform parent, string title, string price, bool isGoldTitle)
        {
            var bottomAreaGO = CreateUIObject("BottomArea", parent);
            var bottomRect = bottomAreaGO.GetComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0, 0);
            bottomRect.anchorMax = new Vector2(1, 0.28f);
            bottomRect.offsetMin = new Vector2(32, 0);
            bottomRect.offsetMax = new Vector2(-32, 0);

            Color titleCol = isGoldTitle ? new Color(1f, 0.89f, 0.28f, 1f) : Color.white;
            var titleTmp = CreateTMPText("Title", bottomAreaGO.transform, title, 52, titleCol, TextAlignmentOptions.MidlineLeft);
            var titleRect = titleTmp.rectTransform;
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(0, 0.5f);
            titleRect.pivot = new Vector2(0, 0.5f);
            titleRect.anchoredPosition = new Vector2(6, 0);
            titleRect.sizeDelta = new Vector2(560, 80);

            var buyBtnGO = CreateUIObject("BuyButton", bottomAreaGO.transform);
            var buyBtnRect = buyBtnGO.GetComponent<RectTransform>();
            buyBtnRect.anchorMin = new Vector2(1, 0.5f);
            buyBtnRect.anchorMax = new Vector2(1, 0.5f);
            buyBtnRect.pivot = new Vector2(1, 0.5f);
            buyBtnRect.anchoredPosition = new Vector2(0, 0);
            buyBtnRect.sizeDelta = new Vector2(340, 94);

            var buyBtnImg = buyBtnGO.AddComponent<Image>();
            buyBtnImg.sprite = LoadSprite("Assets/Asset/Art/ShopGenerated/Shop_Green_Button.png");
            buyBtnImg.type = Image.Type.Sliced;
            var buyBtn = buyBtnGO.AddComponent<Button>();
            buyBtnGO.AddComponent<JuicyButton>();

            var priceTmp = CreateTMPText("PriceText", buyBtnGO.transform, price, 40, Color.white);
            SetStretchFull(priceTmp.rectTransform);

            return (titleTmp, priceTmp, buyBtn);
        }

        private static TextMeshProUGUI CreateTMPText(string name, Transform parent, string text, float fontSize, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center, bool useShadow = true)
        {
            var go = CreateUIObject(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = s_Font;
            if (s_OutlineMaterial != null)
            {
                tmp.fontSharedMaterial = s_OutlineMaterial;
            }
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.raycastTarget = false;
            tmp.outlineColor = Color.black;
            tmp.outlineWidth = 0.35f;
            if (useShadow)
            {
                AddShadow(go, new Color(0, 0, 0, 0.85f), new Vector2(2f, -2.5f));
            }
            return tmp;
        }

        private static void AddShadow(GameObject target, Color color, Vector2 distance)
        {
            var shadow = target.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetStretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;

            if (path.EndsWith(".asset"))
            {
                string pngPath = path.Replace("/Sprite/", "/Texture2D/").Replace("_0.asset", ".png").Replace(".asset", ".png");
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
                if (sprite != null) return sprite;
            }

            return null;
        }
    }
}
#endif
