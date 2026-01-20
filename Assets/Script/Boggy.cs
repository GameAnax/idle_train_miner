using System.Collections.Generic;
using Dreamteck.Splines;
using Dreamteck.Splines.Examples;
using TMPro;
using UnityEngine;

public partial class Boggy : MonoBehaviour
{
    public SplineFollower splineFollower;
    public TrainSplineDriver trainSplineDriver;
    public BoggyDamage boggyDamage;
    public ParticleSystem cuttingParticleSystem;

    [Header("Boggy UI")]
    public Canvas boggyCanvas;
    public TextMeshProUGUI boggyLevelText;

    public BoggyType boggyType;
    public float space;
    public int index;
    public int boggyLevel;

    public List<BoggyTypeData> boggyTypeDatas;



    public void SetBoggyData(int boggyIndex)
    {
        index = boggyIndex;
        // boggyLevel = boggyTypeDatas.FindIndex(i => i.boggyType == boggyType);
        foreach (var item in boggyTypeDatas)
        {
            item.ActivateNewBoggy(boggyType);
        }
        SetActive(true);
        UpdateUI();
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
        // boggyDamage.damageValue = GameManager.instance.boggyConfigs.Find(i => i.boggyType == boggyType).boggyDamage;
        boggyDamage.damageValue = GameManager.instance.boggyConfigs[boggyLevel].boggyDamage;
        UpdateUI();

    }
    public void SetBoggy(int saveBoggyLevel)
    {
        saveBoggyLevel = Mathf.Min(saveBoggyLevel, boggyTypeDatas.Count - 1);

        for (int i = 0; i < boggyTypeDatas.Count; i++)
        {
            boggyTypeDatas[i].boggyObj.SetActive(false);
        }

        boggyTypeDatas[saveBoggyLevel].boggyObj.SetActive(true);
        boggyType = boggyTypeDatas[saveBoggyLevel].boggyType;
        //TODO:- Update damage
        // boggyDamage.damageValue = GameManager.instance.boggyConfigs.Find(i => i.boggyType == boggyType).boggyDamage;
        boggyDamage.damageValue = GameManager.instance.boggyConfigs[saveBoggyLevel].boggyDamage;
        UpdateUI();
    }
    public void DestroyObj()
    {
        Destroy(gameObject);
    }
    public void PlayCuttingParticle()
    {
        if (cuttingParticleSystem != null)
        {
            cuttingParticleSystem.Play();
        }
    }
    private void UpdateUI()
    {
        if (boggyLevelText != null)
            boggyLevelText.text = $"{boggyLevel + 1}";
    }

    // void OnValidate()
    // {
    //     CalculateDistance();
    // }
    // void Update()
    // {
    //     MoveBogeys();
    // }
    // void LateUpdate()
    // {
    //     MoveBogeys();
    // }
    // private void MoveBogeys()
    // {
    //     float leaderCurrentDist = (float)trainSplineDriver.GetSplineFollower.CalculateLength() * (float)trainSplineDriver.GetSplineFollower.result.percent;
    //     // for (int i = 0; i < boggies.Count; i++)
    //     // {
    //     // var boggyItem = boggies[i];
    //     float targetDistanceForThisBogey = leaderCurrentDist - ((index + 1) * space);

    //     // Agar targetDistance negative hai, iska matlab bogey abhi purani spline par honi chahiye
    //     if (targetDistanceForThisBogey < 0)
    //     {
    //         // Yahan aap logic laga sakte hain ki bogey purani spline ke end par ruk jaye
    //         // Ya phir use purani spline par hi movement karwayein
    //         if (trainSplineDriver.GetTrainLoopHandler.previewsCombinedSplineComputer != null)
    //         {
    //             // Debug.Log("Call When Calculation Negitive");
    //             if (splineFollower.spline != trainSplineDriver.GetTrainLoopHandler.previewsCombinedSplineComputer)
    //             {
    //                 splineFollower.spline = trainSplineDriver.GetTrainLoopHandler.previewsCombinedSplineComputer;
    //                 splineFollower.Rebuild();
    //             }
    //             float prevLength = (float)trainSplineDriver.GetTrainLoopHandler.previewsCombinedSplineComputer.CalculateLength();
    //             float bridgeDistance = prevLength + targetDistanceForThisBogey;
    //             splineFollower.SetDistance(bridgeDistance);
    //         }
    //     }
    //     else
    //     {
    //         if (splineFollower.spline != trainSplineDriver.GetSplineFollower.spline)
    //         {
    //             // Debug.Log($"Train and Boggy spline not same at {targetDistanceForThisBogey}, and Train distance - {leaderCurrentDist}");
    //             splineFollower.spline = trainSplineDriver.GetSplineFollower.spline;
    //             splineFollower.Rebuild();
    //         }
    //         splineFollower.SetDistance(targetDistanceForThisBogey);
    //     }
    //     // }
    // }
}
