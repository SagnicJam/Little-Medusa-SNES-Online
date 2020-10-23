using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : TileData
{
    [Header("Scene references")]
    public Transform movePoint;
    public Transform actorTransform;
    public FrameLooper frameLooper;

    [Header("Tweak params")]
    public BoxCollider2D actorCollider2D;
    public EnumData.Projectiles projectileThrownType;
    public int invincibilityTickTimer;
    public int petrificationTimeTickRate;
    public int maxHP;
    public int maxStockLives;
    public int walkSpeed;
    public int damagePerStoppedHit;
    public float primaryMoveAnimationSpeed;
    public float normalAnimationSpeed;
    public float primaryMoveDamage;
    public float petrificationSnapSpeed;

    [Header("Animation Sprites")]
    public Sprite[] idleSprite;
    public Sprite[] leftMoveSprite;
    public Sprite[] rightMoveSprite;
    public Sprite[] downMoveSprite;
    public Sprite[] upMoveSprite;

    [Header("Primary MoveUseAnimation Sprites")]
    public Sprite[] leftPrimaryMoveSprite;
    public Sprite[] rightPrimaryMoveSprite;
    public Sprite[] downPrimaryMoveSprite;
    public Sprite[] upPrimaryMoveSprite;

    [Header("Live Data")]
    public bool isDead;
    public bool isPushed;
    public bool isPetrified;
    public bool isInFlyingState;
    public bool isFiringPrimaryProjectile;
    public bool isInvincible;
    public bool isRespawnningPlayer;
    public bool triggerFaceChangeEvent;
    public int currentHP;
    public int currentStockLives;
    public int chainIDLinkedTo = -1;
    public bool isHeadCollisionWithOtherActor;
    public List<Mapper> mapperList = new List<Mapper>();
    public Vector3Int headOnCollisionCell;
    public FaceDirection headOnCollisionFaceDirection;

    FaceDirection facing;
    FaceDirection previousFaceDirection;

    [Header("Live Units")]
    public ClientMasterController clientMasterController;
    public ServerMasterController serverMasterController;

    [Header("Live Data")]
    public int ownerId;

    [Header("Attack")]
    public Attack rangedAttack;

    [Header("Actor Actions")]
    public WalkAction walkAction = new WalkAction();
    public PetrificationAction petrificationAction = new PetrificationAction();
    public MoveUseAnimationAction primaryMoveUseAnimationAction = new MoveUseAnimationAction();
    public InteractWithTileAction dropAction = new InteractWithTileAction();
    public WaitingForNextAction waitingForInvinciblityToOver = new WaitingForNextAction();

    public virtual void Awake()
    {
        walkAction.Initialise(this);
        petrificationAction.Initialise(this);
        waitingForInvinciblityToOver.Initialise(this);
        primaryMoveUseAnimationAction.Initialise(this);
        dropAction.Initialise(this);
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

    public virtual void Start()
    {
        
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

    public bool isClient()
    {
        return (serverMasterController == null) && (clientMasterController != null);
    }

    public bool hasAuthority()
    {
        return clientMasterController != null && clientMasterController.hasAuthority;
    }

    public bool isServer()
    {
        return (clientMasterController == null) && (serverMasterController != null);
    }

    public void InitialiseHP()
    {
        currentHP = maxHP;
    }

    public void InitialiseStockLives()
    {
        currentStockLives = maxStockLives;
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

    public void UpdateBasicWalkingSprite()
    {
        if (frameLooper == null)
            return;

        if (primaryMoveUseAnimationAction.isBeingUsed)
        {
            switch (facing)
            {
                case FaceDirection.Left:
                    frameLooper.UpdateSpriteArr(leftPrimaryMoveSprite);
                    break;
                case FaceDirection.Right:
                    frameLooper.UpdateSpriteArr(rightPrimaryMoveSprite);
                    break;
                case FaceDirection.Down:
                    frameLooper.UpdateSpriteArr(downPrimaryMoveSprite);
                    break;
                case FaceDirection.Up:
                    frameLooper.UpdateSpriteArr(upPrimaryMoveSprite);
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

    public void Fire(Actor firingActor)
    {
        Debug.Log("Firing here!!!!");
        FireProjectile(firingActor.rangedAttack);
    }

    void FireProjectile(Attack rangedAttack)
    {
        DynamicItem dynamicItem = new DynamicItem
        {
            ranged = rangedAttack,
            activate = new TileBasedProjectileUse()
        };
        dynamicItem.activate.BeginToUse(this, null, dynamicItem.ranged.OnHit);
    }

    //Will run on  server when received from client
    public void Petrify()
    {
        if (!isPushed)
        {
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
            OnPetrified();
            isPetrified = true;
        }
    }

    public void UnPetrify()
    {
        isPetrified = false;
    }

    //Is called from client
    public void PetrificationCommandRegister(int petrifiedByActorId)
    {
        //if (IsInSpawnJarTerritory)
        //{
        //    health = maxHealth;
        //    return;
        //}
        if(!isPushed)
        {
            PetrificationCommand petrificationCommand = new PetrificationCommand(ClientSideGameManager.players[petrifiedByActorId].masterController.localPlayer.GetLocalSequenceNo(), ownerId);
            ClientSend.PetrifyPlayer(petrificationCommand);
        }
        
    }

    void OnPetrified()
    {
        if (primaryMoveUseAnimationAction.isBeingUsed)
        {
            primaryMoveUseAnimationAction.CancelMoveUsage();
        }
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

    public void StartPush(Actor actorToPush, FaceDirection directionOfPush)
    {
        actorToPush.mapperList.Add(new OneDNonCheckingMapper(directionOfPush));
        actorToPush.Facing = directionOfPush;
        actorToPush.isPushed = true;
        actorToPush.isHeadCollisionWithOtherActor = false;
    }

    //Will called on the server only
    public void InitialisePush(int actorToPushId, int pushDirection)
    {
        if (isServer())
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
                        if (IsActorAbleToPush((FaceDirection)pushDirection) && IsActorPushableInDirection(actor, (FaceDirection)pushDirection))
                        {
                            //Perform push here
                            actorToPush.chainIDLinkedTo = ++GridManager.chainIDGlobal;
                            StartPush(actorToPush, (FaceDirection)pushDirection);
                        }
                    }
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

    public void CheckSwitchCellIndex()
    {
        currentMovePointCellPosition = GetMapper().GetNewPathPoint(this);
        Facing = GridManager.instance.GetFaceDirectionFromCurrentPrevPoint(currentMovePointCellPosition, previousMovePointCellPosition, this);
    }

    public Mapper GetMapper()
    {
        if (isPushed)
        {
            foreach (Mapper map in mapperList)
            {
                if (map is OneDNonCheckingMapper)
                {
                    //Debug.Log("OneDirectionalMapperBoss");
                    return map;
                }
            }
        }
        return null;
    }

    public Actor actorPushingMe;
    public Actor actorMePushing;

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

    public void StopPush(Actor actorToStop)
    {
       
        List<Mapper> mapsToDelete = new List<Mapper>();
        foreach (Mapper m in actorToStop.mapperList)
        {
            if (m is OneDNonCheckingMapper)
            {
                mapsToDelete.Add(m);
            }
        }

        for (int i = 0; i < mapsToDelete.Count; i++)
        {
            actorToStop.mapperList.Remove(mapsToDelete[i]);
        }

        OnCantOccupySpace();
        if (actorPushingMe != null)
        {
            actorPushingMe.StopPush(actorPushingMe);
        }
        actorToStop.isPushed = false;
        actorToStop.TakeDamage();
    }


    void TakeDamage()
    {
        currentHP -= damagePerStoppedHit;
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
                Debug.LogError("Game Over");
            }
        }
        else
        {
            UnPetrify();
            MakeInvincible();
        }
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
        Debug.LogError("Collider off");
    }

    public void SetSpawnState()
    {
        actorCollider2D.enabled = true;
        Debug.LogError("Collider on");
    }

    public void MakeUnInvincible()
    {
        isInvincible = false;
    }

    void MakeInvincible()
    {
        isInvincible = true;
        waitingForInvinciblityToOver.ReInitialiseTimerToBegin(invincibilityTickTimer);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!isServer())
        {
            return;
        }

        if (isDead)
        {
            return;
        }

        if (isPetrified && !isPushed)
            return;

        if (collider.GetComponent<Actor>() == null)
        {
            return;
        }

        Actor collidedActorWithMyHead = collider.GetComponent<Actor>();

        if (isPushed)
        {
            if (collidedActorWithMyHead != null && collidedActorWithMyHead.gameObject.GetInstanceID() != actorTransform.gameObject.GetInstanceID() && IsCollidingWithChainElement(collidedActorWithMyHead))
            {
                return;
            }
        }

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
        
        //Debug.Break();
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
        else
        {
            Actor actor = GridManager.instance.GetActorOnPos(cellPos);
            if (actor != null)
            {
                if (actor.ownerId != ownerId)
                {
                    return false;
                }
            }
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
}
public enum FaceDirection
{
    Idle = 0,
    Down,
    Up,
    Left,
    Right
}
