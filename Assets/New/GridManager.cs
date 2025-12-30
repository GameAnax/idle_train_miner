using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    public GameObject gridPrefab;
    public int rows = 10;
    public int cols = 15;
    public float cellSize = 1.1f;
    public List<CustomeGrid> allGrids = new List<CustomeGrid>();

    public static GridManager Instance;
    private Dictionary<int, List<CustomeGrid>> layersDict = new Dictionary<int, List<CustomeGrid>>();


    void Awake() { Instance = this; }

    public void RegisterGrid(CustomeGrid grid)
    {
        if (!layersDict.ContainsKey(grid.gridLayerInt))
            layersDict[grid.gridLayerInt] = new List<CustomeGrid>();
        layersDict[grid.gridLayerInt].Add(grid);
    }
    public bool IsLayerFullyClear(int layerIdx)
    {
        if (!layersDict.ContainsKey(layerIdx)) return true; // Agar layer hi nahi hai, toh clear maano

        foreach (var g in layersDict[layerIdx])
        {
            if (!g.isClear) return false; // Agar ek bhi grid clear nahi hai, toh layer incomplete hai
        }
        return true;
    }
    // Editor mein grid banane ke liye
    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        ClearGrid();
        CustomeGrid[,] gridArray = new CustomeGrid[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = new Vector3(c * cellSize, 0, r * cellSize);
                GameObject obj = Instantiate(gridPrefab, pos, Quaternion.identity, transform);
                CustomeGrid cg = obj.GetComponent<CustomeGrid>();

                cg.gridPosition = new Vector2(c, r);
                cg.objectIndex = (r * cols) + c; // Unique ID
                cg.name = $"Grid_{c}_{r}";

                gridArray[r, c] = cg;
                allGrids.Add(cg);
            }
        }

        // Neighbors connect karna
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (r > 0) gridArray[r, c].bottomGrid = gridArray[r - 1, c];
                if (r < rows - 1) gridArray[r, c].topGrid = gridArray[r + 1, c];
                if (c > 0) gridArray[r, c].leftGrid = gridArray[r, c - 1];
                if (c < cols - 1) gridArray[r, c].rightGrid = gridArray[r, c + 1];
            }
        }
    }

    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        allGrids.Clear();
    }
}