using UnityEngine;

public class Collector : MonoBehaviour
{
    public StorageBoggy storageBoggy;
    public LayerMask grassLayer;

    private void OnTriggerEnter(Collider other)
    {
        // LayerMask check: Is 'other' object ka layer hamare selected layer mein hai?
        if (((1 << other.gameObject.layer) & grassLayer) != 0)
        {
            if (other.transform.parent.TryGetComponent(out Debries debries))
            {
                if (!storageBoggy.CheckIsStorageFull(debries.debriCapacity))
                {
                    storageBoggy.SetUpDebrie(debries);
                }
            }
        }
    }
}
