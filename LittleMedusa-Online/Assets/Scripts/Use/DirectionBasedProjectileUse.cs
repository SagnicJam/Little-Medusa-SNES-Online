using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class DirectionBasedProjectileUse : TileBasedProjectileUse
{
    float angle;
    Vector3 fireDirection;

    public DirectionBasedProjectileUse(float angle, Vector3 fireDirection,OnUsed<Vector3Int>OnReachedTargetPoint)
    {
        this.fireDirection = fireDirection;
        this.angle = angle;
        onPointReached = OnReachedTargetPoint;
    }
    public override void BeginToUse(Actor actorUsing, OnUsed<Actor> onUseBegin, OnUsed<Actor> onDynamicItemUsed)
    {
        base.BeginToUse(actorUsing, onUseBegin, onDynamicItemUsed);
        Vector3 direction = GetVectorAtAngle(angle, fireDirection, -actorUsing.transform.forward).normalized;
        Vector3Int finalCellPos = GridManager.instance.grid.WorldToCell(actorUsing.transform.position + (liveProjectile.projectileTileTravelDistance * direction));
        if(angle==0)
        {
            tileMovementDirection = FaceDirection.Right;
        }
        else if(angle==90)
        {
            tileMovementDirection = FaceDirection.Down;
        }
        else if (angle == 180)
        {
            tileMovementDirection = FaceDirection.Left;
        }
        else if (angle == 270)
        {
            tileMovementDirection = FaceDirection.Up;
        }
        finalPos = GridManager.instance.cellToworld(finalCellPos);
    }

    Vector3 GetVectorAtAngle(float angle, Vector3 vector, Vector3 actorUp)
    {
        return Quaternion.AngleAxis(angle, actorUp) * vector;
    }
}
