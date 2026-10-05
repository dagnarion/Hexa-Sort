using UnityEngine;

public static class Direction
{
    private static readonly Vector2Int[] evenCol = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, -1),
        new Vector2Int(1, 0),
        new Vector2Int(1, -1)
    };

    private static readonly Vector2Int[] oddCol = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(1, 0)
    };

    public static Vector2Int[] GetDirections(Vector2Int position)
    {
        bool isOddCol = Mathf.Abs(position.x) % 2 == 1;
        return isOddCol ? oddCol : evenCol;
    }
}