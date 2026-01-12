using System;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TrainManager trainManager;
    public ClockwiseRingGenerator clockwiseRingGenerator;
    public UIHandler uIHandler;
    public CrusherArea crusherArea;

    public List<BoggyConfig> boggyConfigs;
    public List<BoggyData> boggyDatas = new();
    public BoggyData currentBoggyData = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        if (boggyDatas.Count == 0)
        {
            int index = boggyDatas.Count;
            int boggyConfigsCount = boggyConfigs.Count;
            if (index > boggyConfigsCount)
            {
                Debug.Log("Max Level reached");
                return;
            }
        }

        foreach (var item in boggyDatas)
        {
            if (!item.isLimitReached) currentBoggyData = item;
        }
        if (currentBoggyData == null)
        {
            BoggyConfig bogeyConfig = boggyConfigs[0];
            currentBoggyData = new(maxBoggyCount: bogeyConfig.maxBoggy, currentBoggyCount: bogeyConfig.totalSpawn, boggyType: bogeyConfig.boggyType, isLimitReached: false, boggyDamage: bogeyConfig.boggyDamage);
            boggyDatas.Add(currentBoggyData);
        }
    }

    [Button]
    public void ADDBoggy()
    {
        if (currentBoggyData == null)
        {
            Debug.Log("Current Boggy Data is null");
            return;
        }
        trainManager.boggyAddCount += 1;
        trainManager.UpdateBoggyAddCost();
        currentBoggyData.UpdateData(1);
        trainManager.SpawnBoggy(currentBoggyData.boggyType);

        if (currentBoggyData.isLimitReached)
        {
            int index = boggyDatas.Count; //for get next index
            index = Mathf.Min(boggyConfigs.Count - 1, index);
            //TODO:- Assign next level boggy
            BoggyConfig bogeyConfig = boggyConfigs[index];
            currentBoggyData = new(maxBoggyCount: bogeyConfig.maxBoggy, currentBoggyCount: bogeyConfig.totalSpawn, boggyType: bogeyConfig.boggyType, isLimitReached: false, boggyDamage: bogeyConfig.boggyDamage);
            boggyDatas.Add(currentBoggyData);
        }
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
    }
    public void IncreaseTrainSpeed()
    {
        trainManager.trainSpeedConfig.UpdateSpeed();
        trainManager.trainSplineDriver.UpdateSpeed(0.5f);
    }
    public void UpdateStorageCapacity()
    {
        trainManager.storageBoggy.UpdateStorage();
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
}