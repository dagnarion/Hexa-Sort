using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class OutOfLivesPopup : MonoBehaviour
{
    [SerializeField] private GameObject overlay;
    [SerializeField] private TMP_Text countdown;
    [SerializeField] private Button dismissButton;

    private SaveLoadServices save;
    private string countdownFormat;
    private GameObject previousSelection;
    private double nextRefresh;

    private void Awake()
    {
        countdownFormat = countdown.text;
        dismissButton.onClick.AddListener(Dismiss);
        overlay.SetActive(false);
    }

    public static bool CanPlay(SaveLoadServices save)
    {
        if (save.GetLives(out int seconds) > 0) return true;
        var popup = FindFirstObjectByType<OutOfLivesPopup>();
        if (popup != null) popup.Show(save, seconds);
        return false;
    }

    private void Show(SaveLoadServices service, int seconds)
    {
        save = service;
        ShowTime(seconds);
        if (overlay.activeSelf) return;
        previousSelection = EventSystem.current?.currentSelectedGameObject;
        overlay.SetActive(true);
        dismissButton.Select();
    }

    private void Update()
    {
        if (save == null || Time.realtimeSinceStartupAsDouble < nextRefresh) return;
        if (save.GetLives(out int seconds) > 0) Dismiss();
        else ShowTime(seconds);
    }

    private void ShowTime(int seconds)
    {
        string time = $"{seconds / 60:00}:{seconds % 60:00}";
        countdown.text = countdownFormat.Contains("{time}") ? countdownFormat.Replace("{time}", time) : time;
        nextRefresh = Time.realtimeSinceStartupAsDouble + 0.25;
    }

    private void Dismiss()
    {
        overlay.SetActive(false);
        save = null;
        if (previousSelection != null && previousSelection.activeInHierarchy)
            EventSystem.current?.SetSelectedGameObject(previousSelection);
        previousSelection = null;
    }
}
