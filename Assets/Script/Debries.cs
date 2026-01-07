using UnityEngine;

public class Debries : MonoBehaviour
{
    public JumpEffect jumpEffect;

    public int debriCapacity;
    public bool isCollected;

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
