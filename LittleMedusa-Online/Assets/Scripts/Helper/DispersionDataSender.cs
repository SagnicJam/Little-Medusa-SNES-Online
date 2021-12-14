using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class DispersionDataSender : MonoBehaviour
    {
        public int networkUid;
        void Start()
        {
            if (MultiplayerManager.instance.isServer)
            {
                networkUid = ProjectileUtil.nextProjectileID;
                ProjectileUtil.nextProjectileID++;
                ProjectileData projectileData = new ProjectileData(networkUid, 0, (int)EnumData.Projectiles.DispersedFireBallMirrorKnight, transform.position, 0);
                ServerSideGameManager.projectilesDic.Add(networkUid, projectileData);
            }
        }


        private void FixedUpdate()
        {
            if (MultiplayerManager.instance.isServer)
            {
                ProjectileData projectileData;
                if (ServerSideGameManager.projectilesDic.TryGetValue(networkUid, out projectileData))
                {
                    projectileData.projectilePosition = transform.position;
                    ServerSideGameManager.projectilesDic.Remove(networkUid);
                    ServerSideGameManager.projectilesDic.Add(networkUid, projectileData);
                }
                else
                {
                    Debug.LogError("Doesnot contain the key to set projectile position for");
                }
            }
        }

        private void OnDestroy()
        {
            if (MultiplayerManager.instance.isServer)
            {
                ServerSideGameManager.projectilesDic.Remove(networkUid);
            }
        }
    }
}