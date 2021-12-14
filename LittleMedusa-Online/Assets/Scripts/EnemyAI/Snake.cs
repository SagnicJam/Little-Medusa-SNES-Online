﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class Snake : Enemy
    {
        public bool inLineRange;
        public bool isPreparingToDash;
        public int tileToDash;
        public int dashSpeed;
        public float dashAnimation;
        public float lineRangeForDetection;
        public float dashPreparationTime;

        AimlessWandererMapper wandererMapper = new AimlessWandererMapper();
        DashMapper dashMapper = new DashMapper();
        SenseInLineAction senseInLineAction = new SenseInLineAction();

        float normalAnimation;

        public override void Awake()
        {
            base.Awake();
            normalSpeed = walkSpeed;
            normalAnimation = frameLooper.animationDuration;
            currentMapper = wandererMapper;
            senseInLineAction.Initialise(this);
            senseInLineAction.InitialiseLineSize(lineRangeForDetection);
        }

        IEnumerator ie;

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
            if (isMovementFreezed)
            {
                return;
            }
            if (!isPrimaryMoveActive && !isSecondaryMoveActive)
            {
                inLineRange = senseInLineAction.Perform();
                heroToChase = senseInLineAction.heroInLineOfAction;
                if (!followingTarget)
                {
                    if (followingTarget != inLineRange)
                    {
                        //Triggered ai motion
                        if (ie != null)
                        {
                            StopCoroutine(ie);
                        }
                        ie = PrepareToDashCor();
                        StartCoroutine(ie);
                    }
                    //normal aimless wanderer
                }
                else
                {
                    if ((IsActorOnArrows() || IsActorOnMirror()))
                    {
                        FinishDash();
                    }
                }
                if (waitingForNextActionToCheckForPath.isWaitingForNextActionCheck)
                {
                    waitingForNextActionToCheckForPath.Perform();
                    return;
                }

                if (currentMapper is DashMapper dashMap)
                {
                    if (dashMap.IsDashComplete())
                    {
                        FinishDash();
                        currentMovePointCellPosition = previousMovePointCellPosition;
                        return;
                    }

                    if (completedMotionToMovePoint && !inLineRange)
                    {
                        FinishDash();
                        currentMovePointCellPosition = previousMovePointCellPosition;
                        return;
                    }
                }
                if (completedMotionToMovePoint && !isPreparingToDash)
                {
                    if (IsActorOnArrows())
                    {
                        if (GridManager.instance.IsCellContainingUpArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                        {
                            currentMapper = new OneDNonCheckingMapper(FaceDirection.Up);
                        }
                        else if (GridManager.instance.IsCellContainingDownArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                        {
                            currentMapper = new OneDNonCheckingMapper(FaceDirection.Down);
                        }
                        else if (GridManager.instance.IsCellContainingLeftArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                        {
                            currentMapper = new OneDNonCheckingMapper(FaceDirection.Left);
                        }
                        else if (GridManager.instance.IsCellContainingRightArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                        {
                            currentMapper = new OneDNonCheckingMapper(FaceDirection.Right);
                        }
                    }
                    else if (IsActorOnMirror())
                    {
                        if (GridManager.instance.IsCellBlockedForUnitMotionAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing))) || (GridManager.instance.HasPetrifiedObject(GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing))) && GridManager.instance.IsCellContainingPushedMonsterOnCell(GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing)), this)))
                        {
                            currentMapper = wandererMapper;
                            CheckSwitchCellIndex();
                            currentMapper = new OneDNonCheckingMapper(Facing);
                            return;
                        }
                        else
                        {
                            currentMapper = new OneDNonCheckingMapper(Facing);
                        }
                    }
                    else
                    {
                        if (currentMapper is OneDNonCheckingMapper)
                        {
                            FinishDash();
                        }
                    }
                    CheckSwitchCellIndex();
                }
            }

        }

        public override void OnCantOccupySpace()
        {
            FinishDash();
            base.OnCantOccupySpace();
            //CheckSwitchCellIndex();
        }

        public void FinishDash()
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
            walkSpeed = normalSpeed;
            frameLooper.animationDuration = normalAnimation;
            currentMapper = null;
            currentMapper = wandererMapper;
            if (isPreparingToDash)
            {
                isPreparingToDash = false;
            }
            if (ie != null)
            {
                StopCoroutine(ie);
            }
        }

        IEnumerator PrepareToDashCor()
        {
            Debug.Log("preparing to dash");
            isPreparingToDash = true;
            followingTarget = true;
            frameLooper.animationDuration = dashAnimation;

            yield return new WaitForSeconds(dashPreparationTime);
            Debug.Log("dashing now");

            currentMapper = null;
            frameLooper.animationDuration = normalAnimation;
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
            if (isMovementFreezed)
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
            if (isMovementFreezed)
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
            if (isMovementFreezed)
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
                    Mapper m = currentMapper;
                    if (m != null && m is OneDNonCheckingMapper oneDNonCheckingMapper)
                    {
                        if (GridManager.instance.IsCellContainingUpArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                        {
                            oneDNonCheckingMapper.face = FaceDirection.Up;
                        }
                        else if (GridManager.instance.IsCellContainingDownArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                        {
                            oneDNonCheckingMapper.face = FaceDirection.Down;
                        }
                        else if (GridManager.instance.IsCellContainingLeftArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                        {
                            oneDNonCheckingMapper.face = FaceDirection.Left;
                        }
                        else if (GridManager.instance.IsCellContainingRightArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                        {
                            oneDNonCheckingMapper.face = FaceDirection.Right;
                        }
                        else
                        {
                            oneDNonCheckingMapper.face = Facing;
                        }
                    }
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
                if (switchedPrimaryMoveThisFrame)
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
            else if (!isPrimaryMoveActive && switchedPrimaryMoveThisFrame)
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
            if (isMovementFreezed)
            {
                if (!completedMotionToMovePoint)
                {
                    actorTransform.position = Vector3.MoveTowards(actorTransform.position, movePoint.position, petrificationSnapSpeed * Time.fixedDeltaTime);
                }
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
        }

        bool newIsPrimaryMoveActive;
        bool previousIsPrimaryMoveActive;
        private void FixedUpdate()
        {
            newIsPrimaryMoveActive = IsPlayerInRangeForMelleAttack() && !IsActorOnArrows() && !IsActorOnMirror();
            UpdateMovementState(newIsPrimaryMoveActive, false);
            PerformMovement();

            UpdateAnimationState(newIsPrimaryMoveActive, false);
            PerformAnimations();

            UpdateEventState(newIsPrimaryMoveActive, newIsPrimaryMoveActive != previousIsPrimaryMoveActive, false, false);
            PerformEvents();

            previousIsPrimaryMoveActive = newIsPrimaryMoveActive;
        }

        public override void OnPushStart()
        {
            FinishDash();
            walkSpeed = pushSpeed;
        }

        public override void OnPushStop()
        {
            Debug.Log("OnPushStop : snake");
            //Debug.Break();
            FinishDash();
            walkSpeed = normalSpeed;
            currentMapper = wandererMapper;
        }
    }
}