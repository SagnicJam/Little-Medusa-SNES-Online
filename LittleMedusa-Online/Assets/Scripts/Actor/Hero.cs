using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public abstract class Hero : Actor
{
    [Header("Tweak Params")]
    public Color petrificationColor;
    public Color invincibleColor;
    public int itemEyeLaserMoveAttackRateTickRate;
    public int itemFireballMoveAttackRateTickRate;
    public int bubbleShieldItemMoveAttackRateTickRate;

    [Header("Scene References")]
    public Image healthFillImage;
    public TextMeshProUGUI currentLifeStockText;
    public SpriteRenderer statusSprite;

    [Header("Live Data")]
    public int hero;
    public bool isInputFreezed;
    public bool isFiringServerProjectiles;
    public bool bubbleShieldAttackReady;

    public CastItem itemToCast;

    [Header("Actions")]
    public WaitingForNextAction waitingActionForBubbleShieldItemMove = new WaitingForNextAction();
    public WaitingForNextAction waitingActionForItemEyeLaserMove = new WaitingForNextAction();
    public WaitingForNextAction waitingActionForItemFireballMove = new WaitingForNextAction();

    public override void Awake()
    {
        base.Awake();
        itemToCast = new CastItem(0,0,0);
        waitingActionForBubbleShieldItemMove.Initialise(this);
        waitingActionForBubbleShieldItemMove.ReInitialiseTimerToEnd(bubbleShieldItemMoveAttackRateTickRate);

        waitingActionForItemEyeLaserMove.Initialise(this);
        waitingActionForItemEyeLaserMove.ReInitialiseTimerToEnd(itemEyeLaserMoveAttackRateTickRate);

        waitingActionForItemFireballMove.Initialise(this);
        waitingActionForItemFireballMove.ReInitialiseTimerToEnd(itemFireballMoveAttackRateTickRate);
    }

    public override void MakeInvincible()
    {
        base.MakeInvincible();
        isFiringPrimaryProjectile = false;
        waitingActionForPrimaryMove.ReInitialiseTimerToEnd(primaryMoveAttackRateTickRate);
    }

    public void InitialiseActor(PlayerStateUpdates playerStateUpdates)
    {
        SetActorPositionalState(playerStateUpdates.positionUpdates);
        SetFlyingTickCount(playerStateUpdates.playerFlyData);
        SetActorAnimationState(playerStateUpdates.playerAnimationEvents);
        SetActorEventActionState(playerStateUpdates.playerEvents);
        SetAuthoratativeStates(playerStateUpdates.playerAuthoratativeStates);
    }

    //authoratatively is performed
    public void SetAuthoratativeStates(PlayerAuthoratativeStates playerAuthoratativeStates)
    {
        if(statusSprite!=null)
        {
            if (playerAuthoratativeStates.isPetrified && isPetrified != playerAuthoratativeStates.isPetrified)
            {
                statusSprite.enabled = true;
                statusSprite.color = petrificationColor;
            }
            if (!playerAuthoratativeStates.isPetrified && isPetrified != playerAuthoratativeStates.isPetrified)
            {
                statusSprite.enabled = false;
            }
            if (playerAuthoratativeStates.isInvincible && isInvincible != playerAuthoratativeStates.isInvincible)
            {
                statusSprite.enabled = true;
                statusSprite.color = invincibleColor;
            }
            if (!playerAuthoratativeStates.isInvincible && isInvincible != playerAuthoratativeStates.isInvincible)
            {
                statusSprite.enabled = false;
            }
        }

        if(playerAuthoratativeStates.inCharacterSelectionScreen && inCharacterSelectionScreen!=playerAuthoratativeStates.inCharacterSelectionScreen)
        {
            //turn off model
            //turn on character selection screen
            //Debug.LogError("Turn off model");
            gameObject.transform.parent.gameObject.SetActive(false);
        }
        if (playerAuthoratativeStates.inGame && inGame != playerAuthoratativeStates.inGame)
        {
            //turn on model
            //turn off character selection screen
            //Debug.LogError("Turn on model");
            gameObject.transform.parent.gameObject.SetActive(true);
            if(CharacterSelectionScreen.instance.gameObject.activeSelf)
            {
                CharacterSelectionScreen.instance.gameObject.SetActive(false);
            }
        }

        inCharacterSelectionScreen = playerAuthoratativeStates.inCharacterSelectionScreen;
        inGame = playerAuthoratativeStates.inGame;
        hero = playerAuthoratativeStates.hero;
        isInputFreezed = playerAuthoratativeStates.inputFreezed;
        isPetrified = playerAuthoratativeStates.isPetrified;
        isPushed = playerAuthoratativeStates.isPushed;
        isPhysicsControlled = playerAuthoratativeStates.isPhysicsControlled;
        isInvincible = playerAuthoratativeStates.isInvincible;

        if (!isRespawnningPlayer && isRespawnningPlayer != playerAuthoratativeStates.isRespawnningPlayer)
        {
            //enable crosshair locally
            //disable collider
            if (statusSprite != null)
            {
                statusSprite.gameObject.SetActive(false);
            }
            SetRespawnState();
        }
        if (isRespawnningPlayer && isRespawnningPlayer != playerAuthoratativeStates.isRespawnningPlayer)
        {
            //disable crosshair locally
            //enable back collider
            if (statusSprite != null)
            {
                statusSprite.gameObject.SetActive(true);
            }
            SetSpawnState();
        }
        if (!isPhysicsControlled && isPhysicsControlled != playerAuthoratativeStates.isPhysicsControlled)
        {
            SetActorCollider(false);
        }
        if (isPhysicsControlled && isPhysicsControlled != playerAuthoratativeStates.isPhysicsControlled)
        {
            SetActorCollider(true);
        }

        isRespawnningPlayer = playerAuthoratativeStates.isRespawnningPlayer;

        currentHP = playerAuthoratativeStates.currentHP;
        currentStockLives = playerAuthoratativeStates.currentStockLives;

        itemToCast = new CastItem((EnumData.CastItemTypes)playerAuthoratativeStates.itemToCast.castItemType
                , (EnumData.UsableItemTypes)playerAuthoratativeStates.itemToCast.usableItemType
            , playerAuthoratativeStates.itemToCast.itemCount);

        if (healthFillImage!=null)
        {
            healthFillImage.fillAmount =  (1f*currentHP) / maxHP;
            currentLifeStockText.text =  currentStockLives.ToString();
        }
    }

    


    //authoratatively is performed(but is locally is also done)-correction happens
    public void SetFlyingTickCount(PlayerFlyData playerFlyData)
    {
        flyingTickCountTemp = playerFlyData.flyingTickCount;
    }
    public void SetActorPositionalState(PositionUpdates positionUpdates)
    {
        actorTransform.position = positionUpdates.updatedActorPosition;
        currentMovePointCellPosition = positionUpdates.updatedBlockActorPosition;
        previousMovePointCellPosition = positionUpdates.updatedPreviousBlockActorPosition;
        Facing = GridManager.instance.GetFaceDirectionFromCurrentPrevPoint(currentMovePointCellPosition, previousMovePointCellPosition, this);

        if (Facing != (FaceDirection)positionUpdates.Facing || PreviousFacingDirection != (FaceDirection)positionUpdates.previousFacing)
        {
            Facing = (FaceDirection)positionUpdates.Facing;
            PreviousFacingDirection = (FaceDirection)positionUpdates.previousFacing;
        }
    }

    public void SetActorAnimationState(PlayerAnimationEvents playerAnimationEvents)
    {
        if (isUsingPrimaryMove!= playerAnimationEvents.isPrimaryMoveAnimationBeingPlayed)
        {
            isUsingPrimaryMove = playerAnimationEvents.isPrimaryMoveAnimationBeingPlayed;
            UpdateFrameSprites();
        }

        if (isWalking != playerAnimationEvents.isWalking)
        {
            isWalking = playerAnimationEvents.isWalking;
            UpdateFrameSprites();
        }

        if (isFlying != playerAnimationEvents.isFlying)
        {
            isFlying = playerAnimationEvents.isFlying;
            UpdateFrameSprites();
        }
    }

    //authoratatively is performed(but is locally is also done)
    public void SetActorEventActionState(PlayerEvents playerEvents)
    {
        isFiringPrimaryProjectile = playerEvents.firedPrimaryMoveProjectile;
        isFiringItemEyeLaser = playerEvents.firedItemEyeLaserMoveProjectile;
        isFiringItemFireball = playerEvents.firedItemFireballMoveProjectile;
    }

    /// <summary>
    /// Called on server only
    /// </summary>
    public abstract void ProcessAuthoratativeEvents();

    /// <summary>
    /// Called locally on client and on server
    /// </summary>
    /// <param name="inputs"></param>
    /// <param name="previousInputs"></param>
    public abstract void ProcessAnimationsInputs(bool[] inputs, bool[] previousInputs);

    /// <summary>
    /// Called locally on client,on remote client,on server
    /// </summary>
    public abstract void ProcessInputAnimationControl();

    /// <summary>
    /// Called locally on client and on server
    /// </summary>
    /// <param name="inputs"></param>
    /// <param name="previousInputs"></param>
    public abstract void ProcessEventsInputs(bool[] inputs, bool[] previousInputs);

    /// <summary>
    /// Called locally on client,on remote client,on server
    /// </summary>
    public abstract void ProcessInputEventControl();

    /// <summary>
    /// Called locally on client and on server
    /// </summary>
    /// <param name="inputs"></param>
    /// <param name="previousInputs"></param>
    public abstract void ProcessMovementInputs(bool[] inputs, bool[] previousInputs);
    public abstract void ProcessFlyingControl();
    /// <summary>
    /// Called locally on client and on server
    /// </summary>
    public abstract void ProcessInputMovementsControl();
    public abstract bool IsHeroAbleToFireProjectiles();
    public abstract bool IsHeroAbleToFireProjectiles(FaceDirection facing);
    public abstract bool IsProjectilePlacable(Vector3Int predictedPos,FaceDirection facing);

    //Abilities
    public void CastFlamePillar(Attack attack,Vector3Int predictedCell)
    {
        positionToSpawnProjectile = predictedCell;

        attack.SetAttackingActorId(ownerId);
        DynamicItem dynamicItem = new DynamicItem
        {
            ranged = attack,
            activate = new TileBasedProjectileUse()
        };
        currentAttack = attack;
        dynamicItem.activate.BeginToUse(this, null, dynamicItem.ranged.OnHit);
    }

    public void PlacePortal(Vector3Int cell)
    {
        GridManager.instance.portal.PlacePortal(ownerId, cell);
    }

    public void PlaceTornado(Vector3Int cell)
    {
        //StartCoroutine(RunTornado(5, cell));
        GridManager.instance.tornado.PlaceTornadoObject(ownerId,cell);
    }

    public void CastPitfall(Vector3Int cell)
    {
        //do damage
        //start timer for hole in gridmanager
        Actor actor = GridManager.instance.GetActorOnPos(cell);
        if (actor != null)
        {
            actor.TakeDamage(actor.currentHP);
        }
        GridManager.instance.SetTile(cell, EnumData.TileType.Hole, true, false);
        GridManager.instance.SetTile(cell, EnumData.TileType.Normal, false, false);
        GridManager.instance.SwitchTileAfter(cell, EnumData.TileType.Hole,EnumData.TileType.Normal);
    }

    public void CastBubbleShield(Attack attack,Vector3Int predictedCell,FaceDirection facing)
    {
        positionToSpawnProjectile = predictedCell;
        attack.SetAttackingActorId(ownerId);
        currentAttack = attack;
        switch(facing)
        {
            case FaceDirection.Up:
                DynamicItem dynamicItem = new DynamicItem
                {
                    ranged = attack,
                    activate = new DirectionBasedProjectileUse(270, actorTransform.right, null)
                };
                dynamicItem.activate.BeginToUse(this, null, dynamicItem.ranged.OnHit);
                break;
            case FaceDirection.Down:
                DynamicItem dynamicItem1 = new DynamicItem
                {
                    ranged = attack,
                    activate = new DirectionBasedProjectileUse(90, actorTransform.right, null)
                };
                dynamicItem1.activate.BeginToUse(this, null, dynamicItem1.ranged.OnHit);
                break;
            case FaceDirection.Left:
                DynamicItem dynamicItem2 = new DynamicItem
                {
                    ranged = attack,
                    activate = new DirectionBasedProjectileUse(180, actorTransform.right, null)
                };
                dynamicItem2.activate.BeginToUse(this, null, dynamicItem2.ranged.OnHit);
                break;
            case FaceDirection.Right:
                DynamicItem dynamicItem3 = new DynamicItem
                {
                    ranged = attack,
                    activate = new DirectionBasedProjectileUse(0, actorTransform.right, null)
                };
                dynamicItem3.activate.BeginToUse(this, null, dynamicItem3.ranged.OnHit);
                break;
        }
    }
    //end abilities

    //do stop push
    //do remaining monster code inside head collision
    //-------------------------------------------------//--------------------------------------------------------------------------------------------//--------------------------------

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndPetrified(Actor collidedActorWithMyHead)
    {
        PushActor(collidedActorWithMyHead,Facing);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        PushActor(collidedActorWithMyHead,Facing);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
        if(collidedActorWithMyHead is Enemy enemy)
        {
            if(enemy.leaderNetworkId!=ownerId)
            {
                TakeDamage(enemy.primaryMoveDamage);
            }
        }
    }

    public override void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        PushActor(collidedActorWithMyHead,Facing);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        Debug.Log("medusa case occur when i was snapping back as petrified object and got my head collided");
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndAmPetrified(Actor collidedActorWithMyHead)
    {
        if(!(collidedActorWithMyHead is Hero))
        {
            collidedActorWithMyHead.TakeDamage(currentHP);
        }
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        if (!(collidedActorWithMyHead is Hero))
        {
            collidedActorWithMyHead.TakeDamage(currentHP);
        }
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnBodyCollidedWithPortalTiles(TileData tileData)
    {
        Portal portal = tileData.GetComponent<Portal>();
        portal.ActorUnitEnter(this,currentMovePointCellPosition);
    }

    public override void OnBodyCollidedWithHourGlassTile(Vector3Int hourGlassTile)
    {
        GridManager.instance.SetTile(hourGlassTile, EnumData.TileType.Hourglass, false,false);
        ServerSideGameManager.instance.StopWorldDestruction();
    }
    public override void OnBodyCollidedWithIcarausWingsItemTiles(Vector3Int icarausCollectedOnTilePos)
    {
        if(MultiplayerManager.instance.isServer)
        {
            GridManager.instance.SetTile(icarausCollectedOnTilePos, EnumData.TileType.IcarusWingsItem, false, false);
        }
        flyingTickCountTemp = flyingTickCount;
    }

    public override void OnBodyCollidedWithHeartItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.HeartItem, false, false);
        if(currentHP<maxHP)
        {
            currentHP += 1;
        }
        //increase hp by 1
    }

    public override void OnBodyCollidedWithCereberausHeadItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.CereberausHeadItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.SpawnnableItems, EnumData.UsableItemTypes.CereberausHead,1);
    }

    public override void OnBodyCollidedWithMinionItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.MinionItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.SpawnnableItems, EnumData.UsableItemTypes.Minion, 1);
    }

    public override void OnBodyCollidedWithEyeLaserItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.EyeLaserItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.ClientProjectiles, EnumData.UsableItemTypes.EyeLaser, 5);
    }

    public override void OnBodyCollidedWithBoulderItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.BoulderItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.SpawnnableItems, EnumData.UsableItemTypes.Boulder, 5);
    }

    public override void OnBodyCollidedWithTidalWaveItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.TidalWaveItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.ServerProjectiles, EnumData.UsableItemTypes.TidalWave, 5);
    }

    public override void OnBodyCollidedWithBubbleShieldItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.BubbleShieldItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.ServerProjectiles, EnumData.UsableItemTypes.BubbleShield, 5);
    }

    public override void OnBodyCollidedWithMightyWindItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.MightyWindItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.ServerProjectiles, EnumData.UsableItemTypes.MightyWind, 5);
    }

    public override void OnBodyCollidedWithTornadoItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.TornadoPullItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.SpawnnableItems, EnumData.UsableItemTypes.Tornado, 5);
    }

    public override void OnBodyCollidedWithPortalItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.PortalItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.SpawnnableItems, EnumData.UsableItemTypes.Portal, 1);
    }

    public override void OnBodyCollidedWithPitfallItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.PitfallItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.SpawnnableItems, EnumData.UsableItemTypes.Pitfall, 5);
    }

    public override void OnBodyCollidedWithEarthquakeItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.EarthQuakeItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.SpawnnableItems,EnumData.UsableItemTypes.Earthquake, 5);
    }

    public override void OnBodyCollidedWithFireballItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.FireballItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.ClientProjectiles, EnumData.UsableItemTypes.Fireball, 5);
    }

    public override void OnBodyCollidedWithFlamePillarItemTiles(Vector3Int cellPos)
    {
        GridManager.instance.SetTile(cellPos, EnumData.TileType.FlamePillarItem, false, false);
        itemToCast = new CastItem(EnumData.CastItemTypes.ServerProjectiles, EnumData.UsableItemTypes.FlamePillar, 5);
    }

    public override bool CanOccupy(Vector3Int pos)
    {
        if (isRespawnningPlayer)
        {
            if (GridManager.instance.IsCellBlockedForFlyingUnitsAtPos(pos))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        if (isInFlyingState)
        {
            if (GridManager.instance.IsCellBlockedForFlyingUnitsAtPos(pos))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /*else */
        else if (GridManager.instance.IsCellBlockedForUnitMotionAtPos(pos))
        {
            return false;
        }
        //else if (IsActorOnArrows() && GridManager.instance.IsCellContainingMonsterLinkedWithDifferentChainOnCellPos(pos, this))
        //{
        //    return false;
        //}
        //else if (IsActorOnArrows() && GridManager.instance.IsCellContainingPetrifiedMonsterOnCell(pos, this))
        //{
        //    return false;
        //}
        //else if (IsActorOnMirror() && GridManager.instance.IsCellContainingPetrifiedMonsterOnCell(pos, this))
        //{
        //    return false;
        //}
        else
        {
            return true;
        }
    }

    public override void OnPushStart()
    {
        Debug.Log("OnPushStart ");
        walkSpeed = pushSpeed;
    }

    public override void OnPushStop()
    {
        Debug.Log("OnPushStop ");
        walkSpeed = normalSpeed;
    }

    public override void OnBodyCollidingWithKillingTiles(int killingTileSpawnerId, TileData tileData)
    {
        if(tileData.gameObjectEnums==EnumData.GameObjectEnums.Earthquake)
        {
            if(killingTileSpawnerId != ownerId)
            {
                TakeDamage(currentHP);
            }
        }
        else
        {
            TakeDamage(currentHP);
        }
    }

    public override void OnBodyCollidingWithTornadoEffectTiles(TileData tileData)
    {
        GridManager.instance.tornado.OnEnterTornadoRegion(tileData,this);
    }

    public override void OnCantOccupySpace()
    {
        if (isPetrified || isPushed)
        {
            currentMovePointCellPosition = GridManager.instance.grid.WorldToCell(actorTransform.position);
            previousMovePointCellPosition = currentMovePointCellPosition;
        }
        else
        {
            currentMovePointCellPosition = previousMovePointCellPosition;
            actorTransform.position = GridManager.instance.cellToworld(previousMovePointCellPosition);
        }
    }


    public void SpawnServerProjectiles()
    {
        if (itemToCast.usableItemType == EnumData.UsableItemTypes.TidalWave)
        {
            if (IsHeroAbleToFireProjectiles())
            {
                SpawnItemCommand spawnItemCommand = new SpawnItemCommand(GetLocalSequenceNo(), (int)Facing, (int)itemToCast.usableItemType, GridManager.instance.grid.WorldToCell(actorTransform.position));
                ClientSend.SpawnItemCommand(spawnItemCommand);
                isFiringServerProjectiles = true;
                onCompletedMotionToPoint = () => { isFiringServerProjectiles = false; onCompletedMotionToPoint = null; };
            }
        }
        else if (itemToCast.usableItemType == EnumData.UsableItemTypes.BubbleShield)
        {
            if (bubbleShieldAttackReady)
            {
                waitingActionForBubbleShieldItemMove.ReInitialiseTimerToBegin(bubbleShieldItemMoveAttackRateTickRate);

                SpawnItemCommand spawnItemCommand = new SpawnItemCommand(GetLocalSequenceNo(), (int)Facing, (int)itemToCast.usableItemType, GridManager.instance.grid.WorldToCell(actorTransform.position));
                ClientSend.SpawnItemCommand(spawnItemCommand);

                isFiringServerProjectiles = true;
                onCompletedMotionToPoint = () => { isFiringServerProjectiles = false; onCompletedMotionToPoint = null; };
            }
        }
        else if (itemToCast.usableItemType == EnumData.UsableItemTypes.MightyWind)
        {
            if (IsHeroAbleToFireProjectiles())
            {
                SpawnItemCommand spawnItemCommand = new SpawnItemCommand(GetLocalSequenceNo(), (int)Facing, (int)itemToCast.usableItemType, GridManager.instance.grid.WorldToCell(actorTransform.position));
                ClientSend.SpawnItemCommand(spawnItemCommand);

                isFiringServerProjectiles = true;
                onCompletedMotionToPoint = () => { isFiringServerProjectiles = false; onCompletedMotionToPoint = null; };
            }
        }
        else if (itemToCast.usableItemType == EnumData.UsableItemTypes.FlamePillar)
        {
            if (IsHeroAbleToFireProjectiles())
            {
                SpawnItemCommand spawnItemCommand = new SpawnItemCommand(GetLocalSequenceNo(), (int)Facing, (int)itemToCast.usableItemType, GridManager.instance.grid.WorldToCell(actorTransform.position));
                ClientSend.SpawnItemCommand(spawnItemCommand);
                isFiringServerProjectiles = true;
                onCompletedMotionToPoint = () => { isFiringServerProjectiles = false; onCompletedMotionToPoint = null; };
            }
        }
    }

    public void SpawnClientProjectiles()
    {
        if (itemToCast.usableItemType == EnumData.UsableItemTypes.EyeLaser)
        {
            if (IsHeroAbleToFireProjectiles())
            {
                if (!waitingActionForItemEyeLaserMove.Perform())
                {
                    isFiringItemEyeLaser = true;
                    waitingActionForItemEyeLaserMove.ReInitialiseTimerToBegin(itemEyeLaserMoveAttackRateTickRate);
                }
                else
                {
                    isFiringItemEyeLaser = false;
                }
            }
        }
        else if (itemToCast.usableItemType == EnumData.UsableItemTypes.Fireball)
        {
            if (IsHeroAbleToFireProjectiles())
            {
                if (!waitingActionForItemFireballMove.Perform())
                {
                    isFiringItemFireball = true;
                    waitingActionForItemFireballMove.ReInitialiseTimerToBegin(itemFireballMoveAttackRateTickRate);
                }
                else
                {
                    isFiringItemFireball = false;
                }
            }
        }
    }

    public void ResetClientProjectilesVars()
    {
        if (itemToCast.usableItemType == EnumData.UsableItemTypes.EyeLaser)
        {
            isFiringItemEyeLaser = false;
            waitingActionForItemEyeLaserMove.ReInitialiseTimerToEnd(itemEyeLaserMoveAttackRateTickRate);
        }
        else if (itemToCast.usableItemType == EnumData.UsableItemTypes.Fireball)
        {
            isFiringItemFireball = false;
            waitingActionForItemFireballMove.ReInitialiseTimerToEnd(itemFireballMoveAttackRateTickRate);
        }
    }

    public void SpawnItem()
    {
        if (itemToCast.usableItemType == EnumData.UsableItemTypes.Minion)
        {
            Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
            if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellToCheckFor) && !GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.GameObjectEnums.BoulderAppearing, EnumData.GameObjectEnums.BoulderDisappearing))
            {
                //send command to server of placement
                SpawnItemCommand spawnItemCommand = new SpawnItemCommand(GetLocalSequenceNo(), (int)Facing, (int)itemToCast.usableItemType, cellToCheckFor);
                ClientSend.SpawnItemCommand(spawnItemCommand);
            }
        }
        else if(itemToCast.usableItemType == EnumData.UsableItemTypes.CereberausHead)
        {
            Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
            if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellToCheckFor) && !GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.GameObjectEnums.BoulderAppearing, EnumData.GameObjectEnums.BoulderDisappearing))
            {
                //send command to server of placement
                SpawnItemCommand spawnItemCommand = new SpawnItemCommand(GetLocalSequenceNo(), (int)Facing, (int)itemToCast.usableItemType, cellToCheckFor);
                ClientSend.SpawnItemCommand(spawnItemCommand);
            }
        }
        else if(itemToCast.usableItemType == EnumData.UsableItemTypes.Boulder)
        {
            Vector3Int cellToCheckFor = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
            if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellToCheckFor) && !GridManager.instance.HasTileAtCellPoint(cellToCheckFor, EnumData.GameObjectEnums.BoulderAppearing))
            {
                //send command to server of placement
                SpawnItemCommand spawnItemCommand = new SpawnItemCommand(GetLocalSequenceNo(), (int)Facing, (int)itemToCast.usableItemType, cellToCheckFor);
                ClientSend.SpawnItemCommand(spawnItemCommand);
            }
        }
        else if (itemToCast.usableItemType == EnumData.UsableItemTypes.Pitfall)
        {
            Vector3Int cellToCheck = GridManager.instance.grid.WorldToCell(actorTransform.position + 2 * GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
            if (GridManager.instance.HasTileAtCellPoint(cellToCheck, EnumData.TileType.Normal))
            {
                SpawnItemCommand spawnItemCommand = new SpawnItemCommand(GetLocalSequenceNo(), (int)Facing, (int)itemToCast.usableItemType, Vector3Int.zero);
                ClientSend.SpawnItemCommand(spawnItemCommand);
            }
        }
        else if(itemToCast.usableItemType == EnumData.UsableItemTypes.Earthquake)
        {
            SpawnItemCommand spawnItemCommand = new SpawnItemCommand(GetLocalSequenceNo(), (int)Facing, (int)itemToCast.usableItemType, Vector3Int.zero);
            ClientSend.SpawnItemCommand(spawnItemCommand);
        }
        else if(itemToCast.usableItemType == EnumData.UsableItemTypes.Tornado)
        {
            Vector3Int cellToCheck = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
            if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellToCheck))
            {
                SpawnItemCommand spawnItemCommand = new SpawnItemCommand(GetLocalSequenceNo(), (int)Facing, (int)itemToCast.usableItemType, Vector3Int.zero);
                ClientSend.SpawnItemCommand(spawnItemCommand);
            }
        }
        else if (itemToCast.usableItemType == EnumData.UsableItemTypes.Portal)
        {
            Vector3Int cellToCheck = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
            if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellToCheck))
            {
                SpawnItemCommand spawnItemCommand = new SpawnItemCommand(GetLocalSequenceNo(), (int)Facing, (int)itemToCast.usableItemType, Vector3Int.zero);
                ClientSend.SpawnItemCommand(spawnItemCommand);
            }
        }
    }
    
    public abstract void DealInput();

    public abstract bool[] GetHeroInputs();
}
