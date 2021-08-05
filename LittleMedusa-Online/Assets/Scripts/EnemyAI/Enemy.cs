using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Actor
{
    [Header("Enemy Actions")]
    public WaitingForNextAction waitingForNextActionToCheckForPath = new WaitingForNextAction();

    [Header("Scene references")]
    public Transform HeadTransform;

    [Header("Tweak  Params")]
    public float waitingTimeWhenStuck;
    public Sprite[] petrificationSpriteArr;

    [Header("Live Data")]
    public bool isMelleAttacking;
    public bool followingTarget;
    public Hero heroToChase;

    [Header("Enemy stats")]
    public int id;
    public bool canBePetrified;
    public bool dieOnUnPetrification;

    public static Dictionary<int, Enemy> enemies = new Dictionary<int, Enemy>();
    static int nextEnemyId = 1;

    public override void Awake()
    {
        base.Awake();
        Facing = faceDirectionInit;
        id = nextEnemyId;
        nextEnemyId++;
        enemies.Add(id, this);
        InitialiseHP();
    }

    public void KillMe()
    {
        if(enemies.ContainsKey(id))
        {
            enemies.Remove(id);
            Destroy(HeadTransform.gameObject);
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

    public bool IsPlayerInRangeForAttack()
    {
        Vector3 toCheckPos = actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing);
        Actor actor = GridManager.instance.GetActorOnPos(GridManager.instance.grid.WorldToCell(toCheckPos));
        if (actor!=null&&actor is Hero hero&&completedMotionToMovePoint&&actor.completedMotionToMovePoint)
        {
            return true;
        }
        return false;
    }

    public override void Petrify()
    {
        base.Petrify();
        Debug.LogError("Petrified here");
        //frameLooper.UpdateSpriteArr(petrificationSpriteArr);
        //frameLooper.PlayOneShotAnimation();
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
        PushActor(collidedActorWithMyHead, Facing);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        PushActor(collidedActorWithMyHead, Facing);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        SnapBackAfterCollision();
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


    public abstract void UpdateAnimationState(bool isPrimaryMoveActive);

    public abstract void UpdateMovementState(bool isPrimaryMoveActive);

    public abstract void UpdateEventState(bool isPrimaryMoveActive,bool switchedThisFrame);


    public abstract void PerformAnimations();

    public abstract void PerformMovement();

    public abstract void PerformEvents();
}
