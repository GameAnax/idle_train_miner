using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using DG.Tweening;
using System.Linq;

[ExecuteInEditMode]
public class ModularGridAligner : MonoBehaviour
{
    [Header("Dependencies")]
    public SplineGenerator splineGen;
    public ClockwiseRingGenerator clockwiseRingGenerator;

    [Header("Prefabs")]
    public GameObject straightPiece;
    public GameObject cornerPiece;

    [Header("Animation Settings")]
    public float startYOffset = 5f;
    public float animDuration = 0.6f;
    public float rotationAnimDuration = 0.3f; // Slightly increased for smoothness
    public Vector3 fallingRotation = new Vector3(0, 180, 0);

    [Header("Settings")]
    public Transform piecesParent;
    public float yOffset = 0.1f;
    public bool isLoop = true;

    private HashSet<CustomeGrid> lastPath = new HashSet<CustomeGrid>();


    void Start()
    {
        StartGeneration();
    }

    [Button("Setup All Grids (Pre-Spawn)")]
    public void SetupGrids()
    {
        // Run this once in Editor to attach pieces to all grids
        foreach (var grid in clockwiseRingGenerator.spawnedCubes)
        {
            if (grid.myStraightPiece == null)
                grid.myStraightPiece = Instantiate(straightPiece, grid.transform);

            if (grid.myCornerPiece == null)
                grid.myCornerPiece = Instantiate(cornerPiece, grid.transform);

            grid.myStraightPiece.name = "straight";
            grid.myCornerPiece.name = "corner";
            grid.HideAllTracks();
        }
    }
    [Button("HARD RESET: Delete All Pieces")]
    public void HardDeleteAllPieces()
    {
        int count = 0;

        foreach (var grid in clockwiseRingGenerator.spawnedCubes)
        {
            if (grid == null) continue;

            // Destroy straight piece
            if (grid.myStraightPiece != null)
            {
                if (Application.isPlaying) Destroy(grid.myStraightPiece);
                else DestroyImmediate(grid.myStraightPiece);
                grid.myStraightPiece = null;
                count++;
            }

            // Destroy corner piece
            if (grid.myCornerPiece != null)
            {
                if (Application.isPlaying) Destroy(grid.myCornerPiece);
                else DestroyImmediate(grid.myCornerPiece);
                grid.myCornerPiece = null;
                count++;
            }
        }

        lastPath.Clear();
        Debug.Log($"<color=red>Hard Reset Complete: {count} objects destroyed from grids.</color>");
    }


    [Button("Generate/Update Modular Track")]
    public void StartGeneration()
    {
        if (splineGen == null || splineGen.splineGridPath == null || splineGen.splineGridPath.Count == 0) return;

        // if (!Application.isPlaying)
        // {
        //     return;
        // }

        UpdateTrackWithAnimation(splineGen.splineGridPath);
    }

    public void UpdateTrackWithAnimation(List<CustomeGrid> newPath)
    {
        HashSet<CustomeGrid> newPathSet = new HashSet<CustomeGrid>(newPath);

        // 1. Hide tracks that are no longer part of the path
        foreach (var oldGrid in lastPath)
        {
            if (!newPathSet.Contains(oldGrid) && oldGrid != null)
            {
                oldGrid.HideAllTracks();
            }
        }

        // 2. Update/Show tracks in the new path
        for (int i = 0; i < newPath.Count; i++)
        {
            CustomeGrid curr = newPath[i];
            if (curr == null) continue;

            CustomeGrid prev = (i == 0) ? (isLoop ? newPath[newPath.Count - 1] : null) : newPath[i - 1];
            CustomeGrid next = (i == newPath.Count - 1) ? (isLoop ? newPath[0] : null) : newPath[i + 1];

            if (prev != null && next != null)
            {
                ProcessGridPiece(prev, curr, next);
            }
        }

        foreach (var oldGrid in lastPath)
        {
            if (!newPathSet.Contains(oldGrid))
            {
                Vector2 currentPos = oldGrid.gridPosition;
                CustomeGrid nearestEmpty = newPathSet
        .Where(g => g != oldGrid && g.cubeContainer == null)
        .OrderBy(g => Vector2.Distance(currentPos, g.gridPosition))
        .FirstOrDefault();
                if (nearestEmpty != null)
                {
                    if (oldGrid.cubeContainer != null)
                    {
                        oldGrid.cubeContainer.SetParent(nearestEmpty.transform, false);
                        nearestEmpty.cubeContainer = oldGrid.cubeContainer;
                        nearestEmpty.SetUpNeighbourDebries();
                        continue;
                    }
                }
                else
                {
                    CustomeGrid nearestAny = newPathSet
        .Where(g => g != oldGrid)
        .OrderBy(g => Vector2.Distance(currentPos, g.gridPosition))
        .FirstOrDefault();
                    if (nearestAny != null)
                    {
                        Debug.Log($"Found Non empty grid {nearestAny.gridPosition}");
                        if (oldGrid.cubeContainer != null)
                        {
                            Debug.Log($"old grid contain -  {oldGrid.gridPosition}");
                            foreach (Transform item in oldGrid.cubeContainer)
                            {
                                Debug.Log("Transfering derbies");
                                item.SetParent(nearestAny.cubeContainer, false);
                            }
                            nearestAny.SetUpNeighbourDebries();
                        }
                        continue;
                    }
                }
            }
        }

        lastPath = newPathSet;
    }

    void ProcessGridPiece(CustomeGrid prev, CustomeGrid curr, CustomeGrid next)
    {
        // Direction vectors (Manually rounded for grid snapping)
        Vector3 toPrev = (prev.transform.position - curr.transform.position);
        Vector3 toNext = (next.transform.position - curr.transform.position);

        // Normalize and round to avoid float errors (1, 0, -1 only)
        Vector3 dP = new Vector3(Mathf.Round(toPrev.normalized.x), 0, Mathf.Round(toPrev.normalized.z));
        Vector3 dN = new Vector3(Mathf.Round(toNext.normalized.x), 0, Mathf.Round(toNext.normalized.z));

        float dot = Vector3.Dot(dP, dN);
        GameObject chosenPrefab;
        float targetY = 0;

        if (dot < -0.9f) // STRAIGHT
        {
            chosenPrefab = straightPiece;
            targetY = Quaternion.LookRotation(dN).eulerAngles.y;
        }
        else // CORNER
        {
            chosenPrefab = cornerPiece;

            // Corner Rotation Logic based on which sides are connected:
            // dP aur dN represent karte hain Left, Right, Up, Down (Grid directions)

            if ((dP.x > 0 && dN.z > 0) || (dN.x > 0 && dP.z > 0)) targetY = 0;     // Right & Top
            else if ((dP.x > 0 && dN.z < 0) || (dN.x > 0 && dP.z < 0)) targetY = 90;   // Right & Bottom
            else if ((dP.x < 0 && dN.z < 0) || (dN.x < 0 && dP.z < 0)) targetY = 180;  // Left & Bottom
            else if ((dP.x < 0 && dN.z > 0) || (dN.x < 0 && dP.z > 0)) targetY = 270;  // Left & Top

            // --- NOTE: IMPORTANT ---
            // Agar aapka model default mein "Left & Top" face kar raha hai, 
            // toh upar ki values (+0, +90, +180, +270) ko rotate karke adjust karein.
        }

        curr.SetTrackState(chosenPrefab, targetY, animDuration, startYOffset);
    }
}