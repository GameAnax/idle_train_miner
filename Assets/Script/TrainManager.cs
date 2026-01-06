using EasyButtons;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    public TrainSplineDriver trainSplineDriver;

    public Boggy boggyPrefab;
    public Transform boggyParent;
    public int maxBoggyCount = 12;

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
}
