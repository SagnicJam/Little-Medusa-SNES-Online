using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Dictionary<Vector3Int, Vector3Int> portalEntranceDic = new Dictionary<Vector3Int, Vector3Int>();
    public DummyPorts dummyPorts;

    private void Start()
    {
        portalEntranceDic.Add(dummyPorts.v1, dummyPorts.v2);
    }

    public void ActorUnitEnter(Actor actor,Vector3Int portalCellPosition)
    {
        if(portalEntranceDic.ContainsKey(portalCellPosition))
        {
            actor.transform.position = GridManager.instance.cellToworld(portalEntranceDic[portalCellPosition]);
            actor.currentMovePointCellPosition = portalEntranceDic[portalCellPosition];
            actor.previousMovePointCellPosition = portalEntranceDic[portalCellPosition];
        }
    }
}

[System.Serializable]
public struct DummyPorts
{
    public Vector3Int v1;
    public Vector3Int v2;
}
