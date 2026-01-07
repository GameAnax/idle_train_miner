using UnityEngine;
using System.Collections.Generic;

public class BoggyDamage : MonoBehaviour
{
    public float damageValue = 50f;
    public LayerMask gridLayer;
    public Vector3 boxSize = new Vector3(1.5f, 1.5f, 1.5f);
    public float leftOffset = 2.0f;
    public float forwardOffset = 0f;

    // Isme hum track rakhenge ki abhi kaunse grids box ke andar hain
    private List<CustomeGrid> currentGridsInBox = new List<CustomeGrid>();
    // private List<CustomeGrid> gridsToRemove = new List<CustomeGrid>();

    void FixedUpdate()
    {
        Vector3 detectionPoint = transform.TransformPoint(new Vector3(-leftOffset, 0, forwardOffset));

        // 1. Current frame mein jitne grids box ke andar hain unhe pakadna
        Collider[] hitColliders = Physics.OverlapBox(detectionPoint, boxSize / 2, transform.rotation, gridLayer);

        // Temporary list for this frame
        List<CustomeGrid> gridsFoundThisFrame = new List<CustomeGrid>();

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out CustomeGrid grid))
            {
                gridsFoundThisFrame.Add(grid);

                // 2. Agar ye grid naya hai (pehle list mein nahi tha), toh damage do
                if (!currentGridsInBox.Contains(grid))
                {
                    grid.TakeDamage(damageValue);
                    currentGridsInBox.Add(grid);
                    grid.SetCube(transform.position);
                }
            }
        }

        // 3. Cleanup: Agar koi grid box se bahar nikal gaya hai, toh use list se hatao
        // Taaki wo dobara damage ho sake jab bogey wapas aaye
        for (int i = currentGridsInBox.Count - 1; i >= 0; i--)
        {
            if (!gridsFoundThisFrame.Contains(currentGridsInBox[i]))
            {
                currentGridsInBox.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green; // Clear color for active detection
        Vector3 detectionPoint = transform.TransformPoint(new Vector3(-leftOffset, 0, forwardOffset));
        Gizmos.matrix = Matrix4x4.TRS(detectionPoint, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}