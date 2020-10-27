using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Medusa : Actor
{
    [Header("Tweak Params")]
    public int primaryMoveAttackRateTickRate;
    public Color petrificationColor;
    public Color invincibleColor;

    [Header("Scene References")]
    public Image healthFillImage;
    public TextMeshProUGUI currentLifeStockText;
    public SpriteRenderer statusSprite;

    [Header("Hero Actions")]
    public WaitingForNextAction waitingActionForPrimaryMove = new WaitingForNextAction();

    public override void Start()
    {
        base.Start();
        waitingActionForPrimaryMove.Initialise(this);
        waitingActionForPrimaryMove.ReInitialiseTimerToEnd(primaryMoveAttackRateTickRate);

        primaryMoveUseAnimationAction.SetAnimationSpeedAndSpritesOnUsage(primaryMoveAnimationSpeed,normalAnimationSpeed);
        rangedAttack = new Attack(primaryMoveDamage,ownerId,EnumData.AttackTypes.ProjectileAttack,projectileThrownType);


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
        if(statusSprite!=null)
        {
            if (playerAuthoratativeStates.isPetrified && isPetrified != playerAuthoratativeStates.isPetrified)
            {
                statusSprite.enabled = true;
                statusSprite.color = petrificationColor;
            }
            if (!playerAuthoratativeStates.isPetrified && isPetrified != playerAuthoratativeStates.isPetrified)
            {
                statusSprite.enabled = false;
            }
            if (playerAuthoratativeStates.isInvincible && isInvincible != playerAuthoratativeStates.isInvincible)
            {
                statusSprite.enabled = true;
                statusSprite.color = invincibleColor;
            }
            if (!playerAuthoratativeStates.isInvincible && isInvincible != playerAuthoratativeStates.isInvincible)
            {
                statusSprite.enabled = false;
            }
        }
        
        isPetrified = playerAuthoratativeStates.isPetrified;
        isPushed = playerAuthoratativeStates.isPushed;
        isInvincible = playerAuthoratativeStates.isInvincible;
        
        if(actorCollider2D!=null)
        {
            if (!isRespawnningPlayer && isRespawnningPlayer != playerAuthoratativeStates.isRespawnningPlayer)
            {
                //enable crosshair locally
                //disable collider
                if(statusSprite!=null)
                {
                    statusSprite.gameObject.SetActive(false);
                }
                SetRespawnState();
            }
            if (isRespawnningPlayer && isRespawnningPlayer != playerAuthoratativeStates.isRespawnningPlayer)
            {
                //disable crosshair locally
                //enable back collider
                if (statusSprite != null)
                {
                    statusSprite.gameObject.SetActive(true);
                }
                SetSpawnState();
            }
        }
        

        isRespawnningPlayer = playerAuthoratativeStates.isRespawnningPlayer;
        currentHP = playerAuthoratativeStates.currentHP;
        currentStockLives = playerAuthoratativeStates.currentStockLives;

        if(healthFillImage!=null)
        {
            healthFillImage.fillAmount =  (1f*currentHP) / maxHP;
            currentLifeStockText.text =  currentStockLives.ToString();
        }
    }

    //authoratatively is performed(but is locally is also done)-correction happens
    public void SetActorPositionalState(PositionUpdates positionUpdates)
    {
        actorTransform.position = positionUpdates.updatedActorPosition;
        currentMovePointCellPosition = positionUpdates.updatedBlockActorPosition;
        previousMovePointCellPosition = positionUpdates.updatedPreviousBlockActorPosition;
        Facing = GridManager.instance.GetFaceDirectionFromCurrentPrevPoint(currentMovePointCellPosition, previousMovePointCellPosition, this);

        if (Facing != (FaceDirection)positionUpdates.Facing || PreviousFacingDirection != (FaceDirection)positionUpdates.previousFacing)
        {
            Facing = (FaceDirection)positionUpdates.Facing;
            PreviousFacingDirection = (FaceDirection)positionUpdates.previousFacing;
        }
    }

    public void SetActorAnimationState(PlayerAnimationEvents playerAnimationEvents)
    {
        primaryMoveUseAnimationAction.isBeingUsed = playerAnimationEvents.isPrimaryMoveAnimationBeingPlayed;
    }

    //authoratatively is performed(but is locally is also done)
    public void SetActorEventActionState(PlayerEvents playerEvents)
    {
        isFiringPrimaryProjectile = playerEvents.firedPrimaryMoveProjectile;
    }


    public void ProcessAuthoratativeEvents()
    {
        if(isRespawnningPlayer)
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
                if (GridManager.instance.HasTileAtCellPoint(currentMovePointCellPosition, EnumData.TileType.Empty))
                {
                    StopPushMeOnly(this);
                    return;
                }
                if (GridManager.instance.IsCellBlockedForPetrifiedUnitMotionAtPos(currentMovePointCellPosition))
                {
                    StopPush(this);
                    return;
                }
                walkAction.MoveActorToMovePointCell();
            }
            return;
        }
        if (isPetrified)
        {
            petrificationAction.Perform();
            return;
        }
        if(isInvincible)
        {
            if(!waitingForInvinciblityToOver.Perform())
            {
                MakeUnInvincible();
            }
            return;
        }
    }

    

    public void ProcessAnimationsInputs(bool[] inputs, bool[] previousInputs)
    {
        if (isRespawnningPlayer)
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

        }
        
    }

    

    public void ProcessInputAnimationControl()
    {
        if (isRespawnningPlayer)
        {
            return;
        }
        if (triggerFaceChangeEvent)
        {
            UpdateBasicWalkingSprite();
            triggerFaceChangeEvent = false;
        }
        if (isPushed)
        {
            return;
        }
        if (isPetrified)
        {
            return;
        }
        if (primaryMoveUseAnimationAction.isBeingUsed)
        {
            primaryMoveUseAnimationAction.Perform();
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
            if (!isRespawnningPlayer)
            {
                if(!isInvincible)
                {
                    if (inputs[(int)EnumData.Inputs.Shoot])
                    {
                        if (IsHeroAbleToFireProjectiles())
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
                        waitingActionForPrimaryMove.ReInitialiseTimerToEnd(primaryMoveAttackRateTickRate);
                    }
                }
                
            }
            


            if (isClient() && hasAuthority())
            {
                if (completedMotionToMovePoint)
                {
                    if(isRespawnningPlayer)
                    {
                        if (inputs[(int)EnumData.Inputs.RespawnPlayer] && previousInputs[(int)EnumData.Inputs.RespawnPlayer] != inputs[(int)EnumData.Inputs.RespawnPlayer])
                        {
                            Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position);
                            if (IsPlayerSpawnable(cellToCheckFor))
                            {
                                //Respawn player command
                                RespawnPlayerCommand respawnPlayerCommand = new RespawnPlayerCommand(GetLocalSequenceNo(), cellToCheckFor);
                                ClientSend.RespawnPlayer(respawnPlayerCommand);
                            }
                            else
                            {
                                Debug.LogError("Invalid location to spawn player");
                            }
                        }
                    }
                    else
                    {
                        if (inputs[(int)EnumData.Inputs.Push] && previousInputs[(int)EnumData.Inputs.Push] != inputs[(int)EnumData.Inputs.Push])
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
                        else if (inputs[(int)EnumData.Inputs.PlaceRemovalBoulder] && previousInputs[(int)EnumData.Inputs.PlaceRemovalBoulder] != inputs[(int)EnumData.Inputs.PlaceRemovalBoulder])
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
    }

    public void ProcessInputEventControl()
    {
        if (isRespawnningPlayer)
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
                //if (!IsActorPathBlockedForInputDrivenMovementByAnotherActor(Facing)&&CanOccupy(checkForCellPos))
                //{
                    currentMovePointCellPosition += GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                //}
            }
            else if (inputs[(int)EnumData.Inputs.Left])
            {
                Facing = FaceDirection.Left;
                //if (!IsActorPathBlockedForInputDrivenMovementByAnotherActor(Facing))
                //{
                    currentMovePointCellPosition += GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                //}
            }
            else if (inputs[(int)EnumData.Inputs.Down])
            {
                Facing = FaceDirection.Down;
                //if (!IsActorPathBlockedForInputDrivenMovementByAnotherActor(Facing))
                //{
                    currentMovePointCellPosition += GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                //}
            }
            else if (inputs[(int)EnumData.Inputs.Right])
            {
                Facing = FaceDirection.Right;
                //if (!IsActorPathBlockedForInputDrivenMovementByAnotherActor(Facing))
                //{
                    currentMovePointCellPosition += GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                //}
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
        if(!(collidedActorWithMyHead is Medusa))
        {
            collidedActorWithMyHead.Die();
        }
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        if (!(collidedActorWithMyHead is Medusa))
        {
            collidedActorWithMyHead.Die();
        }
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }
    
    public override bool CanOccupy(Vector3Int pos)
    {
        if (isRespawnningPlayer)
        {
            if (GridManager.instance.IsCellBlockedForFlyingUnitsAtPos(pos))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
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
        /*else */
        else if (GridManager.instance.IsCellBlockedForUnitMotionAtPos(pos))
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
        if(isPetrified||isPushed)
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
}
