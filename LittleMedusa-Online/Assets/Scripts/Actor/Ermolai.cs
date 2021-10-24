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
            if (isFlying)
            {
                isFlying = false;
                UpdateFrameSprites();
            }
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

            if (inputs[(int)EnumData.ErmolaiInputs.Up] || inputs[(int)EnumData.ErmolaiInputs.Down] || inputs[(int)EnumData.ErmolaiInputs.Left] || inputs[(int)EnumData.ErmolaiInputs.Right])
            {
                if (!isWalking)
                {
                    isWalking = true;
                    UpdateFrameSprites();
                }
            }
            else if (!(inputs[(int)EnumData.ErmolaiInputs.Up] || inputs[(int)EnumData.ErmolaiInputs.Down] || inputs[(int)EnumData.ErmolaiInputs.Left] || inputs[(int)EnumData.ErmolaiInputs.Right]))
            {
                if (isWalking)
                {
                    isWalking = false;
                    UpdateFrameSprites();
                }
            }
        }
        else
        {
            if (!isFlying)
            {
                isWalking = false;
                isFlying = true;
                UpdateFrameSprites();
            }
        }
    }

    public override void ProcessAuthoratativeEvents()
    {
        if (isRespawnningPlayer)
        {
            return;
        }
        if (isInFlyingState)
        {
            if (!waitingForFlightToEnd.Perform())
            {
                //land here
                LandPlayer();
                if (!IsPlayerSpawnable(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                {
                    TakeDamage(currentHP);
                }
                return;
            }
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
        if (!MultiplayerManager.instance.isServer && hasAuthority())
        {
            if (completedMotionToMovePoint)
            {
                if (isInFlyingState)
                {
                    if (inputs[(int)EnumData.ErmolaiInputs.LandPlayer] && previousInputs[(int)EnumData.ErmolaiInputs.LandPlayer] != inputs[(int)EnumData.ErmolaiInputs.LandPlayer])
                    {
                        Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position);
                        //land player command
                        LandPlayerCommand landPlayerCommand = new LandPlayerCommand(GetLocalSequenceNo(), cellToCheckFor);
                        ClientSend.LandPlayer(landPlayerCommand);
                    }
                }
                else
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
                    else if (inputs[(int)EnumData.ErmolaiInputs.CastPitfall] && previousInputs[(int)EnumData.ErmolaiInputs.CastPitfall] != inputs[(int)EnumData.ErmolaiInputs.CastPitfall])
                    {
                        Vector3Int cellToCheck = GridManager.instance.grid.WorldToCell(actorTransform.position + 2 * GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                        if (GridManager.instance.HasTileAtCellPoint(cellToCheck, EnumData.TileType.Normal))
                        {
                            CastPitfallCommand castPitfallCommand = new CastPitfallCommand(GetLocalSequenceNo(), (int)Facing);
                            ClientSend.CastPitfall(castPitfallCommand);
                        }
                    }
                    else if (inputs[(int)EnumData.ErmolaiInputs.CastEarthquake] && previousInputs[(int)EnumData.ErmolaiInputs.CastEarthquake] != inputs[(int)EnumData.ErmolaiInputs.CastEarthquake])
                    {
                        CastEarthQuakeCommand castEarthQuakeCommand = new CastEarthQuakeCommand(GetLocalSequenceNo());
                        ClientSend.CastEarthQuake(castEarthQuakeCommand);
                    }
                    else if (/*itemToCast is  SpawnItems spawnItems && */inputs[(int)EnumData.ErmolaiInputs.UseItem] && previousInputs[(int)EnumData.ErmolaiInputs.UseItem] != inputs[(int)EnumData.ErmolaiInputs.UseItem])
                    {
                        Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                        if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellToCheckFor) && !GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.TileType.BoulderAppearing, EnumData.TileType.BoulderDisappearing))
                        {
                            //send command to server of placement
                            //PlaceCereberausHeadCommand placeCereberausHead = new PlaceCereberausHeadCommand(GetLocalSequenceNo(), (int)Facing, cellToCheckFor);
                            //ClientSend.PlaceCereberausHeadCommand(placeCereberausHead);

                            PlaceMinionCommand placeMinionCommand = new PlaceMinionCommand(GetLocalSequenceNo(), (int)Facing, cellToCheckFor);
                            ClientSend.PlaceMinionCommand(placeMinionCommand);
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

    public override void ProcessInputEventControl()
    {
        if (isRespawnningPlayer)
        {
            return;
        }
        if (isInFlyingState)
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

    public override void ProcessMovementInputs(bool[] inputs, bool[] previousInputs)
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
            if (inputs[(int)EnumData.ErmolaiInputs.Up])
            {
                Facing = FaceDirection.Up;
            }
            else if (inputs[(int)EnumData.ErmolaiInputs.Left])
            {
                Facing = FaceDirection.Left;
            }
            else if (inputs[(int)EnumData.ErmolaiInputs.Down])
            {
                Facing = FaceDirection.Down;
            }
            else if (inputs[(int)EnumData.ErmolaiInputs.Right])
            {
                Facing = FaceDirection.Right;
            }

            if ((inputs[(int)EnumData.ErmolaiInputs.Up] || inputs[(int)EnumData.ErmolaiInputs.Left] || inputs[(int)EnumData.ErmolaiInputs.Down] || inputs[(int)EnumData.ErmolaiInputs.Right]))
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
            if (!inputs[(int)EnumData.ErmolaiInputs.Up] && previousInputs[(int)EnumData.ErmolaiInputs.Up] != inputs[(int)EnumData.ErmolaiInputs.Up])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.ErmolaiInputs.Left] && previousInputs[(int)EnumData.ErmolaiInputs.Left] != inputs[(int)EnumData.ErmolaiInputs.Left])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.x);
                if (fractionCovered < GridManager.instance.grid.cellSize.x / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.ErmolaiInputs.Down] && previousInputs[(int)EnumData.ErmolaiInputs.Down] != inputs[(int)EnumData.ErmolaiInputs.Down])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.ErmolaiInputs.Right] && previousInputs[(int)EnumData.ErmolaiInputs.Right] != inputs[(int)EnumData.ErmolaiInputs.Right])
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
    public bool landPlayer;
    public bool useItem;

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
            landPlayer = false;
            useItem = false;
        }
        else if(isFiringServerProjectiles)
        {
            up = false;
            left = false;
            down = false;
            right = false;
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
            landPlayer = Input.GetKey(KeyCode.K);
            useItem = Input.GetKey(KeyCode.I);
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
                respawnPlayer,
                landPlayer,
                useItem
                };
        return inputs;
    }

    public override bool IsProjectilePlacable(Vector3Int predictedPos, FaceDirection facing)
    {
        Vector3 objectPosition = GridManager.instance.cellToworld(predictedPos) + GridManager.instance.GetFacingDirectionOffsetVector3(facing);
        if (!GridManager.instance.IsPositionBlockedForProjectiles(objectPosition))
        {
            return true;
        }
        return false;
    }
}
