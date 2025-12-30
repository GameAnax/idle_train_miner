using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class PathUpdateManager : MonoBehaviour
{
    public static PathUpdateManager Instance;

    public SplineGenerator splineGen;
    public ModularGridAligner gridAligner;
    public LayerMask bogeyLayer; // Train bogeys ki layer
    public float safetyRadius = 2.0f; // Grid se bogey kitni door honi chahiye

    private List<CustomeGrid> pendingGrids = new List<CustomeGrid>();

    void Awake() => Instance = this;

    public void RegisterClearedGrid(CustomeGrid grid)
    {
        if (!pendingGrids.Contains(grid))
            pendingGrids.Add(grid);
    }

    void Update()
    {
        if (pendingGrids.Count == 0) return;

        // Check karein ki kya saari pending grids bogey se free hain
        for (int i = pendingGrids.Count - 1; i >= 0; i--)
        {
            if (IsGridSafeToUpdate(pendingGrids[i]))
            {
                // Agar safe hai, toh update trigger karo
                UpdateAllSystems();
                pendingGrids.RemoveAt(i);
            }
        }
    }

    bool IsGridSafeToUpdate(CustomeGrid grid)
    {
        // Check karein ki grid ke paas koi Bogey toh nahi
        Collider[] bogeys = Physics.OverlapSphere(grid.transform.position, safetyRadius, bogeyLayer);
        return bogeys.Length == 0; // Agar 0 hai matlab bogey aage nikal gayi hai
    }

    public void UpdateAllSystems()
    {
        if (splineGen != null) splineGen.GenerateSpline();
        // if (gridAligner != null) gridAligner.StartGeneration();
    }
    public void OnGridCleared(CustomeGrid grid)
    {
        StartCoroutine(WaitUntilSafe(grid));
    }
    IEnumerator WaitUntilSafe(CustomeGrid grid)
    {
        // Bogey ke nikalne ka wait karein
        while (Physics.OverlapSphere(grid.transform.position, 1.2f, bogeyLayer).Length > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }
        // Ab jab grid puri tarah peeche chhut gayi he, tabhi refresh karein
        splineGen.GenerateSpline();
    }
}