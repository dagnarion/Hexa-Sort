using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine; 
using Random = UnityEngine.Random;
public class HexagonStackSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoint;
    [SerializeField] private Color[] color;
    [SerializeField] private HexagonStack hexagonStackPrefab;
    [SerializeField] private ComponentPoolSO<Hexagon> HexagonPool;
    [SerializeField] private ComponentPoolSO<HexagonStack> HexagonStackPool;
    [MinMaxSlider(1, 10),SerializeField] private Vector2Int spawnRange;
    
    [Button]
    public void Spawn()
    {
        foreach (var point in spawnPoint)
        {
            if(point.childCount > 0) return;
        }

        foreach (var point in spawnPoint)
        {
            SpawnHexagonStack(point);
        }
    }

    public void Release()
    {
        foreach (var point in spawnPoint)
        {
            if(point.childCount <= 0) continue;
            HexagonStack hexagonStack = point.GetChild(0).gameObject.GetComponent<HexagonStack>();
            ReleaseStack(hexagonStack);
        }
    }


    private void ReleaseStack(HexagonStack hexagonStack)
    {
        for (int i = hexagonStack.GetNumberOfElement() - 1; i >= 0; i--)
        {
            Hexagon hexa = hexagonStack.GetElement(i);
            hexagonStack.RemoveElement(hexa);
            HexagonPool.Release(hexa);
        }
        hexagonStack.transform.SetParent(null);
        HexagonStackPool.Release(hexagonStack);
    }
    //test
    private void Update()
    {
        Spawn();
    }

    private void SpawnHexagonStack(Transform target)
    {
        HexagonStack hexagonStack = HexagonStackPool.Get();
        hexagonStack.transform.position = target.position;
        hexagonStack.Render.SetOriginPosition(target.position);
        hexagonStack.transform.SetParent(target);
        Color[] colorHolder = GetRandColour();
        int rand = Random.Range(spawnRange.x, spawnRange.y);
        int randColorRatio = Random.Range(spawnRange.x, rand);
        for (int i = 1; i <= rand; i++)
        {
            if (i < randColorRatio)
            {
                Hexagon hexagon = SpawnHexagon(target, colorHolder[0]);
                hexagon.SetParent(hexagonStack.transform);
                hexagon.render.SetPosition(hexagonStack.Render.GetTopPosition());
                hexagonStack.AddElement(hexagon);
            }
            else
            {
                Hexagon hexagon = SpawnHexagon(target, colorHolder[1]);
                hexagon.SetParent(hexagonStack.transform);
                hexagon.render.SetPosition(hexagonStack.Render.GetTopPosition());
                hexagonStack.AddElement(hexagon);
            }
        }
    }

    private Hexagon SpawnHexagon(Transform target,Color color)
    {
        Hexagon hexa = HexagonPool.Get();
        hexa.render.transform.rotation = Quaternion.Euler(0, 30, 0);
        hexa.Init(color);
        return hexa;
    }

    private Color[] GetRandColour()
    {
        List<Color> colors = new List<Color>();
        colors.AddRange(color);
        if (colors.Count <= 0)
        {
            Debug.LogError("There wasn't have color in holder");
            return null;
        }

        int index = Random.Range(0, colors.Count);
        Color firstcolor = colors[index];
        colors.RemoveAt(index);

        if (colors.Count <= 0)
        {
            Debug.LogError("There weren't have enough color");
            return null;
        }
        
        index = Random.Range(0, colors.Count);
        Color secondcolor = colors[index];
        colors.RemoveAt(index);
        
        return new Color[]{firstcolor,secondcolor};
    }

}