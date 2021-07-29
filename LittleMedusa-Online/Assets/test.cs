using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class test : MonoBehaviour
{
    public Dictionary<int, int> ss=new Dictionary<int, int>();

    private void Awake()
    {
        ss.Add(1,2);
        ss.Add(2,3);
        ss.Add(3,4);

        string s=JsonUtility.ToJson(ss);
        Debug.Log(s);
    }

    //public Sprite sp;
    //public Tilemap tp;

    //public Tile tile;

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Y))
    //    {
    //        sp = tp.GetSprite(Vector3Int.zero);
    //        tp.SetTile(Vector3Int.zero,null);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.X))
    //    {
    //        tile.sprite = sp;
    //        tp.SetTile(Vector3Int.zero, tile);
    //    }
    //}
}
