using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Averna : Hero
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

            if (inputs[(int)EnumData.AvernaInputs.ShootFireBall])
            {
                if (!isUsingPrimaryMove)
                {
                    isUsingPrimaryMove = true;
                    UpdateFrameSprites();
                }
            }
            else if (!inputs[(int)EnumData.AvernaInputs.ShootFireBall])
            {
                if (isUsingPrimaryMove)
                {
                    isUsingPrimaryMove = false;
                    UpdateFrameSprites();
                }
            }

            if (inputs[(int)EnumData.AvernaInputs.Up] || inputs[(int)EnumData.AvernaInputs.Down] || inputs[(int)EnumData.AvernaInputs.Left] || inputs[(int)EnumData.AvernaInputs.Right])
            {
                if (!isWalking)
                {
                    isWalking = true;
                    UpdateFrameSprites();
                }
            }
            else if (!(inputs[(int)EnumData.AvernaInputs.Up] || inputs[(int)EnumData.AvernaInputs.Down] || inputs[(int)EnumData.AvernaInputs.Left] || inputs[(int)EnumData.AvernaInputs.Right]))
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
                if (m!=null && m is OneDNonCheckingMapper oneDNonCheckingMapper)
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
        if(!isInFlyingState)
        {
            if (inputs[(int)EnumData.AvernaInputs.ShootFireBall])
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
            else if (!inputs[(int)EnumData.AvernaInputs.ShootFireBall] && previousInputs[(int)EnumData.AvernaInputs.ShootFireBall] != inputs[(int)EnumData.AvernaInputs.ShootFireBall])
            {
                isFiringPrimaryProjectile = false;
                waitingActionForPrimaryMove.ReInitialiseTimerToEnd(primaryMoveAttackRateTickRate);
            }
        }
        

        if (!MultiplayerManager.instance.isServer && hasAuthority())
        {
            if (completedMotionToMovePoint)
            {
                if (isInFlyingState)
                {
                    if (inputs[(int)EnumData.AvernaInputs.LandPlayer] && previousInputs[(int)EnumData.AvernaInputs.LandPlayer] != inputs[(int)EnumData.AvernaInputs.LandPlayer])
                    {
                        Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position);
                        //land player command
                        LandPlayerCommand landPlayerCommand = new LandPlayerCommand(GetLocalSequenceNo(), cellToCheckFor);
                        ClientSend.LandPlayer(landPlayerCommand);
                    }
                }
                else
                {
                    if (inputs[(int)EnumData.AvernaInputs.RespawnPlayer] && previousInputs[(int)EnumData.AvernaInputs.RespawnPlayer] != inputs[(int)EnumData.AvernaInputs.RespawnPlayer])
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
                    else if (/*itemToCast is  SpawnItems spawnItems && */inputs[(int)EnumData.AvernaInputs.UseItem] && previousInputs[(int)EnumData.AvernaInputs.UseItem] != inputs[(int)EnumData.AvernaInputs.UseItem])
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
            if (!isInFlyingState&&inputs[(int)EnumData.AvernaInputs.CastFlamePillar] && previousInputs[(int)EnumData.AvernaInputs.CastFlamePillar] != inputs[(int)EnumData.AvernaInputs.CastFlamePillar])
            {
                if (IsHeroAbleToFireProjectiles())
                {
                    CastFlamePillar castFlamePillar = new CastFlamePillar(GetLocalSequenceNo(), (int)Facing, GridManager.instance.grid.WorldToCell(actorTransform.position));
                    ClientSend.CastFlamePillar(castFlamePillar);
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
            positionToSpawnProjectile = GridManager.instance.grid.WorldToCell(actorTransform.position);
            rangedAttack_1.SetAttackingActorId(ownerId);
            DynamicItem dynamicItem = new DynamicItem
            {
                ranged = rangedAttack_1,
                activate = new TileBasedProjectileUse()
            };
            currentAttack = rangedAttack_1;
            dynamicItem.activate.BeginToUse(this, null, null);
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
        if(isInputFreezed)
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
            if (inputs[(int)EnumData.AvernaInputs.Up])
            {
                Facing = FaceDirection.Up;
            }
            else if (inputs[(int)EnumData.AvernaInputs.Left])
            {
                Facing = FaceDirection.Left;
            }
            else if (inputs[(int)EnumData.AvernaInputs.Down])
            {
                Facing = FaceDirection.Down;
            }
            else if (inputs[(int)EnumData.AvernaInputs.Right])
            {
                Facing = FaceDirection.Right;
            }

            if ((inputs[(int)EnumData.AvernaInputs.Up] || inputs[(int)EnumData.AvernaInputs.Left] || inputs[(int)EnumData.AvernaInputs.Down] || inputs[(int)EnumData.AvernaInputs.Right]))
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
            if (!inputs[(int)EnumData.AvernaInputs.Up] && previousInputs[(int)EnumData.AvernaInputs.Up] != inputs[(int)EnumData.AvernaInputs.Up])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.AvernaInputs.Left] && previousInputs[(int)EnumData.AvernaInputs.Left] != inputs[(int)EnumData.AvernaInputs.Left])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.x);
                if (fractionCovered < GridManager.instance.grid.cellSize.x / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.AvernaInputs.Down] && previousInputs[(int)EnumData.AvernaInputs.Down] != inputs[(int)EnumData.AvernaInputs.Down])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / 2f)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.AvernaInputs.Right] && previousInputs[(int)EnumData.AvernaInputs.Right] != inputs[(int)EnumData.AvernaInputs.Right])
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
    public bool shootFireBall;
    public bool castFlamePillar;
    public bool respawnPlayer;
    public bool landPlayer;
    public bool useItem;

    public override void DealInput()
    {
        if (!inGame || isPushed || isPetrified || isPhysicsControlled||isInputFreezed)
        {
            up = false;
            left = false;
            down = false;
            right = false;
            shootFireBall = false;
            castFlamePillar = false;
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
            shootFireBall = Input.GetKey(KeyCode.J);
            castFlamePillar = Input.GetKey(KeyCode.K);
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
                shootFireBall,
                castFlamePillar,
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
