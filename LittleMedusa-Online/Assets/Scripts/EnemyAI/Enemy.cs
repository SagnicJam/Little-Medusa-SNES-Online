using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Actor
{
    [Header("Enemy Actions")]
    public WaitingForNextAction waitingForNextActionToCheckForPath = new WaitingForNextAction();
    public WaitingForNextAction waitForPathFindingToWearOff = new WaitingForNextAction();

    [Header("Scene references")]
    public Transform HeadTransform;
    public EnemyDataSender enemyDataSender;

    [Header("Tweak  Params")]
    public float waitingTimeWhenStuck;
    public float petrifyAnimationDuration;
    public int pathFindingWearOffTickCount;

    [Header("Live Data")]
    public bool isMelleAttacking;
    public bool isRangedAttacking;
    public bool followingTarget;
    public int leaderNetworkId;
    public Hero heroToChase;

    [Header("Enemy stats")]
    public bool canBePetrified;
    public bool dieOnUnPetrification;

    public static Dictionary<int, Enemy> enemies = new Dictionary<int, Enemy>();

    public Hero heroGettingHit;

    public override void Awake()
    {
        base.Awake();
        Facing = faceDirectionInit;
        Server.serverID++;
        ownerId = Server.serverID;
        enemies.Add(ownerId, this);
        InitialiseHP();
        waitForPathFindingToWearOff.Initialise(this);
    }

    public override void Start()
    {
        base.Start();
        enemyDataSender.Initialise(this);
    }

    public int GetEnemyState()
    {
        if(isPhysicsControlled)
        {
            return (int)EnumData.EnemyState.PhysicsControlled;
        }
        else if (isPushed)
        {
            return (int)EnumData.EnemyState.Pushed;
        }
        else if(isPetrified)
        {
            return (int)EnumData.EnemyState.Petrified;
        }
        else if (isUsingPrimaryMove)
        {
            return (int)EnumData.EnemyState.PrimaryMoveUse;
        }
        else if (isUsingSecondaryMove)
        {
            return (int)EnumData.EnemyState.SecondaryMoveUse;
        }
        else if(isWalking)
        {
            return (int)EnumData.EnemyState.Walking;
        }
        return (int)EnumData.EnemyState.Idle;
    }

    public void KillMe()
    {
        if (enemies.ContainsKey(ownerId))
        {
            enemies.Remove(ownerId);
            Destroy(HeadTransform.gameObject);
            GridManager.instance.enemySpawnner.currentEnemyCount--;
        }
    }

    public override bool CanOccupy(Vector3Int pos)
    {
        if (GridManager.instance.IsCellBlockedForUnitMotionAtPos(pos)||GridManager.instance.HasPetrifiedObject(pos))
        {
            return false;
        }
        return true;
    }

    public bool IsPlayerInRangeForMelleAttack()
    {
        Vector3 toCheckPos = actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing);
        Actor actor = GridManager.instance.GetActorOnPos(GridManager.instance.grid.WorldToCell(toCheckPos));
        if (actor!=null&&actor is Hero hero && leaderNetworkId!=actor.ownerId && completedMotionToMovePoint &&actor.completedMotionToMovePoint)
        {
            return true;
        }
        return false;
    }

    public Hero GetHeroNextTo()
    {
        Vector3 toCheckPos = actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing);
        Actor actor = GridManager.instance.GetActorOnPos(GridManager.instance.grid.WorldToCell(toCheckPos));
        if (actor != null && actor is Hero hero && completedMotionToMovePoint && actor.completedMotionToMovePoint)
        {
            return hero;
        }
        return null;
    }

    public bool IsPlayerInRangeForRangedAttack(float range)
    {
        Vector3 toCheckPos = actorTransform.position + range * GridManager.instance.GetFacingDirectionOffsetVector3(Facing);
        Actor actor = GridManager.instance.GetActorOnPos(GridManager.instance.grid.WorldToCell(toCheckPos));
        if (actor != null && actor is Hero hero&&leaderNetworkId!=actor.ownerId && completedMotionToMovePoint)
        {
            return true;
        }
        return false;
    }

    public override void Petrify()
    {
        if(!isPetrified)
        {
            if (petrificationSpriteArr.Length > 0)
            {
                frameLooper.UpdateSpriteArr(petrificationSpriteArr);
                frameLooper.PlayOneShotAnimation(petrifyAnimationDuration);
            }
        }
        base.Petrify();
    }

    public override void UnPetrify()
    {
        base.UnPetrify();
        UpdateFrameSprites();
    }

    public override void OnCantOccupySpace()
    {
        if (isPetrified || isPushed)
        {
            currentMovePointCellPosition = GridManager.instance.grid.WorldToCell(actorTransform.position);
            previousMovePointCellPosition = currentMovePointCellPosition;
        }
        else
        {
            currentMovePointCellPosition = previousMovePointCellPosition;
            actorTransform.position = GridManager.instance.cellToworld(previousMovePointCellPosition);
        }
    }

    public override void CheckSwitchCellIndex()
    {
        base.CheckSwitchCellIndex();
        if (currentMovePointCellPosition == previousMovePointCellPosition)
        {
            waitingForNextActionToCheckForPath.ReInitialiseTimerToBegin(waitingTimeWhenStuck);
        }
    }

    

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndPetrified(Actor collidedActorWithMyHead)
    {
        if (!collidedActorWithMyHead.IsInSpawnJarTerritory)
        {
            PushActor(collidedActorWithMyHead, Facing);
        }
        else
        {
            StopPush(this);
        }
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        if(!collidedActorWithMyHead.IsInSpawnJarTerritory)
        {
            PushActor(collidedActorWithMyHead, Facing);
        }
        else
        {
            StopPush(this);
        }
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        if (!collidedActorWithMyHead.IsInSpawnJarTerritory)
        {
            SnapBackAfterCollision();
        }
        else
        {
            collidedActorWithMyHead.TakeDamage(currentHP);
        }
    }

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log(this.gameObject.name.ToString() + " case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        SnapBackAfterCollision();
    }

    public override void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        PushActor(collidedActorWithMyHead, Facing);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        TakeDamage(currentHP);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log(this.gameObject.name.ToString() + " case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        TakeDamage(currentHP);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log(this.gameObject.name.ToString() + " case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        SnapBackAfterCollision();
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log(this.gameObject.name.ToString() + " case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        SnapBackAfterCollision();
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        collidedActorWithMyHead.TakeDamage(collidedActorWithMyHead.currentHP);
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        if (this != collidedActorWithMyHead.actorPushingMe)
        {
            collidedActorWithMyHead.TakeDamage(collidedActorWithMyHead.currentHP);
            TakeDamage(currentHP);
        }
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnBodyCollidingWithTornadoEffectTiles(TileData tileData)
    {
        GridManager.instance.tornado.OnEnterTornadoRegion(tileData, this);
    }

    public override void OnBodyCollidingWithKillingTiles(TileData tileData)
    {
        TakeDamage(currentHP);
    }

    public override void OnBodyCollidedWithHourGlassTile(Vector3Int hourGlassTile)
    {
    }

    public override void OnBodyCollidedWithIcarausWingsTiles(Vector3Int icarausCollectedOnTilePos)
    {
    }

    public abstract void UpdateAnimationState(bool isPrimaryMoveActive,bool isSecondaryMoveActive);

    public abstract void UpdateMovementState(bool isPrimaryMoveActive, bool isSecondaryMoveActive);

    public abstract void UpdateEventState(bool isPrimaryMoveActive, bool switchedPrimaryMoveThisFrame, bool isSecondaryMoveActive, bool switchedSecondaryMoveThisFrame);


    public abstract void PerformAnimations();

    public abstract void PerformMovement();

    public abstract void PerformEvents();
}
