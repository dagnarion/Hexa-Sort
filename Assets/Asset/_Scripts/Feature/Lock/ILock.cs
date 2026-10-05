using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ILock
{
    event Action<ILock> OnUnlocked;
    UniTask UnlockAsync();
    public void Init(int target, Vector3 position);
}