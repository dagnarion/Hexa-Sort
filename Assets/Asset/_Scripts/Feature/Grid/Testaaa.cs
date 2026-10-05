using System;
using UnityEngine;

public class Testaaa : MonoBehaviour
{
    [SerializeField] private Grid gridComponent;

    private void OnDrawGizmos()
    {
        if(gridComponent == null) return;
        for(int i = -10;i<=10;i++)
        for (int j = -10; j <= 10; j++)
        {
            Gizmos.DrawCube(gridComponent.CellToWorld(new Vector3Int(i,j,0)),new Vector3(1,1,1));
        }
    }
}
