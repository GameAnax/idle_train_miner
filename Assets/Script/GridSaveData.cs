
using System;
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
    public List<Vector3> previewsSplinePoints = new();
    public List<Vector3> mainSplinePoints = new();
    public int totalBoggyAddCount = 1;
    public int meargeLevel = 1;
    public int trainSpeedLevel = 1;
    public int capacityLevel = 1;
    public int boggyConfigIndex;
    public int boggyCountCurrentConfig;
    public List<int> boggyLevel = new();
    public Vector2 lastGridPosition;

    public double trainSplinePercentage;
}




[System.Serializable]
public class DebriesDataForSave
{
    public List<DebriesData> debriesDatas = new();
}

public class DebriesData
{
    public Vector2 gridPosition;
    public Vector3 debriesPosition;
    public int damageValue;
    public int Multiplier;
    public string colorHex;
}



[System.Serializable]
public class PlayerDataForSave
{
    public PlayerData playerData = new();
}

[System.Serializable]
public class PlayerData
{
    public IdleCurrency collectedCoin;
    public float levelProgress;
}

[System.Serializable]
public class StorageBoggyDataForSave
{
    public int totalDebris;
    public IdleCurrency totalCapacity;
    public IdleCurrency totalCoins;
}