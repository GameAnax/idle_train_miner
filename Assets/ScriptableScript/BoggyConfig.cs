using UnityEngine;

[CreateAssetMenu(fileName = "BoggyConfig", menuName = "Scriptable Objects/BoggyConfig")]
public class BoggyConfig : ScriptableObject
{
    public BoggyType boggyType;
    public IdleCurrency baseValue;
    public int maxBoggy = 32;
    public int totalSpawn = 0;
    public bool isLimitReached;
    public IdleCurrency boggyDamage;

    // public void SetUpOnStart()
    // {
    //     if (isLimitReached)
    //     {
    //         totalSpawn = maxBoggy;
    //     }
    //     else
    //     {
    //         //TODO:- check and set other data
    //     }
    // }

    // public void UpdateData()
    // {
    //     if (isLimitReached) return;
    //     totalSpawn += 1;
    //     if (totalSpawn == maxBoggy)
    //     {
    //         isLimitReached = true;
    //     }
    // }

    public void Calculte()
    {
        if (isLimitReached) return;
        isLimitReached = totalSpawn == maxBoggy;
    }

    // public float GetBoggyDamage(BoggyType boggyType)
    // {
    //     return this.boggyType == boggyType ? boggyDamage : 0;
    // }
}
