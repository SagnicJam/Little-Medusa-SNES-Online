using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class test : MonoBehaviour
{
    public Sprite sp;
    public Tilemap tp;

    public Tile tile;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            sp = tp.GetSprite(Vector3Int.zero);
            tp.SetTile(Vector3Int.zero,null);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            tile.sprite = sp;
            tp.SetTile(Vector3Int.zero, tile);
        }
    }
}
