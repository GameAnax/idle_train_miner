using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "StorageBoggyConfig", menuName = "Scriptable Objects/StorageBoggyConfig")]
public class StorageBoggyConfig : ScriptableObject
{
    public int level = 0;
    public List<DirectCost> directCost;
    public List<CostRangeMultiplierData> costRangeMultiplierDatas;

    [Header("Capacity")]
    public IdleCurrency baseCapacity = 300;
    public IdleCurrency incremeantValue = 50;
    private IdleCurrency currentCapacity;
    public IdleCurrency GetCapacity => currentCapacity;
    public IdleCurrency filledCapacity;

    [Header("Cost")]
    private IdleCurrency currentCost;

    public int GetCurrentLevel => level;
    public IdleCurrency GetCurrentUpgradeCost => currentCost;

    private bool isEnoughMoneyForUpgradeCapacity;
    public bool IsEnoughMoneyForUpgradeCapacity => isEnoughMoneyForUpgradeCapacity;

    public double GetFilled()
    {
        return filledCapacity.RealValue / GetCapacity.RealValue;
    }
    public void SetUp()
    {
        currentCapacity = baseCapacity + (incremeantValue * (level - 1));
        currentCost = 20;
    }

    public void UpdateCapacity()
    {
        currentCapacity = baseCapacity + (incremeantValue * level);
        level++;
        Debug.Log($"Capacity - Level -{level}, currentCapacity - {currentCapacity}, UpgradeCode - ");
        UpdateCost();
    }

    public void SetUpCoast()
    {
        UpdateCost();
    }
    public void CheckMoney()
    {
        isEnoughMoneyForUpgradeCapacity = currentCost < GameManager.instance.playerDataForSave.playerData.collectedCoin;
    }
    private void UpdateCost()
    {
        var (isFound, data) = CheckDirectLevel();
        if (isFound)
        {
            currentCost = data.cost;
            Debug.Log($"Direct cost - {data.cost}");
            return;
        }
        CostRangeMultiplierData multiplierData = GetCurrentSlotForUpdate();
        float powerValue = Mathf.Pow(level, multiplierData.multiplier);
        double scale = Math.Pow(10, Math.Abs(multiplierData.removeNumberOfDecimal));
        double newValue = Math.Round(powerValue / scale) * scale;
        // IdleCurrency newUpgradePrice = (IdleCurrency)Mathf.Pow(Mathf.Round(level), multiplierData.multiplier);
        // IdleCurrency newUpgradePrice = (IdleCurrency)newValue;
        currentCost = (IdleCurrency)newValue;

        CheckMoney();
        Debug.Log($"New upgrade Price - {currentCost}");

    }

    public bool CheckIsStorageFull(int amountToStore)
    {
        int needSpace = (int)filledCapacity + amountToStore;
        return currentCapacity < needSpace;
    }
    private (bool isFound, DirectCost data) CheckDirectLevel()
    {
        foreach (var item in directCost)
        {
            if (item.level == level)
            {
                return (true, item);
            }
        }
        return (false, new());
    }

    private CostRangeMultiplierData GetCurrentSlotForUpdate()
    {
        foreach (var item in costRangeMultiplierDatas)
        {
            if (item.startLevel <= level && item.endLevel == -1)
            {
                return item;
            }
            if (item.startLevel <= level && level <= item.endLevel)
            {
                return item;
            }
        }
        Debug.Log("something wrong can't get value");
        return new();
    }


    [Serializable]
    public struct DirectCost
    {
        public int level;
        public IdleCurrency cost;
    }

    [Serializable]
    public struct CostRangeMultiplierData
    {
        public int startLevel;
        public int endLevel;
        public float multiplier;
        public int removeNumberOfDecimal;
    }
}
