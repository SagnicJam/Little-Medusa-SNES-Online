using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkAction : Actions
{
    Actor actorWalking;

    public override void Initialise(Actor actorActingThisAction)
    {
        actorWalking = actorActingThisAction;
    }

    public override bool Perform()
    {
        if (actorWalking == null)
        {
            Debug.Log("actor not set");
            return false;
        }
        if (actorWalking.completedMotionToMovePoint)
        {
            return false;
        }
        MoveActorToMovePointCell();
        return true;
    }

    public void MoveActorToMovePointCell()
    {
        actorWalking.actorTransform.position = Vector3.MoveTowards(actorWalking.actorTransform.position, actorWalking.movePoint.position, actorWalking.walkSpeed * Time.fixedDeltaTime);
    }
}
