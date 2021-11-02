using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Centaur : Enemy
{
    public bool inLineRange;
    public float lineRangeForDetection;
    public float circleRangeForDetection;
    public float rangedAttackLineLengthForDetection;
    public int arrowDamage; 


    AimlessWandererMapper wandererMapper = new AimlessWandererMapper();
    AStarPathFindMapper pathfindingMapper = new AStarPathFindMapper();
    SenseInLineAction senseInLineAction = new SenseInLineAction();
    SenseInCircleAction senseInCircleAction = new SenseInCircleAction();


    public override void Awake()
    {
        base.Awake();
        currentMapper = wandererMapper;

        senseInLineAction.Initialise(this);
        senseInLineAction.InitialiseLineSize(lineRangeForDetection);

        senseInCircleAction.Initialise(this);
        senseInCircleAction.InitialiseCircleRange(circleRangeForDetection);
    }

    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.DrawSphere(transform.position, circleRangeForDetection);

    //}
    //update the block pos here
    public override void UpdateMovementState(bool isPrimaryMoveActive, bool isSecondaryMoveActive)
    {
        if (isPhysicsControlled)
        {
            return;
        }
        if (isPushed)
        {
            return;
        }
        if (isPetrified)
        {
            return;
        }

        if (!isPrimaryMoveActive && !isSecondaryMoveActive)
        {
            if (!waitForPathFindingToWearOff.Perform())
            {
                inLineRange = senseInLineAction.Perform();
                heroToChase = senseInLineAction.heroInLineOfAction;
                if (!followingTarget)
                {
                    if (followingTarget != inLineRange)
                    {
                        //Triggered ai motion
                        currentMapper = null;
                        currentMapper = pathfindingMapper;
                        followingTarget = true;
                    }
                    //normal aimless wanderer
                }
                else
                {
                    if (!senseInCircleAction.Perform())
                    {
                        FinishFollowing();
                    }
                }

                if (waitingForNextActionToCheckForPath.isWaitingForNextActionCheck)
                {
                    waitingForNextActionToCheckForPath.Perform();
                    return;
                }
            }
            else
            {
                if (waitingForNextActionToCheckForPath.isWaitingForNextActionCheck)
                {
                    waitingForNextActionToCheckForPath.CompleteTimer();
                }
            }
            if (completedMotionToMovePoint)
            {
                CheckSwitchCellIndex();
            }
        }

    }


    public override void PerformMovement()
    {
        if (isPhysicsControlled)
        {
            return;
        }
        if (isPushed)
        {
            return;
        }
        if (isPetrified)
        {
            return;
        }

        if (!CanOccupy(currentMovePointCellPosition))
        {
            OnCantOccupySpace();
            return;
        }
        walkAction.Perform();
    }

    public override void UpdateAnimationState(bool isPrimaryMoveActive, bool isSecondaryMoveActive)
    {
        if (isPhysicsControlled)
        {
            return;
        }
        if (isPushed)
        {
            return;
        }

        if (isPetrified)
        {
            return;
        }

        if (isPrimaryMoveActive)
        {
            if (!isUsingPrimaryMove)
            {
                isUsingPrimaryMove = true;
                UpdateFrameSprites();
            }
        }
        else
        {
            if (isUsingPrimaryMove)
            {
                isUsingPrimaryMove = false;
                UpdateFrameSprites();
            }
        }

        if (isSecondaryMoveActive)
        {
            if (!isUsingSecondaryMove)
            {
                isUsingSecondaryMove = true;
                UpdateFrameSprites();
            }
        }
        else
        {
            if (isUsingSecondaryMove)
            {
                isUsingSecondaryMove = false;
                UpdateFrameSprites();
            }
        }


        if (!isWalking)
        {
            isWalking = true;
            UpdateFrameSprites();
        }
    }

    public override void PerformAnimations()
    {
        if (isPhysicsControlled)
        {
            return;
        }
        if (isPushed)
        {
            return;
        }
        if (isPetrified)
        {
            return;
        }
        if (triggerFaceChangeEvent)
        {
            UpdateFrameSprites();
            triggerFaceChangeEvent = false;
        }
        frameLooper.UpdateAnimationFrame();
    }

    public override void UpdateEventState(bool isPrimaryMoveActive, bool switchedPrimaryMoveThisFrame, bool isSecondaryMoveActive, bool switchedSecondaryMoveThisFrame)
    {
        if (isPhysicsControlled)
        {
            return;
        }
        if (isPushed)
        {
            if (completedMotionToMovePoint)
            {
                CheckSwitchCellIndex();
                if (GridManager.instance.IsCellBlockedForPetrifiedUnitMotionAtPos(currentMovePointCellPosition))
                {
                    if (isPetrified && isPushed)
                    {
                        StopPush(this);
                    }
                    else
                    {
                        TakeDamage(currentHP);
                    }
                    return;
                }

                Mapper m = currentMapper;
                if (m != null && m is OneDNonCheckingMapper oneDNonCheckingMapper)
                {
                    oneDNonCheckingMapper.face = Facing;
                }
            }
            else
            {
                if (GridManager.instance.HasTileAtCellPoint(currentMovePointCellPosition, EnumData.TileType.Empty))
                {
                    StopPushMeOnly(this);
                    return;
                }
                if (GridManager.instance.IsCellBlockedForPetrifiedUnitMotionAtPos(currentMovePointCellPosition))
                {
                    if (isPetrified && isPushed)
                    {
                        StopPush(this);
                    }
                    else
                    {
                        TakeDamage(currentHP);
                    }
                    return;
                }
                walkAction.MoveActorToMovePointCell();
            }
            return;
        }

        //wait for next attack
        if (isPrimaryMoveActive)
        {
            if (!waitingActionForPrimaryMove.Perform())
            {
                isMelleAttacking = true;
                waitingActionForPrimaryMove.ReInitialiseTimerToBegin(primaryMoveAttackRateTickRate);
            }
            else
            {
                isMelleAttacking = false;
            }
        }
        else if (!isPrimaryMoveActive && switchedPrimaryMoveThisFrame)
        {
            isMelleAttacking = false;
            waitingActionForPrimaryMove.ReInitialiseTimerToEnd(primaryMoveAttackRateTickRate);
        }


        //wait for next attack
        if (isSecondaryMoveActive)
        {
            if (!waitingActionForSecondaryMove.Perform())
            {
                isRangedAttacking = true;
                waitingActionForSecondaryMove.ReInitialiseTimerToBegin(secondaryMoveAttackRateTickRate);
            }
            else
            {
                isRangedAttacking = false;
            }
        }
        else if (!isSecondaryMoveActive && switchedSecondaryMoveThisFrame)
        {
            isRangedAttacking = false;
            waitingActionForSecondaryMove.ReInitialiseTimerToEnd(secondaryMoveAttackRateTickRate);
        }
    }

    public override void PerformEvents()
    {
        if (isPhysicsControlled)
        {
            return;
        }
        if (isPushed)
        {
            return;
        }
        if (isPetrified)
        {
            petrificationAction.Perform();
            return;
        }
        if (isMelleAttacking)
        {
            //Check for player
            //Attack player
            heroGettingHit = GetHeroNextTo();
            if (heroGettingHit != null)
            {
                Debug.Log("Attack player");
                heroGettingHit.TakeDamage(primaryMoveDamage);
            }
            return;
        }
        if (isRangedAttacking)
        {
            //Check for player
            //Attack player
            Debug.Log("Range Attacking player");
            FireProjectile(new Attack(arrowDamage, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.Arrow),
                GridManager.instance.grid.WorldToCell(actorTransform.position));
        }
    }


    bool newIsPrimaryMoveActive;
    bool previousIsPrimaryMoveActive;

    bool newIsSecondaryMoveActive;
    bool previousIsSecondaryMoveActive;
    private void FixedUpdate()
    {
        newIsPrimaryMoveActive = IsPlayerInRangeForMelleAttack();
        newIsSecondaryMoveActive = IsPlayerInRangeForRangedAttack(rangedAttackLineLengthForDetection);

        UpdateMovementState(newIsPrimaryMoveActive, newIsSecondaryMoveActive);
        PerformMovement();

        UpdateAnimationState(newIsPrimaryMoveActive, newIsSecondaryMoveActive);
        PerformAnimations();

        UpdateEventState(newIsPrimaryMoveActive, newIsPrimaryMoveActive != previousIsPrimaryMoveActive, newIsSecondaryMoveActive, newIsSecondaryMoveActive != previousIsSecondaryMoveActive);
        PerformEvents();

        previousIsPrimaryMoveActive = newIsPrimaryMoveActive;
        previousIsSecondaryMoveActive = newIsSecondaryMoveActive;
    }

    public void FinishFollowing()
    {
        if (followingTarget)
        {
            followingTarget = false;
        }
        if (senseInLineAction.heroInLineOfAction != null)
        {
            senseInLineAction.heroInLineOfAction = null;
        }
        if (waitingForNextActionToCheckForPath.isWaitingForNextActionCheck)
        {
            waitingForNextActionToCheckForPath.CompleteTimer();
        }
        currentMapper = null;
        currentMapper = wandererMapper;
    }

    public override void OnPushStart()
    {
        //Empty
        FinishFollowing();
        walkSpeed = pushSpeed;
        Debug.Log("OnPushStart - " + gameObject.name);
    }

    public override void OnPushStop()
    {
        Debug.LogError("OnPushStop " + gameObject.name);
        FinishFollowing();
        walkSpeed = normalSpeed;
    }
}
