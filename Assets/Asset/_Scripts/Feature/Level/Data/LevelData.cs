using System;
using UnityEngine;

[Serializable]
public struct LevelData
{
   [field:SerializeField] public LevelDataSO Level { get; private set; }
   [field:SerializeField] public int Reward { get; private set; }
   [field:SerializeField] public int CollectHexagonTarget { get; private set; }
   [field:SerializeField] public int BreakBlockTarget { get; private set; }
}