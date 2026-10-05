using UnityEngine;

public class Hexagon : MonoBehaviour
{
    public Color ColorType { get; private set; }
    [field:SerializeField] public HexagonRender render { get; private set; }
    [SerializeField] private ComponentPoolSO<Hexagon> hexagonPool;
    [SerializeField] private Collider hexagonCollider;
    public void SetParent(Transform parent) => transform.SetParent(parent);
    
    public void Init(Color color)
    {
        this.ColorType = color;
        render.color = color;
        render.Init();
        CanSelect();
    }
    public void UnSelect() => hexagonCollider.enabled = false;
    public void CanSelect() => hexagonCollider.enabled = true;

    public void ReleaseHexagon()
    {
        hexagonPool.Release(this);
    }
}