using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
public class test : MonoBehaviour
{
    public Grid grid;
    public Tilemap tp;
    public Transform t1;
    public Transform t2;
    public List<Tilemap> allTM;

    [Header("Live Data")]
    public int xMin;
    public int yMin;
    public int xMax;
    public int yMax;

    public Tilemap tp1;
    public Tile tile;

    private void Awake()
    {
        xMin = int.MaxValue;
        yMin = int.MaxValue;
        xMax = -int.MaxValue;
        yMax = -int.MaxValue;


        List<Vector3Int> vlist = GetAllPositionForTileMap();

        for (int i = 0; i < vlist.Count; i++)
        {
            if (xMin > vlist[i].x)
            {
                xMin = vlist[i].x;
            }
            if (yMin > vlist[i].y)
            {
                yMin = vlist[i].y;
            }
            if (xMax < vlist[i].x)
            {
                xMax = vlist[i].x;
            }
            if (yMax < vlist[i].y)
            {
                yMax = vlist[i].y;
            }
        }



        ////t1.position = cellToworld(Vector3Int.zero);
        ////tp.SetTile(new Vector3Int(xMin, 0, 0),null);

        t1.position = cellToworld(new Vector3Int(xMin,yMin, 0));
        t2.position = cellToworld(new Vector3Int(xMax, yMax, 0));

        //for (int i = xMin; i <= xMax; i++)
        //{

        //}
        //while(xMin<=xMax&&yMin<=yMax)
        //{
        //    corners.Add(new Corners(xMin+x,yMin+x,xMax-x,yMax-x));
        //    x++;
        //}

        
    }

    public int x = 0;
    public int y = 0;


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(xMin + x < xMax - x && yMin + y < yMax - y)
            {
                for (int j = yMin + y; j <= yMax - y; j++)
                {
                    tp1.SetTile(new Vector3Int(xMin + x, j, 0), tile);
                }
                for (int j = yMin + y; j <= yMax - y; j++)
                {
                    tp1.SetTile(new Vector3Int(xMax - x, j, 0), tile);
                }
                for (int j = xMin + x; j <= xMax - x; j++)
                {
                    tp1.SetTile(new Vector3Int(j, yMin + y, 0), tile);
                }
                for (int j = xMin + x; j <= xMax - x; j++)
                {
                    tp1.SetTile(new Vector3Int(j, yMax - y, 0), tile);
                }
                if (xMin + x < xMax - x)
                {
                    //corners.Add(new Corners(xMin + x, yMin + y, xMax - x, yMax - y));
                    x++;
                    t1.position = cellToworld(new Vector3Int(xMin + x, yMin + y, 0));
                    t2.position = cellToworld(new Vector3Int(xMax - x, yMax - y, 0));
                    //Debug.LogError("xaxa " + " min: " + new Vector3Int(xMin + x, yMin + y, 0) + " max: " + new Vector3Int(xMax - x, yMax - y, 0));
                }
                if (yMin + y < yMax - y)
                {
                    //corners.Add(new Corners(xMin, yMin+y, xMax, yMax-y));
                    y++;
                    t1.position = cellToworld(new Vector3Int(xMin + x, yMin + y, 0));
                    t2.position = cellToworld(new Vector3Int(xMax - x, yMax - y, 0));
                    //Debug.LogError("fafa" + " min: " + new Vector3Int(xMin + x, yMin + y, 0) + " max: " + new Vector3Int(xMax - x, yMax - y, 0));
                }
            }
            else if(xMin + x < xMax - x || yMin + y < yMax - y)
            {
                if (xMin + x < xMax - x)
                {
                    //corners.Add(new Corners(xMin + x, yMin + y, xMax - x, yMax - y));
                    tp1.SetTile(new Vector3Int(xMin + x, yMin + y, 0), tile);
                    tp1.SetTile(new Vector3Int(xMax - x, yMax - y, 0), tile);
                    x++;
                    t1.position = cellToworld(new Vector3Int(xMin + x, yMin + y, 0));
                    t2.position = cellToworld(new Vector3Int(xMax - x, yMax - y, 0));
                }

                if (yMin + y < yMax - y)
                {
                    //corners.Add(new Corners(xMin, yMin+y, xMax, yMax-y));
                    tp1.SetTile(new Vector3Int(xMin + x, yMin + y, 0), tile);
                    tp1.SetTile(new Vector3Int(xMax - x, yMax - y, 0), tile);
                    y++;
                    t1.position = cellToworld(new Vector3Int(xMin + x, yMin + y, 0));
                    t2.position = cellToworld(new Vector3Int(xMax - x, yMax - y, 0));
                }
            }
            else if ((xMax-xMin)>(yMax-yMin)&&xMin + x == xMax - x)
            {
                if (xMin + x == xMax - x)
                {
                    //corners.Add(new Corners(xMin + x, yMin + y, xMax - x, yMax - y));
                    tp1.SetTile(new Vector3Int(xMin + x, yMin + y, 0), tile);
                    tp1.SetTile(new Vector3Int(xMax - x, yMax - y, 0), tile);
                    x++;
                    t1.position = cellToworld(new Vector3Int(xMin + x, yMin + y, 0));
                    t2.position = cellToworld(new Vector3Int(xMax - x, yMax - y, 0));
                }
            }
            else if ((xMax - xMin) < (yMax - yMin) && yMin + y == yMax - y)
            {
                if (xMin + x == xMax - x)
                {
                    //corners.Add(new Corners(xMin + x, yMin + y, xMax - x, yMax - y));
                    tp1.SetTile(new Vector3Int(xMin + x, yMin + y, 0), tile);
                    tp1.SetTile(new Vector3Int(xMax - x, yMax - y, 0), tile);
                    y++;
                    t1.position = cellToworld(new Vector3Int(xMin + x, yMin + y, 0));
                    t2.position = cellToworld(new Vector3Int(xMax - x, yMax - y, 0));
                }
            }
            else
            {
                //Debug.LogError("finish");
            }
        }
    }

    public Vector3 cellToworld(Vector3Int cell)
    {
        return grid.CellToWorld(cell) + grid.cellSize / 2;
    }

    public List<Vector3Int> GetAllPositionForTileMap()
    {
        Tilemap tilemap = tp;
        List<Vector3Int> cellPositions = new List<Vector3Int>();
        if (tilemap != null)
        {
            for (int n = tilemap.cellBounds.xMin; n < tilemap.cellBounds.xMax; n++)
            {
                for (int p = tilemap.cellBounds.yMin; p < tilemap.cellBounds.yMax; p++)
                {
                    Vector3Int localPlace = (new Vector3Int(n, p, (int)tilemap.transform.position.y));
                    if (tilemap.HasTile(localPlace))
                    {
                        cellPositions.Add(localPlace);
                    }
                    else
                    {
                        //No tile at "place"
                    }
                }
            }
        }
        return cellPositions;
    }
}
