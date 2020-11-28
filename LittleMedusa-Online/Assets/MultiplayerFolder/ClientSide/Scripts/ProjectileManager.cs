using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public int id;

    public void OnInititialise(int id)
    {
        this.id = id;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
}
