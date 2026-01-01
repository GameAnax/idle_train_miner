using UnityEngine;
using System.Collections.Generic;
using EasyButtons;
using Dreamteck.Splines;
using System.Linq;

[RequireComponent(typeof(SplineComputer))]
public class SplineGenerator : MonoBehaviour
{
    public SplineComputer splineComputer;
    public SplineComputer trainSpline;

    public TrainLoopHandler trainLoopHandler;

    // public TrainSplineDriver trainSplineDriver;

    public bool isLoop = true;
    public List<CustomeGrid> splineGridPath = new List<CustomeGrid>();

    [Button("Generate Final Spline")]
    public void GenerateSpline()
    {
        RefreshAllUsability();
        ClockwiseRingGenerator gen = FindObjectOfType<ClockwiseRingGenerator>();
        if (gen != null) CreateSplineFromGrids(gen.spawnedCubes);
    }

    public void CreateSplineFromGrids(List<CustomeGrid> allGrids)
    {
        splineGridPath.Clear();
        HashSet<CustomeGrid> visited = new HashSet<CustomeGrid>();

        // --- STEP 1: Hamesha Leftmost aur Topmost grid ko StartNode banana ---
        // Isse agar left mein koi naya grid add hoga, toh wo automatic startNode ban jayega.
        CustomeGrid startNode = allGrids
            .Where(g => g.isClear && g.isUsable)
            .OrderBy(g => g.transform.position.x) // Sabse chhota X (Left)
            .ThenByDescending(g => g.transform.position.z) // Phir Sabse bada Z (Top)
            .FirstOrDefault();

        if (startNode == null) return;

        CustomeGrid current = startNode;
        // Direction Priority: Right -> Down -> Left -> Up (Clockwise flow)
        Vector2[] dirs = { Vector2.right, Vector2.down, Vector2.left, Vector2.up };
        int currentDirIndex = 0;

        while (current != null)
        {
            splineGridPath.Add(current);
            visited.Add(current);

            CustomeGrid next = null;

            // Loop Check
            if (splineGridPath.Count > 2)
            {
                foreach (Vector2 dir in dirs)
                {
                    if (GetNeighborByDir(current, dir) == startNode)
                    {
                        goto EndLoop;
                    }
                }
            }

            // Pathfinding logic (Aapka original priority logic)
            int[] checkOrder = { (currentDirIndex + 1) % 4, currentDirIndex, (currentDirIndex + 3) % 4 };

            foreach (int i in checkOrder)
            {
                CustomeGrid neighbor = GetNeighborByDir(current, dirs[i]);
                if (neighbor != null && neighbor.isClear && neighbor.isUsable && !visited.Contains(neighbor))
                {
                    if (IsPathViable(neighbor, visited, startNode))
                    {
                        next = neighbor;
                        currentDirIndex = i;
                        break;
                    }
                }
            }
            current = next;

            // Safety break
            if (splineGridPath.Count > allGrids.Count) break;
        }


    EndLoop:
        // Final check: Winding order fix (Shoelace Formula)
        // Taaki train hamesha Forward direction mein chale
        EnsureCorrectWinding();

        ApplyToDreamteckSpline();
    }
    private void EnsureCorrectWinding()
    {
        if (splineGridPath.Count < 3) return;

        float area = 0;
        for (int i = 0; i < splineGridPath.Count; i++)
        {
            Vector3 p1 = splineGridPath[i].transform.position;
            Vector3 p2 = splineGridPath[(i + 1) % splineGridPath.Count].transform.position;
            area += (p2.x - p1.x) * (p2.z + p1.z);
        }

        // Agar area positive hai matlab path clockwise hai.
        // Train direction ke hisaab se agar reverse hai toh:
        if (area < 0)
        {
            splineGridPath.Reverse();
        }
    }
    private bool IsPathViable(CustomeGrid node, HashSet<CustomeGrid> visited, CustomeGrid startNode)
    {
        // Agar node usable nahi hai, toh viable bhi nahi hai
        if (!node.isUsable) return false;

        CustomeGrid[] neighbors = { node.topGrid, node.rightGrid, node.bottomGrid, node.leftGrid };

        foreach (var n in neighbors)
        {
            // Loop completion check
            if (n == startNode && splineGridPath.Count > 2) return true;

            // Agla usable step dhoondo
            if (n != null && n.isClear && n.isUsable && !visited.Contains(n))
            {
                return true;
            }
        }
        return false;
    }



