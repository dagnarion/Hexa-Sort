using UnityEngine;

public static class Direction
{
    private static Vector2Int[] oddRow = new Vector2Int[]
    {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1)
    };

    private static Vector2Int[] evenRow = new Vector2Int[]
    {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1)
    };
    public static Vector2Int[] GetDirections(Vector2Int position)
    {
        bool isOddRow = Mathf.Abs(position.y) % 2 == 1;

        if (isOddRow)
        {
            return oddRow;
        }
        return evenRow;
    }
}