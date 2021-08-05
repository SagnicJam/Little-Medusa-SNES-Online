using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cyclops : Enemy
{
    public bool inLineRange;
    public float lineRangeForDetection;
    public float circleRangeForDetection;

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
    public override void UpdateMovementState(bool isPrimaryMoveActive)
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
        
        if(!isPrimaryMoveActive)
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
                    followingTarget = false;
                    currentMapper = null;
                    currentMapper = wandererMapper;
                }
            }
            if (waitingForNextActionToCheckForPath.isWaitingForNextActionCheck)
            {
                waitingForNextActionToCheckForPath.Perform();
                return;
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

    public override void UpdateAnimationState(bool isPrimaryMoveActive)
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

        if (!isWalking)
        {
            isWalking = true;
            UpdateFrameSprites();
        }
    }

    public override void PerformAnimations()
    {
        if (triggerFaceChangeEvent)
        {
            UpdateFrameSprites();
            triggerFaceChangeEvent = false;
        }
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
        frameLooper.UpdateAnimationFrame();
    }

    public override void UpdateEventState(bool isPrimaryMoveActive,bool switchedThisFrame)
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
                    if(isPetrified&&isPushed)
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
        else if (!isPrimaryMoveActive && switchedThisFrame)
        {
            isMelleAttacking = false;
            waitingActionForPrimaryMove.ReInitialiseTimerToEnd(primaryMoveAttackRateTickRate);
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
            Debug.Log("Attack player");
        }
    }
    bool newIsPrimaryMoveActive;
    bool previousIsPrimaryMoveActive;
    private void FixedUpdate()
    {
        newIsPrimaryMoveActive = IsPlayerInRangeForAttack();

        UpdateMovementState(newIsPrimaryMoveActive);
        PerformMovement();

        UpdateAnimationState(newIsPrimaryMoveActive);
        PerformAnimations();

        UpdateEventState(newIsPrimaryMoveActive, newIsPrimaryMoveActive != previousIsPrimaryMoveActive);
        PerformEvents();

        previousIsPrimaryMoveActive = newIsPrimaryMoveActive;
    }

    public override void OnPushStart()
    {
        //Empty
        Debug.Log("OnPushStart - cyclops");
    }

    public override void OnPushStop()
    {
        currentMapper = wandererMapper;
    }
}
