using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SenseInLineAction : Actions
{
    public Hero heroInLineOfAction;
    float lengthOfDetection;
    Enemy monsterSensing;
    
    public override void Initialise(Actor actorActingThisAction)
    {
        this.monsterSensing = (Enemy)actorActingThisAction;
    }

    public void InitialiseLineSize(float lengthOfDetection)
    {
        this.lengthOfDetection = lengthOfDetection;
    }

    public override bool Perform()
    {
        if (monsterSensing == null)
        {
            Debug.LogError("monster sensing is not set");
            return false;
        }

        if(monsterSensing.IsActorOnArrows() || monsterSensing.IsActorOnMirror())
        {
            return false;
        }
        Debug.DrawLine(monsterSensing.transform.position, monsterSensing.transform.position + lengthOfDetection * GridManager.instance.GetFacingDirectionOffsetVector3(monsterSensing.Facing), Color.blue);

        if (monsterSensing.completedMotionToMovePoint)
        {
            if (monsterSensing.Facing != FaceDirection.Down)
            {
                RaycastHit2D[] hitCols_Up = Physics2D.LinecastAll(monsterSensing.transform.position, monsterSensing.transform.position + lengthOfDetection * GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Up));
                for (int i = 0; i < hitCols_Up.Length; i++)
                {
                    if (hitCols_Up[i].collider != null && hitCols_Up[i].collider.gameObject.GetComponent<Hero>() != null)
                    {
                        Hero heroInLine = hitCols_Up[i].collider.gameObject.GetComponent<Hero>();
                        if(heroInLine.ownerId!=monsterSensing.leaderNetworkId)
                        {
                            monsterSensing.Facing = FaceDirection.Up;
                            heroInLineOfAction = heroInLine;
                            return true;
                        }
                    }
                }
            }

            if (monsterSensing.Facing != FaceDirection.Up)
            {
                RaycastHit2D[] hitCols_Down = Physics2D.LinecastAll(monsterSensing.transform.position, monsterSensing.transform.position + lengthOfDetection * GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Down));
                for (int i = 0; i < hitCols_Down.Length; i++)
                {
                    if (hitCols_Down[i].collider != null && hitCols_Down[i].collider.gameObject.GetComponent<Hero>() != null)
                    {
                        Hero heroInLine = hitCols_Down[i].collider.gameObject.GetComponent<Hero>();
                        if (heroInLine.ownerId != monsterSensing.leaderNetworkId)
                        {
                            monsterSensing.Facing = FaceDirection.Down;
                            heroInLineOfAction = heroInLine;
                            return true;
                        }
                    }
                }
            }

            if (monsterSensing.Facing != FaceDirection.Right)
            {
                RaycastHit2D[] hitCols_Left = Physics2D.LinecastAll(monsterSensing.transform.position, monsterSensing.transform.position + lengthOfDetection * GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Left));
                for (int i = 0; i < hitCols_Left.Length; i++)
                {
                    if (hitCols_Left[i].collider != null && hitCols_Left[i].collider.gameObject.GetComponent<Hero>() != null)
                    {
                        Hero heroInLine = hitCols_Left[i].collider.gameObject.GetComponent<Hero>();
                        if (heroInLine.ownerId != monsterSensing.leaderNetworkId)
                        {
                            monsterSensing.Facing = FaceDirection.Left;
                            heroInLineOfAction = heroInLine;
                            return true;
                        }
                    }
                }
            }

            if (monsterSensing.Facing != FaceDirection.Left)
            {
                RaycastHit2D[] hitCols_Right = Physics2D.LinecastAll(monsterSensing.transform.position, monsterSensing.transform.position + lengthOfDetection * GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Right));
                for (int i = 0; i < hitCols_Right.Length; i++)
                {
                    if (hitCols_Right[i].collider != null && hitCols_Right[i].collider.gameObject.GetComponent<Hero>() != null)
                    {
                        Hero heroInLine = hitCols_Right[i].collider.gameObject.GetComponent<Hero>();
                        if (heroInLine.ownerId != monsterSensing.leaderNetworkId)
                        {
                            monsterSensing.Facing = FaceDirection.Right;
                            heroInLineOfAction = heroInLine;
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
}
