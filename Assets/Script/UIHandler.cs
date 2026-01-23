using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public Sprite disableBG;
    public Sprite disableBalanceBG;
    public Sprite enableBG;
    public Sprite enableBalanceBG;
    public Color disableBalanceTextColor;
    public Color enableBalanceTextColor;
    public Color disableLevelTextColor;
    public Color enableLevelTextColor;

    [Header("Coin")]
    public GameObject coinObj;
    public TextMeshProUGUI coinText;

    [Header("Level Progress")]
    public GameObject levelProgressObj;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelCompleteProgressText;
    public Image levelProgressFillImage;

    [Header("Add Boggy")]
    public Button addBoggy;
    public TextMeshProUGUI boggyAddCostText;
    public TextMeshProUGUI boggyAddLevelText;
    public TextMeshProUGUI boggyCountText;
    public Image iconImage;
    public Image addBoggyFillImage;
    public Image addBgImage;
    public Image addBalanceBGImage;

    [Header("Mearge")]
    public Button meargeBoggy;
    public TextMeshProUGUI meargeCostText;
    public Image meargeIcon1;
    public Image meargeIcon2;
    public Image meargeBgImage;
    public Image meargeBalanceBGImage;

    [Header("Speed")]
    public Button speedBoggy;
    public TextMeshProUGUI speedCostText;
    public TextMeshProUGUI speedLevelText;
    public Image speedBgImage;
    public Image speedBalancedImage;

    [Header("Capcaity")]
    public Button capcityBoggy;
    public TextMeshProUGUI capacityAmountText;
    public TextMeshProUGUI capacityLevelText;
    public TextMeshProUGUI totalCapacityText;
    public Image capacityFillImage;
    public Image capacityBGImage;
    public Image capacityBalanceImage;


    private StorageBoggyConfig storageBoggyConfig;
    private TrainSpeedConfig trainSpeedConfig;
    private TrainMeargeConfig trainMeargeConfig;

    private TrainManager trainManager;

    private bool isEnoughMoneyForAddBoggy = true; //internal check for not update ui every time

    private bool isEnoughMoneyForMeargeBoggy = true;

    private bool isEnoughMoneyForUpgradeSpeed = true;

    private bool isEnoughMoneyForUpgradeCapacity = true;



    void Start()
    {
        addBoggy.onClick.AddListener(OnClickAddBoggy);
        meargeBoggy.onClick.AddListener(OnClickMeargeBoggy);
        speedBoggy.onClick.AddListener(OnClickIncreaseBoggySpeed);
        capcityBoggy.onClick.AddListener(OnClickIncreaseStorageCapacity);
    }

    public void UpdateAllButtonUI()
    {
        UpdateADDBoggyButtonUI();
        UpdateMeargeBoggyButtonUI();
        UpdateSpeedButtonUI();
        UpdateCapacityButtonUI();
    }

    public void UpdateCoinText(IdleCurrency value)
    {
        coinText.text = $"{value.ToShortString()}";
    }

    public void UpdateLevelProgress(float levelProgress)
    {
        levelCompleteProgressText.text = $"{levelProgress:F2}%";
        levelProgressFillImage.fillAmount = levelProgress / 100f;
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

    #region  Train Capcity
    public void SetUpCapcityText()
    {
        if (storageBoggyConfig == null)
            storageBoggyConfig = GameManager.instance.trainManager.storageBoggy.storageBoggyConfig;

        capacityLevelText.text = $"lvl {storageBoggyConfig.GetCurrentLevel}";
        capacityAmountText.text = $"{storageBoggyConfig.GetCurrentUpgradeCost}";
        UpdateCapacityStorageProgress((float)storageBoggyConfig.GetFilled(), (int)storageBoggyConfig.filledCapacity, (int)storageBoggyConfig.GetCapacity);
        UpdateCapacityButtonUI();
    }
    public void UpdateCapacityStorageProgress(float value, int fillAmount, int capcity)
    {
        // capacityFillImage.fillAmount = value;
        totalCapacityText.text = $"{fillAmount} / {capcity}";
        capacityFillImage.DOKill();
        capacityFillImage.DOFillAmount(value, 0.15f).SetEase(Ease.Linear);
    }
    public void UpdateCapacityButtonUI()
    {
        if (!storageBoggyConfig.IsEnoughMoneyForUpgradeCapacity)
        {
            if (isEnoughMoneyForUpgradeCapacity)
            {
                isEnoughMoneyForUpgradeCapacity = storageBoggyConfig.IsEnoughMoneyForUpgradeCapacity;

                capacityBGImage.sprite = disableBG;
                capacityBalanceImage.sprite = disableBalanceBG;
                capacityLevelText.color = disableLevelTextColor;
                capacityAmountText.color = disableBalanceTextColor;
            }
        }
        else
        {
            if (!isEnoughMoneyForUpgradeCapacity)
            {
                isEnoughMoneyForUpgradeCapacity = storageBoggyConfig.IsEnoughMoneyForUpgradeCapacity;

                capacityBGImage.sprite = enableBG;
                capacityBalanceImage.sprite = enableBalanceBG;
                capacityLevelText.color = enableLevelTextColor;
                capacityAmountText.color = enableBalanceTextColor;
            }
        }
    }
    #endregion

    #region  Train Speed
    public void SetUpSpeedText()
    {
        if (trainSpeedConfig == null)
        {
            trainSpeedConfig = GameManager.instance.trainManager.trainSpeedConfig;
        }

        speedLevelText.text = $"lvl {trainSpeedConfig.GetCurrentLevel}";
        speedCostText.text = $"{trainSpeedConfig.GetCurrentCost}";

        UpdateSpeedButtonUI();
    }
    public void UpdateSpeedButtonUI()
    {
        if (!trainSpeedConfig.IsEnoughMoneyForUpgradeSpeed)
        {
            if (isEnoughMoneyForUpgradeSpeed)
            {
                isEnoughMoneyForUpgradeSpeed = trainSpeedConfig.IsEnoughMoneyForUpgradeSpeed;

                speedBgImage.sprite = disableBG;
                speedBalancedImage.sprite = disableBalanceBG;
                speedLevelText.color = disableLevelTextColor;
                speedCostText.color = disableBalanceTextColor;
            }
        }
        else
        {
            if (!isEnoughMoneyForUpgradeSpeed)
            {
                isEnoughMoneyForUpgradeSpeed = trainSpeedConfig.IsEnoughMoneyForUpgradeSpeed;

                speedBgImage.sprite = enableBG;
                speedBalancedImage.sprite = enableBalanceBG;
                speedLevelText.color = enableLevelTextColor;
                speedCostText.color = enableBalanceTextColor;
            }
        }
    }
    #endregion


    #region  Boggy Mearge
    public void SetUpMeargeText()
    {
        if (trainMeargeConfig == null)
        {
            trainMeargeConfig = GameManager.instance.trainManager.trainMeargeConfig;
        }

        meargeCostText.text = $"{trainMeargeConfig.GetCurrentCost}";
        UpdateMeargeBoggyButtonUI();
    }
    public void UpdateMeargeBoggyButtonUI()
    {
        if (!trainMeargeConfig.IsEnoughMoneyForMeargeBoggy)
        {
            if (isEnoughMoneyForMeargeBoggy)
            {
                isEnoughMoneyForMeargeBoggy = trainMeargeConfig.IsEnoughMoneyForMeargeBoggy;
                meargeBgImage.sprite = disableBG;
                meargeBalanceBGImage.sprite = disableBalanceBG;
                meargeCostText.color = disableBalanceTextColor;
            }
        }
        else
        {
            if (!isEnoughMoneyForMeargeBoggy)
            {
                isEnoughMoneyForMeargeBoggy = trainMeargeConfig.IsEnoughMoneyForMeargeBoggy;
                meargeBgImage.sprite = enableBG;
                meargeBalanceBGImage.sprite = enableBalanceBG;
                meargeCostText.color = enableBalanceTextColor;
            }
        }
    }
    #endregion;

    public void SetUpAddBoggyText()
    {
        if (trainManager == null)
        {
            trainManager = GameManager.instance.trainManager;
        }

        boggyAddCostText.text = trainManager.boggyAddCost.ToShortString();
        boggyAddLevelText.text = $"lvl {trainManager.trainSaveData.boggyConfigIndex + 1}";
        boggyCountText.text = $"{GameManager.instance.currentBoggyData.currentBoggyCount}/{GameManager.instance.currentBoggyData.maxBoggyCount}";
        addBoggyFillImage.fillAmount = GameManager.instance.currentBoggyData.GetProgress();

        UpdateADDBoggyButtonUI();
    }
    public void UpdateADDBoggyButtonUI()
    {
        if (!trainManager.IsEnoughMoneyForADDBoggy)
        {
            if (isEnoughMoneyForAddBoggy) //check for update ui if not match, Set disable UI
            {
                isEnoughMoneyForAddBoggy = trainManager.IsEnoughMoneyForADDBoggy;
                addBgImage.sprite = disableBG;
                addBalanceBGImage.sprite = disableBalanceBG;
                boggyAddCostText.color = disableBalanceTextColor;
            }
        }
        else
        {
            if (!isEnoughMoneyForAddBoggy) //Set enable UI
            {
                isEnoughMoneyForAddBoggy = trainManager.IsEnoughMoneyForADDBoggy;
                addBgImage.sprite = enableBG;
                addBalanceBGImage.sprite = enableBalanceBG;
                boggyAddCostText.color = enableBalanceTextColor;
            }
        }
    }

}
