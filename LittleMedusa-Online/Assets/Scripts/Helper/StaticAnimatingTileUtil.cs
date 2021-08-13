using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticAnimatingTileUtil : MonoBehaviour
{
    public bool isServerNetworked;
    public static int nextStaticAnimationTileID = 1;
    public int networkUid;

    public EnumData.StaticAnimatingTiles animationTileType;
    
    public FrameLooper fl;

    public void Initialise(Vector3Int pos)
    {
        if (isServerNetworked)
        {
            networkUid = nextStaticAnimationTileID;
            nextStaticAnimationTileID++;

            AnimatingStaticTile animatingStaticTile = new AnimatingStaticTile(networkUid, (int)animationTileType, fl.spriteIndexToShowCache, pos);
            ServerSideGameManager.animatingStaticTileDic.Add(networkUid, animatingStaticTile);
        }
    }

    public void DestroyObject()
    {
        if (ServerSideGameManager.animatingStaticTileDic.ContainsKey(networkUid))
        {
            ServerSideGameManager.animatingStaticTileDic.Remove(networkUid);
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (isServerNetworked)
        {
            AnimatingStaticTile animatingStaticTile;
            if (ServerSideGameManager.animatingStaticTileDic.TryGetValue(networkUid, out animatingStaticTile))
            {
                animatingStaticTile.animationSpriteIndex = fl.spriteIndexToShowCache;
                ServerSideGameManager.animatingStaticTileDic.Remove(networkUid);
                ServerSideGameManager.animatingStaticTileDic.Add(networkUid, animatingStaticTile);
            }
            else
            {
                Debug.LogError("Doesnot contain the key to set projectile position for");
            }
        }
    }
}