    private bool IsOnBoundary(CustomeGrid g)
    {
        if (g.topGrid != null && !g.topGrid.isClear) return true;
        if (g.bottomGrid != null && !g.bottomGrid.isClear) return true;
        if (g.leftGrid != null && !g.leftGrid.isClear) return true;
        if (g.rightGrid != null && !g.rightGrid.isClear) return true;
        return false;
    }

    private void ApplyToDreamteckSpline()
    {
        if (splineComputer == null) splineComputer = GetComponent<SplineComputer>();
        if (splineGridPath.Count < 2) return;

        List<Vector3> pointPositions = new List<Vector3>();
        for (int i = 0; i < splineGridPath.Count; i++)
        {
            Vector3 currentPos = splineGridPath[i].transform.position;
            pointPositions.Add(currentPos);

            if (i < splineGridPath.Count - 1)
            {
                Vector3 nextPos = splineGridPath[i + 1].transform.position;
                pointPositions.Add(Vector3.Lerp(currentPos, nextPos, 0.5f));
            }
            else if (isLoop)
            {
                Vector3 nextPos = splineGridPath[0].transform.position;
                pointPositions.Add(Vector3.Lerp(currentPos, nextPos, 0.5f));
            }
        }

        SplinePoint[] points = new SplinePoint[pointPositions.Count];
        for (int i = 0; i < pointPositions.Count; i++)
        {
            points[i] = new SplinePoint();
            points[i].position = pointPositions[i];
            points[i].normal = Vector3.up;
            points[i].size = 1f;
        }

        splineComputer.SetPoints(points);
        splineComputer.RebuildImmediate();
        if (isLoop) splineComputer.Close();
        else splineComputer.Break();


    }

    private CustomeGrid GetNeighborByDir(CustomeGrid current, Vector2 dir)
    {
        if (dir == Vector2.right) return current.rightGrid;
        if (dir == Vector2.up) return current.topGrid;
        if (dir == Vector2.left) return current.leftGrid;
        if (dir == Vector2.down) return current.bottomGrid;
        return null;
    }

    [Button("1. Refresh All Grids Usability")]
    public void RefreshAllUsability()
    {
        ClockwiseRingGenerator gen = FindObjectOfType<ClockwiseRingGenerator>();
        if (gen == null) return;
        foreach (var grid in gen.spawnedCubes)
        {
            if (grid != null) grid.UpdateUsability();
        }
        // Debug.Log("All Grids usability updated in Editor!");
    }

    public void UpdateTrainSplineNow()
    {
        // ApplyToSpline(trainSpline);


        // if (pendingPoints.Count < 2) return;

        // SplineFollower[] followers = FindObjectsOfType<SplineFollower>();
        // foreach (var f in followers)
        // {
        //     if (f.spline == trainSpline) f.Rebuild();
        // }
    }

    private void ApplyToSpline(SplineComputer target)
    {
        if (target == null || splineGridPath.Count < 2) return;

        // trainLoopHandler.BeforeCalculate();

        SplinePoint[] points = new SplinePoint[splineGridPath.Count];
        for (int i = 0; i < splineGridPath.Count; i++)
        {
            points[i] = new SplinePoint(splineGridPath[i].transform.position);
            points[i].normal = Vector3.up;
            points[i].size = 1f;
        }
        target.SetPoints(points);
        target.RebuildImmediate();
        if (isLoop) target.Close(); else target.Break();
        // trainLoopHandler.AfterRebuildCalculation();
    }
}