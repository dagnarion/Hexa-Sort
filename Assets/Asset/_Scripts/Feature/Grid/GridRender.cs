using UnityEngine;
using NaughtyAttributes;

public class GridRender : MonoBehaviour
{
    [SerializeField] private GridDataSO data;
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform holder;

    [Button]
    public void GenerateGrid()
    {
        holder.Clear();
        for (int x = 0; x < data.GridSize.x; x++)
        for (int y = 0; y < data.GridSize.y; y++)
        {
            Vector3 pos = grid.CellToWorld(new Vector3Int(x, y, 0));
            Instantiate(slotPrefab, pos, Quaternion.identity, holder); // sau triển khai factory để spawn ra slot bị lock
        }
    }
}
