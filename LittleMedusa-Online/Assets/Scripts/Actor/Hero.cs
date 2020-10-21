using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Actor
{
    [Header("Tweak Params")]
    public EnumData.PlayerStates playerStates;
    public int primaryMoveAttackRateTickRate;

    [Header("Live Data")]
    public bool isPlacingBoulderAnimationPlayed;
    public bool isRemovingBoulderAnimationPlayed;

    [Header("Hero Actions")]
    public WaitingForNextAction waitingActionForPrimaryMove = new WaitingForNextAction();
    readonly BoulderRemoveAction boulderRemoveAction = new BoulderRemoveAction();

    public override void Start()
    {
        base.Start();
        waitingActionForPrimaryMove.Initialise(this);
        waitingActionForPrimaryMove.ReInitialiseTimerToEnd(primaryMoveAttackRateTickRate);

        primaryMoveUseAnimationAction.SetAnimationSpeedAndSpritesOnUsage(primaryMoveAnimationSpeed,normalAnimationSpeed);
        rangedAttack = new Attack(primaryMoveDamage,ownerId,EnumData.AttackTypes.ProjectileAttack,projectileThrownType);

        boulderRemoveAction.Initialise(this);

    }

    public void InitialiseActor(PlayerStateUpdates playerStateUpdates)
    {
        SetActorPositionalState(playerStateUpdates.positionUpdates);
        SetActorAnimationState(playerStateUpdates.playerAnimationEvents);
        SetActorEventActionState(playerStateUpdates.playerEvents);
        SetAuthoratativeStates(playerStateUpdates.playerAuthoratativeStates);
    }


    //authoratatively is performed
    public void SetAuthoratativeStates(PlayerAuthoratativeStates playerAuthoratativeStates)
    {
        isPetrified = playerAuthoratativeStates.isPetrified;
        isPushed = playerAuthoratativeStates.isPushed;
    }

    //authoratatively is performed(but is locally is also done)-correction happens
    public void SetActorPositionalState(PositionUpdates positionUpdates)
    {
        actorTransform.position = positionUpdates.updatedActorPosition;
        currentMovePointCellPosition = positionUpdates.updatedBlockActorPosition;
        previousMovePointCellPosition = positionUpdates.updatedPreviousBlockActorPosition;
        Facing = GridManager.instance.GetFaceDirectionFromCurrentPrevPoint(currentMovePointCellPosition, previousMovePointCellPosition, this);
    }

    public void SetActorAnimationState(PlayerAnimationEvents playerAnimationEvents)
    {
        primaryMoveUseAnimationAction.isBeingUsed = playerAnimationEvents.isPrimaryMoveAnimationBeingPlayed;
        if(playerAnimationEvents.isPlacingBoulderAnimationPlayed)
        {
            Debug.LogError("playerAnimationEvents.isPlacingBoulderAnimationPlayed ");
        }
        isPlacingBoulderAnimationPlayed = playerAnimationEvents.isPlacingBoulderAnimationPlayed;
    }

    //authoratatively is performed(but is locally is also done)
    public void SetActorEventActionState(PlayerEvents playerEvents)
    {
        isFiringPrimaryProjectile = playerEvents.firedPrimaryMoveProjectile;
    }


    public void ProcessAuthoratativeEvents()
    {
        if (isPushed)
        {
            if (completedMotionToMovePoint)
            {
                CheckSwitchCellIndex();
                if (GridManager.instance.IsCellBlockedForPetrifiedUnitMotionAtPos(currentMovePointCellPosition))
                {
                    StopPush(this);
                    return;
                }
                Mapper m = GetMapper();
                if (m is OneDNonCheckingMapper oneDNonCheckingMapper)
                {
                    oneDNonCheckingMapper.face = Facing;
                }
            }
            else
            {
                walkAction.MoveActorToMovePointCell();
            }
            return;
        }
        if (isPetrified)
        {
            petrificationAction.Perform();
            return;
        }
    }

    

    public void ProcessAnimationsInputs(bool[] inputs, bool[] previousInputs)
    {
        if(isPushed)
        {
            return;
        }
        if (isPetrified)
        {
            return;
        }
        
        if (!isInFlyingState)
        {
            if (inputs[(int)EnumData.Inputs.Shoot])
            {
                if (!primaryMoveUseAnimationAction.isBeingUsed)
                {
                    primaryMoveUseAnimationAction.isBeingUsed = true;
                }
            }
            else if (!inputs[(int)EnumData.Inputs.Shoot] && previousInputs[(int)EnumData.Inputs.Shoot] != inputs[(int)EnumData.Inputs.Shoot])
            {
                if (primaryMoveUseAnimationAction.isBeingUsed)
                {
                    primaryMoveUseAnimationAction.isBeingUsed = false;
                    primaryMoveUseAnimationAction.CancelMoveUsage();
                }
            }
            else if(completedMotionToMovePoint)
            {
                if (inputs[(int)EnumData.Inputs.PlaceRemovalBoulder])
                {
                    Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                    if (!GridManager.instance.IsCellBlockedForBoulderPlacementAtPos(cellToCheckFor))
                    {
                        //play animation here 
                        //send command to server of placement
                        Debug.LogError("chala");
                        isPlacingBoulderAnimationPlayed = true;
                    }
                    else
                    {
                        isPlacingBoulderAnimationPlayed = false;
                    }

                    if (GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.TileType.Boulder) && !GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.TileType.BoulderDisappearing))
                    {
                        isRemovingBoulderAnimationPlayed = true;
                    }
                    else
                    {
                        isRemovingBoulderAnimationPlayed = false;
                    }
                }
                else if (!inputs[(int)EnumData.Inputs.PlaceRemovalBoulder] && previousInputs[(int)EnumData.Inputs.PlaceRemovalBoulder] != inputs[(int)EnumData.Inputs.PlaceRemovalBoulder])
                {
                    if (isPlacingBoulderAnimationPlayed)
                    {
                        isPlacingBoulderAnimationPlayed = false;
                    }
                    if (isRemovingBoulderAnimationPlayed)
                    {
                        isRemovingBoulderAnimationPlayed = false;
                    }
                }
            }

        }
        
    }

    

    public void ProcessInputAnimationControl()
    {
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
            UpdateBasicWalkingSprite();
            triggerFaceChangeEvent = false;
        }
        if (primaryMoveUseAnimationAction.isBeingUsed)
        {
            primaryMoveUseAnimationAction.Perform();
        }
        else if(isPlacingBoulderAnimationPlayed)
        {
            Vector3Int celllocationToSpawn = GridManager.instance.grid.WorldToCell(actorTransform.transform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
            GridManager.instance.PlaceBoulderAnimation(celllocationToSpawn);
        }
        else if (isRemovingBoulderAnimationPlayed)
        {
            Vector3Int celllocationToSpawn = GridManager.instance.grid.WorldToCell(actorTransform.transform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
            GridManager.instance.RemoveBoulderAnimation(celllocationToSpawn);
        }
        else
        {
            if (!primaryMoveUseAnimationAction.isBeingUsed && primaryMoveUseAnimationAction.initialiseSprite)
            {
                primaryMoveUseAnimationAction.CancelMoveUsage();
            }
            else if (!completedMotionToMovePoint)
            {
                //for walking
                frameLooper.UpdateAnimationFrame();
            }
        }
    }


    public void ProcessEventsInputs(bool[] inputs, bool[] previousInputs)
    {
        if (isPushed)
        {
            return;
        }
        if (isPetrified)
        {
            return;
        }
        if (!isInFlyingState)
        {
            if (inputs[(int)EnumData.Inputs.Shoot])
            {
                if(IsHeroAbleToFireProjectiles())
                {
                    if (!waitingActionForPrimaryMove.Perform())
                    {
                        isFiringPrimaryProjectile = true;
                        waitingActionForPrimaryMove.ReInitialiseTimerToBegin(primaryMoveAttackRateTickRate);
                    }
                    else
                    {
                        isFiringPrimaryProjectile = false;
                    }
                }
            }
            else if (!inputs[(int)EnumData.Inputs.Shoot] && previousInputs[(int)EnumData.Inputs.Shoot] != inputs[(int)EnumData.Inputs.Shoot])
            {
                isFiringPrimaryProjectile = false;
                waitingActionForPrimaryMove.ReInitialiseTimerToBegin(primaryMoveAttackRateTickRate);
            }


            if (isClient() && hasAuthority())
            {
                if (completedMotionToMovePoint)
                {
                    if (!inputs[(int)EnumData.Inputs.Push] && previousInputs[(int)EnumData.Inputs.Push] != inputs[(int)EnumData.Inputs.Push])
                    {
                        Vector3Int cellPos = currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                        Actor actorToPush = GridManager.instance.GetActorOnPos(cellPos);

                        if (actorToPush != null)
                        {
                            if (IsActorAbleToPush(Facing) && IsActorPushableInDirection(actorToPush, Facing))
                            {
                                //Send reliable request of push to server here
                                PushCommand pushCommand = new PushCommand(GetLocalSequenceNo(), (int)Facing, actorToPush.ownerId);
                                ClientSend.PushPlayerCommand(pushCommand);
                            }
                        }
                    }
                    else if (inputs[(int)EnumData.Inputs.PlaceRemovalBoulder])
                    {
                        Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                        if (!GridManager.instance.IsCellBlockedForBoulderPlacementAtPos(cellToCheckFor))
                        {
                            //send command to server of placement
                            PlaceBoulderCommand placeBoulderCommand = new PlaceBoulderCommand(GetLocalSequenceNo(), cellToCheckFor);
                            ClientSend.PlaceBoulderCommand(placeBoulderCommand);
                        }
                        else if (GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.TileType.Boulder) && !GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.TileType.BoulderDisappearing))
                        {
                            RemoveBoulderCommand removeBoulderCommand = new RemoveBoulderCommand(GetLocalSequenceNo(), cellToCheckFor);
                            ClientSend.RemoveBoulderCommand(removeBoulderCommand);
                        }
                    }
                }
            }
        }
    }

    public void ProcessInputEventControl()
    {
        if (isPushed)
        {
            return;
        }
        if (isPetrified)
        {
            return;
        }
        if (isFiringPrimaryProjectile)
        {
            Fire(this);
        }
    }


    public void ProcessMovementInputs(bool[] inputs,bool[] previousInputs)
    {
        if (isPushed)
        {
            return;
        }
        if (isPetrified)
        {
            return;
        }
        if (completedMotionToMovePoint)
        {
            if (inputs[(int)EnumData.Inputs.Up])
            {
                Facing = FaceDirection.Up;
                Vector3Int checkForCellPos = currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                if (IsActorPathBlockedForInputDrivenMovementByAnotherActor(Facing)&&CanOccupy(checkForCellPos))
                {
                    currentMovePointCellPosition += GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                }
            }
            else if (inputs[(int)EnumData.Inputs.Left])
            {
                Facing = FaceDirection.Left;
                if (IsActorPathBlockedForInputDrivenMovementByAnotherActor(Facing))
                {
                    currentMovePointCellPosition += GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                }
            }
            else if (inputs[(int)EnumData.Inputs.Down])
            {
                Facing = FaceDirection.Down;
                if (IsActorPathBlockedForInputDrivenMovementByAnotherActor(Facing))
                {
                    currentMovePointCellPosition += GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                }
            }
            else if (inputs[(int)EnumData.Inputs.Right])
            {
                Facing = FaceDirection.Right;
                if (IsActorPathBlockedForInputDrivenMovementByAnotherActor(Facing))
                {
                    currentMovePointCellPosition += GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                }
            }

        }
        else
        {
            if (!inputs[(int)EnumData.Inputs.Up] && previousInputs[(int)EnumData.Inputs.Up] != inputs[(int)EnumData.Inputs.Up])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.Inputs.Left] && previousInputs[(int)EnumData.Inputs.Left] != inputs[(int)EnumData.Inputs.Left])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.x);
                if (fractionCovered < GridManager.instance.grid.cellSize.x / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.Inputs.Down] && previousInputs[(int)EnumData.Inputs.Down] != inputs[(int)EnumData.Inputs.Down])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.Inputs.Right] && previousInputs[(int)EnumData.Inputs.Right] != inputs[(int)EnumData.Inputs.Right])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.x);
                if (fractionCovered < GridManager.instance.grid.cellSize.x / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
        }
        
    }

    public void ProcessInputMovementsControl()
    {
        if(isPushed)
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

    public bool IsHeroAbleToFireProjectiles()
    {
        Vector3 objectPosition = actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing);
        if (!GridManager.instance.IsPositionBlockedForProjectiles(objectPosition) && !GridManager.instance.IsPositionContainingPetrifiedActor(objectPosition))
        {
            return true;
        }
        return false;
    }
    //do stop push
    //do remaining monster code inside head collision
    //-------------------------------------------------//--------------------------------------------------------------------------------------------//--------------------------------

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndPetrified(Actor collidedActorWithMyHead)
    {
        collidedActorWithMyHead.SetActorPushingMe(this);
        collidedActorWithMyHead.chainIDLinkedTo = chainIDLinkedTo;
        StartPush(collidedActorWithMyHead,Facing);
        SetActorMePushing(collidedActorWithMyHead);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        collidedActorWithMyHead.SetActorPushingMe(this);
        StartPush(collidedActorWithMyHead,Facing);
        collidedActorWithMyHead.chainIDLinkedTo = chainIDLinkedTo;
        SetActorMePushing(collidedActorWithMyHead);
    }

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        if (IsActorPushableInDirection(collidedActorWithMyHead, Facing))
        {
            collidedActorWithMyHead.SetActorPushingMe(this);
            collidedActorWithMyHead.chainIDLinkedTo = chainIDLinkedTo;
            StartPush(collidedActorWithMyHead,Facing);
            SetActorMePushing(collidedActorWithMyHead);
        }
        else
        {
            StopPush(this);
        }
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        if(!(collidedActorWithMyHead is Hero))
        {
            collidedActorWithMyHead.Die();
        }
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        if (!(collidedActorWithMyHead is Hero))
        {
            collidedActorWithMyHead.Die();
        }
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }
    
    public override bool CanOccupy(Vector3Int pos)
    {
        //if (isInFlyingState)
        //{
        //    //if (GridManager.instance.IsCellBlockedForFlyingUnitsAtPos(pos))
        //    //{
        //    //    return false;
        //    //}
        //    //else
        //    //{
        //        return true;
        //    //}
        //}
        /*else */if (GridManager.instance.IsCellBlockedForUnitMotionAtPos(pos))
        {
            return false;
        }
        //else if (IsActorOnArrows() && GridManager.instance.IsCellContainingMonsterLinkedWithDifferentChainOnCellPos(pos, this))
        //{
        //    return false;
        //}
        //else if (IsActorOnArrows() && GridManager.instance.IsCellContainingPetrifiedMonsterOnCell(pos, this))
        //{
        //    return false;
        //}
        //else if (IsActorOnMirror() && GridManager.instance.IsCellContainingPetrifiedMonsterOnCell(pos, this))
        //{
        //    return false;
        //}
        else
        {
            return true;
        }
    }

    public override void OnCantOccupySpace()
    {
        currentMovePointCellPosition = previousMovePointCellPosition;
        actorTransform.position = GridManager.instance.cellToworld(previousMovePointCellPosition);
    }
}
