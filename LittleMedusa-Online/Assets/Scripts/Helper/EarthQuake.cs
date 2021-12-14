using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class EarthQuake : MonoBehaviour
    {
        public int earthquakeSpawner;
        public void InitialiseEarthquake(int earthquakeSpawner)
        {
            this.earthquakeSpawner = earthquakeSpawner;
        }
    }
}