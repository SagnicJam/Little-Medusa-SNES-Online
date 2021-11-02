using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthQuake : MonoBehaviour
{
    public int earthquakeSpawner;
    public void InitialiseEarthquake(int earthquakeSpawner)
    {
        this.earthquakeSpawner = earthquakeSpawner;
    }
}
