using UnityEngine;

public class InfiniteRingGenerator : MonoBehaviour
{
    [Header("Settings")]
    public GameObject cubePrefab;
    public float spacing = 1.1f; // Slightly more than 1 to avoid z-fighting

    [Header("Growth Control")]
    [Tooltip("Increase this to add more outer rings")]
    public int targetLayer = 1;

    private int currentMaxLayer = 0;

    void Update()
    {
        // If the user increases the target layer, build the missing shells
        if (targetLayer > currentMaxLayer)
        {
            for (int i = currentMaxLayer + 1; i <= targetLayer; i++)
            {
                BuildSingleLayer(i);
            }
            currentMaxLayer = targetLayer;
        }
    }


    [EasyButtons.Button]
    void BuildSingleLayer(int layerIndex)
    {
        // The distance from center for this specific shell
        int extent = layerIndex;

        for (int x = -extent; x <= extent; x++)
        {
            for (int z = -extent; z <= extent; z++)
            {
                // Only place cubes if they are on the boundary of the CURRENT layer
                bool isEdge = (Mathf.Abs(x) == extent || Mathf.Abs(z) == extent);

                if (isEdge)
                {
                    Vector3 position = new Vector3(x * spacing, 0, z * spacing);
                    GameObject newCube = Instantiate(cubePrefab, position, Quaternion.identity);

                    // Parent them to this object to keep the Hierarchy clean
                    newCube.transform.parent = this.transform;
                }
            }
        }
    }
}