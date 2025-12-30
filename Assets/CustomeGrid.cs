using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomeGrid : MonoBehaviour
{
    public bool isClear;
    public bool isUsable = true;
    public bool isOuterBoundary = false;
    public bool isPermanentlyDisabled = false;
    public int gridLayerInt;
    public Vector2 gridPosition;
    public int objectIndex;

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

    public SplineGenerator splineGenerator;

    private MeshRenderer meshRenderer;
    private Color originalColor;
    private MaterialPropertyBlock propBlock;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");


    void Awake()
    {
        currentHealth = maxHealth;
        meshRenderer = GetComponent<MeshRenderer>();
        propBlock = new MaterialPropertyBlock();

        if (meshRenderer != null)
        {
            // Original color ko initial material se lena
            originalColor = meshRenderer.sharedMaterial.GetColor(BaseColorId);
        }
    }

    public void UpdateUsability()
    {
        if (gridPosition.x == 5 && gridPosition.y == 7)
        {
            Debug.Log("e");
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
            // if (!TR && !BR)
            // {
            //     TR = BR = true;
            // }
            if (bottomGrid == null && !BL && !BR)
            {
                BL = BR = true;
            }
            // if (!TL && !BL)
            // {
            //     TL = BL = true;
            // }
        }

        // --- 3x3 Logic (8-way Check) ---
        // Agar saare 8 padosi (sides + diagonals) clear hain, 
        // tabhi isUsable false hoga (Iska matlab Layer 2 ne ise gher liya hai).
        if (T && B && L && R && TL && TR && BL && BR)
        {
            isPermanentlyDisabled = true;
            isUsable = false;
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

        // Blink effect start karein
        StopAllCoroutines();
        StartCoroutine(BlinkRoutine());

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnGridDestroyed();
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

        if (splineGenerator != null)
        {
            splineGenerator.GenerateSpline();
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
        SetColor(blinkColor);

        yield return new WaitForSeconds(blinkDuration);

        // Back to Original Color
        SetColor(originalColor);
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
}