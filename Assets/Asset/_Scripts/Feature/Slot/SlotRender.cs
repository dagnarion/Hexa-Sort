using UnityEngine;

public class SlotRender : MonoBehaviour
{
    [SerializeField] private MeshRenderer render;
    [SerializeField] private Color hightLightColor;
    private Color baseColor;
    private Material materialInstance;

    private void Awake()
    {
        if (render != null)
        {
            materialInstance = render.material;
            baseColor = materialInstance.color;
        }
    }

    public void Selected()
    {
        if (materialInstance == null && render != null)
        {
            materialInstance = render.material;
            baseColor = materialInstance.color;
        }
        if (materialInstance != null)
            materialInstance.color = hightLightColor;
    }

    public void Deselected()
    {
        if (materialInstance == null && render != null)
        {
            materialInstance = render.material;
            baseColor = materialInstance.color;
        }
        if (materialInstance != null)
            materialInstance.color = baseColor;
    }

    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
            materialInstance = null;
        }
    }
}