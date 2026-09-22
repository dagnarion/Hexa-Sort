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
        Vector3 pos;
        if (hexagonStack.IsEmpty)
        {
            pos = hexagonStack.transform.position;
        }
        else
        {
            Hexagon hexagon = hexagonStack.GetTopElement();
            pos = hexagon.transform.position.With(y: hexagon.transform.position.y + spacing * hexagonStack.GetNumberOfElement());
        }
        return pos;
    }

}