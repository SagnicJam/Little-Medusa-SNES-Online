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
            if (inputs[(int)EnumData.MedusaInputs.Shoot])
            {
                if (!isUsingPrimaryMove)
                {
                    isUsingPrimaryMove = true;
                    UpdateFrameSprites();
                }
            }
            else if (!inputs[(int)EnumData.MedusaInputs.Shoot])
            {
                if (isUsingPrimaryMove)
                {
                    isUsingPrimaryMove = false;
                    UpdateFrameSprites();
                }
            }
            if (inputs[(int)EnumData.MedusaInputs.Up] || inputs[(int)EnumData.MedusaInputs.Down] || inputs[(int)EnumData.MedusaInputs.Left] || inputs[(int)EnumData.MedusaInputs.Right])
            {
                if (!isWalking)
                {
                    isWalking = true;
                    UpdateFrameSprites();
                }
            }
            else if (!(inputs[(int)EnumData.MedusaInputs.Up] || inputs[(int)EnumData.MedusaInputs.Down] || inputs[(int)EnumData.MedusaInputs.Left] || inputs[(int)EnumData.MedusaInputs.Right]))
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


    public override void ProcessEventsInputs(bool[] inputs, bool[] previousInputs)
    {
        if(!isInFlyingState)
        {
            if (inputs[(int)EnumData.MedusaInputs.Shoot])
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
            else if (!inputs[(int)EnumData.MedusaInputs.Shoot] && previousInputs[(int)EnumData.MedusaInputs.Shoot] != inputs[(int)EnumData.MedusaInputs.Shoot])
            {
                isFiringPrimaryProjectile = false;
                waitingActionForPrimaryMove.ReInitialiseTimerToEnd(primaryMoveAttackRateTickRate);
            }
            //else if (/*itemToCast is  SpawnItems spawnItems && */inputs[(int)EnumData.MedusaInputs.UseItem])
            //{
            //    if (IsHeroAbleToFireProjectiles())
            //    {
            //        if (!waitingActionForItemEyeLaserMove.Perform())
            //        {
            //            isFiringItemEyeLaser = true;
            //            waitingActionForItemEyeLaserMove.ReInitialiseTimerToBegin(itemEyeLaserMoveAttackRateTickRate);
            //        }
            //        else
            //        {
            //            isFiringItemEyeLaser = false;
            //        }
            //    }
            //}
            //else if (!inputs[(int)EnumData.MedusaInputs.UseItem] && previousInputs[(int)EnumData.MedusaInputs.UseItem] != inputs[(int)EnumData.MedusaInputs.UseItem])
            //{
            //    isFiringItemEyeLaser = false;
            //    waitingActionForItemEyeLaserMove.ReInitialiseTimerToEnd(itemEyeLaserMoveAttackRateTickRate);
            //}
        }
        

        if (!MultiplayerManager.instance.isServer && hasAuthority())
        {
            if (completedMotionToMovePoint)
            {
                if(isInFlyingState)
                {
                    if (inputs[(int)EnumData.MedusaInputs.LandPlayer] && previousInputs[(int)EnumData.MedusaInputs.LandPlayer] != inputs[(int)EnumData.MedusaInputs.LandPlayer])
                    {
                        Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position);
                        //land player command
                        LandPlayerCommand landPlayerCommand = new LandPlayerCommand(GetLocalSequenceNo(), cellToCheckFor);
                        ClientSend.LandPlayer(landPlayerCommand);
                    }
                }
                else
                {
                    if (inputs[(int)EnumData.MedusaInputs.RespawnPlayer] && previousInputs[(int)EnumData.MedusaInputs.RespawnPlayer] != inputs[(int)EnumData.MedusaInputs.RespawnPlayer])
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
                    else if (inputs[(int)EnumData.MedusaInputs.Push] && previousInputs[(int)EnumData.MedusaInputs.Push] != inputs[(int)EnumData.MedusaInputs.Push])
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

                        ClientEnemyManager clientEnemy = GridManager.instance.GetClientEnemyOnPos(cellPos);

                        if (clientEnemy != null)
                        {
                            if (IsClientEnemyPushable(Facing))
                            {
                                PushCommand pushCommand = new PushCommand(GetLocalSequenceNo(), (int)Facing, clientEnemy.id);
                                ClientSend.PushPlayerCommand(pushCommand);
                            }
                        }
                    }
                    else if (inputs[(int)EnumData.MedusaInputs.PlaceRemovalBoulder] && previousInputs[(int)EnumData.MedusaInputs.PlaceRemovalBoulder] != inputs[(int)EnumData.MedusaInputs.PlaceRemovalBoulder])
                    {
                        Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                        if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellToCheckFor) && !GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.TileType.BoulderAppearing))
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
                    else if (/*itemToCast is  SpawnItems spawnItems && */inputs[(int)EnumData.MedusaInputs.UseItem] && previousInputs[(int)EnumData.MedusaInputs.UseItem] != inputs[(int)EnumData.MedusaInputs.UseItem])
                    {
                        Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
                        if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellToCheckFor) && !GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.TileType.BoulderAppearing, EnumData.TileType.BoulderDisappearing))
                        {
                            //send command to server of placement
                            //PlaceCereberausHeadCommand placeCereberausHead = new PlaceCereberausHeadCommand(GetLocalSequenceNo(),(int)Facing, cellToCheckFor);
                            //ClientSend.PlaceCereberausHeadCommand(placeCereberausHead);

                            PlaceMinionCommand placeMinionCommand = new PlaceMinionCommand(GetLocalSequenceNo(), (int)Facing, cellToCheckFor);
                            ClientSend.PlaceMinionCommand(placeMinionCommand);
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
            Fire();
        }
        if (isFiringItemEyeLaser)
        {
            FireProjectile(new Attack(eyeLaserDamage,EnumData.AttackTypes.ProjectileAttack,EnumData.Projectiles.EyeLaser),GridManager.instance.grid.WorldToCell(actorTransform.position));
        }
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
            if (inputs[(int)EnumData.MedusaInputs.Up])
            {
                Facing = FaceDirection.Up;
            }
            else if (inputs[(int)EnumData.MedusaInputs.Left])
            {
                Facing = FaceDirection.Left;
            }
            else if (inputs[(int)EnumData.MedusaInputs.Down])
            {
                Facing = FaceDirection.Down;
            }
            else if (inputs[(int)EnumData.MedusaInputs.Right])
            {
                Facing = FaceDirection.Right;
            }

            if ((inputs[(int)EnumData.MedusaInputs.Up]|| inputs[(int)EnumData.MedusaInputs.Left]|| inputs[(int)EnumData.MedusaInputs.Down]|| inputs[(int)EnumData.MedusaInputs.Right]))
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
            if (!inputs[(int)EnumData.MedusaInputs.Up] && previousInputs[(int)EnumData.MedusaInputs.Up] != inputs[(int)EnumData.MedusaInputs.Up])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.MedusaInputs.Left] && previousInputs[(int)EnumData.MedusaInputs.Left] != inputs[(int)EnumData.MedusaInputs.Left])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.x);
                if (fractionCovered < GridManager.instance.grid.cellSize.x / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.MedusaInputs.Down] && previousInputs[(int)EnumData.MedusaInputs.Down] != inputs[(int)EnumData.MedusaInputs.Down])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.MedusaInputs.Right] && previousInputs[(int)EnumData.MedusaInputs.Right] != inputs[(int)EnumData.MedusaInputs.Right])
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

    public override bool IsHeroAbleToFireProjectiles()
    {
        Vector3 objectPosition = actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing);
        if (!GridManager.instance.IsPositionBlockedForProjectiles(objectPosition) && !GridManager.instance.IsPositionContainingPetrifiedActor(objectPosition))
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

    [Header("Inputs")]
    public bool up;
    public bool left;
    public bool down;
    public bool right;
    public bool shoot;
    public bool push;
    public bool placeORRemovalBoulder;
    public bool respawnPlayer;
    public bool landPlayer;
    public bool useItem;

    public override void DealInput()
    {
        if (!inGame || isPushed || isPetrified || isPhysicsControlled|| isInputFreezed)
        {
            up = false;
            left = false;
            down = false;
            right = false;
            shoot = false;
            push = false;
            placeORRemovalBoulder = false;
            respawnPlayer = false;
            landPlayer = false;
            useItem = false;
        }
        else if (isFiringServerProjectiles)
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
            shoot = Input.GetKey(KeyCode.J);
            push = Input.GetKey(KeyCode.J);
            placeORRemovalBoulder = Input.GetKey(KeyCode.K);
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
                shoot,
                push,
                placeORRemovalBoulder,
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
