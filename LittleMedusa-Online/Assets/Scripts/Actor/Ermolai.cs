using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ermolai : Hero
{

    public override bool IsHeroAbleToFireProjectiles()
    {
        Vector3 objectPosition = actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing);
        if (!GridManager.instance.IsPositionBlockedForProjectiles(objectPosition))
        {
            return true;
        }
        return false;
    }

    public override bool IsHeroAbleToFireProjectiles(FaceDirection direction)
    {
        Vector3 objectPosition = actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(direction);
        if (!GridManager.instance.IsPositionBlockedForProjectiles(objectPosition))
        {
            return true;
        }
        return false;
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
            if (inputs[(int)EnumData.ErmolaiInputs.CastPitfall])
            {
                if (!isUsingPrimaryMove)
                {
                    isUsingPrimaryMove = true;
                    UpdateFrameSprites();
                }
            }
            else if (!inputs[(int)EnumData.ErmolaiInputs.CastPitfall])
            {
                if (isUsingPrimaryMove)
                {
                    isUsingPrimaryMove = false;
                    UpdateFrameSprites();
                }
            }

            if (inputs[(int)EnumData.Inputs.Up] || inputs[(int)EnumData.Inputs.Down] || inputs[(int)EnumData.Inputs.Left] || inputs[(int)EnumData.Inputs.Right])
            {
                if (!isWalking)
                {
                    isWalking = true;
                    UpdateFrameSprites();
                }
            }
            else if (!(inputs[(int)EnumData.Inputs.Up] || inputs[(int)EnumData.Inputs.Down] || inputs[(int)EnumData.Inputs.Left] || inputs[(int)EnumData.Inputs.Right]))
            {
                if (isWalking)
                {
                    isWalking = false;
                    UpdateFrameSprites();
                }
            }
        }
    }

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

    public override void ProcessEventsInputs(bool[] inputs, bool[] previousInputs)
    {
        if (isInputFreezed)
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
            if (isClient() && hasAuthority())
            {
                if (completedMotionToMovePoint)
                {
                    if (isRespawnningPlayer)
                    {
                        if (inputs[(int)EnumData.ErmolaiInputs.RespawnPlayer] && previousInputs[(int)EnumData.ErmolaiInputs.RespawnPlayer] != inputs[(int)EnumData.ErmolaiInputs.RespawnPlayer])
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
                        if (inputs[(int)EnumData.ErmolaiInputs.CastPitfall] && previousInputs[(int)EnumData.ErmolaiInputs.CastPitfall] != inputs[(int)EnumData.ErmolaiInputs.CastPitfall])
                        {
                            Vector3Int cellToCheck = GridManager.instance.grid.WorldToCell(actorTransform.position+2*GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                            if(GridManager.instance.HasTileAtCellPoint(cellToCheck,EnumData.TileType.Normal))
                            {
                                CastPitfallCommand castPitfallCommand = new CastPitfallCommand(GetLocalSequenceNo(),(int)Facing);
                                ClientSend.CastPitfall(castPitfallCommand);
                            }
                        }
                        else if (inputs[(int)EnumData.ErmolaiInputs.CastEarthquake] && previousInputs[(int)EnumData.ErmolaiInputs.CastEarthquake] != inputs[(int)EnumData.ErmolaiInputs.CastEarthquake])
                        {
                            CastEarthQuakeCommand castEarthQuakeCommand = new CastEarthQuakeCommand(GetLocalSequenceNo());
                            ClientSend.CastEarthQuake(castEarthQuakeCommand);
                        }
                    }
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
            UpdateFrameSprites();
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
        frameLooper.UpdateAnimationFrame();

    }

    public override void ProcessInputEventControl()
    {
        if (isRespawnningPlayer)
        {
            return;
        }
        if (isInputFreezed)
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
            //Fire(this);
        }
    }

    public override void ProcessInputMovementsControl()
    {
        if (isPhysicsControlled)
        {
            return;
        }
        if (isInputFreezed)
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

    public override void ProcessMovementInputs(bool[] inputs, bool[] previousInputs,int movementCommandPressCount)
    {
        if (isPhysicsControlled)
        {
            return;
        }
        if (isInputFreezed)
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

        if (completedMotionToMovePoint)
        {
            if (inputs[(int)EnumData.Inputs.Up])
            {
                Facing = FaceDirection.Up;
            }
            else if (inputs[(int)EnumData.Inputs.Left])
            {
                Facing = FaceDirection.Left;
            }
            else if (inputs[(int)EnumData.Inputs.Down])
            {
                Facing = FaceDirection.Down;
            }
            else if (inputs[(int)EnumData.Inputs.Right])
            {
                Facing = FaceDirection.Right;
            }

            if ((inputs[(int)EnumData.Inputs.Up] || inputs[(int)EnumData.Inputs.Left] || inputs[(int)EnumData.Inputs.Down] || inputs[(int)EnumData.Inputs.Right])
                && movementCommandPressCount > frameDelayForRegisteringInput)
            {
                //Vector3Int checkForCellPos = currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                //if (!IsActorPathBlockedForInputDrivenMovementByAnotherActor(Facing)&&CanOccupy(checkForCellPos))
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

    [Header("Inputs")]
    public bool up;
    public bool left;
    public bool down;
    public bool right;
    public bool castPitfall;
    public bool castEarthQuake;
    public bool respawnPlayer;
    public override void DealInput()
    {
        if (!inGame || isPushed ||isPetrified || isPhysicsControlled||isInputFreezed)
        {
            up = false;
            left = false;
            down = false;
            right = false;
            castPitfall = false;
            castEarthQuake = false;
            respawnPlayer = false;
        }
        else
        {
            up = Input.GetKey(KeyCode.W);
            left = Input.GetKey(KeyCode.A);
            down = Input.GetKey(KeyCode.S);
            right = Input.GetKey(KeyCode.D);
            castPitfall = Input.GetKey(KeyCode.J);
            castEarthQuake = Input.GetKey(KeyCode.K);
            respawnPlayer = Input.GetKey(KeyCode.Return);
        }
    }

    public override bool[] GetHeroInputs()
    {
        bool[] inputs = new bool[]
                {
                up,
                left,
                down,
                right,
                castPitfall,
                castEarthQuake,
                respawnPlayer
                };
        return inputs;
    }
    public override void ProcessInputFrameCount(bool[] inputs, bool[] previousInputs)
    {
        inputFrameCounter.ProcessInputFrameCount(inputs, previousInputs);
    }
}
