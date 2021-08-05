﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForNextAction : Actions
{
    public bool isWaitingForNextActionCheck;
    float NextCheckTime;

    public Actor actor;

    public override void Initialise(Actor actorActingThisAction)
    {
        actor = actorActingThisAction;
    }

    public void ReInitialiseTimerToBegin(float waitDuration)
    {
        NextCheckTime =  waitDuration;
        waitingTimer = 0;
        isWaitingForNextActionCheck = true;
    }

    public void ReInitialiseTimerToEnd(float waitDuration)
    {
        NextCheckTime = waitDuration;
        waitingTimer = NextCheckTime;
        isWaitingForNextActionCheck = true;
    }

    public float waitingTimer=0;

    public override bool Perform()
    {
        if (waitingTimer >= NextCheckTime)
        {
            waitingTimer = 0f;
            isWaitingForNextActionCheck = false;
            return false;
        }
        else
        {
            waitingTimer ++;
            return true;
        }
    }
}
