using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EasyButtons;

public class ClockwiseRingGenerator : MonoBehaviour
{
    [Header("Settings")]
    public CustomeGrid gridPrefab;
    public float spacing = 1.1f;
    public float delayBetweenCubes = 0.02f;

    [Header("Inner Space Settings")]
    public int innerWidth = 1;
    public int innerHeight = 1;
    public int targetLayer = 1;

    private int currentMaxLayer = 0;
    private bool isGenerating = false;
    private int globalIndexCounter = 0;

    private Dictionary<Vector2, CustomeGrid> gridLookup = new Dictionary<Vector2, CustomeGrid>();
    public List<CustomeGrid> spawnedCubes = new List<CustomeGrid>();

    public SplineGenerator splineGen;
    public TrainSplineDriver trainSplineDriver;
    public GameManager gameManager;




    [Button]
    public void Generate()
    {
        if (!isGenerating) StartCoroutine(BuildLayersSequentially());
    }

    [Button]
    public void ClearRings()
    {
        StopAllCoroutines();
        isGenerating = false;
        foreach (var cube in spawnedCubes)
        {
            if (cube != null)
            {
                if (Application.isPlaying) Destroy(cube.gameObject);
                else DestroyImmediate(cube.gameObject);
            }
        }
        spawnedCubes.Clear();
        gridLookup.Clear();
        currentMaxLayer = 0;
        globalIndexCounter = 0;
    }

    IEnumerator BuildLayersSequentially()
    {
        isGenerating = true;
        int offset = Mathf.Max(innerWidth, innerHeight) + (targetLayer - 1);

        while (currentMaxLayer < targetLayer)
        {
            currentMaxLayer++;
            yield return StartCoroutine(BuildSingleLayerClockwise(currentMaxLayer, offset));
        }

        isGenerating = false;
        // gameManager.SaveGrid("level_01");
        Debug.Log("Generation Complete. Ab aap Grids ko 'isClear' mark karke Spline generate kar sakte hain.");
    }

    IEnumerator BuildSingleLayerClockwise(int layer, int offset)
    {
        int xE = innerWidth + (layer - 1);
        int zE = innerHeight + (layer - 1);

        for (int x = -xE; x <= xE; x++) yield return StartCoroutine(SpawnWithDelay(x, zE, layer, offset));
        for (int z = zE - 1; z >= -zE; z--) yield return StartCoroutine(SpawnWithDelay(xE, z, layer, offset));
        for (int x = xE - 1; x >= -xE; x--) yield return StartCoroutine(SpawnWithDelay(x, -zE, layer, offset));
        for (int z = -zE + 1; z <= zE - 1; z++) yield return StartCoroutine(SpawnWithDelay(-xE, z, layer, offset));
    }

    IEnumerator SpawnWithDelay(int x, int z, int layer, int offset)
    {
        int posX = x + offset;
        int posZ = z + offset;

        Vector3 pos = new Vector3(posX * spacing, 0, posZ * spacing);
        CustomeGrid newGrid = Instantiate(gridPrefab, pos, Quaternion.identity, transform);

        newGrid.gridPosition = new Vector2(posX, posZ);
        newGrid.isOuterBoundary = (layer == targetLayer);
        newGrid.gridLayerInt = layer;
        newGrid.objectIndex = globalIndexCounter++;
        newGrid.name = $"Grid_{posX}_{posZ}";
        newGrid.splineGenerator = splineGen;
        newGrid.trainSplineDriver = trainSplineDriver;
        newGrid.currentHealth = newGrid.maxHealth;
        // newGrid.gameObject.layer = 7; //grid layer on 7 index //asign direct on prefab

        spawnedCubes.Add(newGrid);
        gridLookup[newGrid.gridPosition] = newGrid;

        AssignNeighbors(newGrid, posX, posZ);

        // if (gameManager != null)
        // {
        //     gameManager.gridSaveData ??= new();
        //     gameManager.gridSaveData.AddData(GetSerializableData(newGrid));
        // }


        yield return new WaitForSeconds(delayBetweenCubes);
    }

    private void AssignNeighbors(CustomeGrid current, int x, int z)
    {
        Vector2[] dirs = { new Vector2(x + 1, z), new Vector2(x - 1, z), new Vector2(x, z + 1), new Vector2(x, z - 1) };
        for (int i = 0; i < dirs.Length; i++)
        {
            if (gridLookup.TryGetValue(dirs[i], out CustomeGrid n))
            {
                if (i == 0) { current.rightGrid = n; n.leftGrid = current; }
                else if (i == 1) { current.leftGrid = n; n.rightGrid = current; }
                else if (i == 2) { current.topGrid = n; n.bottomGrid = current; }
                else if (i == 3) { current.bottomGrid = n; n.topGrid = current; }
            }
        }
    }

    // public GridCellData GetSerializableData(CustomeGrid cell)
    // {
    //     GridCellData data = new GridCellData
    //     {
    //         // 1. Assign Basic Variables
    //         meshType = cell.meshType,
    //         isClear = cell.isClear,
    //         isUsable = cell.isUsable,
    //         isOuterBoundary = cell.isOuterBoundary,
    //         isPermanentlyDisabled = cell.isPermanentlyDisabled,
    //         gridLayerInt = cell.gridLayerInt,
    //         gridPosition = cell.gridPosition, // Vector2
    //         objectIndex = cell.objectIndex,
    //         maxHealth = cell.maxHealth,
    //         currentHealth = cell.currentHealth,

    //         // 2. Assign Neighbors
    //         // Assuming neighbors are simply +1 or -1 from current position
    //         neighbors = new NeighborData
    //         {
    //             left = cell.leftGrid.gridPosition,
    //             right = cell.rightGrid.gridPosition,
    //             top = cell.topGrid.gridPosition,
    //             bottom = cell.bottomGrid.gridPosition
    //         }
    //     };

    //     return data;
    // }
}