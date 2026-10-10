using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine; 
using Random = UnityEngine.Random;
public class HexagonStackSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoint;
    [SerializeField] private HexagonStack hexagonStackPrefab;
    [SerializeField] private ComponentPoolSO<Hexagon> HexagonPool;
    [SerializeField] private ComponentPoolSO<HexagonStack> HexagonStackPool;
    [SerializeField] private EventChannel<LevelData> LevelLoad;
    [MinMaxSlider(1, 10),SerializeField] private Vector2Int spawnRange;
    private Color[] color;
    private bool canSpawn;
    private bool isSpawning;
    private CancellationTokenSource spawnCts;

    private void OnEnable()
    {
        LevelLoad.OnEventRaise += Init;
    }

    private void OnDisable()
    {
        LevelLoad.OnEventRaise -= Init;
        CancelSpawn();
    }

    private void Init(LevelData data)
    {
        color = data.ColorPallet;
    }

    private void CancelSpawn()
    {
        if (spawnCts != null)
        {
            spawnCts.Cancel();
            spawnCts.Dispose();
            spawnCts = null;
        }
        isSpawning = false;
    }
    
    public async UniTask Spawn()
    {
        CancelSpawn();
        spawnCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        CancellationToken token = spawnCts.Token;
        isSpawning = true;

        try
        {
            for (int i = 0; i < spawnPoint.Length; i++)
            {
                if (token.IsCancellationRequested) return;
                SpawnHexagonStack(spawnPoint[i]);
                await UniTask.WaitForSeconds(0.1f, cancellationToken: token);
            }
        }
        catch (System.OperationCanceledException)
        {
        }
        finally
        {
            if (spawnCts != null && spawnCts.Token == token)
            {
                isSpawning = false;
            }
        }
    }

    public void Release()
    {
        CancelSpawn();
        foreach (var point in spawnPoint)
        {
            for (int i = point.childCount - 1; i >= 0; i--)
            {
                HexagonStack hexagonStack = point.GetChild(i).gameObject.GetComponent<HexagonStack>();
                if (hexagonStack != null)
                {
                    ReleaseStack(hexagonStack);
                }
            }
        }
    }


    private void ReleaseStack(HexagonStack hexagonStack)
    {
        hexagonStack.transform.DOKill();
        hexagonStack.transform.localScale = Vector3.one;

        for (int i = hexagonStack.GetNumberOfElement() - 1; i >= 0; i--)
        {
            Hexagon hexa = hexagonStack.GetElement(i);
            hexagonStack.RemoveElement(hexa);
            hexa.SetParent(null);
            HexagonPool.Release(hexa);
        }
        hexagonStack.transform.SetParent(null);
        HexagonStackPool.Release(hexagonStack);
    }
    
    private void Update()
    {
        if (!isSpawning && IsEmptyPoint() && !canSpawn) canSpawn = true;
        if (canSpawn)
        {
            canSpawn = false;
            Spawn().Forget();
        }
    }

    private bool IsEmptyPoint()
    {
        foreach (var point in spawnPoint)
        {
            if(point.childCount > 0) return false;
        }
        return true;
    }

    private void SpawnHexagonStack(Transform target)
    {
        HexagonStack hexagonStack = HexagonStackPool.Get();
        hexagonStack.transform.DOKill();
        hexagonStack.transform.localScale = Vector3.one;
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
               FillHexagonToHexagonStack(hexagonStack,target,colorHolder[0]);
            }
            else
            {
                FillHexagonToHexagonStack(hexagonStack,target,colorHolder[1]);
            }
        }

        hexagonStack.Render.Appear();
    }

    private void FillHexagonToHexagonStack(HexagonStack hexagonStack,Transform target,Color color)
    {
        Hexagon hexagon = SpawnHexagon(target, color);
        hexagon.SetParent(hexagonStack.transform);
        hexagon.transform.localScale = Vector3.one;
        hexagon.render.SetPosition(hexagonStack.Render.GetTopPosition());
        hexagonStack.AddElement(hexagon);
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