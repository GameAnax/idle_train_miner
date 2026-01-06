using System.Collections.Generic;
using Dreamteck.Splines;
using Dreamteck.Splines.Examples;
using UnityEngine;

public partial class Boggy : MonoBehaviour
{
    public SplineFollower splineFollower;
    public TrainSplineDriver trainSplineDriver;
    public BogeyDamage boggyDamage;

    public BoggyType boggyType;
    public float space;
    public int index;
    public int boggyLevel;

    public List<BoggyTypeData> boggyTypeDatas;


    public void SetBoggyData(int boggyIndex, BoggyType boggyType = BoggyType.Boggy_1)
    {
        index = boggyIndex;
        boggyLevel = boggyTypeDatas.FindIndex(i => i.boggyType == boggyType);
        foreach (var item in boggyTypeDatas)
        {
            item.ActivateNewBoggy(boggyType);
        }
        SetActive(true);
    }
    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
    public void CalculateDistance()
    {
        float leaderCurrentDist = (float)trainSplineDriver.GetSplineFollower.CalculateLength() * (float)trainSplineDriver.GetSplineFollower.result.percent;

        float targetDistanceForThisBogey = leaderCurrentDist - ((index + 1) * space);
        splineFollower.SetDistance(targetDistanceForThisBogey);
    }
    public void UpdateBoggy()
    {
        boggyLevel += 1;
        boggyLevel = Mathf.Min(boggyLevel, boggyTypeDatas.Count - 1);

        for (int i = 0; i < boggyTypeDatas.Count; i++)
        {
            boggyTypeDatas[i].boggyObj.SetActive(false);
        }

        boggyTypeDatas[boggyLevel].boggyObj.SetActive(true);
        boggyType = boggyTypeDatas[boggyLevel].boggyType;
        //TODO:- Update damage
        boggyDamage.damageValue = GameManager.instance.boggyConfigs.Find(i => i.boggyType == boggyType).boggyDamage;

    }
    public void DestroyObj()
    {
        Destroy(gameObject);
    }

    // void OnValidate()
    // {
    //     CalculateDistance();
    // }
}
