using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBasedProjectileUse : Use
{
    public OnUsed<Vector3Int> onPointReached;
    public Vector3 finalPos;
    public ProjectileUtil liveProjectile;
    public Vector3Int previousValidPosCell;
    public Vector3Int currentValidPosCell;

    public bool destroyAtPreviousCell;

    public FaceDirection actorFacingWhenFired;
    public int ownerId;
    public int gameObjectInstanceId;

    public override void PerformUsage()
    {
        if(!endTriggered)
        {
            if (Vector3.Distance(liveProjectile.transform.position, finalPos) >= 0.05f && !GridManager.instance.IsCellBlockedForProjectiles(GridManager.instance.grid.WorldToCell(liveProjectile.transform.position)))
            {
                liveProjectile.transform.position = Vector3.MoveTowards(liveProjectile.transform.position, finalPos, Time.fixedDeltaTime * liveProjectile.projectileSpeed);
                if(currentValidPosCell != GridManager.instance.grid.WorldToCell(liveProjectile.transform.position))
                {
                    previousValidPosCell = currentValidPosCell;
                    currentValidPosCell = GridManager.instance.grid.WorldToCell(liveProjectile.transform.position);
                }
            }
            else
            {
                EndOfUse();
            }
        }
    }

    public override void BeginToUse(Actor actorUsing,OnUsed<Actor> onUseBegin, OnUsed<Actor> onDynamicItemUsed)
    {
        //Casting projectile here
        this.actorUsing = actorUsing;
        onUseOver = onDynamicItemUsed;

        actorFacingWhenFired = actorUsing.Facing;
        gameObjectInstanceId = actorUsing.gameObject.GetInstanceID();
        ownerId = actorUsing.ownerId;

        GameObject gToSpawn = Resources.Load(actorUsing.projectileThrownType.ToString()) as GameObject;

        if (gToSpawn==null)
        {
            Debug.LogError("gToSpawn is null");
            return;
        }
        liveProjectile = GridManager.InstantiateGameObject(gToSpawn).GetComponent<ProjectileUtil>();
        liveProjectile.transform.position = actorUsing.actorTransform.position;
        finalPos = actorUsing.actorTransform.position + (liveProjectile.projectileTileTravelDistance * GridManager.instance.GetFacingDirectionOffsetVector3(actorUsing.Facing));
        liveProjectile.Initialise(this);
    }

    public override void EndOfUse()
    {
        if (!endTriggered)
        {
            if (onPointReached != null)
            {
                if(destroyAtPreviousCell)
                {
                    onPointReached.Invoke(previousValidPosCell);
                }
                else
                {
                    onPointReached.Invoke(currentValidPosCell);
                }
            }
            liveProjectile.DestroyProjectile();
            endTriggered = true;
        }
    }
}