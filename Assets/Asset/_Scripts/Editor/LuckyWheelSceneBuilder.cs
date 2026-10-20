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
    public static class LuckyWheelSceneBuilder
    {
        private static TMP_FontAsset s_Font;
        private static Material s_OutlineMaterial;

        [MenuItem("HexaSort/Build Lucky Wheel UI")]
        public static void BuildLuckyWheelUI()
        {
            // 1. Ensure EventSystem
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

            // 3. Remove existing LuckyWheelCanvas if any
            var existing = GameObject.Find("LuckyWheelCanvas");
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            // 4. Create LuckyWheelCanvas root
            var canvasGO = new GameObject("LuckyWheelCanvas");
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create LuckyWheelCanvas");

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60; // Render above other canvases when opened

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2400); // 20:9
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;

            canvasGO.AddComponent<GraphicRaycaster>();
            var wheelController = canvasGO.AddComponent<LuckyWheelController>();

            // Fullscreen Background
            var bgGO = CreateUIObject("Background", canvasGO.transform);
            SetStretchFull(bgGO.GetComponent<RectTransform>());
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.045f, 0.025f, 0.09f, 0.98f); // Deep dark purple / navy
            bgImg.raycastTarget = true; // Blocks input to elements behind wheel

            // 5. Top 3D Title Banner ("SPIN & WIN")
            var titleBannerGO = CreateUIObject("TitleBanner", canvasGO.transform);
            var titleBannerRect = titleBannerGO.GetComponent<RectTransform>();
            titleBannerRect.anchorMin = new Vector2(0.5f, 1f);
            titleBannerRect.anchorMax = new Vector2(0.5f, 1f);
            titleBannerRect.pivot = new Vector2(0.5f, 1f);
            titleBannerRect.anchoredPosition = new Vector2(0, -140);
            titleBannerRect.sizeDelta = new Vector2(780, 282);
            var titleBannerImg = titleBannerGO.AddComponent<Image>();
            titleBannerImg.sprite = LoadSprite("Assets/Asset/Art/WheelGenerated/Wheel_Title_Cropped.png");
            titleBannerImg.preserveAspect = true;
            titleBannerImg.raycastTarget = false;

            // 6. Progress Bar ("1/5" + Rainbow pinwheel)
            var progressCapsuleGO = CreateUIObject("ProgressCapsule", canvasGO.transform);
            var progressCapsuleRect = progressCapsuleGO.GetComponent<RectTransform>();
            progressCapsuleRect.anchorMin = new Vector2(0.5f, 1f);
            progressCapsuleRect.anchorMax = new Vector2(0.5f, 1f);
            progressCapsuleRect.pivot = new Vector2(0.5f, 1f);
            progressCapsuleRect.anchoredPosition = new Vector2(0, -470);
            progressCapsuleRect.sizeDelta = new Vector2(510, 68);
            var progressBgImg = progressCapsuleGO.AddComponent<Image>();
            progressBgImg.sprite = LoadSprite("Assets/Asset/Art/WheelGenerated/Wheel_Progress_Bg.png");
            progressBgImg.type = Image.Type.Sliced;
            progressBgImg.raycastTarget = false;

            // Yellow Fill (1/5 ~ 38% fill)
            var fillGO = CreateUIObject("Fill", progressCapsuleGO.transform);
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0.5f);
            fillRect.anchorMax = new Vector2(0, 0.5f);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.anchoredPosition = new Vector2(6, 0);
            fillRect.sizeDelta = new Vector2(145, 54);
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.sprite = LoadSprite("Assets/Asset/Art/WheelGenerated/Wheel_Progress_Fill.png");
            fillImg.type = Image.Type.Sliced;
            fillImg.raycastTarget = false;

            // Progress text "1/5"
            var progTmp = CreateTMPText("ProgressText", progressCapsuleGO.transform, "1/5", 36, Color.white);
            SetStretchFull(progTmp.rectTransform);

            // Pinwheel Donut Icon on right end
            var pinwheelIconGO = CreateUIObject("PinwheelIcon", progressCapsuleGO.transform);
            var pinwheelIconRect = pinwheelIconGO.GetComponent<RectTransform>();
            pinwheelIconRect.anchorMin = new Vector2(1f, 0.5f);
            pinwheelIconRect.anchorMax = new Vector2(1f, 0.5f);
            pinwheelIconRect.pivot = new Vector2(0.5f, 0.5f);
            pinwheelIconRect.anchoredPosition = new Vector2(12, 0);
            pinwheelIconRect.sizeDelta = new Vector2(92, 92);
            var pinwheelImg = pinwheelIconGO.AddComponent<Image>();
            pinwheelImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Texture2D/Spin-Icon.png");
            pinwheelImg.preserveAspect = true;
            pinwheelImg.raycastTarget = false;

            // 7. Subtitle ("Win 5 levels to Spin the Wheel")
            var subtitleTmp = CreateTMPText("Subtitle", canvasGO.transform, "Win 5 levels to Spin the Wheel", 40, Color.white);
            var subRect = subtitleTmp.rectTransform;
            subRect.anchorMin = new Vector2(0.5f, 1f);
            subRect.anchorMax = new Vector2(0.5f, 1f);
            subRect.pivot = new Vector2(0.5f, 1f);
            subRect.anchoredPosition = new Vector2(0, -565);
            subRect.sizeDelta = new Vector2(960, 60);

            // 8. Lucky Wheel Main Area
            var wheelAreaGO = CreateUIObject("WheelArea", canvasGO.transform);
            var wheelAreaRect = wheelAreaGO.GetComponent<RectTransform>();
            wheelAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
            wheelAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
            wheelAreaRect.pivot = new Vector2(0.5f, 0.5f);
            wheelAreaRect.anchoredPosition = new Vector2(0, -30);
            wheelAreaRect.sizeDelta = new Vector2(940, 940);

            // Rotatable Wheel Root
            var wheelRotatorGO = CreateUIObject("WheelRotator", wheelAreaGO.transform);
            var wheelRotatorRect = wheelRotatorGO.GetComponent<RectTransform>();
            SetStretchFull(wheelRotatorRect);
            // Initially rotated by 22.5 degrees so segment 0 (Yellow, x100) center is at 12 o'clock under pointer pin
            wheelRotatorRect.localEulerAngles = new Vector3(0, 0, 22.5f);

            // Wheel Disc Image (Spin 1.png)
            var discGO = CreateUIObject("DiscImage", wheelRotatorGO.transform);
            SetStretchFull(discGO.GetComponent<RectTransform>());
            var discImg = discGO.AddComponent<Image>();
            discImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Texture2D/Spin 1.png");
            discImg.preserveAspect = true;
            discImg.raycastTarget = false;

            // 8 Prize Segments
            var prizeConfig = new (string iconPath, string amountText, float radius)[]
            {
                ("Assets/Asset/Art/WheelGenerated/Wheel_Coin.png", "x100", 295f),                 // 0: Yellow (Top)
                ("Assets/Asset/Art/New Folder/Texture2D/Booster-Hand-no-border.png", "x1", 295f),   // 1: Cyan (Top-Right)
                ("Assets/Asset/Art/WheelGenerated/Wheel_Coin.png", "x10", 295f),                  // 2: Blue (Right)
                ("Assets/Asset/Art/New Folder/Texture2D/Booster-Shuffle-no-border.png", "x1", 295f),// 3: Violet (Bottom-Right)
                ("Assets/Asset/Art/WheelGenerated/Wheel_Coin.png", "x5", 295f),                   // 4: Purple (Bottom)
                ("Assets/Asset/Art/New Folder/Texture2D/Booster-Hammer-no-border.png", "x1", 295f), // 5: Magenta (Bottom-Left)
                ("Assets/Asset/Art/WheelGenerated/Wheel_Coin.png", "x5", 295f),                   // 6: Hot Pink (Left)
                ("Assets/Asset/Art/WheelGenerated/Wheel_Coin.png", "x10", 295f)                  // 7: Coral/Orange (Top-Left)
            };

            var prizeTransforms = new RectTransform[8];

            for (int i = 0; i < 8; i++)
            {
                var (iconPath, amountText, radius) = prizeConfig[i];
                // In Spin 1.png, each segment is 45 deg wide, centered at clockwise angle: 22.5 + i * 45
                float cwDeg = 22.5f + i * 45f;
                float rad = cwDeg * Mathf.Deg2Rad;

                var prizeGO = CreateUIObject("Prize_" + i, wheelRotatorGO.transform);
                var prizeRect = prizeGO.GetComponent<RectTransform>();
                prizeRect.anchorMin = new Vector2(0.5f, 0.5f);
                prizeRect.anchorMax = new Vector2(0.5f, 0.5f);
                prizeRect.pivot = new Vector2(0.5f, 0.5f);

                float px = radius * Mathf.Sin(rad); // X is positive to the right (clockwise)
                float py = radius * Mathf.Cos(rad); // Y is positive up
                prizeRect.anchoredPosition = new Vector2(px, py);
                prizeRect.localEulerAngles = new Vector3(0, 0, -cwDeg);
                prizeRect.sizeDelta = new Vector2(160, 160);

                prizeTransforms[i] = prizeRect;

                // Prize Icon
                var pIconGO = CreateUIObject("Icon", prizeGO.transform);
                var pIconRect = pIconGO.GetComponent<RectTransform>();
                pIconRect.anchorMin = new Vector2(0.5f, 0.5f);
                pIconRect.anchorMax = new Vector2(0.5f, 0.5f);
                pIconRect.pivot = new Vector2(0.5f, 0.5f);
                pIconRect.anchoredPosition = new Vector2(0, 26);
                pIconRect.sizeDelta = new Vector2(120, 120);
                var pIconImg = pIconGO.AddComponent<Image>();
                pIconImg.sprite = LoadSprite(iconPath);
                pIconImg.preserveAspect = true;
                pIconImg.raycastTarget = false;

                // Prize Amount Text (e.g. "x100", "x1")
                var pTmp = CreateTMPText("AmountText", prizeGO.transform, amountText, 40, Color.white);
                var pTextRect = pTmp.rectTransform;
                pTextRect.anchorMin = new Vector2(0.5f, 0.5f);
                pTextRect.anchorMax = new Vector2(0.5f, 0.5f);
                pTextRect.pivot = new Vector2(0.5f, 0.5f);
                pTextRect.anchoredPosition = new Vector2(0, -48);
                pTextRect.sizeDelta = new Vector2(150, 44);
            }

            // Center White Glossy Hub
            var centerHubGO = CreateUIObject("CenterHub", wheelRotatorGO.transform);
            var centerHubRect = centerHubGO.GetComponent<RectTransform>();
            centerHubRect.anchorMin = new Vector2(0.5f, 0.5f);
            centerHubRect.anchorMax = new Vector2(0.5f, 0.5f);
            centerHubRect.pivot = new Vector2(0.5f, 0.5f);
            centerHubRect.anchoredPosition = Vector2.zero;
            centerHubRect.sizeDelta = new Vector2(142, 142);
            var centerHubImg = centerHubGO.AddComponent<Image>();
            centerHubImg.sprite = LoadSprite("Assets/Asset/Art/WheelGenerated/Wheel_Center_Hub.png");
            centerHubImg.preserveAspect = true;
            centerHubImg.raycastTarget = false;

            // 9. Top Indicator Pointer Pin (Child of wheelArea, NOT wheelRotator!)
            var pointerGO = CreateUIObject("PointerPin", wheelAreaGO.transform);
            var pointerRect = pointerGO.GetComponent<RectTransform>();
            pointerRect.anchorMin = new Vector2(0.5f, 1f);
            pointerRect.anchorMax = new Vector2(0.5f, 1f);
            pointerRect.pivot = new Vector2(0.5f, 0.7f); // Pivot near top of pin for authentic tick wobble
            pointerRect.anchoredPosition = new Vector2(0, -20);
            pointerRect.sizeDelta = new Vector2(135, 140);
            var pointerImg = pointerGO.AddComponent<Image>();
            pointerImg.sprite = LoadSprite("Assets/Asset/Art/New Folder/Texture2D/Spin-Pin.png");
            pointerImg.preserveAspect = true;
            pointerImg.raycastTarget = false;

            // 10. Large Chunky "SPIN" Button
            var spinBtnGO = CreateUIObject("SpinButton", canvasGO.transform);
            var spinBtnRect = spinBtnGO.GetComponent<RectTransform>();
            spinBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
            spinBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
            spinBtnRect.pivot = new Vector2(0.5f, 0.5f);
            spinBtnRect.anchoredPosition = new Vector2(0, -570);
            spinBtnRect.sizeDelta = new Vector2(620, 138);

            var spinBtnImg = spinBtnGO.AddComponent<Image>();
            spinBtnImg.sprite = LoadSprite("Assets/Asset/Art/WheelGenerated/Wheel_Spin_Button.png");
            spinBtnImg.type = Image.Type.Sliced;
            var spinBtn = spinBtnGO.AddComponent<Button>();
            spinBtnGO.AddComponent<JuicyButton>();

            var spinTmp = CreateTMPText("SpinText", spinBtnGO.transform, "SPIN", 70, Color.white);
            SetStretchFull(spinTmp.rectTransform);
            spinTmp.rectTransform.anchoredPosition = new Vector2(0, 4);

            // 11. Bottom Navigation Bar in background
            BuildBottomBar(canvasGO.transform);

            // 12. Bottom Close / Home Button ("X" icon + "HOME")
            var closeBtnGO = CreateUIObject("CloseButton", canvasGO.transform);
            var closeBtnRect = closeBtnGO.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.5f, 0f);
            closeBtnRect.anchorMax = new Vector2(0.5f, 0f);
            closeBtnRect.pivot = new Vector2(0.5f, 0.5f);
            closeBtnRect.anchoredPosition = new Vector2(0, 120);
            closeBtnRect.sizeDelta = new Vector2(130, 130);

            var closeBtnImg = closeBtnGO.AddComponent<Image>();
            closeBtnImg.sprite = LoadSprite("Assets/Asset/Art/WheelGenerated/Wheel_Close_Btn.png");
            closeBtnImg.preserveAspect = true;
            var closeBtn = closeBtnGO.AddComponent<Button>();
            closeBtnGO.AddComponent<JuicyButton>();

            var homeTmp = CreateTMPText("HomeLabel", closeBtnGO.transform, "HOME", 24, new Color(0.6f, 0.65f, 0.85f, 1f));
            var homeRect = homeTmp.rectTransform;
            homeRect.anchorMin = new Vector2(0.5f, 0f);
            homeRect.anchorMax = new Vector2(0.5f, 0f);
            homeRect.pivot = new Vector2(0.5f, 1f);
            homeRect.anchoredPosition = new Vector2(0, -8);
            homeRect.sizeDelta = new Vector2(120, 30);


            // 13. Wire up controller references
            var so = new SerializedObject(wheelController);
            so.FindProperty("wheelDiscTransform").objectReferenceValue = wheelRotatorRect;
            so.FindProperty("pointerTransform").objectReferenceValue = pointerRect;
            so.FindProperty("spinButton").objectReferenceValue = spinBtn;
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("luckyWheelCanvas").objectReferenceValue = canvas;
            so.FindProperty("subtitleText").objectReferenceValue = subtitleTmp;

            var prizeArrayProp = so.FindProperty("prizeItemTransforms");
            prizeArrayProp.arraySize = prizeTransforms.Length;
            for (int i = 0; i < prizeTransforms.Length; i++)
            {
                prizeArrayProp.GetArrayElementAtIndex(i).objectReferenceValue = prizeTransforms[i];
            }
            so.ApplyModifiedProperties();

            // 14. Save Prefab
            string prefabPath = "Assets/Asset/Prefabs/UI/LuckyWheelCanvas.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(canvasGO, prefabPath, InteractionMode.UserAction);

            // 15. Save scene
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=green><b>[LuckyWheelSceneBuilder]</b> Successfully created LuckyWheelCanvas!</color>");
        }

        private static void BuildBottomBar(Transform parent)
        {
            var navBarGO = CreateUIObject("BottomNavigationBar", parent);
            var navRect = navBarGO.GetComponent<RectTransform>();
            navRect.anchorMin = new Vector2(0, 0);
            navRect.anchorMax = new Vector2(1, 0);
            navRect.pivot = new Vector2(0.5f, 0);
            navRect.anchoredPosition = Vector2.zero;
            navRect.sizeDelta = new Vector2(0, 185);

            var navBg = navBarGO.AddComponent<Image>();
            navBg.color = new Color(0.06f, 0.05f, 0.14f, 0.98f);
            navBg.raycastTarget = false;

            var hlg = navBarGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 0;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            string[] tabSprites = new string[]
            {
                "Assets/Asset/Art/New Folder/Sprite/WorldsPin.asset",
                "Assets/Asset/Art/New Folder/Sprite/Icon_Shop_Flat.asset",
                "Assets/Asset/Art/New Folder/Sprite/HomeIcon.asset",
                "Assets/Asset/Art/New Folder/Sprite/Icon_LeaderBoard_Flat_1.asset",
                "Assets/Asset/Art/New Folder/Sprite/Icon_Collection_Flat_0.asset"
            };

            for (int i = 0; i < 5; i++)
            {
                var tabGO = CreateUIObject("Tab_" + i, navBarGO.transform);
                if (i == 2)
                {
                    // Center tab is Home where the Close button sits
                    continue;
                }

                var iconGO = CreateUIObject("Icon", tabGO.transform);
                var iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = new Vector2(80, 80);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.sprite = LoadSprite(tabSprites[i]);
                iconImg.preserveAspect = true;
                iconImg.color = new Color(0.4f, 0.5f, 0.75f, 0.6f); // Dimmed background icons
                iconImg.raycastTarget = false;
            }
        }

        private static TextMeshProUGUI CreateTMPText(string name, Transform parent, string text, float fontSize, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center, bool useShadow = true)
        {
            var go = CreateUIObject(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = s_Font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.raycastTarget = false;
            if (s_OutlineMaterial != null)
            {
                tmp.fontSharedMaterial = s_OutlineMaterial;
            }
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
