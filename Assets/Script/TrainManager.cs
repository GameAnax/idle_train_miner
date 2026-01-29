using System.Collections.Generic;
using System.IO;
using EasyButtons;
using Newtonsoft.Json;
using UnityEditor.Overlays;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    public TrainSplineDriver trainSplineDriver;
    public TrainLoopHandler trainLoopHandler;
    public StorageBoggy storageBoggy;
    public TrainSpeedConfig trainSpeedConfig;
    public TrainMeargeConfig trainMeargeConfig;

    public Boggy boggyPrefab;
    public Transform boggyParent;
    public int maxBoggyCount = 12;

    public int boggyAddCount = 1; //continue increase no reset
    public IdleCurrency boggyAddCost = 5;

    public TrainSaveData trainSaveData;



    private bool isEnoughMoneyForAddBoggy = false;
    public bool IsEnoughMoneyForADDBoggy => isEnoughMoneyForAddBoggy;

    private bool isSpawnBoggyLimitReached = false;
    public bool IsSpawnBoggyLimitReached => isSpawnBoggyLimitReached;

    private bool isEnoughMoneyForMeargeBoggy = false;
    public bool IsEnoughMoneyForMeargeBoggy => isEnoughMoneyForMeargeBoggy;
    private bool isSameBoggyForMearge = false;
    public bool IsSameBoggyForMearge => isSameBoggyForMearge;







    void Awake()
    {
        trainSpeedConfig = Instantiate(trainSpeedConfig);
        trainMeargeConfig = Instantiate(trainMeargeConfig);
    }


    private string train_data_key => $"{HomeScene.instance.GetCurrentLevelID}_trainData";
    private string GetPath(string fileName) => Path.Combine(Application.persistentDataPath, fileName + ".json");

    public void AddBoggy()
    {
        if (!IsEnoughMoneyForADDBoggy) //check last time status
        {
            CheckNextBoggyADD(); //check current status
            if (!IsEnoughMoneyForADDBoggy) return;
        }
        if (IsSpawnBoggyLimitReached) return;

        GameManager.instance.DeductCoinAndCheck(boggyAddCost);
        boggyAddCount += 1;
        trainSaveData.totalBoggyAddCount = boggyAddCount;
        UpdateBoggyAddCost();
        GameManager.instance.currentBoggyData.UpdateData(1);
        trainSaveData.boggyCountCurrentConfig = GameManager.instance.currentBoggyData.currentBoggyCount;
        SpawnBoggy();

        if (GameManager.instance.currentBoggyData.isLimitReached)
        {
            int index = trainSaveData.boggyConfigIndex + 1; //for get next index
            index = Mathf.Min(GameManager.instance.boggyConfigs.Count - 1, index);
            trainSaveData.boggyConfigIndex = index;
            //TODO:- Assign next level boggy
            BoggyConfig bogeyConfig = GameManager.instance.boggyConfigs[index];
            GameManager.instance.currentBoggyData = new(maxBoggyCount: bogeyConfig.maxBoggy, currentBoggyCount: bogeyConfig.totalSpawn, boggyType: bogeyConfig.boggyType, isLimitReached: false, boggyDamage: bogeyConfig.boggyDamage);
            // boggyDatas.Add(currentBoggyData);
        }
    }
    [Button]
    public void SpawnBoggy()
    {
        if (isSpawnBoggyLimitReached)
        {
            Debug.Log("Boggy Limit Reaced");
            return;
        }

        int totalBoggy = trainSplineDriver.boggies.Count;
        int boggyIndex = totalBoggy - 1;

        Boggy newBoggy = Instantiate(boggyPrefab, boggyParent);
        newBoggy.SetBoggyData(boggyIndex: boggyIndex);
        newBoggy.boggyDamage.damageValue = GameManager.instance.currentBoggyData.boggyDamage;
        newBoggy.boggyLevel = trainSaveData.boggyConfigIndex;
        newBoggy.SetBoggy(trainSaveData.boggyConfigIndex);
        //TODO:- Set boggy Data
        trainSplineDriver.boggies.Insert(boggyIndex, newBoggy);

        //TODO:- Update Dummy Boggy Index
        trainSplineDriver.boggies[^1].SetBoggyData(boggyIndex: boggyIndex + 1);

        if (trainSplineDriver.boggies.Count == maxBoggyCount)
        {
            isSpawnBoggyLimitReached = true;
            GameManager.instance.uIHandler.UpdateADDBoggyButtonUI();
            Debug.Log("Boggy Spawn Limit Reaced");
        }
        CheckIsMeargePossible();
    }
    public void SpawnBoggy(int boggyLevel)
    {
        if (maxBoggyCount == trainSplineDriver.boggies.Count)
        {
            Debug.Log("Boggy Limit Reaced");
            return;
        }

        int totalBoggy = trainSplineDriver.boggies.Count;
        int boggyIndex = totalBoggy - 1;

        Boggy newBoggy = Instantiate(boggyPrefab, boggyParent);
        newBoggy.SetBoggyData(boggyIndex: boggyIndex);
        newBoggy.boggyLevel = boggyLevel;
        newBoggy.SetBoggy(boggyLevel);
        //TODO:- Set boggy Data
        trainSplineDriver.boggies.Insert(boggyIndex, newBoggy);

        //TODO:- Update Dummy Boggy Index
        trainSplineDriver.boggies[^1].SetBoggyData(boggyIndex: boggyIndex + 1);

        if (trainSplineDriver.boggies.Count == maxBoggyCount)
        {
            Debug.Log("Boggy Limit Reaced");
        }
        CheckIsMeargePossible();
    }

    public void CheckIsMeargePossible()
    {
        bool checkPossible = false;
        List<Boggy> boggies = trainSplineDriver.boggies;

        int boggiesCount = boggies.Count;
        isSpawnBoggyLimitReached = boggiesCount == maxBoggyCount;



        for (int i = 0; i < boggiesCount - 1; i++)
        {
            Boggy first = boggies[i];
            if (first.boggyType == BoggyType.Storage || first.boggyType == BoggyType.TrackUpdate) continue;
            Boggy second = boggies[i + 1];
            if (second.boggyType == BoggyType.Storage || second.boggyType == BoggyType.TrackUpdate) continue;

            // if (first.boggyType == second.boggyType)
            if (first.boggyLevel == second.boggyLevel)
            {
                //TODO:- mearge possible
                checkPossible = true;
                break;
            }
        }
        // if (isSameBoggyForMearge != checkPossible)
        // {
        isSameBoggyForMearge = checkPossible;
        GameManager.instance.uIHandler.SetUpMeargeText();
        // }
    }


    public void UpdateBoggyAddCost()
    {
        int m = GetRoundForAddTrain();
        float inner = Mathf.Round(4.5f * Mathf.Pow(boggyAddCount, 1.45f) * Mathf.Pow(1.085f, boggyAddCount));
        float finalCost = Mathf.Round(inner / m) * m;

        if (boggyAddCount == 1) finalCost = 5; //fixed on start, boggyAddCount not resetting


        boggyAddCost = (IdleCurrency)finalCost;
        CheckNextBoggyADD();
        Debug.Log($"boggyAddCount - {boggyAddCount}, Update value - {boggyAddCost.ToShortString()}");
    }
    public void CheckNextBoggyADD()
    {
        isEnoughMoneyForAddBoggy = boggyAddCost < GameManager.instance.playerDataForSave.playerData.collectedCoin;
    }

    private int GetRoundForAddTrain()
    {
        if (boggyAddCost.Value < 1000)
        {
            return 10;
        }
        else if (boggyAddCost.Value < 10000)
        {
            return 100;
        }
        else if (boggyAddCost.Value < 100000)
        {
            return 1000;
        }
        else if (boggyAddCost < new IdleCurrency(1, 6))
        {
            return 10000;
        }
        else
        {
            return 100000;
        }
    }


    void OnDisable()
    {
        trainSaveData.boggyLevel.Clear();
        foreach (var item in trainSplineDriver.boggies)
        {
            if (item.boggyType == BoggyType.Storage || item.boggyType == BoggyType.TrackUpdate)
            {
                continue;
            }
            trainSaveData.boggyLevel.Add(item.boggyLevel);
        }
        Save();
    }
    public void Save()
    {
        // Settings to ensure Vector2 and Enums save cleanly
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        trainSaveData.trainSplinePercentage = trainSplineDriver.GetSplineFollower.result.percent;

        string json = JsonConvert.SerializeObject(trainSaveData, settings);
        File.WriteAllText(GetPath(train_data_key), json);
        Debug.Log("save success");
    }
    public void Load()
    {
        string path = GetPath(train_data_key);

        string json = File.ReadAllText(path);
        trainSaveData = JsonConvert.DeserializeObject<TrainSaveData>(json);

        GameManager.instance.SetCurrentBoggyConfigOnStart();

        trainLoopHandler.UpdateSplineOnStart();
        //Set up Boggy
        var boggies = trainSplineDriver.boggies;
        for (int i = boggies.Count - 1; i >= 0; i--)
        {
            var boggy = boggies[i];
            if (boggy.boggyType != BoggyType.Storage && boggy.boggyType != BoggyType.TrackUpdate)
            {
                if (boggy.gameObject != null)
                {
                    Destroy(boggy.gameObject);
                }
                boggies.RemoveAt(i);
            }
        }

        int totalBoggy = trainSaveData.boggyLevel.Count;
        for (int i = 0; i < totalBoggy; i++)
        {
            SpawnBoggy(trainSaveData.boggyLevel[i]);
        }
        //==========

        //Set Up add Boggy
        boggyAddCount = trainSaveData.totalBoggyAddCount;
        UpdateBoggyAddCost();
        GameManager.instance.currentBoggyData.currentBoggyCount = trainSaveData.boggyCountCurrentConfig;
        //


        //Set up Storage Boggy
        storageBoggy.storageBoggyConfig.level = trainSaveData.capacityLevel;
        storageBoggy.storageBoggyConfig.SetUp();
        storageBoggy.storageBoggyConfig.SetUpCoast();
        //

        //Set up Speed
        trainSpeedConfig.level = trainSaveData.trainSpeedLevel;
        trainSpeedConfig.SetUp();
        //

        //Set up mearge
        trainMeargeConfig.level = trainSaveData.meargeLevel;
        trainMeargeConfig.SetUp();
        CheckIsMeargePossible();
        //

        GameManager.instance.uIHandler.SetUpAddBoggyText();
        GameManager.instance.uIHandler.SetUpCapcityText();
        GameManager.instance.uIHandler.SetUpSpeedText();
        GameManager.instance.uIHandler.SetUpMeargeText();

        trainSplineDriver.GetSplineFollower.SetPercent(trainSaveData.trainSplinePercentage);
        trainSplineDriver.MoveBogeys();
    }

    public bool IsPathAvailable()
    {
        string path = GetPath(train_data_key);
        if (!File.Exists(path))
        {
            Debug.LogError("Train Data not found");
            return false;
        }
        return true;
    }

    public void SetSpeedLevelForSave()
    {
        trainSaveData.trainSpeedLevel = trainSpeedConfig.GetCurrentLevel;
    }
    public void SetMeargeLevelForSave()
    {
        trainSaveData.meargeLevel = trainMeargeConfig.GetCurrentLevel;
    }

}
