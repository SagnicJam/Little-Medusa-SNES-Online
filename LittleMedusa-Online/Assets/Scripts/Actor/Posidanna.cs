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
            if (inputs[(int)EnumData.PosidannaInputs.ShootTidalWave])
            {
                if (!primaryMoveUseAnimationAction.isBeingUsed)
                {
                    primaryMoveUseAnimationAction.isBeingUsed = true;
                }
            }
            else if (!inputs[(int)EnumData.PosidannaInputs.ShootTidalWave] && previousInputs[(int)EnumData.PosidannaInputs.ShootTidalWave] != inputs[(int)EnumData.PosidannaInputs.ShootTidalWave])
            {
                if (primaryMoveUseAnimationAction.isBeingUsed)
                {
                    primaryMoveUseAnimationAction.isBeingUsed = false;
                    primaryMoveUseAnimationAction.CancelMoveUsage();
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
                    }
                    else
                    {
                        if (inputs[(int)EnumData.PosidannaInputs.ShootTidalWave] && previousInputs[(int)EnumData.PosidannaInputs.ShootTidalWave] != inputs[(int)EnumData.PosidannaInputs.ShootTidalWave])
                        {
                            if (IsHeroAbleToFireProjectiles())
                            {
                                FireTidalWaveCommand fireTidalWaveCommand = new FireTidalWaveCommand(GetLocalSequenceNo(), (int)Facing);
                                ClientSend.FireTidalWave(fireTidalWaveCommand);
                            }
                        }
                        else if(inputs[(int)EnumData.PosidannaInputs.CastBubbleShield] && previousInputs[(int)EnumData.PosidannaInputs.CastBubbleShield] != inputs[(int)EnumData.PosidannaInputs.CastBubbleShield])
                        {
                            CastBubbleShieldCommand castBubbleShieldCommand = new CastBubbleShieldCommand(GetLocalSequenceNo());
                            ClientSend.CastBubbleShield(castBubbleShieldCommand);
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

    public override void ProcessInputEventControl()
    {
        if (isPhysicsControlled)
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

    [Header("Inputs")]
    public bool up;
    public bool left;
    public bool down;
    public bool right;
    public bool shootTidalWave;
    public bool castBubbleShield;
    public bool respawnPlayer;

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
                respawnPlayer
                };
        return inputs;
    }
}
