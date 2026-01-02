using UnityEngine;
using System.Collections.Generic;

public class DummyBogeyLogic : MonoBehaviour
{
    public LayerMask gridLayer;
    public Vector3 boxSize = new Vector3(1.5f, 1.5f, 1.5f);
    public float offset = 2.0f;


    [Header("Track Reference")]
    public ModularGridAligner trackAligner;

    void FixedUpdate()
    {
        // Detection point calculation
        Vector3 detectionPoint = transform.TransformPoint(new Vector3(-offset, 0, 0));
        Collider[] hitColliders = Physics.OverlapBox(detectionPoint, boxSize / 2, transform.rotation, gridLayer);

        bool trackNeedsUpdate = false;

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out CustomeGrid grid))
            {
                // Agar health khatam ho gayi hai aur grid abhi tak "Clear" mark nahi hua hai
                if (grid.currentHealth <= 0 && !grid.isClear)
                {
                    // Logic processing yahan hogi
                    grid.OnGridDestroyed();
                    trackNeedsUpdate = true;
                    // Debug.Log("Dummy Bogey Processed Logic for: " + hit.name);
                }
            }
        }

        // if (trackNeedsUpdate && trackAligner != null)
        // {
        //     trackAligner.StartGeneration();
        // }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Red for logic/dummy detection
        Vector3 detectionPoint = transform.TransformPoint(new Vector3(-offset, 0, 0));
        Gizmos.matrix = Matrix4x4.TRS(detectionPoint, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}