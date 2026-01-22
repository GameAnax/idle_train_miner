using UnityEngine;

public class Debries : MonoBehaviour
{
    public JumpEffect jumpEffect;

    public int debriCapacity;
    public int Multiplier { get; set; }
    public bool isCollected;

    public Vector2 gridPostion; //parent grid position
    public Vector3 currentPosition;
    private Color currentColor;
    public Color GetColor => currentColor;

    public MaterialPropertyBlock materialPropertyBlock;
    public Renderer rendererBlock;

    public int GetCoinAmount()
    {
        return debriCapacity * Multiplier;
    }


    void Awake()
    {
        materialPropertyBlock ??= new MaterialPropertyBlock();
        rendererBlock.GetPropertyBlock(materialPropertyBlock);
    }

    private void InitPropertyBlock()
    {
        materialPropertyBlock = new MaterialPropertyBlock();
    }

    public void UpdateColor(Color color)
    {
        if (materialPropertyBlock == null) InitPropertyBlock();
        materialPropertyBlock.SetColor("_BaseColor", color);
        rendererBlock.SetPropertyBlock(materialPropertyBlock);
        currentColor = color;
    }


    public void UpdateData(Vector2 gridPosition)
    {
        this.gridPostion = gridPosition;
        currentPosition = transform.position;
    }

    // public void SetInStorage(StorageBoggy storageBoggy)
    // {
    //     isCollected = true;
    //     transform.parent = storageBoggy.debriesContainer.transform;
    //     Vector3 storePosition = storageBoggy.CalculateStackPosition();
    //     jumpEffect.StartJump(transform.position, storePosition, 1, 0.2f, () =>
    //     {
    //         // transform.parent = storageBoggy.transform;
    //         transform.position = storePosition;
    //         transform.rotation = Quaternion.identity;
    //     });
    // }
}
