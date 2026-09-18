using UnityEngine;

public class SlotRender : MonoBehaviour
{
    [SerializeField] private MeshRenderer render;
    [SerializeField] private Color hightLightColor;
    private Color baseColor;

    private void Start()
    {
        baseColor = render.material.color;
    }
        
    public void Selected() => render.material.color = hightLightColor;
    public void Deselected() => render.material.color = baseColor;
}