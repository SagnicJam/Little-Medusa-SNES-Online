﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Posidanna : Hero
{
    public override void Start()
    {
        base.Start();
        rangedAttack_2 = new Attack(primaryMoveDamage, EnumData.AttackTypes.ProjectileAttack, projectileThrownType_2);
    }

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

            if (inputs[(int)EnumData.PosidannaInputs.ShootTidalWave])
            {
                if (!isUsingPrimaryMove)
                {
                    isUsingPrimaryMove = true;
                    UpdateFrameSprites();
                }
            }
            else if (!inputs[(int)EnumData.PosidannaInputs.ShootTidalWave])
            {
                if (isUsingPrimaryMove)
                {
                    isUsingPrimaryMove = false;
                    UpdateFrameSprites();
                }
            }

            if (inputs[(int)EnumData.PosidannaInputs.Up] || inputs[(int)EnumData.PosidannaInputs.Down] || inputs[(int)EnumData.PosidannaInputs.Left] || inputs[(int)EnumData.PosidannaInputs.Right])
            {
                if (!isWalking)
                {
                    isWalking = true;
                    UpdateFrameSprites();
                }
            }
            else if (!(inputs[(int)EnumData.PosidannaInputs.Up] || inputs[(int)EnumData.PosidannaInputs.Down] || inputs[(int)EnumData.PosidannaInputs.Left] || inputs[(int)EnumData.PosidannaInputs.Right]))
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
        bool secondaryAttackReady = !waitingActionForSecondaryMove.Perform();
        if (!MultiplayerManager.instance.isServer && hasAuthority())
        {
            if (completedMotionToMovePoint)
            {
                if (isInFlyingState)
                {
                    if (inputs[(int)EnumData.PosidannaInputs.LandPlayer] && previousInputs[(int)EnumData.PosidannaInputs.LandPlayer] != inputs[(int)EnumData.PosidannaInputs.LandPlayer])
                    {
                        Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position);
                        //land player command
                        LandPlayerCommand landPlayerCommand = new LandPlayerCommand(GetLocalSequenceNo(), cellToCheckFor);
                        ClientSend.LandPlayer(landPlayerCommand);
                    }
                }
                else
                {
                    if (inputs[(int)EnumData.PosidannaInputs.RespawnPlayer] && previousInputs[(int)EnumData.PosidannaInputs.RespawnPlayer] != inputs[(int)EnumData.PosidannaInputs.RespawnPlayer])
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
                    else if (/*itemToCast is  SpawnItems spawnItems && */inputs[(int)EnumData.PosidannaInputs.UseItem] && previousInputs[(int)EnumData.PosidannaInputs.UseItem] != inputs[(int)EnumData.PosidannaInputs.UseItem])
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
            if(!isInFlyingState)
            {
                if (inputs[(int)EnumData.PosidannaInputs.ShootTidalWave] && previousInputs[(int)EnumData.PosidannaInputs.ShootTidalWave] != inputs[(int)EnumData.PosidannaInputs.ShootTidalWave])
                {
                    if (IsHeroAbleToFireProjectiles())
                    {
                        FireTidalWaveCommand fireTidalWaveCommand = new FireTidalWaveCommand(GetLocalSequenceNo(), (int)Facing, GridManager.instance.grid.WorldToCell(actorTransform.position));
                        ClientSend.FireTidalWave(fireTidalWaveCommand);
                        isFiringServerProjectiles = true;
                        onCompletedMotionToPoint = () => { isFiringServerProjectiles = false; onCompletedMotionToPoint = null; };
                    }
                }
                else if (inputs[(int)EnumData.PosidannaInputs.CastBubbleShield] && previousInputs[(int)EnumData.PosidannaInputs.CastBubbleShield] != inputs[(int)EnumData.PosidannaInputs.CastBubbleShield] && secondaryAttackReady)
                {
                    waitingActionForSecondaryMove.ReInitialiseTimerToBegin(secondaryMoveAttackRateTickRate);
                    CastBubbleShieldCommand castBubbleShieldCommand = new CastBubbleShieldCommand(GetLocalSequenceNo(), GridManager.instance.grid.WorldToCell(actorTransform.position));
                    ClientSend.CastBubbleShield(castBubbleShieldCommand);

                    isFiringServerProjectiles = true;
                    onCompletedMotionToPoint = () => { isFiringServerProjectiles = false; onCompletedMotionToPoint = null; };
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
        if (isPhysicsControlled)
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

    public override void ProcessMovementInputs(bool[] inputs, bool[] previousInputs/*,int movementCommandPressCount*/)
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
            if (inputs[(int)EnumData.PosidannaInputs.Up])
            {
                Facing = FaceDirection.Up;
            }
            else if (inputs[(int)EnumData.PosidannaInputs.Left])
            {
                Facing = FaceDirection.Left;
            }
            else if (inputs[(int)EnumData.PosidannaInputs.Down])
            {
                Facing = FaceDirection.Down;
            }
            else if (inputs[(int)EnumData.PosidannaInputs.Right])
            {
                Facing = FaceDirection.Right;
            }

            if ((inputs[(int)EnumData.PosidannaInputs.Up] || inputs[(int)EnumData.PosidannaInputs.Left] || inputs[(int)EnumData.PosidannaInputs.Down] || inputs[(int)EnumData.PosidannaInputs.Right]))
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
            if (!inputs[(int)EnumData.PosidannaInputs.Up] && previousInputs[(int)EnumData.PosidannaInputs.Up] != inputs[(int)EnumData.PosidannaInputs.Up])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.PosidannaInputs.Left] && previousInputs[(int)EnumData.PosidannaInputs.Left] != inputs[(int)EnumData.PosidannaInputs.Left])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.x);
                if (fractionCovered < GridManager.instance.grid.cellSize.x / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.PosidannaInputs.Down] && previousInputs[(int)EnumData.PosidannaInputs.Down] != inputs[(int)EnumData.PosidannaInputs.Down])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.PosidannaInputs.Right] && previousInputs[(int)EnumData.PosidannaInputs.Right] != inputs[(int)EnumData.PosidannaInputs.Right])
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
    public bool shootTidalWave;
    public bool castBubbleShield;
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
            shootTidalWave = false;
            castBubbleShield = false;
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
            shootTidalWave = Input.GetKey(KeyCode.J);
            castBubbleShield = Input.GetKey(KeyCode.K);
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
                shootTidalWave,
                castBubbleShield,
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
