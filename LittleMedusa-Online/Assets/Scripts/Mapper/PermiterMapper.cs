using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class PermiterMapper : Mapper
    {
        public override Vector3Int GetNewPathPoint(Actor actorWandering)
        {
            bool canOccupyUp = false;
            bool canOccupyDown = false;
            bool canOccupyLeft = false;
            bool canOccupyRight = false;
            if (passableDirectionEnumList.Count > 0)
            {
                passableDirectionEnumList.Clear();
            }
            Enemy monsterWandering = (Enemy)actorWandering;
            bool isLastResortPointInitialised = false;
            FaceDirection lastResortFaceDirection = GetLastResortFaceDirection(monsterWandering.Facing);
            Vector3Int sliceUpPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Up));

            if (monsterWandering.CanOccupy(sliceUpPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(sliceUpPoint, monsterWandering))
            {
                Vector3Int pointUpRight = sliceUpPoint + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Right));
                Vector3Int pointUpLeft = sliceUpPoint + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Left));
                Vector3Int point = sliceUpPoint + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Down));
                Vector3Int pointRight = point + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Right));
                Vector3Int pointLeft = point + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Left));
                if (!monsterWandering.CanOccupy(pointUpRight) || !monsterWandering.CanOccupy(pointUpLeft) || !monsterWandering.CanOccupy(pointRight) || !monsterWandering.CanOccupy(pointLeft))
                {
                    if (lastResortFaceDirection == FaceDirection.Up)
                    {
                        isLastResortPointInitialised = true;
                    }
                    else
                    {
                        //Debug.Log("sliceUpPoint "+ sliceUpPoint);
                        passableDirectionEnumList.Add(FaceDirection.Up);
                    }
                }
                else
                {
                    canOccupyUp = true;
                }
            }


            Vector3Int sliceDownPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Down));

            if (monsterWandering.CanOccupy(sliceDownPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(sliceDownPoint, monsterWandering))
            {
                Vector3Int pointDownRight = sliceDownPoint + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Right));
                Vector3Int pointDownLeft = sliceDownPoint + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Left));
                Vector3Int point = sliceDownPoint + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Up));
                Vector3Int pointRight = point + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Right));
                Vector3Int pointLeft = point + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Left));

                if (!monsterWandering.CanOccupy(pointDownRight) || !monsterWandering.CanOccupy(pointDownLeft) || !monsterWandering.CanOccupy(pointRight) || !monsterWandering.CanOccupy(pointLeft))
                {
                    if (lastResortFaceDirection == FaceDirection.Down)
                    {
                        isLastResortPointInitialised = true;
                    }
                    else
                    {
                        //Debug.Log("sliceDownPoint " + sliceDownPoint);
                        passableDirectionEnumList.Add(FaceDirection.Down);
                    }
                }
                else
                {
                    canOccupyDown = true;
                }
            }

            Vector3Int sliceRightPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Right));

            if (monsterWandering.CanOccupy(sliceRightPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(sliceRightPoint, monsterWandering))
            {
                Vector3Int pointRightUp = sliceRightPoint + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Up));
                Vector3Int pointRightDown = sliceRightPoint + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Down));
                Vector3Int point = sliceRightPoint + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Left));
                Vector3Int pointUp = point + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Up));
                Vector3Int pointDown = point + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Down));

                if (!monsterWandering.CanOccupy(pointRightUp) || !monsterWandering.CanOccupy(pointRightDown) || !monsterWandering.CanOccupy(pointUp) || !monsterWandering.CanOccupy(pointDown))
                {
                    if (lastResortFaceDirection == FaceDirection.Right)
                    {
                        isLastResortPointInitialised = true;
                    }
                    else
                    {
                        //Debug.Log("sliceRightPoint " + sliceRightPoint);
                        passableDirectionEnumList.Add(FaceDirection.Right);
                    }
                }
                else
                {
                    canOccupyRight = true;
                }
            }

            Vector3Int sliceLeftPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Left));

            if (monsterWandering.CanOccupy(sliceLeftPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(sliceLeftPoint, monsterWandering))
            {
                Vector3Int pointLeftUp = sliceLeftPoint + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Up));
                Vector3Int pointLeftDown = sliceLeftPoint + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Down));
                Vector3Int point = sliceLeftPoint + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Right));
                Vector3Int pointUp = point + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Up));
                Vector3Int pointDown = point + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Down));

                if (!monsterWandering.CanOccupy(pointLeftUp) || !monsterWandering.CanOccupy(pointLeftDown) || !monsterWandering.CanOccupy(pointUp) || !monsterWandering.CanOccupy(pointDown))
                {
                    if (lastResortFaceDirection == FaceDirection.Left)
                    {
                        isLastResortPointInitialised = true;
                    }
                    else
                    {
                        //Debug.Log("sliceLeftPoint " + sliceLeftPoint);
                        passableDirectionEnumList.Add(FaceDirection.Left);
                    }
                }
                else
                {
                    canOccupyLeft = true;
                }
            }

            if (monsterWandering.isHeadCollisionWithOtherActor && passableDirectionEnumList.Contains(monsterWandering.headOnCollisionFaceDirection))
            {
                Debug.Log("removing parametre: " + monsterWandering.headOnCollisionFaceDirection + "  " + monsterWandering.transform.parent.name);
                passableDirectionEnumList.Remove(monsterWandering.headOnCollisionFaceDirection);
                monsterWandering.isHeadCollisionWithOtherActor = false;
            }
            if (passableDirectionEnumList.Count > 0)
            {
                //foreach (FaceDirection v in passableDirectionEnumList)
                //{
                //    Debug.Log("points: " + v);
                //}
                //if (isLastResortPointInitialised)
                //{
                //    Debug.Log("lastResortPoint " + lastResortFaceDirection);
                //}
                //Debug.Break();
                //Debug.Log("pointSelectedForPath "+ pointSelectedForPath);

                FaceDirection facing = passableDirectionEnumList[UnityEngine.Random.Range(0, passableDirectionEnumList.Count)];
                Vector3Int newCell = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(facing));
                return newCell;
            }
            else
            {
                if (isLastResortPointInitialised)
                {
                    FaceDirection facing = lastResortFaceDirection;
                    Vector3Int newCell = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(facing));
                    return newCell;
                }
                else
                {
                    if (canOccupyUp && canOccupyDown && canOccupyLeft && canOccupyRight)
                    {
                        Debug.Log("left stranded");
                        return GetClosestPoint(monsterWandering);
                    }
                    else
                    {
                        Debug.Log("Blocked");
                        return GetClosestPoint(monsterWandering);
                    }
                }
            }
        }

        Vector3Int GetClosestPoint(Enemy monsterWandering)
        {
            float upDistance = Mathf.Infinity;
            float downDistance = Mathf.Infinity;
            float leftDistance = Mathf.Infinity;
            float rightDistance = Mathf.Infinity;

            bool upDistanceSelected = false;
            bool downDistanceSelected = false;
            bool leftDistanceSelected = false;
            bool rightDistanceSelected = false;

            Vector3Int sliceUpPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Up));
            RaycastHit2D[] hitUp2D = Physics2D.RaycastAll(monsterWandering.transform.position, monsterWandering.transform.up, Mathf.Infinity);

            for (int i = 0; i < hitUp2D.Length; i++)
            {
                TileData tileDataUp = hitUp2D[i].collider.GetComponent<TileData>();
                if (tileDataUp != null && tileDataUp.blockUnitMotion)
                {
                    upDistance = Vector3.Distance(hitUp2D[i].point, monsterWandering.transform.position);
                    break;
                }
            }

            Vector3Int sliceDownPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Down));
            RaycastHit2D[] hitDown2D = Physics2D.RaycastAll(monsterWandering.transform.position, -monsterWandering.transform.up, Mathf.Infinity);

            for (int i = 0; i < hitDown2D.Length; i++)
            {
                TileData tileDataDown = hitDown2D[i].collider.GetComponent<TileData>();
                if (tileDataDown != null && tileDataDown.blockUnitMotion)
                {
                    downDistance = Vector3.Distance(hitDown2D[i].point, monsterWandering.transform.position);
                    break;
                }
            }

            Vector3Int sliceRightPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Right));
            RaycastHit2D[] hitRight2D = Physics2D.RaycastAll(monsterWandering.transform.position, monsterWandering.transform.right, Mathf.Infinity);

            for (int i = 0; i < hitRight2D.Length; i++)
            {
                TileData tileRightDown = hitRight2D[i].collider.GetComponent<TileData>();
                if (tileRightDown != null && tileRightDown.blockUnitMotion)
                {
                    rightDistance = Vector3.Distance(hitRight2D[i].point, monsterWandering.transform.position);
                    break;
                }
            }

            Vector3Int sliceLeftPoint = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Left));
            RaycastHit2D[] hitLeft2D = Physics2D.RaycastAll(monsterWandering.transform.position, -monsterWandering.transform.right, Mathf.Infinity);

            for (int i = 0; i < hitLeft2D.Length; i++)
            {
                TileData tileLeftDown = hitLeft2D[i].collider.GetComponent<TileData>();
                if (tileLeftDown != null && tileLeftDown.blockUnitMotion)
                {
                    leftDistance = Vector3.Distance(hitLeft2D[i].point, monsterWandering.transform.position);
                    break;
                }
            }

            float leastDistance = Mathf.Infinity;
            if (leastDistance > upDistance && monsterWandering.CanOccupy(sliceUpPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(sliceUpPoint, monsterWandering))
            {
                leastDistance = upDistance;
                upDistanceSelected = true;
                downDistanceSelected = false;
                leftDistanceSelected = false;
                rightDistanceSelected = false;
            }
            if (leastDistance > downDistance && monsterWandering.CanOccupy(sliceDownPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(sliceDownPoint, monsterWandering))
            {
                leastDistance = downDistance;
                upDistanceSelected = false;
                downDistanceSelected = true;
                leftDistanceSelected = false;
                rightDistanceSelected = false;
            }
            if (leastDistance > leftDistance && monsterWandering.CanOccupy(sliceLeftPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(sliceLeftPoint, monsterWandering))
            {
                leastDistance = leftDistance;
                upDistanceSelected = false;
                downDistanceSelected = false;
                leftDistanceSelected = true;
                rightDistanceSelected = false;
            }
            if (leastDistance > rightDistance && monsterWandering.CanOccupy(sliceRightPoint) && !GridManager.instance.IsCellBlockedForMonsterWithAnotherMonster(sliceRightPoint, monsterWandering))
            {
                leastDistance = rightDistance;
                upDistanceSelected = false;
                downDistanceSelected = false;
                leftDistanceSelected = false;
                rightDistanceSelected = true;
            }
            Vector3Int newCell = monsterWandering.currentMovePointCellPosition;

            //Debug.Log("updistance: "+ upDistance);
            //Debug.Log("downDistance: " + downDistance);
            //Debug.Log("leftDistance: " + leftDistance);
            //Debug.Log("rightDistance: " + rightDistance);

            if (upDistanceSelected)
            {
                newCell = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Up));
                //Debug.Log("upDistanceSelected ");
            }
            if (downDistanceSelected)
            {
                newCell = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Down));
                //Debug.Log("downDistanceSelected ");
            }
            if (leftDistanceSelected)
            {
                newCell = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Left));
                //Debug.Log("leftDistanceSelected ");
            }
            if (rightDistanceSelected)
            {
                newCell = monsterWandering.currentMovePointCellPosition + GridManager.instance.grid.WorldToCell(GridManager.instance.GetFacingDirectionOffsetVector3(FaceDirection.Right));
                //Debug.Log("rightDistanceSelected ");
            }
            return newCell;
        }

        FaceDirection GetLastResortFaceDirection(FaceDirection faceDirection)
        {
            switch (faceDirection)
            {
                case FaceDirection.Up:
                    return FaceDirection.Down;
                case FaceDirection.Left:
                    return FaceDirection.Right;
                case FaceDirection.Down:
                    return FaceDirection.Up;
                case FaceDirection.Right:
                    return FaceDirection.Left;
                default:
                    return faceDirection;
            }
        }
    }
}