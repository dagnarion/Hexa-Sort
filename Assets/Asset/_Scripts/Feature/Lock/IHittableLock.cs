
using Cysharp.Threading.Tasks;

public interface IHittableLock : ILock
{
    UniTask<bool> TakeHitAsync(int damage = 1);
}