using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBasedProjectileUse : Use
{
    public OnUsed<Vector3Int> onPointReached;
    public Vector3 finalPos;
    public Vector3 initPos;
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

    public Attack attack;

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
                Vector3Int currentProjectileCell = GridManager.instance.grid.WorldToCell(liveProjectile.transform.position);

                TileData portalTile = GridManager.instance.GetTileAtCellPoint(currentProjectileCell, EnumData.TileType.Portal);
                if (portalTile != null)
                {
                    PortalTracker portal = portalTile.GetComponent<PortalTracker>();
                    portal.ProjectileUnitEnter(liveProjectile, currentProjectileCell);
                }

                if (projectileTypeThrown == EnumData.Projectiles.TidalWave
                    || projectileTypeThrown == EnumData.Projectiles.BubbleShield ||
                    projectileTypeThrown == EnumData.Projectiles.FlamePillar ||
                    projectileTypeThrown == EnumData.Projectiles.MightyWind|| projectileTypeThrown == EnumData.Projectiles.MightyWindMirrorKnight)
                {

                    if (GridManager.instance.HasTileAtCellPoint(currentProjectileCell, EnumData.TileType.Boulder))
                    {
                        GridManager.instance.boulderTracker.RemoveBoulder(currentProjectileCell);
                    }
                    liveProjectile.transform.position = Vector3.MoveTowards(liveProjectile.transform.position, finalPos, Time.fixedDeltaTime * liveProjectile.projectileSpeed);
                    if (currentValidPosCell != currentProjectileCell)
                    {
                        previousValidPosCell = currentValidPosCell;
                        currentValidPosCell = currentProjectileCell;
                    }

                    if (GridManager.instance.IsCellBlockedForProjectiles(currentProjectileCell))
                    {
                        
                        EndOfUse();
                        return;
                    }
                }
                else
                {
                    if(projectileTypeThrown==EnumData.Projectiles.FireBall||projectileTypeThrown==EnumData.Projectiles.FireBallMirrorKnight)
                    {
                        if(GridManager.instance.IsCellBlockedForProjectiles(GridManager.instance.grid.WorldToCell(liveProjectile.transform.position + GridManager.instance.GetFacingDirectionOffsetVector3(tileMovementDirection))))
                        {
                            EndOfUse();
                            return;
                        }
                    }
                    else
                    {
                        if (GridManager.instance.IsCellBlockedForProjectiles(currentProjectileCell))
                        {
                            EndOfUse();
                            return;
                        }
                    }
                    liveProjectile.transform.position = Vector3.MoveTowards(liveProjectile.transform.position, finalPos, Time.fixedDeltaTime * liveProjectile.projectileSpeed);
                    if (currentValidPosCell != currentProjectileCell)
                    {
                        previousValidPosCell = currentValidPosCell;
                        currentValidPosCell = currentProjectileCell;
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

        attack = actorUsing.currentAttack;
        projectileTypeThrown = actorUsing.currentAttack.projectiles;
        actorHadAuthority = actorUsing.hasAuthority();
        gameObjectInstanceId = actorUsing.gameObject.GetInstanceID();
        ownerId = actorUsing.ownerId;
        //Debug.LogError("PProjectile to throw type "+projectileTypeThrown.ToString());
        GameObject gToSpawn = Resources.Load(projectileTypeThrown.ToString()) as GameObject;
        if (gToSpawn==null)
        {
            Debug.LogError("gToSpawn is null");
            return;
        }
        liveProjectile = GridManager.InstantiateGameObject(gToSpawn).GetComponent<ProjectileUtil>();
        liveProjectile.transform.position = GridManager.instance.cellToworld(actorUsing.positionToSpawnProjectile);
        initPos = liveProjectile.transform.position;
        finalPos = actorUsing.actorTransform.position + (liveProjectile.projectileTileTravelDistance * GridManager.instance.GetFacingDirectionOffsetVector3(actorUsing.Facing));
        tileMovementDirection = actorUsing.Facing;
        liveProjectile.Initialise(this);
        
        //if (projectileTypeThrown==EnumData.Projectiles.FlamePillar)
        //{
        //    liveProjectile.transform.position = actorUsing.actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(actorUsing.Facing);
        //    finalPos = actorUsing.actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(actorUsing.Facing) + (liveProjectile.projectileTileTravelDistance * GridManager.instance.GetFacingDirectionOffsetVector3(actorUsing.Facing));
        //}
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