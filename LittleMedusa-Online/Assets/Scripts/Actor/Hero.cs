using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public abstract class Hero : Actor
{
    [Header("Tweak Params")]
    public int primaryMoveAttackRateTickRate;
    public Color petrificationColor;
    public Color invincibleColor;

    [Header("Scene References")]
    public Image healthFillImage;
    public TextMeshProUGUI currentLifeStockText;
    public SpriteRenderer statusSprite;
    public InputFrameCount inputFrameCounter;

    [Header("Hero Actions")]
    public WaitingForNextAction waitingActionForPrimaryMove = new WaitingForNextAction();

    [Header("Live Data")]
    public int hero;
    public bool isInputFreezed;
    public int frameDelayForRegisteringInput;

    public override void Awake()
    {
        base.Awake();

        waitingActionForPrimaryMove.Initialise(this);
        waitingActionForPrimaryMove.ReInitialiseTimerToEnd(primaryMoveAttackRateTickRate);

        primaryMoveUseAction.SetAnimationSpeedAndSpritesOnUsage(primaryMoveAnimationSpeed, normalAnimationSpeed);
        rangedAttack_1 = new Attack(primaryMoveDamage, EnumData.AttackTypes.ProjectileAttack, projectileThrownType);
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
            CharacterSelectionScreen.instance.gameObject.SetActive(false);
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
        primaryMoveUseAction.isBeingUsed = playerAnimationEvents.isPrimaryMoveAnimationBeingPlayed;
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
            collidedActorWithMyHead.Die();
        }
        StopPush(this);
        isHeadCollisionWithOtherActor = false;
    }

    public override void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead)
    {
        if (!(collidedActorWithMyHead is Hero))
        {
            collidedActorWithMyHead.Die();
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
