using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DailyRewardPanel : MonoBehaviour
{
    private SaveLoadServices save;
    private Button claim;
    private TMP_Text claimLabel;
    private readonly GameObject[] highlights = new GameObject[7];
    private readonly GameObject[] backgrounds = new GameObject[7];
    private readonly GameObject[] claimedOverlays = new GameObject[7];
    private DateTime displayedDate;

    private void Awake()
    {
        save = SaveLoadServices.FindForMenu(gameObject);
        claim = transform.Find("Bottom/Collect").GetComponent<Button>();
        claimLabel = claim.GetComponentInChildren<TMP_Text>(true);
        claim.onClick.AddListener(Claim);
        var layout = transform.Find("Board/Layout");
        for (int i = 0; i < highlights.Length; i++)
        {
            var card = layout.Find($"Day {i + 1}");
            highlights[i] = card.Find("Can Claim BG").gameObject;
            backgrounds[i] = card.Find("Can't Claim BG").gameObject;
            card.Find("Day Text").GetComponent<TMP_Text>().text = $"DAY {i + 1}";
            var overlay = Instantiate(backgrounds[i].GetComponent<Image>(), card, false);
            overlay.name = "Claimed Overlay";
            overlay.color = new Color(0f, 0f, 0f, 0.45f);
            overlay.raycastTarget = false;
            claimedOverlays[i] = overlay.gameObject;
        }
    }

    private void OnEnable() => Refresh();

    private void Update()
    {
        if (displayedDate != DateTime.Today) Refresh();
    }

    private void Claim()
    {
        bool granted = save.TryClaimDailyReward();
        Refresh();
        if (!granted && claim.interactable) claimLabel.text = "RETRY";
    }

    private void Refresh()
    {
        displayedDate = DateTime.Today;
        bool available = save.CanClaimDailyReward;
        int claimed = save.DailyRewardsInCycle;
        if (available && claimed == highlights.Length) claimed = 0;
        for (int i = 0; i < highlights.Length; i++)
        {
            bool ready = available && i == claimed;
            highlights[i].SetActive(ready);
            backgrounds[i].SetActive(!ready);
            claimedOverlays[i].SetActive(i < claimed);
        }
        claim.interactable = available;
        claimLabel.text = available ? "CLAIM" : "CLAIMED";
    }
}
