using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePhysics : MonoBehaviour
{
    public float force;
    Actor actor;
    public BoxCollider2D hardboxCollider2D;
    public BoxCollider2D gameCollider2D;
    Rigidbody2D rb;

    public Vector3 direction;
    public bool isPhysicsEnabled;
    private void Awake()
    {
        actor = GetComponent<Actor>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void EnablePhysics()
    {
        rb.isKinematic = false;
        rb.gravityScale = 0;
        rb.angularDrag = 0;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if(hardboxCollider2D==null|| gameCollider2D==null)
        {
            Debug.LogError("NotAssigned for: "+actor.actorTransform.gameObject.name);
        }
        hardboxCollider2D.enabled = true;
        gameCollider2D.enabled = false;
        actor.isPhysicsControlled = true;
        isPhysicsEnabled = true;

        if(actor.isPushed)
        {
            actor.OnPushStop();
        }
    }

    public void UpdateDirection(Vector3 direction)
    {
        this.direction = direction;
    }

    private void FixedUpdate()
    {
        if(isPhysicsEnabled)
        {
            rb.velocity = direction * force;
            if (actor != null)
            {
                actor.currentMovePointCellPosition = GridManager.instance.grid.WorldToCell(actor.transform.position);
            }
        }
    }

    public void DisablePhysics()
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
