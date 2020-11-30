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
    public Actor actorMePushing;
    public EnumData.Projectiles projectileTypeThrown;
    public bool actorHadAuthority;
    public int ownerId;
    public int gameObjectInstanceId;

    public FaceDirection tileMovementDirection;

    public void SetActorMePushing(Actor actorMePushing)
    {
        this.actorMePushing = actorMePushing;
    }

    public override void PerformUsage()
    {
        if(!endTriggered)
        {
            //Debug.Log(Vector3.Distance(liveProjectile.transform.position, finalPos) >= 0.05f);
            //Debug.Log(!GridManager.instance.IsCellBlockedForProjectiles(GridManager.instance.grid.WorldToCell(liveProjectile.transform.position)));
            
            if (Vector3.Distance(liveProjectile.transform.position, finalPos) >= 0.05f)
            {
                if (projectileTypeThrown == EnumData.Projectiles.TidalWave
                    || projectileTypeThrown == EnumData.Projectiles.BubbleShield ||
                    projectileTypeThrown == EnumData.Projectiles.MightyWind)
                {
                    if (GridManager.instance.HasTileAtCellPoint(GridManager.instance.grid.WorldToCell(liveProjectile.transform.position),EnumData.TileType.Boulder))
                    {
                        GridManager.instance.SetTile(GridManager.instance.grid.WorldToCell(liveProjectile.transform.position),EnumData.TileType.Boulder,false,false);
                    }
                    liveProjectile.transform.position = Vector3.MoveTowards(liveProjectile.transform.position, finalPos, Time.fixedDeltaTime * liveProjectile.projectileSpeed);
                    if (currentValidPosCell != GridManager.instance.grid.WorldToCell(liveProjectile.transform.position))
                    {
                        previousValidPosCell = currentValidPosCell;
                        currentValidPosCell = GridManager.instance.grid.WorldToCell(liveProjectile.transform.position);
                    }
                }
                else
                {
                    if (!GridManager.instance.IsCellBlockedForProjectiles(GridManager.instance.grid.WorldToCell(liveProjectile.transform.position)))
                    {
                        liveProjectile.transform.position = Vector3.MoveTowards(liveProjectile.transform.position, finalPos, Time.fixedDeltaTime * liveProjectile.projectileSpeed);
                        if (currentValidPosCell != GridManager.instance.grid.WorldToCell(liveProjectile.transform.position))
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

        projectileTypeThrown = actorUsing.currentAttack.projectiles;
        actorHadAuthority = actorUsing.hasAuthority();
        gameObjectInstanceId = actorUsing.gameObject.GetInstanceID();
        ownerId = actorUsing.ownerId;

        GameObject gToSpawn = Resources.Load(projectileTypeThrown.ToString()) as GameObject;
        if (gToSpawn==null)
        {
            Debug.LogError("gToSpawn is null");
            return;
        }
        liveProjectile = GridManager.InstantiateGameObject(gToSpawn).GetComponent<ProjectileUtil>();
        liveProjectile.transform.position = actorUsing.actorTransform.position;
        finalPos = actorUsing.actorTransform.position + (liveProjectile.projectileTileTravelDistance * GridManager.instance.GetFacingDirectionOffsetVector3(actorUsing.Facing));
        liveProjectile.Initialise(this);
        tileMovementDirection = actorUsing.Facing;
        
        if (projectileTypeThrown==EnumData.Projectiles.FlamePillar)
        {
            liveProjectile.transform.position = actorUsing.actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(actorUsing.Facing);
            finalPos = actorUsing.actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(actorUsing.Facing) + (liveProjectile.projectileTileTravelDistance * GridManager.instance.GetFacingDirectionOffsetVector3(actorUsing.Facing));
        }
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