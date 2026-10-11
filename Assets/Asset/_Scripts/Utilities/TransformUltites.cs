using UnityEngine;

public static class TransformUltites
{
    public static void Clear(this Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                GameObject.Destroy(transform.GetChild(i).gameObject);
            else
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}