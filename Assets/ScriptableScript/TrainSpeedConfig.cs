using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrainSpeedConfig", menuName = "Scriptable Objects/TrainSpeedConfig")]
public class TrainSpeedConfig : ScriptableObject
{
    public IdleCurrency baseCost;
    public int level = 1;
    public List<SpeedMultiplierData> speedMultiplierDatas;
    public IdleCurrency currentCost;

    public IdleCurrency GetCurrentCost => currentCost;
    public int GetCurrentLevel => level;

    public void UpdateSpeed()
    {
        level++;
        UpdateCost();
    }

    public void SetUp()
    {
        UpdateCost();
    }

    private void UpdateCost()
    {
        SpeedMultiplierData speedMultiplierData = GetMultiplierData();
        try
        {
            if (level >= 0 && level <= 3)
            {
                currentCost = 20 * level;
            }
            else if (level >= 4 && level <= 15)
            {
                currentCost = (IdleCurrency)(150 * level * 2.2f);
            }
            else if (level >= 16)
            {
                currentCost = (IdleCurrency)Mathf.Pow((18 * level), 2.2f);
            }
            Debug.Log($"Current cost = {currentCost}");
        }
        catch (Exception e)
        {
            Debug.Log($"Train Speed Config - {e}");
        }
    }

    private SpeedMultiplierData GetMultiplierData()
    {
        foreach (var item in speedMultiplierDatas)
        {
            if (level >= item.startLevel && level <= item.endLevel)
            {
                return item;
            }
        }
        return null;
    }



    [Serializable]
    public class SpeedMultiplierData
    {
        public int startLevel;
        public int endLevel;
        public float baseValueForMultipy;
        public float multiplier;
    }
}
