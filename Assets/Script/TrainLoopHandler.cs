using UnityEngine;
using Dreamteck.Splines;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrainLoopHandler : MonoBehaviour
{
    public SplineFollower follower;
    public SplineGenerator splineGen;
    public TrainSplineDriver trainSplineDriver;

    public SplineComputer[] splines = new SplineComputer[3];
    private int currentIndex = 0;
    private CustomeGrid lastCustomeGridAddedInSpline;
    private int pointsPerSpline = 5;

    public SplineComputer combinedSplineComputer;
    public SplineComputer previewsCombinedSplineComputer;

    public List<CustomeGrid> customeGrids;

    void Start()
    {
        customeGrids = new(splineGen.splineGridPath);
        // Debug.Log($"Total Grid - {customeGrids.Count}");
        pointsPerSpline = customeGrids.Count / 3;
        // Debug.Log($"Point on Each spline - {pointsPerSpline}");

        UpdateSpline(spline: splines[0]);
        UpdateSpline(spline: splines[1]);

        MergeTwoSplines();

        follower = GetComponent<SplineFollower>();
        // follower.spline = splines[splineCurrentIndex];
        follower.spline = combinedSplineComputer;
        // currentSplineComputer = splines[splineCurrentIndex]; ;
        // Jab train loop khatam karegi tab ye event fire hoga
        follower.onEndReached += (double d) =>
        {
            trainSplineDriver.isMainComplete = true;
            // Debug.Log("Train Path complete");
            CopySplineData();
            // Debug.Log("Previews Path Created");

            customeGrids = new(splineGen.splineGridPath);

            // pointsPerSpline = customeGrids.Count / 3;
            UpdateSpline(spline: splines[0]);
            UpdateSpline(spline: splines[1]);


            MergeTwoSplines();



            // int indexForUpdatespline = splineCurrentIndex + 2;
            // int index = indexForUpdatespline % splines.Length;
            // UpdateSpline(spline: splines[index]);

            // int index_nextSplineForAsssignToTrain = splineCurrentIndex + 1;
            // index = index_nextSplineForAsssignToTrain % splines.Length;
            // follower.spline = splines[index];

            // follower.SetPercent(0);
            // follower.RebuildImmediate();

            // currentSplineComputer = splines[index];

            // splineCurrentIndex++;
            //TODO:- will reset splinecurrentindex for now not needed


            // foreach (var item in trainSplineDriver.boggies)
            // {
            //     item.wagon.GetSegment.spline = follower.spline;
            // }
        };
    }

    public void MergeTwoSplines()
    {

        SplinePoint[] pointsA = splines[0].GetPoints();


        SplinePoint[] pointsB = splines[1].GetPoints();

        // 2. Naya array banana jo dono ka total size ho
        SplinePoint[] combinedPoints = new SplinePoint[pointsA.Length + pointsB.Length];

        // 3. Pehle Spline A ke points copy karna
        for (int i = 0; i < pointsA.Length; i++)
        {
            combinedPoints[i] = pointsA[i];
        }

        // 4. Phir Spline B ke points copy karna
        for (int i = 0; i < pointsB.Length; i++)
        {
            // Index offset: pointsA.Length se shuru karenge
            combinedPoints[pointsA.Length + i] = pointsB[i];
        }

        // 5. Main Spline ko naye points dena
        combinedSplineComputer.SetPoints(combinedPoints);
        combinedSplineComputer.RebuildImmediate();

    }
    public void CopySplineData()
    {
        if (combinedSplineComputer == null || previewsCombinedSplineComputer == null) return;

        // 1. Saare points ko array mein nikalen
        SplinePoint[] points = combinedSplineComputer.GetPoints();

        // 2. Doosre spline computer par points set karein
        previewsCombinedSplineComputer.SetPoints(points);

        // 3. Settings copy karein (optional but recommended)
        previewsCombinedSplineComputer.type = combinedSplineComputer.type;
        previewsCombinedSplineComputer.sampleMode = combinedSplineComputer.sampleMode;

        // 4. Sabse zaroori: Spline ko rebuild karein taaki changes apply hon
        previewsCombinedSplineComputer.RebuildImmediate();
        trainSplineDriver.UpdateSpline();

        // Debug.Log("Spline copied successfully!");
    }



    void UpdateSpline(SplineComputer spline)
    {
        int lastIndexAdded = 0;
        if (lastCustomeGridAddedInSpline != null)
        {
            int index = customeGrids.IndexOf(lastCustomeGridAddedInSpline);
            // Debug.Log($"last custome grid index - {index}, Current Index - {currentIndex}");
            if (index != -1) currentIndex = index;

        }

        List<Vector3> pointPositions = new List<Vector3>();
        for (int i = 0; i < pointsPerSpline; i++)
        {
            lastCustomeGridAddedInSpline = customeGrids[currentIndex];
            Vector3 currentPos = customeGrids[currentIndex].transform.position;
            pointPositions.Add(currentPos);
            if (i < pointsPerSpline)
            {
                int tempIndex = currentIndex + 1;
                if (tempIndex >= customeGrids.Count)
                {
                    tempIndex = 0;
                }
                if (i == pointsPerSpline - 1)
                {
                    // Debug.Log("Last index");
                    continue;
                }
                Vector3 nextPos = customeGrids[tempIndex].transform.position;
                pointPositions.Add(Vector3.Lerp(currentPos, nextPos, 0.5f));
            }
            lastIndexAdded = currentIndex;
            currentIndex++;
            if (currentIndex >= customeGrids.Count)
            {
                currentIndex = 0; // Wapas 0 se start
            }
        }
        SplinePoint[] newPoints = new SplinePoint[pointPositions.Count];
        for (int i = 0; i < pointPositions.Count; i++)
        {
            newPoints[i] = new SplinePoint();
            newPoints[i].position = pointPositions[i];
            newPoints[i].normal = Vector3.up;
            newPoints[i].size = 1f;
        }



        spline.SetPoints(newPoints);
        currentIndex = lastIndexAdded;
        spline.RebuildImmediate();
    }
}