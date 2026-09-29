using System;
using Cysharp.Threading.Tasks;

public interface ILock
{
    event Action<ILock> OnUnlocked;
    UniTask UnlockAsync();
}