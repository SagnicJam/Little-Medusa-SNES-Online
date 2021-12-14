using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class AStarPathFindMapper : Mapper
    {
        float pathFindingWearOff;
        public void InitialisePathWearOffTime(float timeForWearOff)
        {
            pathFindingWearOff = timeForWearOff;
        }

        public override Vector3Int GetNewPathPoint(Actor tobeMappedActor)
        {
            Enemy monsterFindingPath = (Enemy)tobeMappedActor;
            if (monsterFindingPath.heroToChase == null)
            {
                return GridManager.instance.grid.WorldToCell(monsterFindingPath.transform.position);
            }
            Vector3Int startCell = GridManager.instance.grid.WorldToCell(monsterFindingPath.transform.position);
            Vector3Int targetCell = GridManager.instance.grid.WorldToCell(monsterFindingPath.heroToChase.transform.position);
            GridManager.instance.aStar.ReInitialiseStaticObstaclesForMonster(monsterFindingPath);
            if (monsterFindingPath.isHeadCollisionWithOtherActor)
            {
                GridManager.instance.aStar.BlockSpot(monsterFindingPath.headOnCollisionCell);
            }
            List<Vector3Int> pathList = GridManager.instance.aStar.Compute(startCell, targetCell);
            if (pathList.Count > 0)
            {
                pathList.RemoveAt(0);
            }
            //foreach (Vector3Int v in pathList)
            //{
            //    Debug.LogError("actor: " + tobeMappedActor.gameObject.GetInstanceID() + " list: " + v);
            //    Debug.LogError("path for me to go to is : "+ pathList[pathList.Count - 2]);
            //}
            //Debug.Break();
            if (pathList.Count > 1)
            {
                if (monsterFindingPath.isHeadCollisionWithOtherActor)
                {
                    //Unblock
                    GridManager.instance.aStar.UnBlockSpot(monsterFindingPath.headOnCollisionCell);
                    monsterFindingPath.isHeadCollisionWithOtherActor = false;
                }
                if (GridManager.instance.IsCellPointTheNextPointToMoveInForPathFindingAnyMonster(pathList[pathList.Count - 2]))
                {
                    if (tobeMappedActor is Enemy monsterToBeMapped)
                    {
                        if (monsterToBeMapped is MirrorKnight mirrorKnight)
                        {
                            mirrorKnight.FinishFollowing();
                        }
                        else if (monsterToBeMapped is Cyclops cyclops)
                        {
                            cyclops.FinishFollowing();
                        }
                        else if (monsterToBeMapped is Centaur centaur)
                        {
                            centaur.FinishFollowing();
                        }
                        monsterToBeMapped.waitForPathFindingToWearOff.ReInitialiseTimerToBegin(monsterToBeMapped.pathFindingWearOffTickCount);
                    }
                    //Debug.LogError("Returning start cell: "+startCell);
                    return startCell;
                }
                else
                {
                    //Debug.LogError("Returning pathList.Count - 2 cell: " + pathList[pathList.Count - 2]);
                    return pathList[pathList.Count - 2];
                }
            }
            else
            {
                if (tobeMappedActor is Enemy monsterToBeMapped)
                {
                    if (monsterToBeMapped is MirrorKnight mirrorKnight)
                    {
                        mirrorKnight.FinishFollowing();
                    }
                    else if (monsterToBeMapped is Cyclops cyclops)
                    {
                        cyclops.FinishFollowing();
                    }
                    else if (monsterToBeMapped is Centaur centaur)
                    {
                        centaur.FinishFollowing();
                    }
                    monsterToBeMapped.waitForPathFindingToWearOff.ReInitialiseTimerToBegin(monsterToBeMapped.pathFindingWearOffTickCount);
                }
                return startCell;
            }
        }
    }
}