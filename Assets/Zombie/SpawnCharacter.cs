using System;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;

public class SpawnCharacter : MonoBehaviour
{
    [SerializeField]
    private ClockwiseRingGenerator clockwiseRingGenerator;

    [SerializeField]
    private GameObject spanwObj;
    [SerializeField]
    private Transform parentTransform;
    [SerializeField]
    private bool isSpawnFromClassData;

    [Serializable]
    public class SpawnData
    {
        public List<CustomeGrid> customeGrids;
        public GameObject forSpawn;
    }
    public List<SpawnData> spawnDatas;


    void Start()
    {
        SetupGrids();
    }

    [Button]
    public void SetupGrids()
    {
        if (isSpawnFromClassData)
        {
            foreach (SpawnData sd in spawnDatas)
            {
                foreach (CustomeGrid cg in sd.customeGrids)
                {
                    GameObject temp = Instantiate(sd.forSpawn, parentTransform);
                    temp.transform.position = cg.transform.position;
                }
            }
        }
        else
        {
            foreach (var grid in clockwiseRingGenerator.spawnedCubes)
            {
                if (grid.isClear) continue;

                GameObject temp = Instantiate(spanwObj, parentTransform);
                temp.transform.position = grid.transform.position;
            }
        }

    }
    [Button]
    public void ClearParent()
    {
        // We must work backwards when destroying children to avoid index issues
        for (int i = parentTransform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parentTransform.GetChild(i).gameObject);
        }

        Debug.Log("Parent cleared in Editor!");
    }
}
