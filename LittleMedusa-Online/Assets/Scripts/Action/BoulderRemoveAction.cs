using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderRemoveAction : Actions
{
    Hero actorRemoving;
    Vector3Int celllocationToRemove;

    public override void Initialise(Actor actorActingThisAction)
    {
        actorRemoving = (Hero)actorActingThisAction;
    }

    public override bool Perform()
    {
        if (actorRemoving == null)
        {
            Debug.LogError("Actor not set");
            return false;
        }
        celllocationToRemove = GridManager.instance.grid.WorldToCell(actorRemoving.transform.position + GridManager.instance.GetFacingDirectionOffsetVector3(actorRemoving.Facing));
        GridManager.instance.SetTile(celllocationToRemove, EnumData.TileType.Boulder,false);
        GridManager.instance.RemoveBoulderAnimation(celllocationToRemove);
        return true;
    }
}
