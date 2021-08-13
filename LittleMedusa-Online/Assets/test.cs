using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class test : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);
    }
    //public GameObject gToSpawn;
    //public List<Enemy> enemiesToPetrify;
    //public List<Enemy> enemiesToPush;

    //private void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.Space))
    //    {
    //        foreach(Enemy e in enemiesToPetrify)
    //        {
    //            e.Petrify();
    //        }
    //    }

    //    if (Input.GetKeyDown(KeyCode.X))
    //    {
    //        foreach (Enemy e in enemiesToPush)
    //        {
    //            e.StartPush(e,FaceDirection.Left);
    //        }
    //    }
    //}
}
