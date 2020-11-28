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
    public static int nextProjectileID=1;
    public int networkUid;


    public void Initialise(TileBasedProjectileUse pU)
    {
        this.pU = pU;
        if (MultiplayerManager.instance.isServer)
        {
            if (isServerNetworked)
            {
                networkUid = nextProjectileID;
                nextProjectileID++;
                ProjectileData projectileData = new ProjectileData(networkUid, (int)pU.projectileTypeThrown, pU.liveProjectile.transform.position);
                ServerSideGameManager.projectilesDic.Add(networkUid, projectileData);

                if(pU.projectileTypeThrown == EnumData.Projectiles.TidalWave)
                {
                    chainIDLinkedTo = ++GridManager.chainIDGlobal;
                }
            }
        }

        if (frameLooper!=null)
        {
            if(isDynamicTravellingSprites)
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
        if (MultiplayerManager.instance.isServer)
        {
            if (isServerNetworked)
            {
                ProjectileData projectileData;
                if(ServerSideGameManager.projectilesDic.TryGetValue(networkUid,out projectileData))
                {
                    projectileData.projectilePosition = pU.liveProjectile.transform.position;
                    ServerSideGameManager.projectilesDic.Remove(networkUid);
                    ServerSideGameManager.projectilesDic.Add(networkUid, projectileData);
                }
                else
                {
                    Debug.LogError("Doesnot contain the key to set projectile position for");
                }
            }
        }
        pU.PerformUsage();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Actor collidedActorWithMyHead = collision.GetComponent<Actor>();
        if (collidedActorWithMyHead != null && pU.onUseOver != null)
        {
            if (pU.gameObjectInstanceId != collidedActorWithMyHead.gameObject.GetInstanceID() && pU.ownerId!= collidedActorWithMyHead.ownerId)
            {
                if (pU.projectileTypeThrown == EnumData.Projectiles.EyeLaser)
                {
                    if (!MultiplayerManager.instance.isServer && pU.actorHadAuthority)
                    {
                        //Send by client to server
                        pU.onUseOver.Invoke(collidedActorWithMyHead);
                    }
                }
                else if (pU.projectileTypeThrown == EnumData.Projectiles.TidalWave)
                {
                    if (MultiplayerManager.instance.isServer)
                    {
                        if (GridManager.instance.IsHeadCollision(collidedActorWithMyHead.actorTransform.position, transform.position, pU.tileMovementDirection))
                        {
                            collidedActorWithMyHead.StartGettingPushedDueToTidalWave(pU);
                        }
                    }
                }
                if (selfDestroyOnTargetTouch)
                {
                    pU.EndOfUse();
                }
            }
        }
        
    }



    public void OnProjectileDieBegin()
    {
        if(frameLooper!=null)
        {
            switch (pU.actorFacingWhenFired)
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
        if (MultiplayerManager.instance.isServer)
        {
            if (isServerNetworked)
            {
                ServerSideGameManager.projectilesDic.Remove(networkUid);
            }
        }
        Debug.Log("Destroy here");
        Destroy(gameObject);
    }
}
