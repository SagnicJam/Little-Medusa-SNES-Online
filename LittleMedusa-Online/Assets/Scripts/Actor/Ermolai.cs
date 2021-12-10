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

    public override void ProcessFlyingControl()
    {
        if (flyingTickCountTemp > 0)
        {
            flyingTickCountTemp--;
            //is flying
            if (!isInFlyingState)
            {
                //Start flying here
                FlyPlayer();
            }
        }
        else
        {
            if (isInFlyingState)
            {
                //land here
                LandPlayer(GridManager.instance.grid.WorldToCell(actorTransform.position));
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
                Mapper m = currentMapper;
                if (m != null && m is OneDNonCheckingMapper oneDNonCheckingMapper)
                {
                    if (GridManager.instance.IsCellContainingUpArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                    {
                        oneDNonCheckingMapper.face = FaceDirection.Up;
                    }
                    else if (GridManager.instance.IsCellContainingDownArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                    {
                        oneDNonCheckingMapper.face = FaceDirection.Down;
                    }
                    else if (GridManager.instance.IsCellContainingLeftArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                    {
                        oneDNonCheckingMapper.face = FaceDirection.Left;
                    }
                    else if (GridManager.instance.IsCellContainingRightArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
                    {
                        oneDNonCheckingMapper.face = FaceDirection.Right;
                    }
                    else
                    {
                        oneDNonCheckingMapper.face = Facing;
                    }
                }
                CheckSwitchCellIndex();
                if (GridManager.instance.IsCellBlockedForPetrifiedUnitMotionAtPos(currentMovePointCellPosition))
                {
                    StopPush(this);
                    return;
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
            if (itemToCast != null&& itemToCast.castableItemType == EnumData.CastItemTypes.ClientProjectiles)
            {
                if (itemToCast.itemCount > 0 && inputs[(int)EnumData.ErmolaiInputs.UseItem])
                {
                    SpawnClientProjectiles();
                }
                else if (itemToCast.itemCount <= 0 || (!inputs[(int)EnumData.ErmolaiInputs.UseItem] && previousInputs[(int)EnumData.ErmolaiInputs.UseItem] != inputs[(int)EnumData.ErmolaiInputs.UseItem]))
                {
                    ResetClientProjectilesVars();
                }
            }
        }
        if (!MultiplayerManager.instance.isServer && hasAuthority())
        {
            if (completedMotionToMovePoint)
            {
                if (isInFlyingState)
                {
                    if (inputs[(int)EnumData.ErmolaiInputs.LandPlayer] && previousInputs[(int)EnumData.ErmolaiInputs.LandPlayer] != inputs[(int)EnumData.ErmolaiInputs.LandPlayer])
                    {
                        if (isInFlyingState)
                        {
                            //land here
                            flyingTickCountTemp = 0;

                            LandPlayerCommand landPlayerCommand = new LandPlayerCommand(GetLocalSequenceNo());
                            ClientSend.LandPlayer(landPlayerCommand);
                        }
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
                            RespawnPlayerCommand respawnPlayerCommand = new RespawnPlayerCommand(GetLocalSequenceNo());
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
                    else if (itemToCast != null&& itemToCast.itemCount > 0 && itemToCast.castableItemType == EnumData.CastItemTypes.SpawnnableItems)
                    {
                        if (inputs[(int)EnumData.ErmolaiInputs.UseItem] && previousInputs[(int)EnumData.ErmolaiInputs.UseItem] != inputs[(int)EnumData.ErmolaiInputs.UseItem])
                        {
                            SpawnItem();
                        }
                    }
                }
            }
            bubbleShieldAttackReady = !waitingActionForBubbleShieldItemMove.Perform();

            if (!isInFlyingState)
            {
                if (itemToCast != null&& itemToCast.itemCount > 0&&itemToCast.castableItemType == EnumData.CastItemTypes.ServerProjectiles)
                {
                    if (inputs[(int)EnumData.ErmolaiInputs.UseItem] && previousInputs[(int)EnumData.ErmolaiInputs.UseItem] != inputs[(int)EnumData.ErmolaiInputs.UseItem])
                    {
                        SpawnServerProjectiles();
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
    public override void ProcessRemoteClientInputEventControl()
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
        }
        if (isFiringItemEyeLaser)
        {
            FireProjectile(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.EyeLaser), GridManager.instance.grid.WorldToCell(actorTransform.position));

        }
        if (isFiringItemFireball)
        {
            if (fireballUsedCount > 0)
            {
                FireProjectile(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.FireBall), GridManager.instance.grid.WorldToCell(actorTransform.position));
            }
        }
        if (isFiringItemStarShower)
        {
            FireDirectionalProjectile(new Attack(GameConfig.starshowerDamage, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.StarShower), GridManager.instance.grid.WorldToCell(actorTransform.position), FaceDirection.Up);
            FireDirectionalProjectile(new Attack(GameConfig.starshowerDamage, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.StarShower), GridManager.instance.grid.WorldToCell(actorTransform.position), FaceDirection.Down);
            FireDirectionalProjectile(new Attack(GameConfig.starshowerDamage, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.StarShower), GridManager.instance.grid.WorldToCell(actorTransform.position), FaceDirection.Left);
            FireDirectionalProjectile(new Attack(GameConfig.starshowerDamage, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.StarShower), GridManager.instance.grid.WorldToCell(actorTransform.position), FaceDirection.Right);
        }
        if (isFiringItemCentaurBow)
        {
            FireProjectile(new Attack(GameConfig.arrowDamage, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.CentaurBow), GridManager.instance.grid.WorldToCell(actorTransform.position));

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
            //Fire(this);
        }
        if (isFiringItemEyeLaser)
        {
            if (itemToCast != null && itemToCast.itemCount > 0 && itemToCast.castableItemType == EnumData.CastItemTypes.ClientProjectiles)
            {
                FireProjectile(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.EyeLaser), GridManager.instance.grid.WorldToCell(actorTransform.position));
                if (MultiplayerManager.instance.isServer)
                {
                    itemToCast.itemCount--;
                }
            }
        }
        if (isFiringItemFireball)
        {
            if (fireballUsedCount > 0)
            {
                if (itemToCast != null && itemToCast.itemCount > 0 && itemToCast.castableItemType == EnumData.CastItemTypes.ClientProjectiles)
                {
                    FireProjectile(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.FireBall), GridManager.instance.grid.WorldToCell(actorTransform.position));
                    if (MultiplayerManager.instance.isServer)
                    {
                        itemToCast.itemCount--;
                    }
                }
            }
        }
        if (isFiringItemStarShower)
        {
            if (itemToCast != null && itemToCast.itemCount > 0 && itemToCast.castableItemType == EnumData.CastItemTypes.ClientProjectiles)
            {
                FireDirectionalProjectile(new Attack(GameConfig.starshowerDamage, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.StarShower), GridManager.instance.grid.WorldToCell(actorTransform.position), FaceDirection.Up);
                FireDirectionalProjectile(new Attack(GameConfig.starshowerDamage, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.StarShower), GridManager.instance.grid.WorldToCell(actorTransform.position), FaceDirection.Down);
                FireDirectionalProjectile(new Attack(GameConfig.starshowerDamage, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.StarShower), GridManager.instance.grid.WorldToCell(actorTransform.position), FaceDirection.Left);
                FireDirectionalProjectile(new Attack(GameConfig.starshowerDamage, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.StarShower), GridManager.instance.grid.WorldToCell(actorTransform.position), FaceDirection.Right);
                if (MultiplayerManager.instance.isServer)
                {
                    itemToCast.itemCount--;
                }
            }
        }
        if (isFiringItemCentaurBow)
        {
            if (itemToCast != null && itemToCast.itemCount > 0 && itemToCast.castableItemType == EnumData.CastItemTypes.ClientProjectiles)
            {
                FireProjectile(new Attack(GameConfig.arrowDamage, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.CentaurBow), GridManager.instance.grid.WorldToCell(actorTransform.position));
                if (MultiplayerManager.instance.isServer)
                {
                    itemToCast.itemCount--;
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
                if (fractionCovered < GridManager.instance.grid.cellSize.y / GameConfig.fractionToConsiderAsInput)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.ErmolaiInputs.Left] && previousInputs[(int)EnumData.ErmolaiInputs.Left] != inputs[(int)EnumData.ErmolaiInputs.Left])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.x);
                if (fractionCovered < GridManager.instance.grid.cellSize.x / GameConfig.fractionToConsiderAsInput)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.ErmolaiInputs.Down] && previousInputs[(int)EnumData.ErmolaiInputs.Down] != inputs[(int)EnumData.ErmolaiInputs.Down])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                if (fractionCovered < GridManager.instance.grid.cellSize.y / GameConfig.fractionToConsiderAsInput)
                {
                    currentMovePointCellPosition = previousMovePointCellPosition;
                }
            }
            else if (!inputs[(int)EnumData.ErmolaiInputs.Right] && previousInputs[(int)EnumData.ErmolaiInputs.Right] != inputs[(int)EnumData.ErmolaiInputs.Right])
            {
                float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.x);
                if (fractionCovered < GridManager.instance.grid.cellSize.x / GameConfig.fractionToConsiderAsInput)
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
        else if(isFiringServerProjectiles || isMovementFreezed)
        {
            up = false;
            left = false;
            down = false;
            right = false;
        }
        else if (!isInFlyingState && !isRespawnningPlayer && GridManager.instance.IsCellContainingUpArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
        {
            up = true;
            left = false;
            down = false;
            right = false;
        }
        else if (!isInFlyingState && !isRespawnningPlayer && GridManager.instance.IsCellContainingDownArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
        {
            up = false;
            left = false;
            down = true;
            right = false;
        }
        else if (!isInFlyingState && !isRespawnningPlayer && GridManager.instance.IsCellContainingLeftArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
        {
            up = false;
            left = true;
            down = false;
            right = false;
        }
        else if (!isInFlyingState && !isRespawnningPlayer && GridManager.instance.IsCellContainingRightArrowAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)))
        {
            up = false;
            left = false;
            down = false;
            right = true;
        }
        else if (!isInFlyingState && !isRespawnningPlayer && GridManager.instance.IsCellContainingMirrorAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position)) && !GridManager.instance.IsCellBlockedForUnitMotionAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing))))
        {
            switch (Facing)
            {
                case FaceDirection.Up:
                    up = true;
                    left = false;
                    down = false;
                    right = false;
                    break;
                case FaceDirection.Down:
                    up = false;
                    left = false;
                    down = true;
                    right = false;
                    break;
                case FaceDirection.Left:
                    up = false;
                    left = true;
                    down = false;
                    right = false;
                    break;
                case FaceDirection.Right:
                    up = false;
                    left = false;
                    down = false;
                    right = true;
                    break;
            }
        }
        else
        {
            up = Input.GetKey(KeyCode.W);
            left = Input.GetKey(KeyCode.A);
            down = Input.GetKey(KeyCode.S);
            right = Input.GetKey(KeyCode.D);

            if (completedMotionToMovePoint)
            {
                if (!CanOccupy(GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing))))
                {
                    if (Facing == FaceDirection.Up)
                    {
                        up = false;
                    }
                    else if (Facing == FaceDirection.Down)
                    {
                        down = false;
                    }
                    else if (Facing == FaceDirection.Left)
                    {
                        left = false;
                    }
                    else if (Facing == FaceDirection.Right)
                    {
                        right = false;
                    }
                }
            }

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
