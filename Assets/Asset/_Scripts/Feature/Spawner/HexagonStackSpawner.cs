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
    [SerializeField] private Hexagon hexagonPrefab;
    [MinMaxSlider(1, 10),SerializeField] private Vector2Int spawnRange;
    
    [Button]
    private void Spawn()
    {
        foreach (var point in spawnPoint)
        {
            if(point.childCount > 0) continue;
            SpawnHexagonStack(point);
        }
    }
    //test
    private void Update()
    {
        Spawn();
    }

    private void SpawnHexagonStack(Transform target)
    {
        HexagonStack hexagonStack = Instantiate<HexagonStack>(hexagonStackPrefab,target.position,Quaternion.identity);
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
                hexagon.render.SetPosition(hexagonStack.GetTopPosition());
                hexagonStack.AddElement(hexagon);
            }
            else
            {
                Hexagon hexagon = SpawnHexagon(target, colorHolder[1]);
                hexagon.SetParent(hexagonStack.transform);
                hexagon.render.SetPosition(hexagonStack.GetTopPosition());
                hexagonStack.AddElement(hexagon);
            }
        }
    }

    private Hexagon SpawnHexagon(Transform target,Color color)
    {
        Hexagon hexa = Instantiate(hexagonPrefab, target.position, Quaternion.identity);
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