using UnityEngine;

public class CutterTextureRotate : MonoBehaviour
{
    public int materialIndex = 0;
    public float scrollSpeedX = 0.5f;
    public float scrollSpeedY = 0f;
    public Renderer rend;

    void Start()
    {
        // Object ka renderer component get karein
        if (rend == null)
            rend = GetComponent<Renderer>();

        if (materialIndex >= rend.materials.Length)
        {
            Debug.LogError("Material Index out of range on " + gameObject.name);
        }
    }

    void Update()
    {
        // Time ke hisaab se offset calculate karein
        float offsetX = Time.time * scrollSpeedX;
        float offsetY = Time.time * scrollSpeedY;

        // Material ki main texture offset set karein
        rend.material.mainTextureOffset = new Vector2(offsetX, offsetY);
    }
}