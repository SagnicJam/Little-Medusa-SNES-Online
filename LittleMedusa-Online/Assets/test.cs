using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
public class test : MonoBehaviour
{
    public int[] numArr;
    //public bool IsKepe;
    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("other: "+other.name);
    //}

    public List<int> maxNums;
    public int maxNo;
    private void Start()
    {
        foreach (int item in numArr)
        {
            if (item > maxNo)
            {
                maxNo = item;
                maxNums = new List<int>() { maxNo};
            }
            else if(item==maxNo)
            {
                maxNums.Add(item);
            }
        }
    }
}
