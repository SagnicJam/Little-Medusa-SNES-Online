using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashMapper : Mapper
{
    public FaceDirection faceDirection;
    public int tileToDash;

    public void InitialiseDirection(FaceDirection faceDirection)
    {
        this.faceDirection = faceDirection;
    }

    public void InitialiseTileLength(int tileToDash)
    {
        this.tileToDash = tileToDash;
    }

    public override Vector3Int GetNewPathPoint(Actor tobeMappedActor)
    {
        Enemy enemyToBeMapped = (Enemy)tobeMappedActor;
        Vector3Int posToAnalyseForNextPoint = tobeMappedActor.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(enemyToBeMapped.Facing));
        tileToDash--;
        return posToAnalyseForNextPoint;
    }

    public bool IsDashComplete()
    {
        return tileToDash < 0;
    }
}
