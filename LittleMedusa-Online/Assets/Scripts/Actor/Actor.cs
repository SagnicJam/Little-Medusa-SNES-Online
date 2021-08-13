using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public abstract class Actor : TileData
{
    [Header("Scene references")]
    public Transform movePoint;
    public Transform actorTransform;
    public FrameLooper frameLooper;
    public GamePhysics gamePhysics;

    [Header("Tweak params")]
    public BoxCollider2D actorCollider2D;
    public EnumData.Projectiles projectileThrownType;
    public EnumData.Projectiles projectileThrownType_2;
    public int invincibilityTickTimer;
    public int petrificationTimeTickRate;
    public int maxHP;
    public int maxStockLives;
    public int walkSpeed;
    public int damagePerStoppedHit;
    public int primaryMoveDamage;
    public float petrificationSnapSpeed;
    public int primaryMoveAttackRateTickRate;
    public int secondaryMoveAttackRateTickRate;
    public int pushSpeed;
    public FaceDirection faceDirectionInit;

    [Header("Animation Sprites")]
    public Sprite[] leftIdleSprite;
    public Sprite[] rightIdleSprite;
    public Sprite[] upIdleSprite;
    public Sprite[] downIdleSprite;

    public Sprite[] leftMoveSprite;
    public Sprite[] rightMoveSprite;
    public Sprite[] downMoveSprite;
    public Sprite[] upMoveSprite;

    public Sprite[] leftFlySprite;
    public Sprite[] rightFlySprite;
    public Sprite[] downFlySprite;
    public Sprite[] upFlySprite;

    public Sprite[] petrificationSpriteArr;

    [Header("Primary MoveUseAnimation Sprites")]
    public MoveUseAnimationAction.MoveAnimationSprites primaryMoveAnimationSprites;
    public MoveUseAnimationAction.MoveAnimationSprites secondaryMoveAnimationSprites;

    [Header("Unit Template")]
    public Sprite crosshairSprite;

    [Header("Live Data")]
    public bool isDead;
    public bool isPushed;
    public bool isPetrified;
    public bool isInFlyingState;
    public bool isFiringPrimaryProjectile;
    public bool isInvincible;
    public bool isRespawnningPlayer;
    public bool inCharacterSelectionScreen;
    public bool inGame;
    public bool triggerFaceChangeEvent;

    [Header("Hero Specific Data")]
    public bool isWalking;
    public bool isUsingPrimaryMove;
    public bool isUsingSecondaryMove;

    public int currentHP;
    public int currentStockLives;
    public bool isHeadCollisionWithOtherActor;
    public Mapper currentMapper;
    public Vector3Int headOnCollisionCell;
    public FaceDirection headOnCollisionFaceDirection;
    public int chainIDLinkedTo = -1;

    FaceDirection facing;
    FaceDirection previousFaceDirection;

    [Header("Live Units")]
    public ClientMasterController clientMasterController;
    public ServerMasterController serverMasterController;

    [Header("Live Data")]
    public int ownerId;
    public Attack currentAttack;

    [Header("Attack")]
    public Attack rangedAttack_1;
    public Attack rangedAttack_2;

    [Header("Actor Actions")]
    public WalkAction walkAction = new WalkAction();
    public PetrificationAction petrificationAction = new PetrificationAction();
    public MoveUseAnimationAction primaryMoveUseAction = new MoveUseAnimationAction();
    public InteractWithTileAction dropAction = new InteractWithTileAction();
    public WaitingForNextAction waitingForInvinciblityToOver = new WaitingForNextAction();


    [Header("Action Primary Actions")]
    public WaitingForNextAction waitingActionForPrimaryMove = new WaitingForNextAction();
    public WaitingForNextAction waitingActionForSecondaryMove = new WaitingForNextAction();

    public virtual void Awake()
    {
        walkAction.Initialise(this);
        petrificationAction.Initialise(this);
        waitingForInvinciblityToOver.Initialise(this);
        primaryMoveUseAction.Initialise(this);
        dropAction.Initialise(this);

        waitingActionForPrimaryMove.Initialise(this);
        waitingActionForPrimaryMove.ReInitialiseTimerToEnd(primaryMoveAttackRateTickRate);

        waitingActionForSecondaryMove.Initialise(this);
        waitingActionForSecondaryMove.ReInitialiseTimerToEnd(secondaryMoveAttackRateTickRate);
    }

   

    public virtual void Start()
    {
    }


    public void InitialiseClientActor(ClientMasterController clientMasterController, int ownerId)
    {
        this.clientMasterController = clientMasterController;
        this.clientMasterController.id = ownerId;
        this.ownerId = ownerId;
    }

    public void InitialiseServerActor(ServerMasterController serverMasterController, int ownerId)
    {
        this.serverMasterController = serverMasterController;
        this.ownerId = ownerId;
    }
    public int GetNetworkSequenceNo()
    {
        if (serverMasterController != null)
        {
            return serverMasterController.playerSequenceNumberProcessed;
        }
        else
        {
            return clientMasterController.serverSequenceNumberToBeProcessed;
        }
    }

    public int GetLocalSequenceNo()
    {
        if (clientMasterController != null)
        {
            return clientMasterController.localSequenceNumber;
        }
        else
        {
            return serverMasterController.serverLocalSequenceNumber;
        }
    }

    public bool hasAuthority()
    {
        return clientMasterController != null && clientMasterController.hasAuthority;
    }

    public void InitialiseHP()
    {
        currentHP = maxHP;
    }

    public void InitialiseStockLives()
    {
        currentStockLives = maxStockLives;
    }

    void RespawnPlayer()
    {
        isRespawnningPlayer = true;
        SetRespawnState();
    }

    public void SpawnPlayer()
    {
        isRespawnningPlayer = false;
        InitialiseHP();
        SetSpawnState();
    }

    public void SetRespawnState()
    {
        actorCollider2D.enabled = false;
        if (!MultiplayerManager.instance.isServer)
        {
            if (hasAuthority())
            {
                //crosshair replaces sprite
                frameLooper.spriteRenderer.sprite = crosshairSprite;
            }
            else
            {
                //sprite enabled false
                frameLooper.spriteRenderer.enabled = false;
            }

        }
        //Debug.LogError("Collider off");
    }

    public void SetSpawnState()
    {
        actorCollider2D.enabled = true;
        if (!MultiplayerManager.instance.isServer)
        {
            if (hasAuthority())
            {
                //sprite replaces crosshair
            }
            else
            {
                //sprite enabled true
                frameLooper.spriteRenderer.enabled = true;
            }
        }
        //Debug.LogError("Collider on");
    }

    public void MakeUnInvincible()
    {
        isInvincible = false;
    }

    void MakeInvincible()
    {
        Debug.Log("MakeInvincible");
        isInvincible = true;
        waitingForInvinciblityToOver.ReInitialiseTimerToBegin(invincibilityTickTimer);
    }


    public bool completedMotionToMovePoint
    {
        get
        {
            if (Vector3.Distance(actorTransform.position, movePoint.position) <= 0.005f)
            {
                actorTransform.position = movePoint.position;
                previousMovePointCellPosition = currentMovePointCellPosition;
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    public Vector3Int previousMovePointCellPosition;

    public Vector3Int currentMovePointCellPosition
    {
        get
        {
            return GridManager.instance.grid.WorldToCell(movePoint.position);
        }
        set
        {
            movePoint.position = GridManager.instance.cellToworld(value);

        }
    }

    public FaceDirection Facing
    {
        set
        {
            facing = value;

            if (previousFaceDirection != facing)
            {
                //Debug.Log("Updating here with face: "+facing);
                triggerFaceChangeEvent = true;
                previousFaceDirection = facing;
            }
        }
        get
        {
            return facing;
        }
    }


    public FaceDirection PreviousFacingDirection
    {
        set
        {
            previousFaceDirection = value;
        }
        get
        {
            return previousFaceDirection;
        }
    }

    public void Die()
    {
        isDead = true;
        Debug.Log("Die here");
    }



    public void UpdateFrameSprites()
    {
        if (frameLooper == null)
            return;
        if (isUsingPrimaryMove)
        {
            switch (facing)
            {
                case FaceDirection.Left:
                    frameLooper.UpdateSpriteArr(primaryMoveAnimationSprites.leftMove);
                    break;
                case FaceDirection.Right:
                    frameLooper.UpdateSpriteArr(primaryMoveAnimationSprites.rightMove);
                    break;
                case FaceDirection.Down:
                    frameLooper.UpdateSpriteArr(primaryMoveAnimationSprites.downMove);
                    break;
                case FaceDirection.Up:
                    frameLooper.UpdateSpriteArr(primaryMoveAnimationSprites.upMove);
                    break;
            }
        }
        else if (isUsingSecondaryMove)
        {
            switch (facing)
            {
                case FaceDirection.Left:
                    frameLooper.UpdateSpriteArr(secondaryMoveAnimationSprites.leftMove);
                    break;
                case FaceDirection.Right:
                    frameLooper.UpdateSpriteArr(secondaryMoveAnimationSprites.rightMove);
                    break;
                case FaceDirection.Down:
                    frameLooper.UpdateSpriteArr(secondaryMoveAnimationSprites.downMove);
                    break;
                case FaceDirection.Up:
                    frameLooper.UpdateSpriteArr(secondaryMoveAnimationSprites.upMove);
                    break;
            }
        }
        else
        {
            if (isWalking)
            {
                switch (facing)
                {
                    case FaceDirection.Left:
                        frameLooper.UpdateSpriteArr(leftMoveSprite);
                        break;
                    case FaceDirection.Right:
                        frameLooper.UpdateSpriteArr(rightMoveSprite);
                        break;
                    case FaceDirection.Down:
                        frameLooper.UpdateSpriteArr(downMoveSprite);
                        break;
                    case FaceDirection.Up:
                        frameLooper.UpdateSpriteArr(upMoveSprite);
                        break;
                }
            }
            else
            {
                switch (facing)
                {
                    case FaceDirection.Left:
                        frameLooper.UpdateSpriteArr(leftIdleSprite);
                        break;
                    case FaceDirection.Right:
                        frameLooper.UpdateSpriteArr(rightIdleSprite);
                        break;
                    case FaceDirection.Down:
                        frameLooper.UpdateSpriteArr(downIdleSprite);
                        break;
                    case FaceDirection.Up:
                        frameLooper.UpdateSpriteArr(upIdleSprite);
                        break;
                }
            }
        }
    }

    public void UpdateBasicWalkingSprite()
    {
        if (frameLooper == null)
            return;
        if (primaryMoveUseAction.canPerformMoveUseAnimations)
        {
            switch (facing)
            {
                case FaceDirection.Left:
                    frameLooper.UpdateSpriteArr(primaryMoveAnimationSprites.leftMove);
                    break;
                case FaceDirection.Right:
                    frameLooper.UpdateSpriteArr(primaryMoveAnimationSprites.rightMove);
                    break;
                case FaceDirection.Down:
                    frameLooper.UpdateSpriteArr(primaryMoveAnimationSprites.downMove);
                    break;
                case FaceDirection.Up:
                    frameLooper.UpdateSpriteArr(primaryMoveAnimationSprites.upMove);
                    break;
            }
        }
        else
        {
            if (isInFlyingState)
            {
                switch (facing)
                {
                    case FaceDirection.Left:
                        frameLooper.UpdateSpriteArr(leftFlySprite);
                        break;
                    case FaceDirection.Right:
                        frameLooper.UpdateSpriteArr(rightFlySprite);
                        break;
                    case FaceDirection.Down:
                        frameLooper.UpdateSpriteArr(downFlySprite);
                        break;
                    case FaceDirection.Up:
                        frameLooper.UpdateSpriteArr(upFlySprite);
                        break;
                }
            }
            else
            {
                switch (facing)
                {
                    case FaceDirection.Left:
                        frameLooper.UpdateSpriteArr(leftMoveSprite);
                        break;
                    case FaceDirection.Right:
                        frameLooper.UpdateSpriteArr(rightMoveSprite);
                        break;
                    case FaceDirection.Down:
                        frameLooper.UpdateSpriteArr(downMoveSprite);
                        break;
                    case FaceDirection.Up:
                        frameLooper.UpdateSpriteArr(upMoveSprite);
                        break;
                }
            }
        }


    }

    public void Fire()
    {
        Debug.Log("Firing here!!!!");
        FireProjectile();
    }

    public IEnumerator forceTravelCorCache;

    public void StopForceTravelCor()
    {
        if (forceTravelCorCache != null)
        {
            StopCoroutine(forceTravelCorCache);
        }
    }

    public void StartForceTravelCor()
    {
        if (forceTravelCorCache != null)
        {
            StopCoroutine(forceTravelCorCache);
            StartCoroutine(forceTravelCorCache);
        }
    }

    void FireProjectile()
    {
        rangedAttack_1.SetAttackingActorId(ownerId);
        DynamicItem dynamicItem = new DynamicItem
        {
            ranged = rangedAttack_1,
            activate = new TileBasedProjectileUse()
        };
        currentAttack = rangedAttack_1;
        dynamicItem.activate.BeginToUse(this, null, dynamicItem.ranged.OnHit);
    }

    public void FireProjectile(Attack rangedAttack)
    {
        rangedAttack.SetAttackingActorId(ownerId);
        DynamicItem dynamicItem = new DynamicItem
        {
            ranged = rangedAttack,
            activate = new TileBasedProjectileUse()
        };
        currentAttack = rangedAttack;
        dynamicItem.activate.BeginToUse(this, null, dynamicItem.ranged.OnHit);
    }

    //Will run on  server when received from client
    public virtual void Petrify()
    {
        if (isPhysicsControlled||isPushed)
        {
            return;
        }
        petrificationAction.ReInitialise();

        float fractionX = Mathf.Abs(actorTransform.position.x - movePoint.position.x) / GridManager.instance.grid.cellSize.x;
        float fractionY = Mathf.Abs(actorTransform.position.y - movePoint.position.y) / GridManager.instance.grid.cellSize.y;

        if (fractionX >= (GridManager.instance.grid.cellSize.x / 2f))
        {
            currentMovePointCellPosition = previousMovePointCellPosition;
        }

        if (fractionY >= (GridManager.instance.grid.cellSize.y / 2f))
        {
            currentMovePointCellPosition = previousMovePointCellPosition;
        }
        isPetrified = true;
        OnPetrified();
    }

    public bool IsInSpawnJarTerritory
    {
        get
        {
            return !isInFlyingState && GridManager.instance.IsCellBlockedBySpawnJar(GridManager.instance.grid.WorldToCell(actorTransform.position));
        }
    }
    public virtual void UnPetrify()
    {
        isPetrified = false;
    }

    //Is called from client
    public void PetrificationCommandRegister(int petrifiedByActorId)
    {
        if (IsInSpawnJarTerritory)
        {
            currentHP = maxHP;
            return;
        }
        if (!isPushed)
        {
            if(this is Hero)
            {
                PetrificationCommand petrificationCommand = new PetrificationCommand(ClientSideGameManager.players[petrifiedByActorId].masterController.localPlayer.GetLocalSequenceNo(), ownerId);
                ClientSend.PetrifyPlayer(petrificationCommand);
            }
            else
            {
                Petrify();
            }
        }
    }

    void OnPetrified()
    {
        if (primaryMoveUseAction.canPerformMoveUseAnimations)
        {
            if (primaryMoveUseAction != null)
            {
                primaryMoveUseAction.CancelMoveUsage();
            }
        }
    }

    public Actor GetNextChainElement()
    {
        Vector3Int cellPos = GridManager.instance.grid.WorldToCell(actorTransform.position+GridManager.instance.GetFacingDirectionOffsetVector3(Facing));
        Vector3 objectPosition = GridManager.instance.cellToworld(cellPos);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, GridManager.instance.grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            Actor actor = hit2DArr[i].collider.GetComponent<Actor>();
            if (actor != null)
            {
                if (actor.isPushed)
                {
                    if(actor.chainIDLinkedTo==chainIDLinkedTo)
                    {
                        return actor;
                    }
                }
            }
        }
        return this;
    }

    public Actor GetNextPetrifiedActorInDirection(FaceDirection direction)
    {
        Vector3Int cellPos = currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(direction));
        Vector3 objectPosition = GridManager.instance.cellToworld(cellPos);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, GridManager.instance.grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            Actor actor = hit2DArr[i].collider.GetComponent<Actor>();
            if (actor != null)
            {
                if (actor.isPetrified)
                {
                    return actor;
                }
            }
        }
        return this;
    }
    public void PushActor(Actor actor,FaceDirection faceDirectionToPushActor)
    {
        if (IsActorPushableInDirection(actor, faceDirectionToPushActor))
        {
            actor.SetActorPushingMe(this);
            actor.chainIDLinkedTo = chainIDLinkedTo;
            StartPush(actor, faceDirectionToPushActor);
            SetActorMePushing(actor);
        }
        else
        {
            StopPush(this);
        }
    }

    public void StartGettingPushedByProjectile(TileBasedProjectileUse tileBasedProjectile)
    {
        if(IsActorPushableInDirection(this, tileBasedProjectile.tileMovementDirection))
        {
            tileBasedProjectile.SetActorMePushing(this);
            this.SetProjectilePushingMe(tileBasedProjectile);
            this.chainIDLinkedTo = tileBasedProjectile.liveProjectile.chainIDLinkedTo;
            StartPush(this, tileBasedProjectile.tileMovementDirection);
        }
        else
        {
            StopPush(this);
        }
    }

    public void StartPush(Actor actorToPush, FaceDirection directionOfPush)
    {
        actorToPush.OnPushStart();
        Debug.LogError("Start push "+actorToPush.actorTransform.gameObject.name);
        actorToPush.currentMapper = null;
        actorToPush.currentMapper = new OneDNonCheckingMapper(directionOfPush);
        actorToPush.Facing = directionOfPush;
        actorToPush.isPushed = true;
        actorToPush.isHeadCollisionWithOtherActor = false;
    }
    //Will called on the server only
    public void InitialiseEnemyPush(Enemy enemyToPush, int pushDirection)
    {
        StartPush(enemyToPush, (FaceDirection)pushDirection);
    }

    //Will called on the server only
    public void InitialiseHeroPush(int actorToPushId, int pushDirection)
    {
        if (MultiplayerManager.instance.isServer)
        {
            Actor actorToPush = Server.clients[actorToPushId].serverMasterController.serverInstanceHero;
            if (actorToPush != null)
            {
                if (completedMotionToMovePoint)
                {
                    Vector3Int cellPos = currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3((FaceDirection)pushDirection));
                    Actor actor = GridManager.instance.GetActorOnPos(cellPos);

                    if (actor != null)
                    {
                        if(IsActorAbleToPush((FaceDirection)pushDirection))
                        {
                            if(IsActorPushableInDirection(actor, (FaceDirection)pushDirection))
                            {
                                //Perform push here
                                actorToPush.chainIDLinkedTo = ++GridManager.chainIDGlobal;
                                StartPush(actorToPush, (FaceDirection)pushDirection);
                            }
                            else
                            {
                                Debug.LogError("The actor which is to be pushed cant be pushed!");
                            }
                        }
                        else
                        {
                            Debug.LogError("Actor cant push in direction");
                        }
                    }
                    else
                    {
                        Debug.LogError("No actor to push");
                    }
                }
                else
                {
                    Debug.LogError("Not completed motion hence push failed");
                }
            }
        }
    }

    public bool IsActorPushableInDirection(Actor collidedActorWithMyHead, FaceDirection facing)
    {
        Actor lastActor = GridManager.instance.GetTheLastActorInChain(collidedActorWithMyHead, facing);
        Vector3Int cellPosToCheck = lastActor.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(facing));
        //Debug.LogError("Last actor id is : "+lastActor.gameObject.GetInstanceID());
        if (GridManager.instance.IsCellBlockedForPetrifiedUnitMotionAtPos(cellPosToCheck))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool IsClientEnemyPushable(FaceDirection pushDirection)
    {
        Vector3 objectPosition = actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(pushDirection);
        //check if enemy has completed motion
        //check if enemy is petrified and is not pushed
        if (GridManager.instance.IsClientEnemyOnPositionPushable(objectPosition))
        {
            return true;
        }
        return false;
    }

    public bool IsActorAbleToPush(FaceDirection pushDirection)
    {
        Vector3 objectPosition = actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(pushDirection);
        //check if enemy has completed motion
        //check if enemy is petrified and is not pushed
        if (GridManager.instance.IsActorOnPositionPushable(objectPosition))
        {
            return true;
        }
        return false;
    }

    public virtual void CheckSwitchCellIndex()
    {
        if(currentMapper==null)
        {
            Debug.LogError("Mapper not set");
            return;
        }
        currentMovePointCellPosition = currentMapper.GetNewPathPoint(this);
        Facing = GridManager.instance.GetFaceDirectionFromCurrentPrevPoint(currentMovePointCellPosition, previousMovePointCellPosition, this);
    }

    public Actor actorPushingMe;
    public Actor actorMePushing;

    public TileBasedProjectileUse tilePushingMe;

    public void SetProjectilePushingMe(TileBasedProjectileUse tilePushingMe)
    {
        this.tilePushingMe = tilePushingMe;
        actorTransform.position = this.tilePushingMe.liveProjectile.transform.position + GridManager.instance.GetFacingDirectionOffsetVector3(tilePushingMe.tileMovementDirection);

        currentMovePointCellPosition = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(tilePushingMe.tileMovementDirection));
        previousMovePointCellPosition = GridManager.instance.grid.WorldToCell(actorTransform.position);
    }

    public void SetActorPushingMe(Actor actorPushingMe)
    {
        this.actorPushingMe = actorPushingMe;
        actorTransform.position = this.actorPushingMe.actorTransform.transform.position + GridManager.instance.GetFacingDirectionOffsetVector3(this.actorPushingMe.Facing);

        currentMovePointCellPosition = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(this.actorPushingMe.Facing));
        previousMovePointCellPosition = GridManager.instance.grid.WorldToCell(actorTransform.position);

        actorPushingMe.isHeadCollisionWithOtherActor = false;
    }

    public void SetActorMePushing(Actor actorMePushing)
    {
        this.actorMePushing = actorMePushing;
    }


    public abstract bool CanOccupy(Vector3Int cellPos);

    public abstract void OnCantOccupySpace();

    public void StopPushWithoutDamage(Actor actorToStop)
    {
        actorToStop.currentMapper = null;
        OnPushStop();
        actorToStop.OnCantOccupySpace();
        actorToStop.isPushed = false;
        if (actorMePushing != null)
        {
            if (actorMePushing.isPushed)
            {
                if (actorMePushing == this)
                {
                    Debug.LogError("Founds!!!");
                }
                else
                {
                    //Debug.Log("Stopping for : " + actorMePushing.gameObject.GetInstanceID());
                    actorMePushing.StopPushWithoutDamage(actorMePushing);
                }

            }
        }
    }

    public void StopPush(Actor actorToStop)
    {
        actorToStop.currentMapper = null;
        actorToStop.OnPushStop();
        actorToStop.OnCantOccupySpace();
        actorToStop.TakeDamage(damagePerStoppedHit);
        actorToStop.isPushed = false;
        if (actorPushingMe != null)
        {
            if (actorPushingMe.isPushed)
            {
                if (actorPushingMe == this)
                {
                    Debug.LogError("Founds!!!");
                }
                else
                {
                    //Debug.Log("Stopping for : " + actorMePushing.gameObject.GetInstanceID());
                    actorPushingMe.StopPush(actorPushingMe);
                }

            }
        }
        if(tilePushingMe!=null)
        {
            tilePushingMe.EndOfUse();
        }
    }

    public void StopPushMeOnly(Actor actorToStop)
    {
        actorToStop.currentMapper = null;
        actorToStop.OnPushStop();
        actorToStop.TakeDamage(maxHP);
        actorToStop.OnCantOccupySpace();
        actorToStop.isPushed = false;
    }


    public void TakeDamage(int damageReceived)
    {
        if(isInvincible)
        {
            Debug.Log("Damage not taken actor is invincible");
            return;
        }
        Debug.Log("Taking damage "+damageReceived);
        currentHP -= damageReceived;
        if (currentHP <= 0)
        {
            //Death occurs
            if (currentStockLives > 0)
            {
                currentStockLives--;
                Debug.LogError("Respawn Player");
                UnPetrify();
                RespawnPlayer();
            }
            else
            {
                if(this is Enemy enemy)
                {
                    if (isPetrified&&GridManager.instance.HasTileAtCellPoint(GridManager.instance.grid.WorldToCell(currentMovePointCellPosition), EnumData.TileType.Empty))
                    {
                        Debug.Log("chala");
                        GridManager.instance.SetTile(GridManager.instance.grid.WorldToCell(currentMovePointCellPosition), EnumData.TileType.Empty, false, false);
                        GridManager.instance.SetTile(GridManager.instance.grid.WorldToCell(currentMovePointCellPosition), EnumData.TileType.Normal, true, false);
                    }
                    enemy.KillMe();
                }
                Debug.LogError("Game Over");
            }
        }
        else
        {
            if(this is Hero)
            {
                UnPetrify();
                MakeInvincible();
            }
            
        }
    }

    

    private void OnTriggerEnter2D(Collider2D collider)
    {
        //Debug.LogError("chal raha hai na "+gamePhysics.gameCollider2D.enabled);
        if (!MultiplayerManager.instance.isServer)
        {
            return;
        }

        if (isDead)
        {
            return;
        }
        if (isPetrified && !isPushed)
        return;

        

        TileData collidedTile = collider.GetComponent<TileData>();

        if(collidedTile!=null)
        {
            if (collidedTile.killUnitsInstantlyIfInTheirRegion&& !IsInSpawnJarTerritory && !isInFlyingState)
            {
                OnBodyCollidingWithKillingTiles(collidedTile);
            }
        }

        if (isPhysicsControlled)
        {
            return;
        }
        ProjectileUtil projectileUtilCollidedWithMyHead = collider.GetComponent<ProjectileUtil>();
        if (projectileUtilCollidedWithMyHead != null)
        {
            if(projectileUtilCollidedWithMyHead.pU.gameObjectInstanceId!=this.gameObject.GetInstanceID())
            {
                if(GridManager.instance.IsHeadCollision(projectileUtilCollidedWithMyHead.transform.position, actorTransform.position, Facing))
                {
                    if(isPushed)
                    {
                        StopPush(this);
                        return;
                    }
                }
            }
        }

        if (collider.GetComponent<Actor>() == null)
        {
            return;
        }

        Actor collidedActorWithMyHead = collider.GetComponent<Actor>();

        //if (isPushed)
        //{
        //    if (collidedActorWithMyHead != null && collidedActorWithMyHead.gameObject.GetInstanceID() != actorTransform.gameObject.GetInstanceID() && IsCollidingWithChainElement(collidedActorWithMyHead))
        //    {
        //        return;
        //    }
        //}

        if (collidedActorWithMyHead != null &&!collidedActorWithMyHead.isInvincible&& collidedActorWithMyHead.gameObject.GetInstanceID() != actorTransform.gameObject.GetInstanceID() && GridManager.instance.IsHeadCollision(collidedActorWithMyHead.actorTransform.position, actorTransform.position, Facing))
        {
            if (collidedActorWithMyHead.isDead)
            {
                return;
            }
            isHeadCollisionWithOtherActor = true;
            OnHeadCollision(collidedActorWithMyHead);
        }
    }
    public virtual void OnHeadCollision(Actor collidedActorWithMyHead)
    {

        if (collidedActorWithMyHead.isPushed)
        {
            if (collidedActorWithMyHead.isPetrified)
            {
                if (isPushed)
                {
                    if (isPetrified)
                    {
                        OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmPushedAndAmPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmPushedAndAmPetrified " + transform.parent.name + " ID : " + transform.gameObject.GetInstanceID());
                    }
                    else
                    {
                        OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmPushedAndNotPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmPushedAndNotPetrified" + transform.parent.name);
                    }
                }
                else
                {
                    if (isPetrified)
                    {
                        OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmNotPushedAndAmPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmNotPushedAndAmPetrified" + transform.parent.name);
                    }
                    else
                    {
                        OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmNotPushedAndNotPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmNotPushedAndNotPetrified" + transform.parent.name);
                    }
                }
            }
            else
            {
                if (isPushed)
                {
                    if (isPetrified)
                    {
                        OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndAmPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndAmPetrified" + transform.parent.name);
                    }
                    else
                    {
                        OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndNotPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndNotPetrified" + transform.parent.name);
                    }
                }
                else
                {
                    if (isPetrified)
                    {
                        OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmNotPushedAndAmPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmNotPushedAndAmPetrified" + transform.parent.name);
                    }
                    else
                    {
                        OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmNotPushedAndNotPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmNotPushedAndNotPetrified" + transform.parent.name);
                    }
                }

            }
        }
        else
        {
            if (collidedActorWithMyHead.isPetrified)
            {
                if (isPushed)
                {
                    if (isPetrified)
                    {
                        OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmPushedAndAmPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmPushedAndAmPetrified" + transform.parent.name);
                    }
                    else
                    {
                        OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified" + transform.parent.name);
                    }
                }
                else
                {
                    if (isPetrified)
                    {
                        OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmNotPushedAndAmPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmNotPushedAndAmPetrified" + transform.parent.name);
                    }
                    else
                    {
                        OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified" + transform.parent.name);
                    }
                }
            }
            else
            {
                if (isPushed)
                {
                    if (isPetrified)
                    {
                        OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndPetrified" + transform.parent.name);
                    }
                    else
                    {
                        OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified" + transform.parent.name);
                    }
                }
                else 
                {
                    if (isPetrified)
                    {
                        OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndAmPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndAmPetrified" + transform.parent.name);
                    }
                    else
                    {
                        OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(collidedActorWithMyHead);
                        Debug.Log("OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified" + transform.parent.name);
                    }
                }
            }
        }
    }

    public abstract void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithANonPetrifiedNonPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmPushedAndAmPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithAPetrifiedNonPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead);

    public abstract void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmPushedAndAmPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithAPetrifiedPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmNotPushedAndAmPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmNotPushedAndNotPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndAmPetrified(Actor collidedActorWithMyHead);
    public abstract void OnHeadCollidingWithANonPetrifiedPushedObjectWhereIAmPushedAndNotPetrified(Actor collidedActorWithMyHead);

    public abstract void OnBodyCollidingWithKillingTiles(TileData tileData);

    public abstract void OnPushStart();
    public abstract void OnPushStop();

    public bool IsCollidingWithChainElement(Actor collidingNeigbor)
    {
        if (collidingNeigbor.chainIDLinkedTo == chainIDLinkedTo)
        {
            Debug.Log("collided " + collidingNeigbor.gameObject.GetInstanceID() + " me " + gameObject.GetInstanceID());
            return true;
        }
        else
        {
            return false;
        }
    }

    

    public void SnapBackAfterCollision()
    {
        //Snapping back to previous position because of head on collision
        //Debug.Log("bcurrentMovePointCellPosition " + currentMovePointCellPosition + "  previousMovePointCellPosition " + previousMovePointCellPosition);
        Vector3Int prevPos = previousMovePointCellPosition;
        headOnCollisionCell = currentMovePointCellPosition;
        previousMovePointCellPosition = currentMovePointCellPosition;
        currentMovePointCellPosition = prevPos;
        headOnCollisionFaceDirection = Facing;
        switch (headOnCollisionFaceDirection)
        {
            case FaceDirection.Up:
                Facing = FaceDirection.Down;
                break;
            case FaceDirection.Down:
                Facing = FaceDirection.Up;
                break;
            case FaceDirection.Left:
                Facing = FaceDirection.Right;
                break;
            case FaceDirection.Right:
                Facing = FaceDirection.Left;
                break;
        }
        //Debug.Log("acurrentMovePointCellPosition " + currentMovePointCellPosition + "  previousMovePointCellPosition " + previousMovePointCellPosition);
    }

    public bool IsPlayerSpawnable(Vector3Int cellPos)
    {
        if (GridManager.instance.IsCellBlockedForUnitMotionAtPos(cellPos))
        {
            return false;
        }
        return true;
    }

    public bool IsActorPathBlockedForInputDrivenMovementByAnotherActor(FaceDirection direction)
    {
        if(!isRespawnningPlayer)
        {
            if (!isInFlyingState)
            {
                Vector3Int cellPos = GridManager.instance.grid.WorldToCell(actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(direction));
                Actor actor = GridManager.instance.GetActorOnPos(cellPos);
                if (actor != null)
                {
                    if (actor.ownerId != ownerId)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    #region MultiplayerAbstractFunctions
    #endregion
}
public enum FaceDirection
{
    Idle = 0,
    Down,
    Up,
    Left,
    Right
}
