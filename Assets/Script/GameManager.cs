using System;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TrainManager trainManager;

    public List<BogeyConfig> boggyConfigs;
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
            BogeyConfig bogeyConfig = boggyConfigs[0];
            currentBoggyData = new(maxBoggyCount: bogeyConfig.maxBoggy, currentBoggyCount: bogeyConfig.totalSpawn, boggyType: bogeyConfig.boggyType, isLimitReached: false, boggyDamage: bogeyConfig.boggyDamage);
            boggyDatas.Add(currentBoggyData);
        }
    }

    [Button]
    private void ADDBoggy()
    {
        if (currentBoggyData == null)
        {
            Debug.Log("Current Boggy Data is null");
            return;
        }
        currentBoggyData.UpdateData(1);
        trainManager.SpawnBoggy(currentBoggyData.boggyType);

        if (currentBoggyData.isLimitReached)
        {
            int index = boggyDatas.Count; //for get next index
            index = Mathf.Min(boggyConfigs.Count - 1, index);
            //TODO:- Assign next level boggy
            BogeyConfig bogeyConfig = boggyConfigs[index];
            currentBoggyData = new(maxBoggyCount: bogeyConfig.maxBoggy, currentBoggyCount: bogeyConfig.totalSpawn, boggyType: bogeyConfig.boggyType, isLimitReached: false, boggyDamage: bogeyConfig.boggyDamage);
            boggyDatas.Add(currentBoggyData);
        }
    }
    [Button]
    private void MeargeBoggy()
    {
        List<Boggy> boggies = trainManager.trainSplineDriver.boggies;
        bool hasMerged;
        do
        {
            hasMerged = false;
            for (int i = 0; i < boggies.Count - 1; i++)
            {
                // if(i == 0)
                Boggy first = boggies[i];
                Boggy second = boggies[i + 1];

                if (first.boggyType == second.boggyType)
                {
                    first.UpdateBoggy();
                    boggies.RemoveAt(i + 1);
                    second.DestroyObj();

                    hasMerged = true;
                    break;
                }
            }
        }
        while (hasMerged);
        for (int i = 0; i < boggies.Count; i++)
        {
            boggies[i].index = i;
        }
    }
}


[Serializable]
public class BoggyData
{
    public int maxBoggyCount;
    public int currentBoggyCount;
    public BoggyType boggyType;
    public bool isLimitReached;
    public float boggyDamage;

    public BoggyData()
    { }
    public BoggyData(int maxBoggyCount, int currentBoggyCount, BoggyType boggyType, bool isLimitReached, float boggyDamage)
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