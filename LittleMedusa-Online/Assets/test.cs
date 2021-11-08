using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
public class test : MonoBehaviour
{
    public bool IsKepe;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("other: "+other.name);
    }
}
