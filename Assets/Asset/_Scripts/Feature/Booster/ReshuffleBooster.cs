using UnityEngine;

public class ReshuffleBooster : MonoBehaviour
{
    [SerializeField] private int remain;
    [SerializeField] private HexagonStackSpawner hexagonStackSpawner;

    public void Reshuffle()
    {
        if(remain <= 0) return;
        hexagonStackSpawner.Release();
        hexagonStackSpawner.Spawn();
        remain--;
    }
}
