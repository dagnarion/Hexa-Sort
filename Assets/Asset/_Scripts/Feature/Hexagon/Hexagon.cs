using UnityEngine;

public class Hexagon : MonoBehaviour
{
    public Color32 ColorType { get; private set; }
    [SerializeField] private HexagonRender render;

    public void Init(Color32 color)
    {
        this.ColorType = color;
        render.color = color;
    }
}