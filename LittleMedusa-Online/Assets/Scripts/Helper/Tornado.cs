using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tornado : MonoBehaviour
{
    public int ownerCasting;
    List<Vector3Int> positionsOfTile=new List<Vector3Int>();
    public bool solidify;

    Vector3 directionVectorCache;
    Vector3 actorResultantDirectionOfPullCache;

    public bool tornadoActive;

    private void FixedUpdate()
    {
        if(tornadoActive)
        {
            positionsOfTile = GridManager.instance.GetAllPositionForTileMap(EnumData.TileType.Tornado);

            foreach(KeyValuePair<int,ServerSideClient>kvp in Server.clients)
            {
                if(kvp.Value.serverMasterController != null)
                {
                    if (kvp.Key != ownerCasting)
                    {
                        actorResultantDirectionOfPullCache = Vector3.zero;
                        directionVectorCache = Vector3.zero;
                        foreach (Vector3Int v in positionsOfTile)
                        {
                            directionVectorCache = (GridManager.instance.cellToworld(v) - kvp.Value.serverMasterController.serverInstanceHero.actorTransform.position).normalized;
                            actorResultantDirectionOfPullCache += directionVectorCache;
                        }
                        actorResultantDirectionOfPullCache.Normalize();
                        if(!kvp.Value.serverMasterController.serverInstanceHero.gamePhysics.isPhysicsEnabled)
                        {
                            kvp.Value.serverMasterController.serverInstanceHero.gamePhysics.EnablePhysics();
                        }
                        kvp.Value.serverMasterController.serverInstanceHero.gamePhysics.UpdateDirection(actorResultantDirectionOfPullCache);
                        Debug.DrawLine(kvp.Value.serverMasterController.serverInstanceHero.actorTransform.position
                            , kvp.Value.serverMasterController.serverInstanceHero.actorTransform.position + (10f * actorResultantDirectionOfPullCache), Color.red);
                    }
                }
            }
            foreach(KeyValuePair<int,Enemy>kvp in Enemy.enemies)
            {
                actorResultantDirectionOfPullCache = Vector3.zero;
                directionVectorCache = Vector3.zero;
                foreach (Vector3Int v in positionsOfTile)
                {
                    directionVectorCache = (GridManager.instance.cellToworld(v) - kvp.Value.actorTransform.position).normalized;
                    actorResultantDirectionOfPullCache += directionVectorCache;
                }
                actorResultantDirectionOfPullCache.Normalize();
                if(!kvp.Value.gamePhysics.isPhysicsEnabled)
                {
                    kvp.Value.gamePhysics.EnablePhysics();
                }
                kvp.Value.gamePhysics.UpdateDirection(actorResultantDirectionOfPullCache);

                Debug.DrawLine(kvp.Value.actorTransform.position
                    , kvp.Value.actorTransform.position + (10f * actorResultantDirectionOfPullCache), Color.blue);
            }
        }
    }

    public void UpdateTornadoActivation(int ownerCasting)
    {
        this.ownerCasting = ownerCasting;
        positionsOfTile = GridManager.instance.GetAllPositionForTileMap(EnumData.TileType.Tornado);
        tornadoActive = (positionsOfTile.Count > 0);
        if(tornadoActive)
        {
            if(!solidify)
            {
                GridManager.instance.SolidifyTiles();
                solidify = true;
            }
        }
        else
        {
            if (solidify)
            {
                GridManager.instance.NormaliseTiles();
                solidify = false;
            }

            foreach (KeyValuePair<int, ServerSideClient> kvp in Server.clients)
            {
                if (kvp.Value.serverMasterController != null)
                {
                    if (kvp.Key != ownerCasting)
                    {
                        if (kvp.Value.serverMasterController.serverInstanceHero.gamePhysics.isPhysicsEnabled)
                        {
                            kvp.Value.serverMasterController.serverInstanceHero.gamePhysics.DisablePhysics();
                        }
                    }
                }
            }
            foreach (KeyValuePair<int, Enemy> kvp in Enemy.enemies)
            {
                if (kvp.Value.gamePhysics.isPhysicsEnabled)
                {
                    kvp.Value.gamePhysics.DisablePhysics();
                }
            }
        }
    }
}
