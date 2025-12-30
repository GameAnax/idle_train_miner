using UnityEngine;
using System.Collections.Generic;
using EasyButtons;
using Dreamteck.Splines;

[RequireComponent(typeof(SplineComputer))]
public class SplineGenerator : MonoBehaviour
{
    public SplineComputer splineComputer;
    public SplineComputer trainSpline;

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

        CustomeGrid startNode = allGrids.Find(g => g.isClear && g.isUsable && IsOnBoundary(g));
        if (startNode == null) startNode = allGrids.Find(g => g.isClear && g.isUsable);
        if (startNode == null) return;

        CustomeGrid current = startNode;
        Vector2[] dirs = { Vector2.right, Vector2.up, Vector2.left, Vector2.down };
        int currentDirIndex = 0;

        while (current != null)
        {
            splineGridPath.Add(current);
            visited.Add(current);

            CustomeGrid next = null;

            // --- STEP 1: Loop Closing Check ---
            if (splineGridPath.Count > 5)
            {
                foreach (Vector2 dir in dirs)
                {
                    CustomeGrid neighbor = GetNeighborByDir(current, dir);
                    if (neighbor == startNode)
                    {
                        current = null;
                        goto EndLoop;
                    }
                }
            }

            // --- STEP 2: Pathfinding with Viability & Boundary Priority ---
            int[] checkOrder = { (currentDirIndex + 1) % 4, currentDirIndex, (currentDirIndex + 3) % 4 };

            // Pehle viable boundary nodes check karein (Sabse best option)
            foreach (int i in checkOrder)
            {
                CustomeGrid neighbor = GetNeighborByDir(current, dirs[i]);

                if (neighbor != null && neighbor.isClear && neighbor.isUsable && !visited.Contains(neighbor))
                {
                    if (IsOnBoundary(neighbor) && IsPathViable(neighbor, visited, startNode))
                    {
                        next = neighbor;
                        currentDirIndex = i;
                        break;
                    }
                }
            }

            // Agar viable boundary node nahi mila, toh koi bhi viable usable neighbor le lo
            if (next == null)
            {
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
            }
            current = next;
        }

    EndLoop:
        ApplyToDreamteckSpline();
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

    // Look-ahead logic: Kya is node par jane ke baad aage rasta mil raha hai?
    // private bool IsPathViable(CustomeGrid node, HashSet<CustomeGrid> visited, CustomeGrid startNode)
    // {
    //     CustomeGrid[] neighbors = { node.topGrid, node.rightGrid, node.bottomGrid, node.leftGrid };

    //     foreach (var n in neighbors)
    //     {
    //         // Case 1: Start node mil gaya (Loop complete)
    //         if (n == startNode && splineGridPath.Count > 2) return true;

    //         // Case 2: Agla step check karein
    //         if (n != null && n.isClear && n.isUsable && !visited.Contains(n))
    //         {
    //             // LOOK-AHEAD: Check neighbor's neighbors
    //             CustomeGrid[] nextNeighbors = { n.topGrid, n.rightGrid, n.bottomGrid, n.leftGrid };
    //             foreach (var nn in nextNeighbors)
    //             {
    //                 if (nn == startNode) return true;
    //                 // Agar agla padosi available hai aur wo 'current' node nahi hai jahan se hum aa rahe hain
    //                 if (nn != null && nn.isClear && nn.isUsable && !visited.Contains(nn) && nn != node)
    //                 {
    //                     return true;
    //                 }
    //             }
    //         }
    //     }
    //     return false;
    // }

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
        Debug.Log("All Grids usability updated in Editor!");
    }

    public void UpdateTrainSplineNow()
    {
        // if (pendingPoints.Count < 2) return;
        ApplyToSpline(trainSpline);

        // SplineFollower[] followers = FindObjectsOfType<SplineFollower>();
        // foreach (var f in followers)
        // {
        //     if (f.spline == trainSpline) f.Rebuild();
        // }
    }

    private void ApplyToSpline(SplineComputer target)
    {
        if (target == null || splineGridPath.Count < 2) return;

        SplinePoint[] points = new SplinePoint[splineGridPath.Count];
        for (int i = 0; i < splineGridPath.Count; i++)
        {
            points[i] = new SplinePoint(splineGridPath[i].transform.position);
            points[i].normal = Vector3.up;
            points[i].size = 1f;
        }
        target.SetPoints(points);
        if (isLoop) target.Close(); else target.Break();
    }
}