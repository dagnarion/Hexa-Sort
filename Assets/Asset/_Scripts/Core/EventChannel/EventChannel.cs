using System;
using UnityEngine;

public abstract class EventChannel<T> : ScriptableObject
{
    public event Action<T> OnEventRaise;
    
    public void Raise(T value)
    {
        OnEventRaise?.Invoke(value);
    }
}
