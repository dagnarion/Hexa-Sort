using System;
using UnityEngine;

[CreateAssetMenu(menuName = "EventChannel/NoParameter")]
public class NoParameterEventChannel : ScriptableObject
{
    public event Action OnEventRaise;
    
    public void Raise()
    {
        OnEventRaise?.Invoke();
    }
}