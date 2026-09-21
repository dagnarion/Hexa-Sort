using UnityEngine;
[CreateAssetMenu(menuName = "EventChannel/DropEventChannel")]
public class DropEventChannel : EventChannel<(HexagonStack,Vector3)>
{
}