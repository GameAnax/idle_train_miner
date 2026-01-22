using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [Header("Add Boggy")]
    public Button addBoggy;
    public TextMeshProUGUI boggyAddCostText;
    public TextMeshProUGUI boggyAddLevelText;
    public TextMeshProUGUI boggyCountText;
    public Image addBoggyFillImage;

    [Header("Mearge")]
    public Button meargeBoggy;
    public TextMeshProUGUI meargeCostText;

    [Header("Speed")]
    public Button speedBoggy;
    public TextMeshProUGUI speedCostText;
    public TextMeshProUGUI speedLevelText;

    [Header("Capcaity")]
    public Button capcityBoggy;
    public TextMeshProUGUI capacityAmountText;
    public TextMeshProUGUI capacityLevelText;
    public TextMeshProUGUI totalCapacityText;
    public Image capacityFillImage;
    private StorageBoggyConfig storageBoggyConfig;
    private TrainSpeedConfig trainSpeedConfig;
    private TrainMeargeConfig trainMeargeConfig;



    public TextMeshProUGUI levelCompleteProgressText;

    void Start()
    {
        addBoggy.onClick.AddListener(OnClickAddBoggy);
        meargeBoggy.onClick.AddListener(OnClickMeargeBoggy);
        speedBoggy.onClick.AddListener(OnClickIncreaseBoggySpeed);
        capcityBoggy.onClick.AddListener(OnClickIncreaseStorageCapacity);
    }

    public void UpdateLevelProgress(float levelProgress)
    {
        levelCompleteProgressText.text = $"Level Completed - {levelProgress:F2}%";
    }

    private void OnClickIncreaseStorageCapacity()
    {
        GameManager.instance.UpdateStorageCapacity();
        SetUpCapcityText();
    }

    private void OnClickIncreaseBoggySpeed()
    {
        GameManager.instance.IncreaseTrainSpeed();
        SetUpSpeedText();
    }

    private void OnClickMeargeBoggy()
    {
        GameManager.instance.MeargeBoggy();
    }

    private void OnClickAddBoggy()
    {
        GameManager.instance.ADDBoggy();
        SetUpAddBoggyText();
    }

    public void SetUpCapcityText()
    {
        storageBoggyConfig = GameManager.instance.trainManager.storageBoggy.storageBoggyConfig;

        capacityLevelText.text = $"lvl {storageBoggyConfig.GetCurrentLevel}";
        capacityAmountText.text = $"upgrade cost - {storageBoggyConfig.GetCurrentUpgradeCost}";
        UpdateCapacityStorageProgress((float)storageBoggyConfig.GetFilled(), (int)storageBoggyConfig.filledCapacity, (int)storageBoggyConfig.GetCapacity);
        // totalCapacityText.text = $"Capacity - {storageBoggyConfig.GetCapacity}";
    }
    public void UpdateCapacityStorageProgress(float value, int fillAmount, int capcity)
    {
        // capacityFillImage.fillAmount = value;
        totalCapacityText.text = $"{fillAmount} / {capcity}";
        capacityFillImage.DOKill();
        capacityFillImage.DOFillAmount(value, 0.15f).SetEase(Ease.Linear);
    }
    public void SetUpSpeedText()
    {
        if (trainSpeedConfig == null)
        {
            trainSpeedConfig = GameManager.instance.trainManager.trainSpeedConfig;
        }

        speedLevelText.text = $"lvl {trainSpeedConfig.GetCurrentLevel}";
        speedCostText.text = $"cost {trainSpeedConfig.GetCurrentCost}";
    }
    public void SetUpMeargeText()
    {
        if (trainMeargeConfig == null)
        {
            trainMeargeConfig = GameManager.instance.trainManager.trainMeargeConfig;
        }

        meargeCostText.text = $"{trainMeargeConfig.GetCurrentCost}";
    }

    public void SetUpAddBoggyText()
    {
        boggyAddCostText.text = GameManager.instance.trainManager.boggyAddCost.ToShortString();
        boggyAddLevelText.text = $"lvl {GameManager.instance.trainManager.trainSaveData.boggyConfigIndex + 1}";
        boggyCountText.text = $"{GameManager.instance.currentBoggyData.currentBoggyCount}/{GameManager.instance.currentBoggyData.maxBoggyCount}";
        addBoggyFillImage.fillAmount = GameManager.instance.currentBoggyData.GetProgress();
    }

}
