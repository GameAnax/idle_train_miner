using UnityEngine;
using Dreamteck.Splines;

public class TrainLoopHandler : MonoBehaviour
{
    public SplineFollower follower;
    public SplineGenerator splineGen;

    void Start()
    {
        follower = GetComponent<SplineFollower>();
        // Jab train loop khatam karegi tab ye event fire hoga
        follower.onEndReached += (double d) =>
        {
            // Train loop khatam karte hi naya rasta apply karein
            splineGen.UpdateTrainSplineNow();
            Debug.Log("Train spline updated at loop end.");
        };
    }


}