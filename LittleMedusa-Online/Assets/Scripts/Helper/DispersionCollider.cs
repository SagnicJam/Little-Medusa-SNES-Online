using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispersionCollider : MonoBehaviour
{
    [Header("Tweak Params")]
    public int damage;

    [Header("LiveData")]
    public float dispersionRadius;
    public float dispersionSpeed;
    public int ownerId;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Actor collidedActorWithMyHead = collision.GetComponent<Actor>();
        if (collidedActorWithMyHead != null)
        {
            if (ownerId != collidedActorWithMyHead.ownerId)
            {
                if (!MultiplayerManager.instance.isServer)
                {
                    OnHitByDispersedFireBall onHitByDispersedFireBall = new OnHitByDispersedFireBall(ClientSideGameManager.players[ownerId].masterController.localPlayer.GetLocalSequenceNo(),collidedActorWithMyHead.ownerId,damage);
                    ClientSend.OnPlayerHitByDispersedFireBall(onHitByDispersedFireBall);
                }
            }
        }
    }

    public void Grow(float dispersionRadius, float dispersionSpeed, int ownerId)
    {

        this.dispersionRadius = dispersionRadius;
        this.dispersionSpeed = dispersionSpeed/2;
        this.ownerId = ownerId;
        IEnumerator ie = GrowCor();
        StopCoroutine(ie);
        StartCoroutine(ie);
    }

    float t;

    IEnumerator GrowCor()
    {
        while(t<1)
        {
            t += Time.fixedDeltaTime * dispersionSpeed;
            transform.localScale = Vector3.Lerp(Vector3.zero,dispersionRadius*Vector3.one,t);
            yield return new WaitForFixedUpdate();
        }
        Destroy(gameObject);
        yield break;
    }
}
