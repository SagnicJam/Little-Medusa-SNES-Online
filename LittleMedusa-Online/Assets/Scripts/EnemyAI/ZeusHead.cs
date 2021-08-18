using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeusHead : Enemy
{
    public int thunderDropTickRate;

    AimlessWandererMapper wandererMapper = new AimlessWandererMapper();

    public bool isDroppingThunder;
    int normalSpeed;

    public override void Awake()
    {
        base.Awake();
        normalSpeed = walkSpeed;
        currentMapper = wandererMapper;
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

        if (!isPrimaryMoveActive&&!isSecondaryMoveActive)
        {
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

        if (isSecondaryMoveActive)
        {
            if (!waitingActionForSecondaryMove.Perform())
            {
                isDroppingThunder = true;
                waitingActionForSecondaryMove.ReInitialiseTimerToBegin(secondaryMoveAttackRateTickRate);
            }
            else
            {
                isDroppingThunder = false;
            }
        }
        else if (!isSecondaryMoveActive && switchedSecondaryMoveThisFrame)
        {
            isDroppingThunder = false;
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
        }
        if (isDroppingThunder)
        {
            //Check for player
            //Attack player
            Debug.Log("Dropping thunder");
            GridManager.instance.ExplodeCells(currentMovePointCellPosition);
        }
    }
    bool newIsPrimaryMoveActive;
    bool previousIsPrimaryMoveActive;

    bool newIsSecondaryMoveActive;
    bool previousIsSecondaryMoveActive;
    private void FixedUpdate()
    {
        newIsPrimaryMoveActive = IsPlayerInRangeForMelleAttack();
        newIsSecondaryMoveActive = true;

        UpdateMovementState(newIsPrimaryMoveActive, false);
        PerformMovement();

        UpdateAnimationState(newIsPrimaryMoveActive, false);
        PerformAnimations();

        UpdateEventState(newIsPrimaryMoveActive, newIsPrimaryMoveActive != previousIsPrimaryMoveActive, newIsSecondaryMoveActive, newIsSecondaryMoveActive!= previousIsSecondaryMoveActive);
        PerformEvents();

        previousIsPrimaryMoveActive = newIsPrimaryMoveActive;
    }

    public override void OnPushStart()
    {
        //Empty
        walkSpeed = pushSpeed;
        if (waitingForNextActionToCheckForPath.isWaitingForNextActionCheck)
        {
            waitingForNextActionToCheckForPath.CompleteTimer();
        }
        Debug.Log("OnPushStart - " + gameObject.name);
    }

    public override void OnPushStop()
    {
        Debug.LogError("OnPushStop " + gameObject.name);
        walkSpeed = normalSpeed;
        if (waitingForNextActionToCheckForPath.isWaitingForNextActionCheck)
        {
            waitingForNextActionToCheckForPath.CompleteTimer();
        }
        currentMapper = null;
        currentMapper = wandererMapper;
    }

    public override void OnBodyCollidingWithKillingTiles(TileData tileData)
    {
        if(tileData.tileType != EnumData.TileType.LightningBolt &&
            tileData.tileType != EnumData.TileType.ThunderStruck)
        {
            base.OnBodyCollidingWithKillingTiles(tileData);
        }
    }
}
