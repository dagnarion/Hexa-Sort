using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class OutOfLivesPopup : MonoBehaviour
{
    private SaveLoadServices save;
    private TMP_Text countdown;
    private GameObject previousSelection;
    private double nextRefresh;

    public static bool CanPlay(SaveLoadServices save)
    {
        if (save.GetLives(out int seconds) > 0) return true;
        if (FindFirstObjectByType<OutOfLivesPopup>() == null)
        {
            var root = new GameObject("Out of lives", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(Image));
            root.AddComponent<OutOfLivesPopup>().Build(save, seconds);
        }
        return false;
    }

    private void Build(SaveLoadServices service, int seconds)
    {
        save = service;
        var canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        var scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        scaler.matchWidthOrHeight = 0.5f;
        GetComponent<Image>().color = new Color(0.06f, 0.04f, 0.12f, 0.78f);

        var card = Element("Card", transform, 510, 306, 0);
        card.gameObject.AddComponent<Image>().color = new Color32(246, 242, 255, 255);
        Label(card, "OUT OF LIVES", 42, 93, new Color32(105, 42, 96, 255));
        countdown = Label(card, "", 32, 7, new Color32(58, 44, 83, 255));
        ShowTime(seconds);
        var buttonRect = Element("Dismiss", card, 212, 65, -90);
        var fill = buttonRect.gameObject.AddComponent<Image>();
        fill.color = new Color32(45, 115, 199, 255);
        var button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = fill;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.onClick.AddListener(Dismiss);
        Label(buttonRect, "OK", 34, 0, Color.white);
        previousSelection = EventSystem.current?.currentSelectedGameObject;
        button.Select();
    }

    private static RectTransform Element(string name, Transform parent, float width, float height, float y)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(0, y);
        return rect;
    }

    private static TMP_Text Label(RectTransform parent, string value, float size, float y, Color color)
    {
        var rect = Element("Label", parent, parent.sizeDelta.x - 20, 60, y);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.color = color;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private void Update()
    {
        if (Time.realtimeSinceStartupAsDouble < nextRefresh) return;
        if (save.GetLives(out int seconds) > 0) Dismiss();
        else ShowTime(seconds);
    }

    private void ShowTime(int seconds)
    {
        countdown.text = $"Next life in {seconds / 60:00}:{seconds % 60:00}";
        nextRefresh = Time.realtimeSinceStartupAsDouble + 0.25;
    }

    private void Dismiss()
    {
        if (previousSelection != null && previousSelection.activeInHierarchy)
            EventSystem.current?.SetSelectedGameObject(previousSelection);
        Destroy(gameObject);
    }
}
