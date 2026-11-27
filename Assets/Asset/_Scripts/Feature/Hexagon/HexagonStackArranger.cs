using UnityEngine;

public class HexagonStackArranger
{
    private HexagonStack hexagonStack;
    private const float Spacing = 0.2f;

    public HexagonStackArranger(HexagonStack hexagonStack)
    {
        this.hexagonStack = hexagonStack;
    }

    public static Vector3 GetLocalPosition(int index) => Vector3.up * (Spacing * index);

    public Vector3 GetTopPosition()
    {
        return hexagonStack.transform.TransformPoint(GetLocalPosition(hexagonStack.GetNumberOfElement()));
    }
}
