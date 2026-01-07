using System;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public Button addBoggy;
    public Button meargeBoggy;
    public Button speedBoggy;
    public Button capcityBoggy;

    void Start()
    {
        addBoggy.onClick.AddListener(OnClickAddBoggy);
        meargeBoggy.onClick.AddListener(OnClickMeargeBoggy);
        speedBoggy.onClick.AddListener(OnClickIncreaseBoggySpeed);
        capcityBoggy.onClick.AddListener(OnClickIncreaseStorageCapacity);
    }

    private void OnClickIncreaseStorageCapacity()
    {
        throw new NotImplementedException();
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
