using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyButtons;
using Newtonsoft.Json;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TrainManager trainManager;
    public ClockwiseRingGenerator clockwiseRingGenerator;
    public UIHandler uIHandler;
    public CrusherArea crusherArea;

    public List<BoggyConfig> boggyConfigs;
    // public List<BoggyData> boggyDatas = new();
    public BoggyData currentBoggyData = null;
    public List<Debries> debriesList = new();
    public DebriesDataForSave debriesDataForSave = new();

    private Dictionary<Vector2, List<DebriesData>> _debrisLookup = new();


    private string GetPath(string fileName) => Path.Combine(Application.persistentDataPath, fileName + ".json");
    public GridSaveData gridSaveData;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void OnDisable()
    {
        SaveGrid("level_01");

        debriesDataForSave.debriesDatas.Clear();
        foreach (Debries debries in debriesList)
        {
            debriesDataForSave.debriesDatas.Add(new()
            {
                gridPosition = debries.gridPostion,
                debriesPosition = debries.currentPosition,
                damageValue = debries.debriCapacity,
                // color = debries.GetColor
            });
        }
        SaveDebriData();
    }
    void Start()
    {
        // if (boggyDatas.Count == 0)
        // {
        //     int index = boggyDatas.Count;
        //     int boggyConfigsCount = boggyConfigs.Count;
        //     if (index > boggyConfigsCount)
        //     {
        //         Debug.Log("Max Level reached");
        //         return;
        //     }
        // }

        // foreach (var item in boggyDatas)
        // {
        //     if (!item.isLimitReached) currentBoggyData = item;
        // }
        // if (currentBoggyData == null)
        // {
        //     BoggyConfig bogeyConfig = boggyConfigs[0];
        //     currentBoggyData = new(maxBoggyCount: bogeyConfig.maxBoggy, currentBoggyCount: bogeyConfig.totalSpawn, boggyType: bogeyConfig.boggyType, isLimitReached: false, boggyDamage: bogeyConfig.boggyDamage);
        //     // boggyDatas.Add(currentBoggyData);
        // }

        bool isAvailable = IsPathAvailable("level_01");
        if (!isAvailable)
        {
            gridSaveData ??= new();
            foreach (var item in clockwiseRingGenerator.spawnedCubes)
            {

                gridSaveData.AddData(GetSerializableData(item));
            }
            SaveGrid("level_01");
        }
        else
        {
            LoadGrid(clockwiseRingGenerator.spawnedCubes, "level_01");
        }
    }
    public void SetCurrentBoggyConfigOnStart()
    {
        BoggyConfig bogeyConfig = boggyConfigs[trainManager.trainSaveData.boggyConfigIndex];
        currentBoggyData = new(maxBoggyCount: bogeyConfig.maxBoggy, currentBoggyCount: bogeyConfig.totalSpawn, boggyType: bogeyConfig.boggyType, isLimitReached: false, boggyDamage: bogeyConfig.boggyDamage);
    }

    [Button]
    public void ADDBoggy()
    {
        if (currentBoggyData == null)
        {
            Debug.Log("Current Boggy Data is null");
            return;
        }
        trainManager.AddBoggy();
        // trainManager.boggyAddCount += 1;
        // trainManager.UpdateBoggyAddCost();
        // currentBoggyData.UpdateData(1);
        // trainManager.SpawnBoggy();

        // if (currentBoggyData.isLimitReached)
        // {
        //     int index = trainManager.trainSaveData.boggyConfigIndex + 1; //for get next index
        //     index = Mathf.Min(boggyConfigs.Count - 1, index);
        //     trainManager.trainSaveData.boggyConfigIndex = index;
        //     //TODO:- Assign next level boggy
        //     BoggyConfig bogeyConfig = boggyConfigs[index];
        //     currentBoggyData = new(maxBoggyCount: bogeyConfig.maxBoggy, currentBoggyCount: bogeyConfig.totalSpawn, boggyType: bogeyConfig.boggyType, isLimitReached: false, boggyDamage: bogeyConfig.boggyDamage);
        //     // boggyDatas.Add(currentBoggyData);
        // }
    }
    [Button]
    public void MeargeBoggy()
    {
        List<Boggy> boggies = trainManager.trainSplineDriver.boggies;
        for (int i = 0; i < boggies.Count - 1; i++)
        {
            Boggy first = boggies[i];
            if (first.boggyType == BoggyType.Storage || first.boggyType == BoggyType.TrackUpdate) continue;
            Boggy second = boggies[i + 1];
            if (second.boggyType == BoggyType.Storage || second.boggyType == BoggyType.TrackUpdate) continue;

            // if (first.boggyType == second.boggyType)
            if (first.boggyLevel == second.boggyLevel)
            {
                first.UpdateBoggy();
                boggies.RemoveAt(i + 1);
                second.DestroyObj();
                break;
            }
        }
        for (int i = 0; i < boggies.Count; i++)
        {
            boggies[i].index = i;
        }
        trainManager.trainMeargeConfig.UpdateMearge();
        trainManager.SetMeargeLevelForSave();

        uIHandler.SetUpMeargeText();
    }
    public void IncreaseTrainSpeed()
    {
        trainManager.trainSpeedConfig.UpdateSpeed();
        trainManager.trainSplineDriver.UpdateSpeed(0.5f);
        trainManager.SetSpeedLevelForSave();

        uIHandler.SetUpSpeedText();
    }
    public void UpdateStorageCapacity()
    {
        trainManager.storageBoggy.UpdateStorage();
        trainManager.trainSaveData.capacityLevel = trainManager.storageBoggy.storageBoggyConfig.level;
    }
    public void CheckIsAllGridClear()
    {
        bool isLevelFinished = clockwiseRingGenerator.spawnedCubes.All(g => g.isClear);
        if (isLevelFinished)
        {
            Debug.Log($"Level Complte");
        }
        UpdateProgress();
    }

    public void SaveGrid(string fileName)
    {
        // Settings to ensure Vector2 and Enums save cleanly
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        string json = JsonConvert.SerializeObject(gridSaveData, settings);
        File.WriteAllText(GetPath(fileName), json);
        Debug.Log("save success");
    }
    public void SaveDebriData()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        string json = JsonConvert.SerializeObject(debriesDataForSave, settings);
        File.WriteAllText(GetPath("debries"), json);
        Debug.Log("save success debries data");
    }
    public void LoadDebriData()
    {
        string path = GetPath("debries");
        if (!File.Exists(path))
        {
            Debug.LogError("Debries Not Found");
            return;
        }
        string json = File.ReadAllText(path);
        debriesDataForSave = JsonConvert.DeserializeObject<DebriesDataForSave>(json);
    }
    public void LoadAndPopulateDictionary()
    {
        LoadDebriData(); // Your existing load method

        // 1. Clear existing data without destroying the dictionary object
        _debrisLookup.Clear();

        // 2. Cache the list reference to avoid repeated property access
        var dataList = debriesDataForSave.debriesDatas;
        int count = dataList.Count;

        // 3. Use a for loop (slightly faster than foreach in some Unity versions)
        for (int i = 0; i < count; i++)
        {
            Vector2 gridPos = dataList[i].gridPosition;
            DebriesData debriesData = dataList[i];

            if (_debrisLookup.TryGetValue(gridPos, out List<DebriesData> list))
            {
                list.Add(debriesData);
            }
            else
            {
                // Only allocate a new list if the key is unique
                _debrisLookup[gridPos] = new List<DebriesData> { debriesData };
            }
        }
    }

    public bool IsPathAvailable(string fileName)
    {
        string path = GetPath(fileName);
        if (!File.Exists(path))
        {
            Debug.LogError("Level Not Found");
            return false;
        }
        return true;
    }
    public void LoadGrid(List<CustomeGrid> sceneScripts, string levelName)
    {
        string path = GetPath(levelName);
        if (!File.Exists(path))
        {
            Debug.LogError("Level Not Found");
            return;
        }

        var scriptLookup = sceneScripts.ToDictionary(s => s.gridPosition);

        string json = File.ReadAllText(path);
        gridSaveData = JsonConvert.DeserializeObject<GridSaveData>(json);

        LoadAndPopulateDictionary();

        foreach (GridCellData data in gridSaveData.allCells)
        {
            if (scriptLookup.TryGetValue(data.gridPosition, out CustomeGrid targetScript))
            {
                targetScript.meshType = data.meshType;
                targetScript.isClear = data.isClear;
                targetScript.isUsable = data.isUsable;
                targetScript.isOuterBoundary = data.isOuterBoundary;
                targetScript.isPermanentlyDisabled = data.isPermanentlyDisabled;
                targetScript.gridLayerInt = data.gridLayerInt;
                targetScript.gridPosition = data.gridPosition;
                targetScript.objectIndex = data.objectIndex;
                targetScript.maxHealth = data.maxHealth;
                targetScript.currentHealth = data.currentHealth;
                targetScript.gridCellDataForSave = data;

                if (data.currentHealth <= 0)
                {
                    GridRenderManager.instance.HideMesh(meshType: data.meshType, gpuMeshIndex: data.gpuMeshIndex);
                }

                if (_debrisLookup.TryGetValue(data.gridPosition, out List<DebriesData> debrisPositions))
                {
                    targetScript.GenerateDebriesOnLevelStart(debrisPositions);
                }
            }
        }
        clockwiseRingGenerator.splineGen.GenerateSpline();
        trainManager.trainSplineDriver.modularGridAligner.StartGeneration();
        trainManager.trainLoopHandler.UpdateSplineOnStart();
    }
    public GridCellData GetSerializableData(CustomeGrid cell)
    {
        GridCellData data = new GridCellData
        {
            // 1. Assign Basic Variables
            meshType = cell.meshType,
            isClear = cell.isClear,
            isUsable = cell.isUsable,
            isOuterBoundary = cell.isOuterBoundary,
            isPermanentlyDisabled = cell.isPermanentlyDisabled,
            gridLayerInt = cell.gridLayerInt,
            gridPosition = cell.gridPosition, // Vector2
            objectIndex = cell.objectIndex,
            maxHealth = cell.maxHealth,
            currentHealth = cell.currentHealth,
            gpuMeshIndex = cell.gpuMeshIndex,

            // 2. Assign Neighbors
            // Assuming neighbors are simply +1 or -1 from current position
            // neighbors = new NeighborData()
            // {
            //     left = cell.leftGrid != null ? cell.leftGrid.gridPosition : Vector2.zero,
            //     right = cell.rightGrid.gridPosition,
            //     top = cell.topGrid.gridPosition,
            //     bottom = cell.bottomGrid.gridPosition
            // }
        };
        cell.gridCellDataForSave = data;
        return data;
    }

    private void UpdateProgress()
    {
        if (clockwiseRingGenerator.spawnedCubes.Count == 0) return;

        float clearedCount = clockwiseRingGenerator.spawnedCubes.Count(g => g.isClear);
        float totalCount = clockwiseRingGenerator.spawnedCubes.Count;

        // Percentage Formula: (Cleared / Total) * 100
        float progressPercent = (clearedCount / totalCount) * 100f;

        //TODO:- UI Update
        uIHandler.UpdateLevelProgress(progressPercent);

    }
}


[Serializable]
public class BoggyData
{
    public int maxBoggyCount;
    public int currentBoggyCount;
    public BoggyType boggyType;
    public bool isLimitReached;
    public IdleCurrency boggyDamage;

    public BoggyData()
    { }
    public BoggyData(int maxBoggyCount, int currentBoggyCount, BoggyType boggyType, bool isLimitReached, IdleCurrency boggyDamage)
    {
        this.maxBoggyCount = maxBoggyCount;
        this.currentBoggyCount = currentBoggyCount;
        this.boggyType = boggyType;
        this.isLimitReached = isLimitReached;
        this.boggyDamage = boggyDamage;
    }

    public void UpdateData(int increaseAmount)
    {
        currentBoggyCount += increaseAmount;
        if (currentBoggyCount == maxBoggyCount)
        {
            isLimitReached = true;
        }
    }

    public float GetProgress()
    {
        return (float)currentBoggyCount / maxBoggyCount;
    }
}