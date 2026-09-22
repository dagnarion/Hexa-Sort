using UnityEngine;

public class Hexagon : MonoBehaviour
{
    public Color ColorType { get; private set; }
    [field:SerializeField] public HexagonRender render { get; private set; }

    public void SetParent(Transform parent) => transform.SetParent(parent);
    
    public void Init(Color color)
    {
        this.ColorType = color;
        render.color = color;
    }
}