using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heliemis : Hero
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
            if (inputs[(int)EnumData.HeliemisInputs.ShootMightyWind])
            {
                if (!isUsingPrimaryMove)
                {
                    isUsingPrimaryMove = true;
                    UpdateFrameSprites();
                }
            }
            else if (!inputs[(int)EnumData.HeliemisInputs.ShootMightyWind])
            {
                if (isUsingPrimaryMove)
                {
                    isUsingPrimaryMove = false;
                    UpdateFrameSprites();
                }
            }

            if (inputs[(int)EnumData.HeliemisInputs.Up] || inputs[(int)EnumData.HeliemisInputs.Down] || inputs[(int)EnumData.HeliemisInputs.Left] || inputs[(int)EnumData.HeliemisInputs.Right])
            {
                if (!isWalking)
                {
                    isWalking = true;
                    UpdateFrameSprites();
                }
            }
            else if (!(inputs[(int)EnumData.HeliemisInputs.Up] || inputs[(int)EnumData.HeliemisInputs.Down] || inputs[(int)EnumData.HeliemisInputs.Left] || inputs[(int)EnumData.HeliemisInputs.Right]))
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
        //if (isInFlyingState)
        //{
        //    if (!waitingForFlightToEnd.Perform())
        //    {
        //        //land here
        //        LandPlayer();
        //        if (!IsPlayerSpawnable(GridManager.instance.grid.WorldToCell(actorTransform.position)))
        //        {
        //            TakeDamage(currentHP);
        //        }
        //        return;
        //    }
        //}
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
                    if (inputs[(int)EnumData.HeliemisInputs.LandPlayer] && previousInputs[(int)EnumData.HeliemisInputs.LandPlayer] != inputs[(int)EnumData.HeliemisInputs.LandPlayer])
                    {
                        Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position);
                        //land player command
                        LandPlayerCommand landPlayerCommand = new LandPlayerCommand(GetLocalSequenceNo(), cellToCheckFor);
                        ClientSend.LandPlayer(landPlayerCommand);
                    }
                }
                else
                {
                    if (inputs[(int)EnumData.HeliemisInputs.RespawnPlayer] && previousInputs[(int)EnumData.HeliemisInputs.RespawnPlayer] != inputs[(int)EnumData.HeliemisInputs.RespawnPlayer])
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
                    else if (inputs[(int)EnumData.HeliemisInputs.PlaceTornado] && previousInputs[(int)EnumData.HeliemisInputs.PlaceTornado] != inputs[(int)EnumData.HeliemisInputs.PlaceTornado])
                    {
                        Vector3Int cellToCheck = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                        if (!GridManager.instance.IsCellBlockedForUnitMotionAtPos(cellToCheck))
                        {
                            PlaceTornadoCommand placeTornoadoCommand = new PlaceTornadoCommand(GetLocalSequenceNo(), (int)Facing);
                            ClientSend.PlaceTornadoCommand(placeTornoadoCommand);
                        }
                    }
                }
            }
            if (!isInFlyingState)
            {
                if (inputs[(int)EnumData.HeliemisInputs.ShootMightyWind] && previousInputs[(int)EnumData.HeliemisInputs.ShootMightyWind] != inputs[(int)EnumData.HeliemisInputs.ShootMightyWind])
                {
                    if (IsHeroAbleToFireProjectiles())
                    {
                        FireMightyWindCommand fireMightyWindCommand = new FireMightyWindCommand(GetLocalSequenceNo(), (int)Facing, GridManager.instance.grid.WorldToCell(actorTransform.position));
                        ClientSend.FireMightyWind(fireMightyWindCommand);
                        isFiringServerProjectiles = true;
                        onCompletedMotionToPoint = () => { isFiringServerProjectiles = false; onCompletedMotionToPoint = null; };
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
        if(isPhysicsControlled)
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
        if(isPhysicsControlled)
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
            if (inputs[(int)EnumData.HeliemisInputs.Up])
            {
                Facing = FaceDirection.Up;
            }
            else if (inputs[(int)EnumData.HeliemisInputs.Left])
            {
                Facing = FaceDirection.Left;
            }
            else if (inputs[(int)EnumData.HeliemisInputs.Down])
            {
                Facing = FaceDirection.Down;
            }
            else if (inputs[(int)EnumData.HeliemisInputs.Right])
            {
                Facing = FaceDirection.Right;
            }

            if ((inputs[(int)EnumData.HeliemisInputs.Up] || inputs[(int)EnumData.HeliemisInputs.Left] || inputs[(int)EnumData.HeliemisInputs.Down] || inputs[(int)EnumData.HeliemisInputs.Right]))
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
            if (!inputs[(int)EnumData.HeliemisInputs.Up] && previousInputs[(int)EnumData.HeliemisInputs.Up] != inputs[(int)EnumData.HeliemisInputs.Up])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.HeliemisInputs.Left] && previousInputs[(int)EnumData.HeliemisInputs.Left] != inputs[(int)EnumData.HeliemisInputs.Left])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.x);
                if (fractionCovered < GridManager.instance.grid.cellSize.x / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.HeliemisInputs.Down] && previousInputs[(int)EnumData.HeliemisInputs.Down] != inputs[(int)EnumData.HeliemisInputs.Down])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.HeliemisInputs.Right] && previousInputs[(int)EnumData.HeliemisInputs.Right] != inputs[(int)EnumData.HeliemisInputs.Right])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.x);
                if (fractionCovered < GridManager.instance.grid.cellSize.x / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
        }
    }

    [Header("Input")]
    public bool up;
    public bool left;
    public bool down;
    public bool right;
    public bool shootMightyWind;
    public bool placeTornado;
    public bool respawnPlayer;
    public bool landPlayer;

    public override void DealInput()
    {
        if (!inGame || isPushed || isPetrified || isPhysicsControlled||isInputFreezed)
        {
            up = false;
            left = false;
            down = false;
            right = false;
            shootMightyWind = false;
            placeTornado = false;
            respawnPlayer = false;
            landPlayer = false;
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
            shootMightyWind = Input.GetKey(KeyCode.J);
            placeTornado = Input.GetKey(KeyCode.K);
            respawnPlayer = Input.GetKey(KeyCode.Return);
            landPlayer = Input.GetKey(KeyCode.K);
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
                shootMightyWind,
                placeTornado,
                respawnPlayer,
                landPlayer
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
