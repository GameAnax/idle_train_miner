using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrainMeargeConfig", menuName = "Scriptable Objects/TrainMeargeConfig")]
public class TrainMeargeConfig : ScriptableObject
{
    // public IdleCurrency baseCoast;
    public int level = 1;
    // public List<MeargeMultiplierData> meargeMultiplierDatas;
    public IdleCurrency currentCost;


    public int GetCurrentLevel => level;
    public IdleCurrency GetCurrentCost => currentCost;


    public void UpdateMearge()
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

        if (level >= 0 && level <= 10)
        {
            currentCost = (IdleCurrency)(Mathf.Pow(5.08f * (level), 2) - (4.63f * level) + 1.02f);
        }
        else if (level >= 11 && level <= 20)
        {
            currentCost = (IdleCurrency)(72.72f * level) - 60.08f;
        }
        else if (level >= 21 && level <= 30)
        {
            currentCost = (IdleCurrency)(367.88f * level) - 6170.91f;
        }
        else if (level >= 31 && level <= 40)
        {
            currentCost = (IdleCurrency)(700.61f * level) - 18051.52f;
        }
        else if (level >= 41 && level <= 47)
        {
            currentCost = (IdleCurrency)(1235.71f * level) - 37814.29f;
        }
        else if (level >= 48)
        {
            currentCost = (IdleCurrency)(1235.71f * level) - 37814.29f;
        }
        Debug.Log($"Current Cost - {currentCost}");
    }



    [Serializable]
    public class MeargeMultiplierData
    {
        public int startLevel;
        public int endLevel;



        public IdleCurrency CalculateRange_1_10(int currentLevel)
        {
            return (IdleCurrency)(Mathf.Pow(5.08f * (currentLevel), 2) - (4.63f * currentLevel) + 1.02f);
        }
    }
}
