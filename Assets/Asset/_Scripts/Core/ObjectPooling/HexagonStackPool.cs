using UnityEngine;
[CreateAssetMenu(menuName = "Pool/HexagonStack")]
public class HexagonStackPool : ComponentPoolSO<HexagonStack>
{
    protected override void OnTakeFromPool(HexagonStack instance)
    {
        instance.SetPool(this);
        instance.Clear();
        base.OnTakeFromPool(instance);
    }
}