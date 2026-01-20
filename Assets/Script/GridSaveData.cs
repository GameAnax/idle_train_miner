
using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;



[System.Serializable]
public class GridSaveData
{
    public List<GridCellData> allCells = new List<GridCellData>();

    public void AddData(GridCellData gridCellData)
    {
        allCells.Add(gridCellData);
    }
}

[System.Serializable]
public class GridCellData
{
    public MeshType meshType; // Enum
    public bool isClear;
    public bool isUsable;
    public bool isOuterBoundary;
    public bool isPermanentlyDisabled;
    public int gridLayerInt;
    public Vector2 gridPosition;
    public int objectIndex;
    public float maxHealth;
    public float currentHealth;
    public int gpuMeshIndex;

    // Neighbor positions (Left, Right, Top, Bottom)
    // public NeighborData neighbors = new();
}

[System.Serializable]
public class NeighborData
{
    public Vector2 left;
    public Vector2 right;
    public Vector2 top;
    public Vector2 bottom;
}



[System.Serializable]
public class TrainSaveData
{
    public List<Vector3> previewsSplinePoints;
    public List<Vector3> mainSplinePoints;
    public int totalBoggyAddCount;
    public int meargeLevel;
    public int trainSpeedLevel;
    public int capacityLevel;
    public int boggyConfigIndex;
    public int boggyCountCurrentConfig;
    public List<int> boggyLevel;
    public Vector2 lastGridPosition;
}