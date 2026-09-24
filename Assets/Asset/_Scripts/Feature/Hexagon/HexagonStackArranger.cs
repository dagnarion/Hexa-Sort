using UnityEngine;

public class HexagonStackArranger
{
    private HexagonStack hexagonStack;
    private float spacing = 0.2f;

    public HexagonStackArranger(HexagonStack hexagonStack)
    {
        this.hexagonStack = hexagonStack;
    }

    public Vector3 GetTopPosition()
    {
        if (hexagonStack.IsEmpty)
        {
            return hexagonStack.transform.position;
        }
        return hexagonStack.transform.position + Vector3.up * spacing * hexagonStack.GetNumberOfElement();
    }
}