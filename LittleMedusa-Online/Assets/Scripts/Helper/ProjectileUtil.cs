using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ProjectileUtil : MonoBehaviour
{
    [Header("Tweak params")]
    public int projectileTileTravelDistance;
    public float projectileSpeed;
    
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

    TileBasedProjectileUse pU;

    public void Initialise(TileBasedProjectileUse pU)
    {
        this.pU = pU;

        if(frameLooper!=null)
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
        pU.PerformUsage();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Actor collidedActorWithMyHead = collision.GetComponent<Actor>();

        if (collidedActorWithMyHead != null && pU.onUseOver != null)
        {
            if (pU.gameObjectInstanceId != collidedActorWithMyHead.gameObject.GetInstanceID() && pU.ownerId!= collidedActorWithMyHead.ownerId)
            {
                if (pU.actorUsing!=null)
                {
                    if(pU.actorUsing.isClient()&&pU.actorUsing.hasAuthority())
                    {
                        //Send by client to server
                        pU.onUseOver.Invoke(collidedActorWithMyHead);
                    }
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
        Destroy(gameObject);
    }
}
