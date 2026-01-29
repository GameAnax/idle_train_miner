using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoggyUnlockScreen : MonoBehaviour
{
    public GameObject contentObj;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI previewsLevelDamageText;
    public TextMeshProUGUI currentLevelDamageText;
    public Button greatButton;

    public Action greatButtonAction;

    void Start()
    {
        greatButton.onClick.AddListener(OnClickGreatButton);
    }

    public void ShowScreen()
    {
        gameObject.SetActive(true);
    }
    public void SetUpData(int currentBoggyLevel)
    {
        int totalLength = GameManager.instance.boggyConfigs.Count;
        int upgradeBoggyLevel = currentBoggyLevel + 1;
        upgradeBoggyLevel = Mathf.Min(upgradeBoggyLevel, totalLength - 1);

        var currentBoggyConfig = GameManager.instance.boggyConfigs[currentBoggyLevel];
        var upgradeBoggyData = GameManager.instance.boggyConfigs[upgradeBoggyLevel];

        levelText.text = $"Level {currentBoggyLevel + 2}";
        previewsLevelDamageText.text = $"{currentBoggyConfig.boggyDamage}";
        currentLevelDamageText.text = $"{upgradeBoggyData.boggyDamage}";
    }
    public void HideScreen()
    {
        gameObject.SetActive(false);
    }





    private void OnClickGreatButton()
    {
        greatButtonAction?.Invoke();
        HideScreen();
    }
}
