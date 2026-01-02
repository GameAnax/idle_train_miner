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
    public float rotationAnimDuration = 0.3f; // Slightly increased for smoothness
    public Vector3 fallingRotation = new Vector3(0, 180, 0);

    [Header("Settings")]
    public Transform piecesParent;
    public float yOffset = 0.1f;
    public bool isLoop = true;

    private Dictionary<CustomeGrid, GameObject> activePieces = new Dictionary<CustomeGrid, GameObject>();

    [Button("Generate/Update Modular Track")]
    public void StartGeneration()
    {
        if (splineGen == null || splineGen.splineGridPath == null || splineGen.splineGridPath.Count == 0) return;

        if (!Application.isPlaying)
        {
            GenerateInstant();
            return;
        }

        UpdateTrackWithAnimation(splineGen.splineGridPath);
    }

    public void UpdateTrackWithAnimation(List<CustomeGrid> newPath)
    {
        if (piecesParent == null) piecesParent = this.transform;

        // Create a HashSet for faster lookup
        HashSet<CustomeGrid> newPathSet = new HashSet<CustomeGrid>(newPath);

        // 1. Remove pieces that are no longer in the path
        List<CustomeGrid> toRemove = new List<CustomeGrid>();
        foreach (var grid in activePieces.Keys)
        {
            if (!newPathSet.Contains(grid)) toRemove.Add(grid);
        }

        foreach (var grid in toRemove)
        {
            if (activePieces.TryGetValue(grid, out GameObject obj))
            {
                if (obj != null)
                {
                    obj.transform.DOKill(); // Stop animations before destroying
                    Destroy(obj);
                }
                activePieces.Remove(grid);
            }
        }

        // 2. Process existing or new pieces
        for (int i = 0; i < newPath.Count; i++)
        {
            CustomeGrid curr = newPath[i];
            if (curr == null) continue;

            CustomeGrid prev = (i == 0) ? (isLoop ? newPath[newPath.Count - 1] : null) : newPath[i - 1];
            CustomeGrid next = (i == newPath.Count - 1) ? (isLoop ? newPath[0] : null) : newPath[i + 1];

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

        // Determine if straight or corner
        if (Mathf.Abs(Vector3.Dot(dP, dN) + 1.0f) < 0.1f)
        {
            prefabToUse = straightPiece;
            targetYRotation = Quaternion.LookRotation(dN).eulerAngles.y;
        }
        else
        {
            prefabToUse = cornerPiece;
            // Logic for corner rotations
            if ((dP.x > 0.5f && dN.z > 0.5f) || (dN.x > 0.5f && dP.z > 0.5f)) targetYRotation = 0;
            else if ((dP.x > 0.5f && dN.z < -0.5f) || (dN.x > 0.5f && dP.z < -0.5f)) targetYRotation = 90;
            else if ((dP.x < -0.5f && dN.z < -0.5f) || (dN.x < -0.5f && dP.z < -0.5f)) targetYRotation = 180;
            else if ((dP.x < -0.5f && dN.z > 0.5f) || (dN.x < -0.5f && dP.z > 0.5f)) targetYRotation = 270;
        }

        Vector3 finalPos = curr.transform.position + Vector3.up * yOffset;

        if (activePieces.TryGetValue(curr, out GameObject existingPiece) && existingPiece != null)
        {
            // Check if the prefab type changed (Straight vs Corner)
            bool isSameType = existingPiece.name == prefabToUse.name;

            if (!isSameType)
            {
                // Different piece type: Replace it
                existingPiece.transform.DOKill();
                Destroy(existingPiece);
                activePieces.Remove(curr);
                SpawnNewAnimatedPiece(prefabToUse, finalPos, targetYRotation, curr);
            }
            else
            {
                // Same piece type: Just check if rotation needs update
                float currentRot = existingPiece.transform.eulerAngles.y;
                if (Mathf.Abs(Mathf.DeltaAngle(currentRot, targetYRotation)) > 0.1f)
                {
                    existingPiece.transform.DORotate(new Vector3(0, targetYRotation, 0), rotationAnimDuration).SetEase(Ease.Linear);
                }
            }
        }
        else
        {
            // Grid is empty: Spawn new
            SpawnNewAnimatedPiece(prefabToUse, finalPos, targetYRotation, curr);
        }
    }

    void SpawnNewAnimatedPiece(GameObject prefab, Vector3 finalPos, float angle, CustomeGrid gridKey)
    {
        Vector3 spawnPos = finalPos + Vector3.up * startYOffset;
        GameObject newPiece = Instantiate(prefab, spawnPos, Quaternion.Euler(fallingRotation), piecesParent);

        newPiece.name = prefab.name; // Crucial for type checking later

        // Animation
        newPiece.transform.DOMove(finalPos, animDuration).SetEase(Ease.OutBounce);
        newPiece.transform.DORotate(new Vector3(0, angle, 0), animDuration, RotateMode.FastBeyond360).SetEase(Ease.OutQuad);

        activePieces[gridKey] = newPiece;
    }

    [Button("Clear All Pieces")]
    public void ClearModularTrack()
    {
        foreach (var obj in activePieces.Values)
        {
            if (obj != null)
            {
                obj.transform.DOKill();
                if (Application.isPlaying) Destroy(obj);
                else DestroyImmediate(obj);
            }
        }
        activePieces.Clear();
    }

    void GenerateInstant()
    {
        ClearModularTrack();
        // Standard loop without DOTween for editor mode...
    }
}