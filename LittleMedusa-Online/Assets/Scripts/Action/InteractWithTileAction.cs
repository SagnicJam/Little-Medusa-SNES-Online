using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractWithTileAction : Actions
{
    Actor actorInteracting;
    public Vector3Int celllocationToSpawn;
    public OnUsed<Vector3Int> onActionOverOnCellPosition;
    public OnUsed<Actor> onActionOver;

    public override void Initialise(Actor actorActingThisAction)
    {
        actorInteracting = actorActingThisAction;
    }

    public override bool Perform()
    {
        if (actorInteracting == null)
        {
            Debug.LogError("Actor not set");
            return false;
        }
        celllocationToSpawn = GridManager.instance.grid.WorldToCell(actorInteracting.transform.position + GridManager.instance.GetFacingDirectionOffsetVector3(actorInteracting.Facing));
        Debug.Log("Drop performing here");
        if(onActionOver != null)
        {
            Debug.Log("onActionOver");
            onActionOver.Invoke(actorInteracting);
        }
        if (onActionOverOnCellPosition != null)
        {
            Debug.Log("onActionOverOnCellPosition");
            onActionOverOnCellPosition.Invoke(celllocationToSpawn);
        }
        return true;
    }
}
