using UnityEngine;
using Dreamteck.Splines;
using System.Collections.Generic;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrainLoopHandler : MonoBehaviour
{
    public SplineFollower follower;
    public SplineGenerator splineGen;
    public TrainSplineDriver trainSplineDriver;
    public TrainManager trainManager;

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

        if (trainManager.IsPathAvailable())
        {
            trainManager.Load();
        }
        else
        {
            UpdateSpline(spline: splines[0]);
            UpdateSpline(spline: splines[1]);

            MergeTwoSplines();
        }

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
        };
    }

    public void UpdateSplineOnStart()
    {
        lastCustomeGridAddedInSpline = customeGrids.Find(i => i.gridPosition == trainManager.trainSaveData.lastGridPosition);

        SplinePoint[] newMainPoints = new SplinePoint[trainManager.trainSaveData.mainSplinePoints.Count];
        for (int i = 0; i < newMainPoints.Length; i++)
        {
            newMainPoints[i] = new SplinePoint();
            newMainPoints[i].position = trainManager.trainSaveData.mainSplinePoints[i];
            newMainPoints[i].normal = Vector3.up;
            newMainPoints[i].size = 1f;
        }
        combinedSplineComputer.SetPoints(newMainPoints);
        combinedSplineComputer.RebuildImmediate();


        SplinePoint[] newPoints = new SplinePoint[trainManager.trainSaveData.previewsSplinePoints.Count];
        for (int i = 0; i < newPoints.Length; i++)
        {
            newPoints[i] = new SplinePoint();
            newPoints[i].position = trainManager.trainSaveData.previewsSplinePoints[i];
            newPoints[i].normal = Vector3.up;
            newPoints[i].size = 1f;
        }
        previewsCombinedSplineComputer.SetPoints(newPoints);
        previewsCombinedSplineComputer.RebuildImmediate();
        trainSplineDriver.UpdateSpline();

    }

    public void MergeTwoSplines()
    {

        SplinePoint[] pointsA = splines[0].GetPoints();


        SplinePoint[] pointsB = splines[1].GetPoints();

        // 2. Naya array banana jo dono ka total size ho
        SplinePoint[] combinedPoints = new SplinePoint[pointsA.Length + pointsB.Length];

        trainManager.trainSaveData.mainSplinePoints?.Clear();

        // 3. Pehle Spline A ke points copy karna
        for (int i = 0; i < pointsA.Length; i++)
        {
            combinedPoints[i] = pointsA[i];
            trainManager.trainSaveData.mainSplinePoints.Add(pointsA[i].position);
        }

        // 4. Phir Spline B ke points copy karna
        for (int i = 0; i < pointsB.Length; i++)
        {
            // Index offset: pointsA.Length se shuru karenge
            combinedPoints[pointsA.Length + i] = pointsB[i];
            trainManager.trainSaveData.mainSplinePoints.Add(pointsB[i].position);
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

        var pointList = previewsCombinedSplineComputer.GetPoints();
        trainManager.trainSaveData.previewsSplinePoints.Clear();
        for (int i = 0; i < pointList.Length; i++)
        {
            trainManager.trainSaveData.previewsSplinePoints.Add(pointList[i].position);
        }

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
            if (lastCustomeGridAddedInSpline != null)
            {
                trainManager.trainSaveData.lastGridPosition = lastCustomeGridAddedInSpline.gridPosition;
            }
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