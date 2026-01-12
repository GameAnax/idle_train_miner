using EasyButtons;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    public TrainSplineDriver trainSplineDriver;
    public StorageBoggy storageBoggy;
    public TrainSpeedConfig trainSpeedConfig;
    public TrainMeargeConfig trainMeargeConfig;

    public Boggy boggyPrefab;
    public Transform boggyParent;
    public int maxBoggyCount = 12;

    public int boggyAddCount = 1; //continue increase no reset
    public IdleCurrency boggyAddCost = 5;

    [Button]
    public void SpawnBoggy(BoggyType boggyType)
    {
        if (maxBoggyCount == trainSplineDriver.boggies.Count)
        {
            Debug.Log("Boggy Limit Reaced");
            return;
        }

        int totalBoggy = trainSplineDriver.boggies.Count;
        int boggyIndex = totalBoggy - 1;

        Boggy newBoggy = Instantiate(boggyPrefab, boggyParent);
        newBoggy.SetBoggyData(boggyIndex: boggyIndex);
        //TODO:- Set boggy Data
        trainSplineDriver.boggies.Insert(boggyIndex, newBoggy);

        //TODO:- Update Dummy Boggy Index
        trainSplineDriver.boggies[^1].SetBoggyData(boggyIndex: boggyIndex + 1, boggyType);

        if (trainSplineDriver.boggies.Count == maxBoggyCount)
        {
            Debug.Log("Boggy Limit Reaced");
        }
    }



    public void UpdateBoggyAddCost()
    {
        int m = GetRoundForAddTrain();
        float inner = Mathf.Round(4.5f * Mathf.Pow(boggyAddCount, 1.45f) * Mathf.Pow(1.085f, boggyAddCount));
        float finalCost = Mathf.Round(inner / m) * m;

        boggyAddCost = (IdleCurrency)finalCost;
        Debug.Log($"boggyAddCount - {boggyAddCount}, Update value - {boggyAddCost.ToShortString()}");
    }

    private int GetRoundForAddTrain()
    {
        if (boggyAddCost.Value < 1000)
        {
            return 10;
        }
        else if (boggyAddCost.Value < 10000)
        {
            return 100;
        }
        else if (boggyAddCost.Value < 100000)
        {
            return 1000;
        }
        else if (boggyAddCost < new IdleCurrency(1, 6))
        {
            return 10000;
        }
        else
        {
            return 100000;
        }
    }
}
