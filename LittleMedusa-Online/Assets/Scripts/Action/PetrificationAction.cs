using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetrificationAction : Actions
{
    //Blink blink;
    Actor actorGettingPetrified;
    int petrificationTime;

    public override void Initialise(Actor actorActingThisAction)
    {
        actorGettingPetrified = actorActingThisAction;
        //blink = actorGettingPetrified.GetComponent<Blink>();
    }

    public void ReInitialise()
    {
        petrificationTime = actorGettingPetrified.petrificationTimeTickRate;
        //blink.StopBlink(Color.white);
    }


    public override bool Perform()
    {
        if(actorGettingPetrified == null)
        {
            Debug.LogError("monster not set");
            return false;
        }

        if(actorGettingPetrified.isServer())
        {
            if (petrificationTime > 0)
            {
                if (!actorGettingPetrified.completedMotionToMovePoint)
                {
                    actorGettingPetrified.actorTransform.position = Vector3.MoveTowards(actorGettingPetrified.actorTransform.position, actorGettingPetrified.movePoint.position, actorGettingPetrified.petrificationSnapSpeed * Time.fixedDeltaTime);
                }
                petrificationTime --;
                return true;
            }
            else
            {
                //blink.StopBlink(Color.white);
                //actorGettingPetrified.UpdateBasicWalkingSprite();
                actorGettingPetrified.UnPetrify();
                return false;
            }

        }
        else
        {
            return true;
        }
        //petrificationTime -= Time.fixedDeltaTime;
        //if(petrificationTime> GameConfig.petrificationDuration- GameConfig.showHitSpriteAtPetrificationDuration)
        //{
        //    //actorGettingPetrified.frameLooper.SetStaticFrame(actorGettingPetrified.onHitSprite);
        //}
        //else
        //{
        //    //actorGettingPetrified.frameLooper.SetStaticFrame(actorGettingPetrified.onPetrifiedSprite);
        //    //if(petrificationTime<=GameConfig.flashSpriteDuration)
        //    //{
        //    //    if(!blink.isBlinking)
        //    //    {
        //    //        blink.StartBlink(Color.white);
        //    //    }
        //    //}
        //}

        //if (petrificationTime > 0)
        //{
        //    if(!actorGettingPetrified.completedMotionToMovePoint)
        //    {
        //        actorGettingPetrified.transform.position = Vector3.MoveTowards(actorGettingPetrified.transform.position, actorGettingPetrified.movePoint.position, GameConfig.actorSnapSpeed * Time.fixedDeltaTime);
        //    }
        //    return true;
        //}
        //else
        //{
            //blink.StopBlink(Color.white);
            //actorGettingPetrified.UpdateBasicWalkingSprite();
            //actorGettingPetrified.UnPetrify();
        //    return false;
        //}
    }
}
