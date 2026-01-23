using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CrusherArea : MonoBehaviour
{
    public List<CustomeGrid> startGrid;
    public List<CustomeGrid> currentGridList;
    public List<CustomeGrid> nextGridNeedClear;
    public Collider crusherCollider;
    public Transform debriesStorePoint;

    public Transform windMillFan;
    public Transform dotedLine;

    private bool isRotating = false;
    private float rotationDuration = 3f;
    private int totalRounds = 2;

    private bool isTrainInside;

    // private Vector3 rotationAmount = new Vector3(0, 0, 360);

    void Start()
    {
        // currentGridList = new(startGrid);
        // nextGridNeedClear = new();
        // if (currentGridList[0].leftGrid != null)
        // {
        //     nextGridNeedClear.Add(currentGridList[0].leftGrid);
        // }
        // if (currentGridList[1].leftGrid != null)
        // {
        //     nextGridNeedClear.Add(currentGridList[1].leftGrid);
        // }
        // if (currentGridList[2].leftGrid != null)
        // {
        //     nextGridNeedClear.Add(currentGridList[2].leftGrid);
        // }
    }

    void Update()
    {
        foreach (var item in GameManager.instance.trainManager.trainSplineDriver.boggies)
        {
            if (CheckBoggyInsideStorageArea(item.transform.position))
            {
                if (!isTrainInside)
                {
                    isTrainInside = true;
                    DoAnimation();
                }
                return;
            }
        }
        if (isTrainInside)
        {
            isTrainInside = false;
            DoAnimation();
        }
    }
    private void DoAnimation()
    {
        if (isTrainInside)
        {
            dotedLine.localScale = Vector3.one * 0.7f;
        }
        else
        {
            dotedLine.localScale = Vector3.one * 0.58f;
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
                bool isStorageAvaialble = GameManager.instance.trainManager.storageBoggy.storageBoggyConfig.filledCapacity > 0;
                if (!isStorageAvaialble) return;

                //TODO:- Transfer 
                List<Debries> debries = GameManager.instance.trainManager.storageBoggy.TransferAndClearStorage();
                foreach (Debries debrie in debries)
                {
                    debrie.transform.SetParent(debriesStorePoint, false);
                }
                Invoke(nameof(ClearStorage), 1f);
                StartRotation();
                break;
            }
        }
    }

    private void ClearStorage()
    {
        foreach (Transform temp in debriesStorePoint)
        {
            Destroy(temp.gameObject);
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
            UpdateArePosition(); //Check Again is possible to move 
        }
        else
        {
            nextGridNeedClear = null;
        }
    }

    private void StartRotation()
    {
        if (isRotating) return;

        isRotating = true;

        Vector3 rotationAmount = new Vector3(0, 0, 360 * totalRounds);

        windMillFan.DOLocalRotate(rotationAmount, rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutQuad) // Slow start -> Fast -> Slow end
            .OnComplete(() =>
            {
                isRotating = false; // Khatam hone par bool reset
            });
    }
}
