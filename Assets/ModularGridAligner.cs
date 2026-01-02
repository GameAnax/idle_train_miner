using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using DG.Tweening;

public class ModularGridAligner : MonoBehaviour
{
    [Header("Dependencies")]
    public SplineGenerator splineGen;

    [Header("Prefabs")]
    public GameObject straightPiece;
    public GameObject cornerPiece;

    [Header("Animation Settings")]
    public float startYOffset = 5f;
    public float animDuration = 0.6f;
    public float rotationAnimDuration = 0.1f;
    public Vector3 fallingRotation = new Vector3(0, 180, 0);

    [Header("Settings")]
    public Transform piecesParent;
    public float yOffset = 0.1f;
    public bool isLoop = true;

    // Is dictionary mein hum track karenge ki kis grid par konsa piece hai
    private Dictionary<CustomeGrid, GameObject> activePieces = new Dictionary<CustomeGrid, GameObject>();

    [Button("Generate/Update Modular Track")]
    public void StartGeneration()
    {
        if (splineGen == null || splineGen.splineGridPath.Count == 0) return;

        if (!Application.isPlaying)
        {
            GenerateInstant();
            return;
        }

        UpdateTrackWithAnimation(splineGen.splineGridPath);
    }

    public void UpdateTrackWithAnimation(List<CustomeGrid> newPath)
    {
        // return;
        if (piecesParent == null) piecesParent = this.transform;

        // --- STEP 1: Filter the path ---
        // Hum sirf unhi nodes ko use karenge jo "CustomeGrid" hain
        // Taki spline ke extra mid-points track piece generation ko kharab na karein
        List<CustomeGrid> gridOnlyPath = new List<CustomeGrid>();
        foreach (var grid in newPath)
        {
            if (grid != null) gridOnlyPath.Add(grid);
        }

        // 1. Purane pieces hatao
        List<CustomeGrid> gridsToRemove = new List<CustomeGrid>();
        foreach (var kvp in activePieces)
        {
            if (!gridOnlyPath.Contains(kvp.Key)) gridsToRemove.Add(kvp.Key);
        }
        foreach (var grid in gridsToRemove)
        {
            if (activePieces.ContainsKey(grid))
            {
                Destroy(activePieces[grid]);
                activePieces.Remove(grid);
            }
        }

        // 2. Naye path ke hisaab se pieces update (Using gridOnlyPath)
        for (int i = 0; i < gridOnlyPath.Count; i++)
        {
            CustomeGrid curr = gridOnlyPath[i];
            CustomeGrid prev = (i == 0) ? (isLoop ? gridOnlyPath[gridOnlyPath.Count - 1] : null) : gridOnlyPath[i - 1];
            CustomeGrid next = (i == gridOnlyPath.Count - 1) ? (isLoop ? gridOnlyPath[0] : null) : gridOnlyPath[i + 1];

            if (prev != null && next != null)
            {
                ProcessPiece(prev, curr, next);
            }
        }
    }

    void ProcessPiece(CustomeGrid prev, CustomeGrid curr, CustomeGrid next)
    {
        Vector3 dP = (prev.transform.position - curr.transform.position).normalized;
        Vector3 dN = (next.transform.position - curr.transform.position).normalized;

        GameObject prefabToUse;
        float targetYRotation = 0;

        if (Mathf.Abs(Vector3.Dot(dP, dN) + 1.0f) < 0.1f)
        {
            prefabToUse = straightPiece;
            targetYRotation = Quaternion.LookRotation(dN).eulerAngles.y;
        }
        else
        {
            prefabToUse = cornerPiece;
            if ((dP.x > 0.5f && dN.z > 0.5f) || (dN.x > 0.5f && dP.z > 0.5f)) targetYRotation = 0;
            else if ((dP.x > 0.5f && dN.z < -0.5f) || (dN.x > 0.5f && dP.z < -0.5f)) targetYRotation = 90;
            else if ((dP.x < -0.5f && dN.z < -0.5f) || (dN.x < -0.5f && dP.z < -0.5f)) targetYRotation = 180;
            else if ((dP.x < -0.5f && dN.z > 0.5f) || (dN.x < -0.5f && dP.z > 0.5f)) targetYRotation = 270;
        }

        Vector3 finalPos = curr.transform.position + Vector3.up * yOffset;

        if (activePieces.ContainsKey(curr))
        {
            GameObject existingPiece = activePieces[curr];
            if (existingPiece.name.Replace("(Clone)", "").Trim() != prefabToUse.name)
            {
                Destroy(existingPiece);
                activePieces.Remove(curr);
                SpawnNewAnimatedPiece(prefabToUse, finalPos, targetYRotation, curr);
            }
            else
            {
                if (Mathf.Abs(existingPiece.transform.eulerAngles.y - targetYRotation) > 0.1f)
                {
                    existingPiece.transform.DORotate(new Vector3(0, targetYRotation, 0), 0.3f);
                }
            }
        }
        else
        {
            SpawnNewAnimatedPiece(prefabToUse, finalPos, targetYRotation, curr);
        }
    }
    void SpawnNewAnimatedPiece(GameObject prefab, Vector3 finalPos, float angle, CustomeGrid gridKey)
    {
        Vector3 spawnPos = finalPos + Vector3.up * startYOffset;
        GameObject newPiece = Instantiate(prefab, spawnPos, Quaternion.Euler(fallingRotation), piecesParent);

        // Assign name to compare later
        newPiece.name = prefab.name;

        newPiece.transform.DOMove(finalPos, animDuration).SetEase(Ease.OutBounce);
        newPiece.transform.DORotate(new Vector3(0, angle, 0), rotationAnimDuration, RotateMode.FastBeyond360).SetEase(Ease.OutQuad);

        activePieces.Add(gridKey, newPiece);
    }
    [Button("Clear All Pieces")]
    public void ClearModularTrack()
    {
        foreach (var kvp in activePieces)
        {
            if (kvp.Value != null) DestroyImmediate(kvp.Value);
        }
        activePieces.Clear();

        if (piecesParent != null)
        {
            for (int i = piecesParent.childCount - 1; i >= 0; i--)
                DestroyImmediate(piecesParent.GetChild(i).gameObject);
        }
    }

    void GenerateInstant()
    {
        // Editor mode ke liye normal generation
        ClearModularTrack();
        // ... (Optional: Fast loop for editor)
    }
}