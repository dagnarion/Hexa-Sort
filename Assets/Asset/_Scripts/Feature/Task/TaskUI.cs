using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskUI : MonoBehaviour
{
    [SerializeField] private EventChannel<int> TaskProcess;
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private Slider progressBar;
    [SerializeField] private int max;
    private void OnEnable()
    {
        TaskProcess.OnEventRaise += UpdateUI;
    }

    private void OnDisable()
    {
        TaskProcess.OnEventRaise -= UpdateUI;
    }
    
    private void UpdateUI(int count)
    {
        progressBar.value = (float) count / max;
        tmp.text = count.ToString();
    }
}