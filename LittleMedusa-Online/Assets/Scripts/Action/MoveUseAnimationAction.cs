using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class MoveUseAnimationAction : Actions
{
    public OnUsed<Actor> onMoveUseActionOver;
    public OnUsed<Actor> onMoveUseActionBegin;
    public float animationSpeed;
    public float normalSpeed;
    Actor actorUsingMove;

    public override void Initialise(Actor actorActingThisAction)
    {
        actorUsingMove = actorActingThisAction;
    }

    public void SetAnimationSpeedAndSpritesOnUsage(float animationSpeed,float normalSpeed)
    {
        this.animationSpeed = animationSpeed;
        this.normalSpeed = normalSpeed;
    }


    public bool initialiseSprite;
    public bool isBeingUsed;

    public override bool Perform()
    {
        if(!initialiseSprite)
        {
            if(onMoveUseActionBegin!=null)
            {
                onMoveUseActionBegin.Invoke(actorUsingMove);
            }
            //Debug.Log("initialiseSprite");
            actorUsingMove.UpdateBasicWalkingSprite();
            actorUsingMove.frameLooper.timeBetweenFrames = animationSpeed;
            initialiseSprite = true;
        }
        else
        {

            if (!actorUsingMove.frameLooper.isRepeatingLoop)
            {
                actorUsingMove.frameLooper.UpdateAnimationFrame();
                return true;
            }
            else
            {
                if(!isBeingUsed)
                {
                    CancelMoveUsage();
                }
                
                if (onMoveUseActionOver != null)
                {
                    //Debug.LogError("sasa");
                    onMoveUseActionOver.Invoke(actorUsingMove);
                }
                return false;
            }
        }
        return false;
    }

    public void CancelMoveUsage()
    {
        //Debug.Log("CancelMoveUsage");
        if(initialiseSprite)
        {
            initialiseSprite = false;
            //Debug.Log("normalSpeed "+ normalSpeed);
            actorUsingMove.frameLooper.timeBetweenFrames = normalSpeed;
            actorUsingMove.UpdateBasicWalkingSprite();
        }
        
    }
}


