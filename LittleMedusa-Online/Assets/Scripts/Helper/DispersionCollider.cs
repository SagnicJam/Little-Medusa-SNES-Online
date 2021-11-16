using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispersionCollider : MonoBehaviour
{
    [Header("LiveData")]
    public int ownerId;

    public void Initialise(int ownerId)
    {
        this.ownerId = ownerId;
    }

    private void FixedUpdate()
    {
        if (GridManager.instance.IsCellBlockedForProjectiles(GridManager.instance.grid.WorldToCell(transform.position)))
        {
            Destroy(gameObject);
            return;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (MultiplayerManager.instance.isServer)
        {
            Actor collidedActor = collision.GetComponent<Actor>();
            if (collidedActor != null)
            {
                if (ownerId != collidedActor.ownerId && !collidedActor.isInvincible&&!collidedActor.isDead)
                {
                    if (collidedActor is Enemy enemy)
                    {
                        Debug.LogError("Enemy taking damage");
                        enemy.TakeDamage(enemy.currentHP);
                    }
                    else if (collidedActor is Hero hero)
                    {
                        Debug.LogError("Herop taking damage");
                        hero.TakeDamage(GameConfig.dispersedFireBallDamage);
                    }
                }
            }
        }
    }
}
