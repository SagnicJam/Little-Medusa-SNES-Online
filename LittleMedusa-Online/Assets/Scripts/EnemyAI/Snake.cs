using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : Enemy
{
    public bool inLineRange;
    public bool isPreparingToDash;
    public int tileToDash;
    public int dashSpeed;
    public float lineRangeForDetection;
    public float dashPreparationTime;

    AimlessWandererMapper wandererMapper = new AimlessWandererMapper();
    DashMapper dashMapper = new DashMapper();
    SenseInLineAction senseInLineAction = new SenseInLineAction();

    int normalSpeed;

    public override void Awake()
    {
        base.Awake();
        normalSpeed = walkSpeed;
        currentMapper = wandererMapper;
        senseInLineAction.Initialise(this);
        senseInLineAction.InitialiseLineSize(lineRangeForDetection);
    }

    IEnumerator ie;

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
       
        if (!isPrimaryMoveActive)
        {
            inLineRange = senseInLineAction.Perform();
            heroToChase = senseInLineAction.heroInLineOfAction;
            if (!followingTarget)
            {
                if (followingTarget != inLineRange)
                {
                    //Triggered ai motion
                    if(ie!=null)
                    {
                        StopCoroutine(ie);
                    }
                    ie = PrepareToDashCor();
                    StartCoroutine(ie);
                }
                //normal aimless wanderer
            }

            if (waitingForNextActionToCheckForPath.isWaitingForNextActionCheck)
            {
                waitingForNextActionToCheckForPath.Perform();
                return;
            }

            if(currentMapper is DashMapper dashMap)
            {
                if(dashMap.IsDashComplete())
                {
                    FinishDash();
                    currentMovePointCellPosition = previousMovePointCellPosition;
                    return;
                }

                if (completedMotionToMovePoint&&!inLineRange)
                {
                    FinishDash();
                    currentMovePointCellPosition = previousMovePointCellPosition;
                    return;
                }
            }
            if (completedMotionToMovePoint&& !isPreparingToDash)
            {
                CheckSwitchCellIndex();
            }
        }

    }

    public override void OnCantOccupySpace()
    {
        FinishDash();
        base.OnCantOccupySpace();
    }

    public void FinishDash()
    {
        if(followingTarget)
        {
            followingTarget = false;
            walkSpeed = normalSpeed;
            senseInLineAction.heroInLineOfAction = null;
            currentMapper = wandererMapper;
        }
    }

    IEnumerator PrepareToDashCor()
    {
        isPreparingToDash = true;
        followingTarget = true;
        frameLooper.animationDuration /= 2;

        yield return new WaitForSeconds(dashPreparationTime);

        currentMapper = null;
        frameLooper.animationDuration *= 2;
        walkSpeed = dashSpeed;

        dashMapper.InitialiseDirection(Facing);
        dashMapper.InitialiseTileLength(tileToDash);

        currentMapper = dashMapper;
        isPreparingToDash = false;
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

    public override void UpdateEventState(bool isPrimaryMoveActive, bool switchedThisFrame)
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
            if(switchedThisFrame)
            {
                FinishDash();
            }
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

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        FinishDash();
        base.OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(collidedActorWithMyHead);
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
        FinishDash();
    }

    public override void OnPushStop()
    {
        currentMapper = wandererMapper;
    }

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        FinishDash();
        base.OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(collidedActorWithMyHead);
    }
}
