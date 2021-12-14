using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
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

        public override void ProcessFlyingControl()
        {
            if (flyingTickCountTemp > 0)
            {
                //is flying
                flyingTickCountTemp--;
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
            if (!isInFlyingState)
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
                else if (itemToCast != null && itemToCast.castableItemType == EnumData.CastItemTypes.ClientProjectiles)
                {
                    if (itemToCast.itemCount > 0 && inputs[(int)EnumData.MedusaInputs.UseItem])
                    {
                        SpawnClientProjectiles();
                    }
                    else if (itemToCast.itemCount <= 0 || (!inputs[(int)EnumData.MedusaInputs.UseItem] && previousInputs[(int)EnumData.MedusaInputs.UseItem] != inputs[(int)EnumData.MedusaInputs.UseItem]))
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
                        if (inputs[(int)EnumData.MedusaInputs.LandPlayer] && previousInputs[(int)EnumData.MedusaInputs.LandPlayer] != inputs[(int)EnumData.MedusaInputs.LandPlayer])
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
                        if (inputs[(int)EnumData.MedusaInputs.RespawnPlayer] && previousInputs[(int)EnumData.MedusaInputs.RespawnPlayer] != inputs[(int)EnumData.MedusaInputs.RespawnPlayer])
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
                            if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellToCheckFor) && !GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.TileType.NoBoulder) && !GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.GameObjectEnums.BoulderAppearing))
                            {
                                //send command to server of placement
                                PlaceBoulderCommand placeBoulderCommand = new PlaceBoulderCommand(GetLocalSequenceNo(), cellToCheckFor);
                                ClientSend.PlaceBoulderCommand(placeBoulderCommand);
                            }
                            else if (GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.TileType.Boulder) && !GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.TileType.NoBoulder) && !GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.GameObjectEnums.BoulderDisappearing))
                            {
                                RemoveBoulderCommand removeBoulderCommand = new RemoveBoulderCommand(GetLocalSequenceNo(), cellToCheckFor);
                                ClientSend.RemoveBoulderCommand(removeBoulderCommand);
                            }
                        }
                        else if (itemToCast != null && itemToCast.itemCount > 0 && itemToCast.castableItemType == EnumData.CastItemTypes.SpawnnableItems)
                        {
                            if (inputs[(int)EnumData.MedusaInputs.UseItem] && previousInputs[(int)EnumData.MedusaInputs.UseItem] != inputs[(int)EnumData.MedusaInputs.UseItem])
                            {
                                SpawnItem();
                            }
                        }
                    }
                }

                bubbleShieldAttackReady = !waitingActionForBubbleShieldItemMove.Perform();

                if (!isInFlyingState)
                {
                    if (itemToCast != null && itemToCast.itemCount > 0 && itemToCast.castableItemType == EnumData.CastItemTypes.ServerProjectiles)
                    {
                        if (inputs[(int)EnumData.MedusaInputs.UseItem] && previousInputs[(int)EnumData.MedusaInputs.UseItem] != inputs[(int)EnumData.MedusaInputs.UseItem])
                        {
                            SpawnServerProjectiles();
                        }
                    }
                }
            }
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
                FireProjectile(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.EyeLaser), GridManager.instance.grid.WorldToCell(actorTransform.position));
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
                FireProjectile(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.EyeLaser), GridManager.instance.grid.WorldToCell(actorTransform.position));
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

                if ((inputs[(int)EnumData.MedusaInputs.Up] || inputs[(int)EnumData.MedusaInputs.Left] || inputs[(int)EnumData.MedusaInputs.Down] || inputs[(int)EnumData.MedusaInputs.Right]))
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
                    if (fractionCovered < GridManager.instance.grid.cellSize.y / GameConfig.fractionToConsiderAsInput)
                    {
                        currentMovePointCellPosition = previousMovePointCellPosition;
                    }
                }
                else if (!inputs[(int)EnumData.MedusaInputs.Left] && previousInputs[(int)EnumData.MedusaInputs.Left] != inputs[(int)EnumData.MedusaInputs.Left])
                {
                    float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.x);
                    if (fractionCovered < GridManager.instance.grid.cellSize.x / GameConfig.fractionToConsiderAsInput)
                    {
                        currentMovePointCellPosition = previousMovePointCellPosition;
                    }
                }
                else if (!inputs[(int)EnumData.MedusaInputs.Down] && previousInputs[(int)EnumData.MedusaInputs.Down] != inputs[(int)EnumData.MedusaInputs.Down])
                {
                    float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.y);
                    if (fractionCovered < GridManager.instance.grid.cellSize.y / GameConfig.fractionToConsiderAsInput)
                    {
                        currentMovePointCellPosition = previousMovePointCellPosition;
                    }
                }
                else if (!inputs[(int)EnumData.MedusaInputs.Right] && previousInputs[(int)EnumData.MedusaInputs.Right] != inputs[(int)EnumData.MedusaInputs.Right])
                {
                    float fractionCovered = 1f - (Vector3.Distance(actorTransform.position, movePoint.position) / GridManager.instance.grid.cellSize.x);
                    if (fractionCovered < GridManager.instance.grid.cellSize.x / GameConfig.fractionToConsiderAsInput)
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

        //private void OnDrawGizmos()
        //{
        //    Gizmos.DrawCube(actorTransform.position, GameConfig.boxCastCellSizePercent* GridManager.instance.grid.cellSize);
        //}

        public override void DealInput()
        {
            if (!inGame || isPushed || isPetrified || isPhysicsControlled || isInputFreezed)
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
            else if (isFiringServerProjectiles || isMovementFreezed)
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
            else if (!isInFlyingState && !isRespawnningPlayer && GridManager.instance.IsCellContainingMirrorAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position))
                && (!GridManager.instance.IsCellBlockedForUnitMotionAtPos(GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing)))
                && !GridManager.instance.HasPetrifiedObject(GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing)))))
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
}