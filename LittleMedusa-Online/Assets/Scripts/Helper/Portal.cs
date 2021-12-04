using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Portal : MonoBehaviour
{
    public Dictionary<Vector3Int, PortalInfo> portalEntranceDic = new Dictionary<Vector3Int, PortalInfo>();

    public void PlacePortal(int portalOwner,Vector3Int portalInLet,OnWorkDone onSuccess)
    {
        if(!portalEntranceDic.ContainsKey(portalInLet))
        {
            List<Vector3Int> placablePositions = GetAllPlacablePortalPoints(GetActorsWithMaxLives(portalOwner));

            if(placablePositions.Count>0)
            {
                Vector3Int portalOutlet = placablePositions[UnityEngine.Random.Range(0, placablePositions.Count)];
                GridManager.instance.SetTile(portalInLet, EnumData.TileType.Portal, true, false);
                GridManager.instance.SetTile(portalOutlet, EnumData.TileType.Portal, true, false);
                portalEntranceDic.Add(portalInLet, new PortalInfo(portalOwner, portalOutlet));

                //IEnumerator ie = PortalTimer(portalInLet);
                //StopCoroutine(ie);
                //StartCoroutine(ie);
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError("No placable position");
            }
        }
    }

    IEnumerator PortalTimer(Vector3Int portalInlet)
    {
        int temp = 0;
        while(temp<GameConfig.portalExistenceTickCount)
        {
            yield return new WaitForFixedUpdate();
            temp++;
        }

        GridManager.instance.SetTile(portalEntranceDic[portalInlet].portalOutlet, EnumData.TileType.Portal, false, false);
        GridManager.instance.SetTile(portalInlet, EnumData.TileType.Portal, false, false);
        portalEntranceDic.Remove(portalInlet);
        yield break;
    }

    List<Vector3Int>GetAllPlacablePortalPoints(List<Actor>actors)
    {
        List<Vector3Int> placablePoints = new List<Vector3Int>();
        foreach (Actor actor in actors)
        {
            Vector3Int positionToPlaceOnUp = GridManager.instance.grid.WorldToCell(actor.actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Up));
            if(!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(positionToPlaceOnUp))
            {
                placablePoints.Add(positionToPlaceOnUp);
            }

            Vector3Int positionToPlaceOnDown = GridManager.instance.grid.WorldToCell(actor.actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Down));
            if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(positionToPlaceOnDown))
            {
                placablePoints.Add(positionToPlaceOnDown);
            }

            Vector3Int positionToPlaceOnRight = GridManager.instance.grid.WorldToCell(actor.actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Right));
            if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(positionToPlaceOnRight))
            {
                placablePoints.Add(positionToPlaceOnRight);
            }

            Vector3Int positionToPlaceOnLeft = GridManager.instance.grid.WorldToCell(actor.actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Left));
            if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(positionToPlaceOnLeft))
            {
                placablePoints.Add(positionToPlaceOnLeft);
            }
        }
        return placablePoints;
    }

    List<Actor>GetActorsWithMaxLives(int ownerId)
    {
        int maxNo=0;
        List<Actor> maxLivesActor = new List<Actor>();
        foreach (KeyValuePair<int, ServerSideClient> kvp in Server.clients)
        {
            if (kvp.Value.serverMasterController != null)
            {
                Actor actor = kvp.Value.serverMasterController.serverInstanceHero;
                if (actor.ownerId != ownerId)
                {
                    if (kvp.Value.serverMasterController.serverInstanceHero.currentStockLives > maxNo)
                    {
                        maxNo = kvp.Value.serverMasterController.serverInstanceHero.currentStockLives;
                        maxLivesActor = new List<Actor>() { kvp.Value.serverMasterController.serverInstanceHero };
                    }
                    else if (kvp.Value.serverMasterController.serverInstanceHero.currentStockLives == maxNo)
                    {
                        maxLivesActor.Add(kvp.Value.serverMasterController.serverInstanceHero);
                    }
                }
            }
        }
        return maxLivesActor;
    }

    public void ActorUnitEnter(Actor actor,Vector3Int portalCellPosition)
    {
        if(portalEntranceDic.ContainsKey(portalCellPosition))
        {
            if(actor is Hero)
            {
                if (actor.ownerId == portalEntranceDic[portalCellPosition].portalOwner)
                {
                    TeleportActor(actor, portalEntranceDic[portalCellPosition].portalOutlet);
                }
            }
            else if(actor is Enemy enemy)
            {
                TeleportActor(enemy, portalEntranceDic[portalCellPosition].portalOutlet);
            }
        }
    }

    void TeleportActor(Actor actor,Vector3Int finalOutlet)
    {
        actor.transform.position = GridManager.instance.cellToworld(finalOutlet);
        actor.currentMovePointCellPosition = finalOutlet;
        actor.previousMovePointCellPosition = finalOutlet;
    }

    void TeleportProjectiles(ProjectileUtil projectileUtil, Vector3Int portalCellPosition)
    {
        float distanceTravelled = Vector3.Distance(projectileUtil.pU.initPos, projectileUtil.transform.position);
        float totalDistanceToTravelled = Vector3.Distance(projectileUtil.pU.initPos, projectileUtil.pU.finalPos);

        float distanceLeft = totalDistanceToTravelled - distanceTravelled;

        projectileUtil.pU.initPos = GridManager.instance.cellToworld(portalCellPosition);
        projectileUtil.pU.finalPos = projectileUtil.pU.initPos + (distanceLeft * GridManager.instance.GetFacingDirectionOffsetVector3(projectileUtil.pU.tileMovementDirection));
        projectileUtil.transform.position = GridManager.instance.cellToworld(portalCellPosition);
    }

    public void ProjectileUnitEnter(ProjectileUtil projectileUtil, Vector3Int portalCellPosition)
    {
        if (portalEntranceDic.ContainsKey(portalCellPosition))
        {
            if(projectileUtil.pU.ownerId == portalEntranceDic[portalCellPosition].portalOwner)
            {
                TeleportProjectiles(projectileUtil, portalEntranceDic[portalCellPosition].portalOutlet);
            }
        }
    }
}
[System.Serializable]
public struct PortalInfo
{
    public int portalOwner;
    public Vector3Int portalOutlet;

    public PortalInfo(int portalOwner, Vector3Int portalOutlet)
    {
        this.portalOwner = portalOwner;
        this.portalOutlet = portalOutlet;
    }
}
