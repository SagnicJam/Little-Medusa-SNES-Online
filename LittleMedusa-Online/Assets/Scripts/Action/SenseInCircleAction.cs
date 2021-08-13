using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SenseInCircleAction : Actions
{
    Enemy monsterSensing;
    float circleRange;

    public void InitialiseCircleRange(float circleRange)
    {
        this.circleRange = circleRange;
    }

    public override void Initialise(Actor actorActingThisAction)
    {
        monsterSensing = (Enemy)actorActingThisAction;
    }

    public override bool Perform()
    {
        if (monsterSensing == null)
        {
            Debug.LogError("monster sensing is not set");
            return false;
        }
        Collider2D[] hitCols = Physics2D.OverlapCircleAll(monsterSensing.transform.position, circleRange);
        for (int i = 0; i < hitCols.Length; i++)
        {
            if (hitCols[i].gameObject != null && hitCols[i].gameObject.GetComponent<Hero>()!=null && hitCols[i].gameObject.GetComponent<Hero>().GetInstanceID()== monsterSensing.heroToChase.GetInstanceID())
            {
                return true;
            }
        }
        return false;
    }

    
}
