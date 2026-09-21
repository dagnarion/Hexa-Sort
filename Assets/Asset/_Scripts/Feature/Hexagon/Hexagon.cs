using UnityEngine;

public class Hexagon : MonoBehaviour
{
    public Color ColorType { get; private set; }
    [SerializeField] public HexagonRender render;
    
    public void SetParent(Transform parent) => transform.SetParent(parent);
    
    public void Init(Color color)
    {
        this.ColorType = color;
        render.color = color;
    }
}