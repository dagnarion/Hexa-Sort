using System;
using TMPro;
using UnityEngine;

public class TaskManager : MonoBehaviour // task lấy data từ json
{
    [SerializeField] private EventChannel<int> OnHexagonCollected;
    [SerializeField] private EventChannel<int> OnCountChange;
    [SerializeField] private TextMeshProUGUI tmp;
    private int count = 0;

    private void Start()
    {
        Init(0);
    }

    private void OnEnable()
    {
        OnHexagonCollected.OnEventRaise += Collect;
    }

    private void OnDisable()
    {
        OnHexagonCollected.OnEventRaise -= Collect;
    }

    public void Init(int count)
    {
        count = 0;
    }
    
    private void Collect(int amount)
    {
        count += amount;
        OnCountChange?.Raise(count);
        tmp.text = count.ToString();
    }
    
    
}