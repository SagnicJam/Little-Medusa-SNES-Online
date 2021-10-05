using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public abstract class Hero : Actor
{
    [Header("Tweak Params")]
    public int frameDelayForRegisteringInput;
    public Color petrificationColor;
    public Color invincibleColor;

    [Header("Scene References")]
    public Image healthFillImage;
    public TextMeshProUGUI currentLifeStockText;
    public SpriteRenderer statusSprite;
    public InputFrameCount inputFrameCounter;

    

    [Header("Live Data")]
    public int hero;
    public bool isInputFreezed;

    public override void Awake()
    {
        base.Awake();
        rangedAttack_1 = new Attack(primaryMoveDamage, EnumData.AttackTypes.ProjectileAttack, projectileThrownType);
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
        
        if(actorCollider2D!=null)
        {
            
            if (!isRespawnningPlayer && isRespawnningPlayer != playerAuthoratativeStates.isRespawnningPlayer)
            {
                //enable crosshair locally
                //disable collider
                if(statusSprite!=null)
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
                actorCollider2D.enabled = false;
            }
            if (isPhysicsControlled && isPhysicsControlled != playerAuthoratativeStates.isPhysicsControlled)
            {
                actorCollider2D.enabled = true;
            }
        }
        

        isRespawnningPlayer = playerAuthoratativeStates.isRespawnningPlayer;
        currentHP = playerAuthoratativeStates.currentHP;
        currentStockLives = playerAuthoratativeStates.currentStockLives;

        if(healthFillImage!=null)
        {
            healthFillImage.fillAmount =  (1f*currentHP) / maxHP;
            currentLifeStockText.text =  currentStockLives.ToString();
        }
    }

    //authoratatively is performed(but is locally is also done)-correction happens
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
    }

    //authoratatively is performed(but is locally is also done)
    public void SetActorEventActionState(PlayerEvents playerEvents)
    {
        isFiringPrimaryProjectile = playerEvents.firedPrimaryMoveProjectile;
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
    public abstract void ProcessMovementInputs(bool[] inputs, bool[] previousInputs,int movementCommandPressCount);

    public abstract void ProcessInputFrameCount(bool[]inputs,bool[]previousInputs);

    /// <summary>
    /// Called locally on client and on server
    /// </summary>
    public abstract void ProcessInputMovementsControl();
    public abstract bool IsHeroAbleToFireProjectiles();
    public abstract bool IsHeroAbleToFireProjectiles(FaceDirection facing);

    //Abilities
    public void CastFlamePillar()
    {
        rangedAttack_2.SetAttackingActorId(ownerId);
        DynamicItem dynamicItem = new DynamicItem
        {
            ranged = rangedAttack_2,
            activate = new TileBasedProjectileUse()
        };
        currentAttack = rangedAttack_2;
        dynamicItem.activate.BeginToUse(this, null, dynamicItem.ranged.OnHit);
    }


    public void PlaceTornado(Vector3Int cell)
    {
        StartCoroutine(RunTornado(5, cell));
    }

    IEnumerator RunTornado(float time,Vector3Int cell)
    {
        GridManager.instance.SetTile(cell, EnumData.TileType.Tornado, true, false);
        GridManager.instance.tornado.UpdateTornadoActivation(ownerId);
        yield return new WaitForSeconds(time);
        GridManager.instance.SetTile(cell, EnumData.TileType.Tornado, false, false);
        GridManager.instance.tornado.UpdateTornadoActivation(ownerId);
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
        GridManager.instance.RemoveTileAfter(cell, EnumData.TileType.Hole);
    }

    public void CastBubbleShield()
    {
        rangedAttack_2.SetAttackingActorId(ownerId);
        currentAttack = rangedAttack_2;
        DynamicItem dynamicItem = new DynamicItem
        {
            ranged = rangedAttack_2,
            activate = new DirectionBasedProjectileUse(0, actorTransform.right, null)
        };
        dynamicItem.activate.BeginToUse(this, null, dynamicItem.ranged.OnHit);

        DynamicItem dynamicItem2 = new DynamicItem
        {
            ranged = rangedAttack_2,
            activate = new DirectionBasedProjectileUse(90, actorTransform.right, null)
        };
        dynamicItem2.activate.BeginToUse(this, null, dynamicItem2.ranged.OnHit);

        DynamicItem dynamicItem3 = new DynamicItem
        {
            ranged = rangedAttack_2,
            activate = new DirectionBasedProjectileUse(180, actorTransform.right, null)
        };
        dynamicItem3.activate.BeginToUse(this, null, dynamicItem3.ranged.OnHit);

        DynamicItem dynamicItem4 = new DynamicItem
        {
            ranged = rangedAttack_2,
            activate = new DirectionBasedProjectileUse(270, actorTransform.right, null)
        };
        dynamicItem4.activate.BeginToUse(this, null, dynamicItem4.ranged.OnHit);

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
        //if (isInFlyingState)
        //{
        //    //if (GridManager.instance.IsCellBlockedForFlyingUnitsAtPos(pos))
        //    //{
        //    //    return false;
        //    //}
        //    //else
        //    //{
        //        return true;
        //    //}
        //}
        /*else */
        else if (GridManager.instance.IsCellBlockedForUnitMotionAtPos(pos)||GridManager.instance.IsCellContainingMonster(pos,this))
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
    }

    public override void OnPushStop()
    {
        Debug.Log("OnPushStop ");
    }

    public override void OnBodyCollidingWithKillingTiles(TileData tileData)
    {
        TakeDamage(currentHP);
    }


    public override void OnCantOccupySpace()
    {
        if(isPetrified||isPushed)
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

    
    public abstract void DealInput();

    public abstract bool[] GetHeroInputs();
}
