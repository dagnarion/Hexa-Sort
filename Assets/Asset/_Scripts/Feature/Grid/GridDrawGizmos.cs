using System;
using UnityEngine;

public class GridDrawGizmos : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private GridDataSO data;
    [SerializeField] private bool isFlatTopped = false;

    private void OnDrawGizmos()
    {
        if (grid == null || data == null) return;
        Gizmos.color = Color.green;
        float radius = isFlatTopped ? grid.cellSize.x / 2f : grid.cellSize.y / 2f;
        Vector3[] localVertices = GetHexagonLocalVertices(radius, isFlatTopped);
        for (int x = 0; x < data.GridSize.x; x++)
        {
            for (int y = 0; y < data.GridSize.y; y++)
            {
                Vector3 pos = grid.CellToWorld(new Vector3Int(x, y, 0));
                DrawHexagonAtPosition(pos, localVertices);
            }
        }
    }

    private Vector3[] GetHexagonLocalVertices(float radius, bool flatTopped)
    {
        Vector3[] vertices = new Vector3[6];
        float angleOffset = flatTopped ? 0f : (Mathf.PI / 6f);
        for (int i = 0; i < 6; i++)
        {
            float angle = (i * Mathf.PI * 2f / 6f) + angleOffset;
            float calcX = Mathf.Cos(angle) * radius;
            float calcZ = Mathf.Sin(angle) * radius;
            vertices[i] = new Vector3(calcX, 0, calcZ);
        }

        return vertices;
    }

    private void DrawHexagonAtPosition(Vector3 center, Vector3[] localVertices)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 currentPoint = center + localVertices[i];
            Vector3 nextPoint = center + localVertices[(i + 1) % 6];
            Gizmos.DrawLine(currentPoint, nextPoint);
        }
    }
}
