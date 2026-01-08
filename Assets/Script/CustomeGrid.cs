using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CustomeGrid : MonoBehaviour
{
    public MeshType meshType;
    public bool isClear;
    public bool isUsable = true;
    public bool isOuterBoundary = false;
    public bool isPermanentlyDisabled = false;
    public int gridLayerInt;
    public Vector2 gridPosition;
    public int gpuMeshIndex;
    public int objectIndex;
    public LayerMask layerMaskForDebriePerentSet;

    [Header("Health System")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Blink Settings")]
    public Color blinkColor = Color.white;
    public float blinkDuration = 0.15f;

    [Header("Neighbors")]
    public CustomeGrid rightGrid;
    public CustomeGrid leftGrid;
    public CustomeGrid topGrid;
    public CustomeGrid bottomGrid;

    [Header("Track References")]
    public GameObject myStraightPiece;
    public GameObject myCornerPiece;
    private GameObject currentActivePiece;


    public float distance; // Temp only for check

    public SplineGenerator splineGenerator;
    public TrainSplineDriver trainSplineDriver;
    public Debries debriPrefab;

    private MeshRenderer meshRenderer;
    private Color originalColor;
    private MaterialPropertyBlock propBlock;
    private bool _isPendingSplineUpdate = false;
    private float lastDamageValue;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    [Header("References"), HideInInspector]
    public Transform cubeContainer;




    void Awake()
    {
        currentHealth = maxHealth;
        meshRenderer = GetComponent<MeshRenderer>();
        propBlock = new MaterialPropertyBlock();

        if (meshRenderer != null)
        {
            // Original color ko initial material se lena
            meshRenderer.enabled = false;
            originalColor = meshRenderer.sharedMaterial.GetColor(BaseColorId);
        }
    }

    public void UpdateUsability()
    {
        if (gridPosition.x == 8 && gridPosition.y == 5)
        {
            // Debug.Log("e");
        }

        if (!isClear || isPermanentlyDisabled)
        {
            isUsable = false;
            return;
        }

        // Outer Boundary (Yellow Layer) hamesha usable rahegi
        if (isOuterBoundary) { isUsable = true; return; }

        // 1. Check 4 Main Sides (T, B, L, R)
        bool T = (topGrid != null && topGrid.isClear);
        bool B = (bottomGrid != null && bottomGrid.isClear);
        bool L = (leftGrid != null && leftGrid.isClear);
        bool R = (rightGrid != null && rightGrid.isClear);

        if (gridLayerInt == 1)
        {
            if (bottomGrid == null)
            {
                B = true;
            }
            if (topGrid == null) T = true;
            if (leftGrid == null) L = true;
            if (rightGrid == null) R = true;
        }


        // 2. Check 4 Diagonals (Corner neighbors)
        // Hum padosi ke padosi se diagonals nikaalte hain
        bool TL = (topGrid != null && topGrid.leftGrid != null && topGrid.leftGrid.isClear);
        bool TR = (topGrid != null && topGrid.rightGrid != null && topGrid.rightGrid.isClear);
        bool BL = (bottomGrid != null && bottomGrid.leftGrid != null && bottomGrid.leftGrid.isClear);
        bool BR = (bottomGrid != null && bottomGrid.rightGrid != null && bottomGrid.rightGrid.isClear);

        if (gridLayerInt == 1)
        {
            bool isCorner = IsCorner();
            if (isCorner)
            {
                if (TL && TR && BL)
                {
                    // if(bottomGrid.rightGrid == null)
                    //Top Left Corner
                    BR = true;
                }
                if (TR && TL && BR)
                {
                    //Top Right Corner
                    BL = true;
                }
                if (TL && BL && BR)
                {
                    //Bottom Left
                    TR = true;
                }
                if (TR && BL && BR)
                {
                    //Top Left Corner
                    TL = true;
                }
            }

            if (topGrid == null && !TL && !TR)
            {
                TL = TR = true;
            }
            if (topGrid != null && rightGrid == null && bottomGrid != null && rightGrid == null && !TR && !BR)
            {
                TR = BR = true;
            }
            if (bottomGrid == null && !BL && !BR)
            {
                BL = BR = true;
            }
            if (topGrid != null && leftGrid == null && bottomGrid != null && leftGrid == null && !TL && !BL)
            {
                TL = BL = true;
            }
        }

        // --- 3x3 Logic (8-way Check) ---
        // Agar saare 8 padosi (sides + diagonals) clear hain, 
        // tabhi isUsable false hoga (Iska matlab Layer 2 ne ise gher liya hai).
        if (T && B && L && R && TL && TR && BL && BR)
        {
            isPermanentlyDisabled = true;
            isUsable = false;
            // gameObject.SetActive(false);
        }
        else
        {
            // 3. Common Path Logic (Horizontal, Vertical, Corners)
            // Agar rasta 2-grid wide hai (Horizontal ya Vertical) 
            // ya Corner mod hai, toh usable true rahega.
            bool isHorizontal = (L || R);
            bool isVertical = (T || B);
            bool isCorner = (T && R) || (T && L) || (B && R) || (B && L);

            if (isHorizontal || isVertical || isCorner)
            {
                isUsable = true;
            }
            else
            {
                isUsable = false; // Isolated grid
            }
        }
    }
    public void TakeDamage(float amount)
    {
        if (isClear) return;

        currentHealth -= amount;
        lastDamageValue = amount;

        // Blink effect start karein
        StopAllCoroutines();
        StartCoroutine(BlinkRoutine());

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (meshRenderer != null) meshRenderer.enabled = false;
            GridRenderManager.instance.HideMesh(meshType: meshType, gpuMeshIndex: gpuMeshIndex);
            // OnGridDestroyed();
        }
    }

    public void RegisterGridForUpdate()
    {
        if (splineGenerator != null)
        {
            _isPendingSplineUpdate = true;
            // splineGenerator.GenerateSpline();
            trainSplineDriver.RegisterPendingGrid(this);
        }
    }

    public void OnGridDestroyed()
    {
        isClear = true;
        if (meshRenderer != null) meshRenderer.enabled = false;

        // Neighbor updates for usability chain reaction
        UpdateUsability();
        if (topGrid != null) topGrid.UpdateUsability();
        if (bottomGrid != null) bottomGrid.UpdateUsability();
        if (leftGrid != null) leftGrid.UpdateUsability();
        if (rightGrid != null) rightGrid.UpdateUsability();

        GameManager.instance.CheckIsAllGridClear();

        // if (splineGenerator != null)
        // {
        //     _isPendingSplineUpdate = true;
        //     // splineGenerator.GenerateSpline();
        //     trainSplineDriver.RegisterPendingGrid(this);
        // }
    }
    public void TriggerSplineUpdate()
    {
        if (_isPendingSplineUpdate && splineGenerator != null)
        {
            splineGenerator.GenerateSpline();
            _isPendingSplineUpdate = false;
        }
    }
    // Editor mein changes hote hi ye apne aap call hoga
    private void OnValidate()
    {
        // Sirf tabhi kaam kare jab editor mode ho aur scene loaded ho
        if (!Application.isPlaying)
        {
            UpdateUsability();
        }
    }
    IEnumerator BlinkRoutine()
    {
        // Color change to Blink Color
        // SetColor(blinkColor);
        GridRenderManager.instance.BlinkMesh(meshType: meshType, gpuMeshIndex, blinkColor);

        yield return new WaitForSeconds(blinkDuration);

        // Back to Original Color
        // SetColor(originalColor);
        GridRenderManager.instance.BlinkMesh(meshType: meshType, gpuMeshIndex, originalColor);
    }
    private void SetColor(Color color)
    {
        if (meshRenderer == null) return;

        // Mobile optimized way to change color
        meshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(BaseColorId, color);
        meshRenderer.SetPropertyBlock(propBlock);
    }

    private bool IsCorner()
    {
        bool TR = topGrid != null && topGrid.rightGrid == null;
        bool TL = topGrid != null && topGrid.leftGrid == null;
        bool BL = bottomGrid != null && bottomGrid.leftGrid == null;
        bool BR = bottomGrid != null && bottomGrid.rightGrid == null;

        if (!TR && !TL && !BL && BR)
        {
            return true;
        }
        else if (!TL && !TR && !BR && BL)
        {
            return true;
        }
        else if (!TL && !BL && !BR && TR)
        {
            return true;
        }
        else if (!TR && !BR && !BL && TL)
        {
            return true;
        }


        return false;
    }
    private Vector3 GetRandom(Vector3 orignalPosition)
    {
        orignalPosition.x = GetNewPos(orignalPosition.x);
        orignalPosition.z = GetNewPos(orignalPosition.z);
        return orignalPosition;

        float GetNewPos(float value)
        {
            float tempRandom = UnityEngine.Random.Range(-0.3f, 0.3f);
            value += tempRandom;
            return value;
        }
    }

    //For Track
    public void SetTrackState(GameObject prefabToUse, float targetRotation, float animDuration, float startY)
    {
        // Yahan hum prefab ki script/type ke bajaye direct reference check karte hain
        // Taki name change ka issue na ho
        bool isStraight = (prefabToUse.name.ToLower().Contains("straight"));

        GameObject targetPiece = isStraight ? myStraightPiece : myCornerPiece;
        GameObject otherPiece = isStraight ? myCornerPiece : myStraightPiece;

        if (otherPiece != null) otherPiece.SetActive(false);
        if (targetPiece == null) return;

        if (!Application.isPlaying)
        {
            if (!targetPiece.activeSelf)
            {
                targetPiece.SetActive(true);
                targetPiece.transform.rotation = Quaternion.Euler(0, targetRotation, 0);
            }
            else
                targetPiece.transform.rotation = Quaternion.Euler(new Vector3(0, targetRotation, 0));
        }
        else
        {

            // Piece ko enable karein
            if (!targetPiece.activeSelf)
            {
                targetPiece.SetActive(true);

                // Animation se pehle initial state set karein
                // Vector3 finalPos = transform.position + Vector3.up * 0.1f;
                Vector3 finalPos = transform.position + Vector3.up * 0f;
                targetPiece.transform.position = finalPos + Vector3.up * startY;
                targetPiece.transform.rotation = Quaternion.Euler(0, targetRotation, 0);

                targetPiece.transform.DOKill();
                targetPiece.transform.DOMove(finalPos, animDuration).SetEase(Ease.OutBounce);
            }
            else
            {
                // Agar piece type wahi hai, toh sirf rotation smoothly change karein
                targetPiece.transform.DORotate(new Vector3(0, targetRotation, 0), 0.2f);
            }
        }
    }
    public void HideAllTracks()
    {
        if (myStraightPiece != null) myStraightPiece.SetActive(false);
        if (myCornerPiece != null) myCornerPiece.SetActive(false);
        currentActivePiece = null;

        // UpdateDerbiesPosition();
    }

    public bool IsInTrackList(CustomeGrid gridToCheck)
    {
        if (gridToCheck == null || splineGenerator == null) return false;

        List<CustomeGrid> track = splineGenerator.splineGridPath;
        for (int i = 0; i < track.Count; i++)
        {
            if (track[i] == gridToCheck) return true;
        }
        return false;
    }


    public void SetCube(Vector3 hitPosition)
    {
        if (rightGrid != null)
        {
            if (IsInTrackList(rightGrid))
            {
                rightGrid.GenerateDebrie(transform.position);
                return;
            }
        }
        if (leftGrid != null)
        {
            if (IsInTrackList(leftGrid))
            {
                leftGrid.GenerateDebrie(transform.position);
                return;
            }
        }
        if (topGrid != null)
        {
            if (IsInTrackList(topGrid))
            {
                topGrid.GenerateDebrie(transform.position);
                return;
            }
        }
        if (bottomGrid != null)
        {
            if (IsInTrackList(bottomGrid))
            {
                bottomGrid.GenerateDebrie(transform.position);
                return;
            }
        }
        return;

        // if (cubeContainer == null)
        // {
        //     GameObject containerObj = new GameObject("CubeContainer");
        //     cubeContainer = containerObj.transform;
        //     cubeContainer.SetParent(this.transform);
        //     cubeContainer.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        // }

        Debries debri = Instantiate(debriPrefab, transform.position, Quaternion.identity, transform);
        debri.transform.SetParent(cubeContainer);
        debri.debriCapacity = (int)lastDamageValue;
        debri.jumpEffect.StartJump(transform.position, GetRandom(hitPosition), 1, 0.2f, () =>
        {

            Vector3 center = debri.transform.position;
            Vector3 halfExtents = debri.transform.localScale / 2;

            Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, debri.transform.rotation, layerMaskForDebriePerentSet);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent(out CustomeGrid customeGrid))
                {
                    if (customeGrid.cubeContainer == null)
                    {
                        GameObject containerObj = new("CubeContainer");
                        customeGrid.cubeContainer = containerObj.transform;
                        customeGrid.cubeContainer.SetParent(customeGrid.transform);
                        customeGrid.cubeContainer.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    }
                    debri.transform.SetParent(customeGrid.cubeContainer);

                    if (!customeGrid.isUsable)
                    {
                        // UpdateDerbiesPosition();
                        Debug.Log("Grid is not used in track, but derie drop here");
                    }
                }
                break;
            }
        });
        // cube.transform.position = GetRandom(hitPosition);
        // cube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
    }

    private void GenerateDebrie(Vector3 startPosition)
    {
        if (cubeContainer == null)
        {
            GameObject containerObj = new("CubeContainer");
            cubeContainer = containerObj.transform;
            cubeContainer.SetParent(this.transform);
            cubeContainer.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
        Debries debri = Instantiate(debriPrefab, cubeContainer.position, Quaternion.identity, cubeContainer);
        debri.debriCapacity = (int)lastDamageValue;
        debri.jumpEffect.StartJump(startPosition, GetRandom(transform.position), 1, 0.2f);
    }

    public void SetUpNeighbourDebries()
    {
        foreach (Transform item in cubeContainer)
        {
            item.position = GetRandom(cubeContainer.transform.position);
        }
    }
}