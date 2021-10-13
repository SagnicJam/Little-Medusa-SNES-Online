using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePhysics : MonoBehaviour
{
    public float force;
    Actor actor;
    public BoxCollider2D hardboxCollider2D;
    public BoxCollider2D gameCollider2D;
    public List<Vector3> tilePullPositions = new List<Vector3>();
    Rigidbody2D rb;

    Vector3 actorResultantDirectionOfPullCache;

    public Vector3 direction;
    public bool isPhysicsEnabled;

    private void Awake()
    {
        actor = GetComponent<Actor>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void AddForcePoint(Vector3 tilePositionToAdd)
    {
        //Debug.LogError("Tile added : "+tilePositionToAdd +" id: "+ actor.ownerId);
        if(tilePullPositions.Count==0)
        {
            rb.isKinematic = false;
            rb.gravityScale = 0;
            rb.angularDrag = 0;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            if (hardboxCollider2D == null || gameCollider2D == null)
            {
                Debug.LogError("NotAssigned for: " + actor.actorTransform.gameObject.name);
            }
            hardboxCollider2D.enabled = true;
            gameCollider2D.enabled = false;
            actor.isPhysicsControlled = true;
            isPhysicsEnabled = true;

            if (actor.isPushed)
            {
                actor.OnPushStop();
            }
        }
        
        tilePullPositions.Add(tilePositionToAdd);
    }

    private void FixedUpdate()
    {
        if(isPhysicsEnabled)
        {
            rb.velocity = direction * force;
            if (actor != null)
            {
                actorResultantDirectionOfPullCache = Vector3.zero;
                foreach (Vector3 v in tilePullPositions)
                {
                    actorResultantDirectionOfPullCache += (v - actor.actorTransform.position);
                }

                direction = actorResultantDirectionOfPullCache.normalized;
                actor.currentMovePointCellPosition = GridManager.instance.grid.WorldToCell(actor.transform.position);

                Debug.DrawLine(actor.actorTransform.position
                            , actor.actorTransform.position + (10f * direction), Color.red);
            }
        }
    }

    public void RemoveForcePoint(Vector3 tilePositionToRemove)
    {
        //Debug.LogError("Tile removed : " + tilePositionToRemove + " id: " + actor.ownerId);
        tilePullPositions.Remove(tilePositionToRemove);
        if (tilePullPositions.Count==0)
        {
            if (actor != null)
            {
                actor.currentMovePointCellPosition = GridManager.instance.grid.WorldToCell(actor.transform.position);
            }
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            hardboxCollider2D.enabled = false;
            gameCollider2D.enabled = true;
            actor.isPhysicsControlled = false;

            this.direction = Vector3.zero;
            isPhysicsEnabled = false;
        }
    }
}
