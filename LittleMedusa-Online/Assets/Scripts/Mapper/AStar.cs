using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AStar : MonoBehaviour
{
    public Spot[,] spotGrid;
    [HideInInspector]
    public List<Spot> openSet;
    [HideInInspector]
    public List<Spot> closedSet;
    [HideInInspector]
    public Spot startSpot;
    [HideInInspector]
    public Spot targetSpot;
    [HideInInspector]
    public Spot current;

    [HideInInspector]
    public int cols;
    [HideInInspector]
    public int rows;

    bool computePath;
    bool nosolution;

    public List<Vector3Int> aiPathToTarget = new List<Vector3Int>();

    [Serializable]
    public class Spot
    {
        public float f;
        public float g;
        public float h;

        public int i;
        public int j;

        public bool isBlocked;

        public Spot parentSpot = null;

        public int posX;
        public int posY;

        public List<Spot> neighbour;
    }

    public Vector3Int startCellPos;
    public Vector3Int targetCellPos;

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        startCellPos = GridManager.instance.grid.WorldToCell(testTrans.position);
    //        targetCellPos = GridManager.instance.grid.WorldToCell(testTrans2.position);
    //        aiPathToTarget = Compute(startCellPos, targetCellPos);
    //    }
    //}

    public void Initialise()
    {
        startSpot = new Spot();
        targetSpot = new Spot();
        cols = GridManager.instance.xMax - GridManager.instance.xMin;
        rows = GridManager.instance.yMax - GridManager.instance.yMin;
        spotGrid = new Spot[cols, rows];

        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                spotGrid[i, j] = new Spot();
                spotGrid[i, j].i = i;
                spotGrid[i, j].j = j;
                spotGrid[i, j].posX = GridManager.instance.xMin + i;
                spotGrid[i, j].posY = GridManager.instance.yMin + j;
                spotGrid[i, j].parentSpot = null;
                spotGrid[i, j].isBlocked = GridManager.instance.IsCellBlockedForUnitMotionAtPos(new Vector3Int(spotGrid[i, j].posX, spotGrid[i, j].posY, 0));
            }
        }
        current = null;
    }

    public void BlockSpot(Vector3Int cellPos)
    {
        int i = cellPos.x - GridManager.instance.xMin;
        int j = cellPos.y - GridManager.instance.yMin;
        spotGrid[i, j].isBlocked = true;
    }

    public void UnBlockSpot(Vector3Int cellPos)
    {
        int i = cellPos.x - GridManager.instance.xMin;
        int j = cellPos.y - GridManager.instance.yMin;
        spotGrid[i, j].isBlocked = false;
    }

    public void ReInitialiseStaticObstaclesForMonster(Enemy monster)
    {
        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector3Int cellToCheck = new Vector3Int(spotGrid[i, j].posX, spotGrid[i, j].posY, 0);
                spotGrid[i, j].isBlocked = GridManager.instance.HasPetrifiedObject(cellToCheck) || GridManager.instance.IsCellBlockedForUnitMotionAtPos(cellToCheck) || GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(cellToCheck, monster);
            }
        }
    }

    public List<Vector3Int> Compute(Vector3Int startCellPos, Vector3Int targetCellPos)
    {
        List<Vector3Int> vectorPath = new List<Vector3Int>();
        if (startCellPos.x < GridManager.instance.xMin || startCellPos.x >= GridManager.instance.xMax || startCellPos.y < GridManager.instance.yMin || startCellPos.y >= GridManager.instance.yMax || targetCellPos.x < GridManager.instance.xMin || targetCellPos.x >= GridManager.instance.xMax || targetCellPos.y < GridManager.instance.yMin || targetCellPos.y >= GridManager.instance.yMax)
        {
            Debug.LogError("Wrong position values");
            return vectorPath;
        }
        int startX = startCellPos.x - GridManager.instance.xMin;
        int startY = startCellPos.y - GridManager.instance.yMin;

        int targetX = targetCellPos.x - GridManager.instance.xMin;
        int targetY = targetCellPos.y - GridManager.instance.yMin;

        startSpot = spotGrid[startX, startY];
        targetSpot = spotGrid[targetX, targetY];
        //startSpot.isBlocked = false;
        //targetSpot.isBlocked = false;

        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                spotGrid[i, j].parentSpot = null;
            }
        }
        openSet.Clear();
        closedSet.Clear(); ;
        current = null;
        openSet.Add(startSpot);
        nosolution = false;
        computePath = true;

        List<Spot> path = Compute2();
        for (int i = 0; i < path.Count; i++)
        {
            vectorPath.Add(new Vector3Int(path[i].posX, path[i].posY, 0));
        }
        return vectorPath;
    }

    List<Spot> Compute2()
    {
        List<Spot> path = new List<Spot>();

        while (computePath)
        {
            while (!nosolution)
            {
                if (openSet.Count > 0)
                {
                    int winner = 0;
                    for (int i = 0; i < openSet.Count; i++)
                    {
                        if (openSet[i].f < openSet[winner].f)
                        {
                            winner = i;
                        }
                    }
                    current = openSet[winner];
                    if (current.i == targetSpot.i && current.j == targetSpot.j)
                    {
                        Spot temp = current;
                        path.Add(temp);
                        while (temp.parentSpot != null)
                        {
                            path.Add(temp.parentSpot);
                            temp = temp.parentSpot;
                        }

                        Debug.Log("Done");
                        computePath = false;
                        break;
                    }
                    openSet.Remove(current);
                    closedSet.Add(current);

                    List<Spot> neighBours = GetNeighbours(current);
                    for (int i = 0; i < neighBours.Count; i++)
                    {
                        Spot neighbour = neighBours[i];
                        if (!closedSet.Contains(neighbour) && !neighbour.isBlocked)
                        {
                            float tempG = current.g + 1;

                            if (openSet.Contains(neighbour))
                            {
                                if (tempG < neighbour.g)
                                {
                                    neighbour.g = tempG;
                                }
                            }
                            else
                            {
                                neighbour.g = tempG;
                                openSet.Add(neighbour);
                            }

                            neighbour.h = heuristic(neighbour, targetSpot);
                            neighbour.f = neighbour.g + neighbour.h;
                            neighbour.parentSpot = current;
                        }
                    }

                }
                else
                {
                    //No solution 
                    Debug.Log("no solution");
                    nosolution = true;
                    break;
                }
            }
            if (nosolution)
            {
                break;
            }
            if (computePath)
            {
                break;
            }
        }
        return path;
    }


    public float heuristic(Spot a, Spot b)
    {
        float d = Mathf.Abs(a.i - b.i) + Mathf.Abs(a.j - b.j);
        return d;
    }

    public List<Spot> GetNeighbours(Spot spot)
    {
        List<Spot> neighbours = new List<Spot>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0 || Mathf.Abs(x) == Mathf.Abs(y))
                {
                    continue;
                }
                int checkX = spot.i + x;
                int checkY = spot.j + y;

                if (checkX >= 0 && checkX < cols && checkY >= 0 && checkY < rows)
                {
                    neighbours.Add(spotGrid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }
}
