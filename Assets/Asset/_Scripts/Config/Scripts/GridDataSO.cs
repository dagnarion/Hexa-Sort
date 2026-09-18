using UnityEngine;
[CreateAssetMenu(menuName = "Grid/GridDataSO")]
public class GridDataSO : ScriptableObject
{
    [field:SerializeField] public Vector2Int GridSize { get; private set; }
}