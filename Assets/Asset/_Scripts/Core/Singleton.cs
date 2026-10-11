using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _ins;
    public static T instance => _ins;

    protected virtual void Awake()
    {
        if (_ins == null)
        {
            _ins = this.GetComponent<T>();
            return;
        }
        if (_ins.gameObject.GetInstanceID() != this.gameObject.GetInstanceID())
        {
            Destroy(this.gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_ins == this)
        {
            _ins = null;
        }
    }
}