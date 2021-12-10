using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoCollider : MonoBehaviour
{
    public int ownerCasting;
    public void InitialiseOwner(int owner)
    {
        ownerCasting = owner;
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        Actor collidedActor = collider.GetComponent<Actor>();
        if (collidedActor != null&&collidedActor.ownerId!=ownerCasting)
        {
            if((collidedActor.isPhysicsControlled||collidedActor.isRespawnningPlayer)&&collidedActor.gamePhysics.tilePullPositions.Contains(transform.position))
            {
                return;
            }
            collidedActor.OnBodyCollidingWithTornadoEffectTiles(this.GetComponent<TileData>());
        }
    }
}
