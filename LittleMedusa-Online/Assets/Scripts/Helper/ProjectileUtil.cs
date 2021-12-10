using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ProjectileUtil : MonoBehaviour
{
    [Header("Tweak params")]
    public bool isServerNetworked;
    public int projectileTileTravelDistance;
    public float projectileSpeed;
    public bool selfDestroyOnTargetTouch;
    public bool isDispersing;
    public float dispersionRadius;
    public float dispersionSpeed;

    [Header("UnitTemplate")]
    public GameObject dispersedGO;

    public FrameLooper frameLooper;

    public bool isDynamicTravellingSprites;

    [Header("Static ProjectileAnimationSprites")]
    public Sprite projectileTravelRightSprite;
    public Sprite projectileTravelLeftSprite;
    public Sprite projectileTravelUpSprite;
    public Sprite projectileTravelDownSprite;

    [Header("Dynamic ProjectileAnimationSprites")]
    public Sprite[] dynamicProjectileTravelRightSprite;
    public Sprite[] dynamicProjectileTravelLeftSprite;
    public Sprite[] dynamicProjectileTravelUpSprite;
    public Sprite[] dynamicProjectileTravelDownSprite;

    [Header("ProjectileDieAnimationSprites")]
    public Sprite[] projectileRightDieAnimationSprites;
    public Sprite[] projectileLeftDieAnimationSprites;
    public Sprite[] projectileUpDieAnimationSprites;
    public Sprite[] projectileDownDieAnimationSprites;

    public TileBasedProjectileUse pU;


    public int chainIDLinkedTo = -1;
    public static int nextProjectileID = 1;
    public int networkUid;

    public void Initialise(TileBasedProjectileUse pU)
    {
        this.pU = pU;
        if (MultiplayerManager.instance.isServer&&isServerNetworked)
        {
            networkUid = nextProjectileID;
            nextProjectileID++;
            ProjectileData projectileData = new ProjectileData(networkUid,pU.actorUsing.ownerId, (int)pU.projectileTypeThrown, pU.liveProjectile.transform.position, (int)pU.tileMovementDirection);
            if (pU.actorUsing != null)
            {
                if (pU.actorUsing is Hero serverHero)
                {
                    if(pU.projectileTypeThrown == EnumData.Projectiles.TidalWave)
                    {
                        serverHero.tidalWaveUsedCount--;
                    }
                    else if (pU.projectileTypeThrown == EnumData.Projectiles.FlamePillar)
                    {
                        serverHero.flamePillarUsedCount--;
                    }
                }
            }
            ServerSideGameManager.projectilesDic.Add(networkUid, projectileData);

            if (pU.projectileTypeThrown == EnumData.Projectiles.TidalWave ||
                pU.projectileTypeThrown == EnumData.Projectiles.BubbleShield ||
                pU.projectileTypeThrown == EnumData.Projectiles.MightyWind|| 
                pU.projectileTypeThrown == EnumData.Projectiles.MightyWindMirrorKnight ||
                pU.projectileTypeThrown == EnumData.Projectiles.FlamePillar)
            {
                chainIDLinkedTo = ++GridManager.chainIDGlobal;
            }
        }
        else
        {

            if (pU.projectileTypeThrown == EnumData.Projectiles.FireBall)
            {
                if (pU.actorUsing != null)
                {
                    if (pU.actorUsing is Hero serverHero)
                    {
                        serverHero.fireballUsedCount--;
                    }
                }
            }
        }
        if (frameLooper != null)
        {
            if (isDynamicTravellingSprites)
            {
                switch (pU.actorUsing.Facing)
                {
                    case FaceDirection.Up:
                        frameLooper.UpdateSpriteArr(dynamicProjectileTravelUpSprite);
                        break;
                    case FaceDirection.Down:
                        frameLooper.UpdateSpriteArr(dynamicProjectileTravelDownSprite);
                        break;
                    case FaceDirection.Right:
                        frameLooper.UpdateSpriteArr(dynamicProjectileTravelRightSprite);
                        break;
                    case FaceDirection.Left:
                        frameLooper.UpdateSpriteArr(dynamicProjectileTravelLeftSprite);
                        break;
                }
                frameLooper.PlayOneShotAnimation();
            }
            else
            {
                switch (pU.actorUsing.Facing)
                {
                    case FaceDirection.Up:
                        frameLooper.SetStaticFrame(projectileTravelUpSprite);
                        break;
                    case FaceDirection.Down:
                        frameLooper.SetStaticFrame(projectileTravelDownSprite);
                        break;
                    case FaceDirection.Right:
                        frameLooper.SetStaticFrame(projectileTravelRightSprite);
                        break;
                    case FaceDirection.Left:
                        frameLooper.SetStaticFrame(projectileTravelLeftSprite);
                        break;
                }
            }
        }

    }

    private void FixedUpdate()
    {
        if (MultiplayerManager.instance.isServer&&isServerNetworked)
        {
            ProjectileData projectileData;
            if (ServerSideGameManager.projectilesDic.TryGetValue(networkUid, out projectileData))
            {
                projectileData.projectilePosition = pU.liveProjectile.transform.position;
                projectileData.faceDirection = (int)pU.tileMovementDirection;
                ServerSideGameManager.projectilesDic.Remove(networkUid);
                ServerSideGameManager.projectilesDic.Add(networkUid, projectileData);
            }
            else
            {
                Debug.LogError("Doesnot contain the key to set projectile position for");
            }
        }
        if(pU!=null)
        {
            pU.PerformUsage();
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        TileData collidedTile = collision.GetComponent<TileData>();

        if (collidedTile != null)
        {
            if(collidedTile is Actor collidedActorWithMyHead)
            {
                if (!collidedActorWithMyHead.isPushed && !collidedActorWithMyHead.isPhysicsControlled)
                {
                    if (pU.gameObjectInstanceId != collidedActorWithMyHead.gameObject.GetInstanceID() && pU.ownerId != collidedActorWithMyHead.ownerId)
                    {
                        if (collidedActorWithMyHead is MirrorKnight)
                        {
                            DealProjectileOnMirrorKnight(collidedActorWithMyHead);
                        }
                        else
                        {
                            DealProjectileOnActor(collidedActorWithMyHead);
                        }
                    }
                }
            }
        }
        ClientEnemyManager clientEnemyManager = collision.GetComponent<ClientEnemyManager>();
        if (clientEnemyManager != null)
        {
            if (selfDestroyOnTargetTouch)
            {
                if(pU.projectileTypeThrown == EnumData.Projectiles.FireBall)
                {
                    if(clientEnemyManager.enemyType == EnumData.MonsterBreed.MirrorKnight)
                    {
                        if (GridManager.instance.IsPureHeadOn(transform.position, clientEnemyManager))
                        {
                            Destroy(gameObject);
                        }
                        else if (GridManager.instance.IsPureBackOrSideStab(transform.position, clientEnemyManager))
                        {
                            GridManager.instance.Disperse(
                             dispersedGO
                            , dispersionRadius
                            , dispersionSpeed
                            , pU.ownerId
                            , GridManager.instance.grid.WorldToCell(transform.position));
                            Destroy(gameObject);
                        }
                    }
                    else
                    {
                        GridManager.instance.Disperse(
                        dispersedGO
                       , dispersionRadius
                       , dispersionSpeed
                       , pU.ownerId
                       , GridManager.instance.grid.WorldToCell(transform.position));
                        Destroy(gameObject);
                    }
                }
                else if (pU.projectileTypeThrown == EnumData.Projectiles.EyeLaser)
                {
                    Destroy(gameObject);
                }
                else if (pU.projectileTypeThrown == EnumData.Projectiles.StarShower)
                {
                    Destroy(gameObject);
                }
                else if (pU.projectileTypeThrown == EnumData.Projectiles.CentaurBow)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    void DealProjectileOnMirrorKnight(Actor collidedActorWithMyHead)
    {
        if (pU.projectileTypeThrown == EnumData.Projectiles.EyeLaser || pU.projectileTypeThrown == EnumData.Projectiles.EyeLaserMirrorKnight)
        {
            if (MultiplayerManager.instance.isServer && GridManager.instance.IsPureHeadOn(transform.position, collidedActorWithMyHead))
            {
                collidedActorWithMyHead.FireProjectile(new Attack(pU.attack.damage, pU.attack.attackType, EnumData.Projectiles.EyeLaserMirrorKnight),
                    GridManager.instance.grid.WorldToCell(collidedActorWithMyHead.actorTransform.position));
                pU.EndOfUse();
            }
            else if (GridManager.instance.IsPureBackOrSideStab(transform.position, collidedActorWithMyHead))
            {
                pU.onUseOver.Invoke(collidedActorWithMyHead);
            }
            if (selfDestroyOnTargetTouch)
            {
                pU.EndOfUse();
            }
        }
        else if(pU.projectileTypeThrown == EnumData.Projectiles.FireBall|| pU.projectileTypeThrown == EnumData.Projectiles.FireBallMirrorKnight)
        {
            if (MultiplayerManager.instance.isServer && GridManager.instance.IsPureHeadOn(transform.position, collidedActorWithMyHead))
            {
                collidedActorWithMyHead.FireProjectile(new Attack(pU.attack.damage, pU.attack.attackType, EnumData.Projectiles.FireBallMirrorKnight),
                    GridManager.instance.grid.WorldToCell(collidedActorWithMyHead.actorTransform.position));
                Destroy(gameObject);
            }
            else if (GridManager.instance.IsPureBackOrSideStab(transform.position, collidedActorWithMyHead))
            {
                GridManager.instance.Disperse(
                    dispersedGO
                , dispersionRadius
                , dispersionSpeed
                , pU.ownerId
                , GridManager.instance.grid.WorldToCell(transform.position));
                Destroy(gameObject);
            }
        }
        else if (pU.projectileTypeThrown == EnumData.Projectiles.MightyWind||pU.projectileTypeThrown == EnumData.Projectiles.MightyWindMirrorKnight)
        {
            if (MultiplayerManager.instance.isServer && GridManager.instance.IsPureHeadOn(transform.position, collidedActorWithMyHead))
            {
                pU.EndOfUse();
                collidedActorWithMyHead.FireProjectile(new Attack(pU.attack.damage, pU.attack.attackType, EnumData.Projectiles.MightyWindMirrorKnight),
                    GridManager.instance.grid.WorldToCell(collidedActorWithMyHead.actorTransform.position));
            }
            else if (GridManager.instance.IsPureBackOrSideStab(transform.position, collidedActorWithMyHead))
            {
                if (MultiplayerManager.instance.isServer)
                {
                    if (GridManager.instance.IsHeadCollision(collidedActorWithMyHead.actorTransform.position, transform.position, pU.tileMovementDirection))
                    {
                        if (collidedActorWithMyHead.chainIDLinkedTo != chainIDLinkedTo)
                        {
                            if (!collidedActorWithMyHead.isRespawnningPlayer && !collidedActorWithMyHead.isInvincible && !collidedActorWithMyHead.isDead)
                            {
                                pU.onUseOver.Invoke(collidedActorWithMyHead);

                                if (collidedActorWithMyHead.IsActorPushableInDirection(collidedActorWithMyHead, pU.tileMovementDirection))
                                {
                                    collidedActorWithMyHead.StartGettingPushedByProjectile(pU);
                                }
                                else
                                {
                                    pU.EndOfUse();
                                }
                            }
                        }

                    }
                }
            }
        }
        else if (pU.projectileTypeThrown == EnumData.Projectiles.TidalWave
            || pU.projectileTypeThrown == EnumData.Projectiles.BubbleShield
            || pU.projectileTypeThrown == EnumData.Projectiles.FlamePillar)
        {
            if (MultiplayerManager.instance.isServer)
            {
                if (GridManager.instance.IsHeadCollision(collidedActorWithMyHead.actorTransform.position, transform.position, pU.tileMovementDirection))
                {
                    if (!collidedActorWithMyHead.isRespawnningPlayer && !collidedActorWithMyHead.isInvincible && !collidedActorWithMyHead.isDead)
                    {
                        if (collidedActorWithMyHead.IsActorPushableInDirection(collidedActorWithMyHead, pU.tileMovementDirection))
                        {
                            collidedActorWithMyHead.StartGettingPushedByProjectile(pU);
                        }
                        else
                        {
                            pU.EndOfUse();
                        }
                    }

                }
            }
        }
        else if (pU.projectileTypeThrown == EnumData.Projectiles.StarShower)
        {
            pU.onUseOver.Invoke(collidedActorWithMyHead);
        }
        else if (pU.projectileTypeThrown == EnumData.Projectiles.CentaurBow)
        {
            pU.onUseOver.Invoke(collidedActorWithMyHead);
        }
        if (selfDestroyOnTargetTouch)
        {
            pU.EndOfUse();
        }
    }

    void DealProjectileOnActor(Actor collidedActorWithMyHead)
    {
        if (pU.projectileTypeThrown == EnumData.Projectiles.EyeLaser)
        {
            if (collidedActorWithMyHead is Enemy enemy)
            {
                if (!(enemy is Minnataur))
                {
                    pU.onUseOver.Invoke(collidedActorWithMyHead);
                }
            }
            else if ((!MultiplayerManager.instance.isServer && pU.actorHadAuthority))
            {
                //Send by client to server
                pU.onUseOver.Invoke(collidedActorWithMyHead);
            }
        }
        else if(pU.projectileTypeThrown == EnumData.Projectiles.Arrow)
        {
            if(MultiplayerManager.instance.isServer)
            {
                if (collidedActorWithMyHead is Hero)
                {
                    pU.onUseOver.Invoke(collidedActorWithMyHead);
                }
            }
        }
        else if (pU.projectileTypeThrown == EnumData.Projectiles.EyeLaserMirrorKnight)
        {
            if (MultiplayerManager.instance.isServer)
            {
                if (collidedActorWithMyHead is Enemy enemy)
                {
                    if (!(enemy is Minnataur))
                    {
                        pU.onUseOver.Invoke(collidedActorWithMyHead);
                    }
                }
                else
                {
                    pU.onUseOver.Invoke(collidedActorWithMyHead);
                }
            }

        }
        else if (pU.projectileTypeThrown == EnumData.Projectiles.StarShower)
        {
            pU.onUseOver.Invoke(collidedActorWithMyHead);
        }
        else if (pU.projectileTypeThrown == EnumData.Projectiles.CentaurBow)
        {
            pU.onUseOver.Invoke(collidedActorWithMyHead);
        }
        else if(pU.projectileTypeThrown == EnumData.Projectiles.FireBall)
        {
            GridManager.instance.Disperse( dispersedGO
                , dispersionRadius
                , dispersionSpeed
                , pU.ownerId
                , GridManager.instance.grid.WorldToCell(transform.position));
            Destroy(gameObject);
        }
        else if (pU.projectileTypeThrown == EnumData.Projectiles.FireBallMirrorKnight)
        {
            GridManager.instance.Disperse( dispersedGO
                , dispersionRadius
                , dispersionSpeed
                , pU.ownerId
                , GridManager.instance.grid.WorldToCell(transform.position));
            Destroy(gameObject);
        }
        else if (pU.projectileTypeThrown == EnumData.Projectiles.MightyWindMirrorKnight)
        {
            if (MultiplayerManager.instance.isServer)
            {
                if (GridManager.instance.IsHeadCollision(collidedActorWithMyHead.actorTransform.position, transform.position, pU.tileMovementDirection))
                {
                    if (!collidedActorWithMyHead.isRespawnningPlayer && !collidedActorWithMyHead.isInvincible && !collidedActorWithMyHead.isDead)
                    {
                        pU.onUseOver.Invoke(collidedActorWithMyHead);
                        if (collidedActorWithMyHead.IsActorPushableInDirection(collidedActorWithMyHead, pU.tileMovementDirection))
                        {
                            collidedActorWithMyHead.StartGettingPushedByProjectile(pU);
                        }
                        else
                        {
                            pU.EndOfUse();
                        }
                    }
                }
            }
        }
        else if(pU.projectileTypeThrown == EnumData.Projectiles.MightyWind)
        {
            pU.onUseOver.Invoke(collidedActorWithMyHead);
            if (MultiplayerManager.instance.isServer)
            {
                if (GridManager.instance.IsHeadCollision(collidedActorWithMyHead.actorTransform.position, transform.position, pU.tileMovementDirection))
                {
                    if (collidedActorWithMyHead.chainIDLinkedTo != chainIDLinkedTo)
                    {
                        if (collidedActorWithMyHead.IsActorPushableInDirection(collidedActorWithMyHead, pU.tileMovementDirection))
                        {
                            collidedActorWithMyHead.StartGettingPushedByProjectile(pU);
                        }
                        else
                        {
                            pU.EndOfUse();
                        }
                    }
                }
            }
        }
        else if (pU.projectileTypeThrown == EnumData.Projectiles.TidalWave
            || pU.projectileTypeThrown == EnumData.Projectiles.BubbleShield ||
            pU.projectileTypeThrown == EnumData.Projectiles.FlamePillar)
        {
            if (MultiplayerManager.instance.isServer)
            {
                if (GridManager.instance.IsHeadCollision(collidedActorWithMyHead.actorTransform.position, transform.position, pU.tileMovementDirection))
                {
                    if (collidedActorWithMyHead.chainIDLinkedTo != chainIDLinkedTo)
                    {
                        if (collidedActorWithMyHead.IsActorPushableInDirection(collidedActorWithMyHead, pU.tileMovementDirection))
                        {
                            collidedActorWithMyHead.StartGettingPushedByProjectile(pU);
                        }
                        else
                        {
                            pU.EndOfUse();
                        }
                    }
                }
            }
        }
        if (selfDestroyOnTargetTouch)
        {
            pU.EndOfUse();
        }
    }



    public void OnProjectileDieBegin()
    {
        if (frameLooper != null)
        {
            switch (pU.tileMovementDirection)
            {
                case FaceDirection.Up:
                    frameLooper.UpdateSpriteArr(projectileUpDieAnimationSprites);
                    break;
                case FaceDirection.Down:
                    frameLooper.UpdateSpriteArr(projectileDownDieAnimationSprites);
                    break;
                case FaceDirection.Right:
                    frameLooper.UpdateSpriteArr(projectileRightDieAnimationSprites);
                    break;
                case FaceDirection.Left:
                    frameLooper.UpdateSpriteArr(projectileLeftDieAnimationSprites);
                    break;
            }
            frameLooper.PlayOneShotAnimation();
        }
    }

    public void DestroyProjectile()
    {
        if (MultiplayerManager.instance.isServer&&isServerNetworked)
        {
            if (pU.projectileTypeThrown == EnumData.Projectiles.TidalWave ||
                pU.projectileTypeThrown == EnumData.Projectiles.BubbleShield ||
                pU.projectileTypeThrown == EnumData.Projectiles.MightyWind||
                pU.projectileTypeThrown == EnumData.Projectiles.FlamePillar||
                pU.projectileTypeThrown == EnumData.Projectiles.MightyWindMirrorKnight)
            {
                if (pU.actorMePushing != null)
                {
                    if(!pU.actorMePushing.IsActorOnArrows()&&!pU.actorMePushing.IsActorOnMirror())
                    {
                        pU.actorMePushing.StopPushWithoutDamage(pU.actorMePushing);
                    }
                }
            }
            if (pU.actorUsing != null)
            {
                if (pU.actorUsing is Hero serverHero)
                {
                    if (pU.projectileTypeThrown == EnumData.Projectiles.TidalWave)
                    {
                        serverHero.tidalWaveUsedCount++;
                    }
                    else if (pU.projectileTypeThrown == EnumData.Projectiles.FlamePillar)
                    {
                        serverHero.flamePillarUsedCount++;
                    }
                }
            }
            
            ServerSideGameManager.projectilesDic.Remove(networkUid);
        }
        if (pU.projectileTypeThrown == EnumData.Projectiles.FireBall|| pU.projectileTypeThrown == EnumData.Projectiles.FireBallMirrorKnight)
        {
            //Debug.LogError("DestroyProjectile");
            if (pU.projectileTypeThrown == EnumData.Projectiles.FireBall)
            {
                if (pU.actorUsing != null)
                {
                    if (pU.actorUsing is Hero serverHero)
                    {   
                        serverHero.fireballUsedCount++;
                    }
                }
            }
            GridManager.instance.Disperse( dispersedGO
                , dispersionRadius
                , dispersionSpeed
                , pU.ownerId
                , GridManager.instance.grid.WorldToCell(transform.position));
        }
        Destroy(gameObject);
    }


}
