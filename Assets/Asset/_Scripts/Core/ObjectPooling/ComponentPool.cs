using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public abstract class ComponentPoolSO<T> : ScriptableObject where T : Component
{
    [SerializeField] protected T prefab;
    [SerializeField] private int Capacity;
    [SerializeField] private int MaxSize;
    private ObjectPool<T> Pools;
    private readonly HashSet<T> pooledItems = new HashSet<T>();
    protected Transform PoolRoot;

    public void InitPool(Transform parent = null)
    {
        PoolRoot = parent;
        pooledItems.Clear();
        Pools = new ObjectPool<T>
        (
            createFunc: CreateInstance,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: false,
            defaultCapacity: Capacity,
            maxSize: MaxSize
        );
    }

    protected virtual T CreateInstance()
    {
        T instance = Instantiate(prefab, PoolRoot);
        return instance;
    }

    protected virtual void OnTakeFromPool(T instance)
    {
        pooledItems.Remove(instance);
        instance.gameObject.SetActive(true);
    }

    protected virtual void OnReturnedToPool(T instance)
    {
        instance.gameObject.SetActive(false);
        if (PoolRoot != null)
        {
            instance.transform.SetParent(PoolRoot);
        }
    }

    protected virtual void OnDestroyPoolObject(T instance)
    {
        pooledItems.Remove(instance);
        if (instance != null)
            Destroy(instance.gameObject);
    }

    public T Get()
    {
        if (Pools == null) InitPool();
        return Pools.Get();
    }

    public void Release(T instance)
    {
        if (instance == null) return;
        if (Pools == null)
        {
            Destroy(instance.gameObject);
            return;
        }
        if (!pooledItems.Add(instance)) return;
        Pools.Release(instance);
    }

    public void Clear()
    {
        Pools?.Clear();
        pooledItems.Clear();
        Pools = null;
    }

    private void OnDisable()
    {
        Clear();
    }
}
