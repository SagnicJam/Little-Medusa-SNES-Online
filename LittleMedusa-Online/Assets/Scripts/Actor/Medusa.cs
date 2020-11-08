using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medusa : Hero
{
    public override void ProcessAuthoratativeEvents()
    {
        if (isRespawnningPlayer)
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
        if (isInvincible)
        {
            if (!waitingForInvinciblityToOver.Perform())
            {
                MakeUnInvincible();
            }
            return;
        }
    }



    public override void ProcessAnimationsInputs(bool[] inputs, bool[] previousInputs)
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



    public override void ProcessInputAnimationControl()
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


    public override void ProcessEventsInputs(bool[] inputs, bool[] previousInputs)
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
                if (!isInvincible)
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
                    if (isRespawnningPlayer)
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

    public override void ProcessInputEventControl()
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


    public override void ProcessMovementInputs(bool[] inputs, bool[] previousInputs)
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

    public override void ProcessInputMovementsControl()
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

    public override bool IsHeroAbleToFireProjectiles()
    {
        Vector3 objectPosition = actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing);
        if (!GridManager.instance.IsPositionBlockedForProjectiles(objectPosition) && !GridManager.instance.IsPositionContainingPetrifiedActor(objectPosition))
        {
            return true;
        }
        return false;
    }
}
