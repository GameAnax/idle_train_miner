using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public Button addBoggy;
    public Button meargeBoggy;
    public Button speedBoggy;
    public Button capcityBoggy;
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
    }

    private void OnClickIncreaseBoggySpeed()
    {
        GameManager.instance.IncreaseTrainSpeed();
    }

    private void OnClickMeargeBoggy()
    {
        GameManager.instance.MeargeBoggy();
    }

    private void OnClickAddBoggy()
    {
        GameManager.instance.ADDBoggy();
    }

}
