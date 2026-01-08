using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrusherArea : MonoBehaviour
{
    public List<CustomeGrid> startGrid;
    public List<CustomeGrid> currentGridList;
    public List<CustomeGrid> nextGridNeedClear;
    public Collider crusherCollider;
    public Transform debriesStorePoint;

    void Start()
    {
        currentGridList = new(startGrid);
        nextGridNeedClear = new();
        if (currentGridList[0].leftGrid != null)
        {
            nextGridNeedClear.Add(currentGridList[0].leftGrid);
        }
        if (currentGridList[1].leftGrid != null)
        {
            nextGridNeedClear.Add(currentGridList[1].leftGrid);
        }
        if (currentGridList[2].leftGrid != null)
        {
            nextGridNeedClear.Add(currentGridList[2].leftGrid);
        }
    }

    public void UpdateArePosition()
    {
        if (nextGridNeedClear == null) return;
        bool isReady = nextGridNeedClear.All(i => i.isClear);
        if (isReady)
        {
            Vector3 temp = transform.position;
            temp.x -= 1;
            transform.position = temp;
            UpdateNextGridList();
        }
    }

    public void TransferDebries() //From Train Storage to crusher
    {
        foreach (var item in GameManager.instance.trainManager.trainSplineDriver.boggies)
        {
            if (CheckBoggyInsideStorageArea(item.transform.position))
            {
                //TODO:- Transfer 
                List<Debries> debries = GameManager.instance.trainManager.storageBoggy.TransferAndClearStorage();
                foreach (Debries debrie in debries)
                {
                    debrie.transform.SetParent(debriesStorePoint, false);
                }
                break;
            }
        }

    }

    private bool CheckBoggyInsideStorageArea(Vector3 position)
    {
        return crusherCollider.bounds.Contains(position);
    }
    private void UpdateNextGridList()
    {
        currentGridList = new(nextGridNeedClear);
        if (currentGridList[0].leftGrid != null)
        {
            nextGridNeedClear[0] = currentGridList[0].leftGrid;
            nextGridNeedClear[1] = currentGridList[1].leftGrid;
            nextGridNeedClear[2] = currentGridList[2].leftGrid;
        }
        else
        {
            nextGridNeedClear = null;
        }
    }
}
