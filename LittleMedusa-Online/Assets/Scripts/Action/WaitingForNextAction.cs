using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForNextAction : Actions
{
    float NextCheckTime;

    public Actor actor;

    public override void Initialise(Actor actorActingThisAction)
    {
        actor = actorActingThisAction;
    }

    public void SetWaitingState(float waitingTimer,float waitDuration)
    {
        this.waitingTimer = waitingTimer;
        NextCheckTime = waitDuration;
    }

    public void ReInitialiseTimerToBegin(float waitDuration)
    {
        NextCheckTime =  waitDuration;
        waitingTimer = 0;
    }

    public void ReInitialiseTimerToEnd(float waitDuration)
    {
        NextCheckTime = waitDuration;
        waitingTimer = NextCheckTime;
    }

    public float waitingTimer=0;

    public override bool Perform()
    {
        if (waitingTimer >= NextCheckTime)
        {
            waitingTimer = 0f;
            return false;
        }
        else
        {
            waitingTimer ++;
            return true;
        }
    }
}
