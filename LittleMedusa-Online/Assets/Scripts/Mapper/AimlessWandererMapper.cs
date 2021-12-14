using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class AimlessWandererMapper : Mapper
    {
        public override Vector3Int GetNewPathPoint(Actor actorWandering)
        {
            if (passableDirectionEnumList.Count > 0)
            {
                passableDirectionEnumList.Clear();
            }

            Enemy monsterWandering = (Enemy)actorWandering;
            Vector3Int posToAnalyseForNextPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(monsterWandering.Facing));

            if (!monsterWandering.isHeadCollisionWithOtherActor && monsterWandering.CanOccupy(posToAnalyseForNextPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(posToAnalyseForNextPoint, monsterWandering))
            {
                return posToAnalyseForNextPoint;
            }

            Vector3Int sliceUpPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Up));
            if (monsterWandering.CanOccupy(sliceUpPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(sliceUpPoint, monsterWandering))
            {
                passableDirectionEnumList.Add(FaceDirection.Up);
            }

            Vector3Int sliceDownPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Down));
            if (monsterWandering.CanOccupy(sliceDownPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(sliceDownPoint, monsterWandering))
            {
                passableDirectionEnumList.Add(FaceDirection.Down);
            }

            Vector3Int sliceRightPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Right));
            if (monsterWandering.CanOccupy(sliceRightPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(sliceRightPoint, monsterWandering))
            {
                passableDirectionEnumList.Add(FaceDirection.Right);
            }

            Vector3Int sliceLeftPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Left));
            if (monsterWandering.CanOccupy(sliceLeftPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(sliceLeftPoint, monsterWandering))
            {
                passableDirectionEnumList.Add(FaceDirection.Left);
            }

            if (monsterWandering.isHeadCollisionWithOtherActor && passableDirectionEnumList.Contains(monsterWandering.headOnCollisionFaceDirection))
            {
                Debug.Log("ran for removing " + monsterWandering.headOnCollisionFaceDirection + "  " + monsterWandering.transform.parent.name);
                passableDirectionEnumList.Remove(monsterWandering.headOnCollisionFaceDirection);
                monsterWandering.isHeadCollisionWithOtherActor = false;
            }
            //foreach (FaceDirection v in passableDirectionEnumList)
            //{
            //    Debug.Log("points: " + v);
            //}
            if (passableDirectionEnumList.Count > 0)
            {
                FaceDirection facing = passableDirectionEnumList[UnityEngine.Random.Range(0, passableDirectionEnumList.Count)];
                //Debug.Log("returning: " + facing);
                Vector3Int newCell = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(facing));
                return newCell;
            }
            //else
            //{
            //    Debug.Log("nothin found");
            //}
            return GridManager.instance.grid.WorldToCell(monsterWandering.transform.position);
        }
    }
}