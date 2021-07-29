using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    public static int chainIDGlobal = 0;

    [Header("Scene references")]
    //public AStar aStar;
    public Grid grid;

    [Header("Tweak Params")]
    public float switchToNormalFromHoleAfter = 3;
    public float pullforce = 1;
    public float timeOfForce = 5;
    public GameStateDependentTiles[] gameStateDependentTileArray;

    [Header("Unit Templates")]
    public GameObject rockFormation;
    public GameObject rockRemoval;
    public GameObject bigExplosion;
    public GameObject smallExplosion;

    [Header("Live Data")]
    public int xMin;
    public int yMin;
    public int xMax;
    public int yMax;

    private void Awake()
    {
        instance = this;
        for (int i = 0; i < gameStateDependentTileArray.Length; i++)
        {
            if (gameStateDependentTileArray[i].tileMap != null)
            {
                if (xMin > gameStateDependentTileArray[i].tileMap.cellBounds.xMin)
                {
                    xMin = gameStateDependentTileArray[i].tileMap.cellBounds.xMin;
                }
                if (yMin > gameStateDependentTileArray[i].tileMap.cellBounds.yMin)
                {
                    yMin = gameStateDependentTileArray[i].tileMap.cellBounds.yMin;
                }
                if (xMax < gameStateDependentTileArray[i].tileMap.cellBounds.xMax)
                {
                    xMax = gameStateDependentTileArray[i].tileMap.cellBounds.xMax;
                }
                if (yMax < gameStateDependentTileArray[i].tileMap.cellBounds.yMax)
                {
                    yMax = gameStateDependentTileArray[i].tileMap.cellBounds.yMax;
                }
            }
        }
    }

    public Vector3 GetFacingDirectionOffsetVector3(FaceDirection facing)
    {
        switch (facing)
        {
            case FaceDirection.Down:
                return new Vector3(0, -grid.cellSize.y, 0);
            case FaceDirection.Up:
                return new Vector3(0, grid.cellSize.y, 0);
            case FaceDirection.Left:
                return new Vector3(-grid.cellSize.x, 0, 0);
            case FaceDirection.Right:
                return new Vector3(grid.cellSize.x, 0, 0);
            default:
                return new Vector3(0, -grid.cellSize.y, 0);
        }
    }

    public Vector3 cellToworld(Vector3Int cell)
    {
        return grid.CellToWorld(cell) + grid.cellSize / 2;
    }

    public FaceDirection GetFaceDirectionFromCurrentPrevPoint(Vector3Int currentCell, Vector3Int previousCell, Actor actor)
    {
        if (currentCell.x > previousCell.x)
        {
            return FaceDirection.Right;
        }
        else if (currentCell.x < previousCell.x)
        {
            return FaceDirection.Left;
        }
        else if (currentCell.y > previousCell.y)
        {
            return FaceDirection.Up;
        }
        else if (currentCell.y < previousCell.y)
        {
            return FaceDirection.Down;
        }
        //Debug.LogError("Could not find any direction change");
        return actor.PreviousFacingDirection;
    }


    public bool IsCellBlockedForProjectiles(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && td.blockProjectiles)
            {
                return true;
            }
        }
        return false;
    }

    public IEnumerator ForceTravel(Actor actor, Vector3Int cellPos)
    {
        IEnumerator ie = ForceTravelCor(actor, cellPos);
        return ie;
    }

    public void WaitForForce(Actor actor, OnUsed<Actor> onUsed)
    {
        IEnumerator ie = WaitForForceCor(actor, onUsed);
        StopCoroutine(ie);
        StartCoroutine(ie);
    }

    public IEnumerator ForceTravelCor(Actor actor, Vector3Int cellPos)
    {
        Vector3 pos = GridManager.instance.cellToworld(cellPos);
        while(Vector3.Distance(actor.actorTransform.position,pos)>=0.05f)
        {
            if(actor!=null)
            {
                actor.actorTransform.position = Vector3.MoveTowards(actor.actorTransform.position, pos, Time.fixedDeltaTime * pullforce);
                yield return new WaitForFixedUpdate();
            }
            else
            {
                yield break;
            }
        }
        yield break;
    }

    public IEnumerator WaitForForceCor(Actor actor,OnUsed<Actor> onUsed)
    {
        yield return new WaitForSeconds(timeOfForce);
        if(onUsed!=null)
        {
            onUsed.Invoke(actor);
        }
        yield break;
    }

    public void SwitchTileToAfter(Vector3Int cellPos, EnumData.TileType fromTile, EnumData.TileType toTile)
    {
        IEnumerator ie = SwitchTileToAfterCor(cellPos,fromTile,toTile);
        StopCoroutine(ie);
        StartCoroutine(ie);
    }

    public IEnumerator SwitchTileToAfterCor(Vector3Int cellPos,EnumData.TileType fromTile,EnumData.TileType toTile)
    {
        yield return new WaitForSeconds(switchToNormalFromHoleAfter);
        SetTile(cellPos, fromTile,false,false);
        SetTile(cellPos, toTile, true, true);
        yield break;
    }

    public void Disperse(bool spawnTriggerCollider,GameObject dispersedCollider,GameObject dispersedGO,float dispersionRadius,float dispersionSpeed,int ownerId, Vector3Int dispersePoint)
    {
        List<Vector3Int> vList = new List<Vector3Int>();
        Vector3 dispersedPoint = cellToworld(dispersePoint);
        if(spawnTriggerCollider)
        {
            DispersionCollider dispersionCollider = Instantiate(dispersedCollider, dispersedPoint, Quaternion.identity).GetComponent<DispersionCollider>();
            dispersionCollider.Grow(dispersionRadius, dispersionSpeed, ownerId);
        }
        

        float perAngleJump = 360 / 8;
        for (float angle = 0; angle <= 360; angle += perAngleJump)
        {
            Vector3 finalPoint = dispersedPoint + GetVectorAtAngle(angle, Vector3.right, Vector3.forward) * dispersionRadius;
            GameObject g = Instantiate(dispersedGO, dispersedPoint, Quaternion.identity);
            IEnumerator tt = TravelToPos(g, finalPoint, dispersionSpeed, (x) => {
                Debug.Log(x);
                Destroy(g);
            });
            StopCoroutine(tt);
            StartCoroutine(tt);
        }
    }

    Vector3 GetVectorAtAngle(float angle, Vector3 right, Vector3 forward)
    {
        return Quaternion.AngleAxis(angle, forward) * right;
    }

    IEnumerator TravelToPos(GameObject g, Vector3 finalPoint, float speed, OnUsed<string> onend)
    {
        while (g != null && Vector3.Distance(g.transform.position, finalPoint) >= 0.05f)
        {
            g.transform.position = Vector3.MoveTowards(g.transform.position, finalPoint, Time.fixedDeltaTime * speed);
            yield return new WaitForFixedUpdate();
        }
        if (onend != null)
        {
            onend.Invoke("Destroyed");
        }
        yield break;
    }

    public static GameObject InstantiateGameObject(GameObject g)
    {
        return Instantiate(g);
    }

    public bool IsPositionBlockedForProjectiles(Vector3 cellObjectPosition)
    {
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(cellObjectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, cellObjectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && td.blockProjectiles)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsPositionContainingPetrifiedActor(Vector3 cellObjectPosition)
    {
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(cellObjectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, cellObjectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            Actor actor = hit2DArr[i].collider.gameObject.GetComponent<Actor>();
            if (actor != null && actor.isPetrified)
            {
                return true;
            }

        }
        return false;
    }

    public bool IsActorOnPositionPushable(Vector3 objectPosition)
    {
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            Actor actor = hit2DArr[i].collider.gameObject.GetComponent<Actor>();
            if (actor != null && actor.isPetrified && !actor.isPushed && actor.completedMotionToMovePoint)
            {
                return true;
            }
        }
        return false;
    }

    public Actor GetActorOnPos(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            Actor actor = hit2DArr[i].collider.gameObject.GetComponent<Actor>();
            if (actor != null)
            {
                return actor;
            }
        }
        return null;
    }

    

    public Actor GetLastPushedActorInChain(Actor firstActor)
    {
        bool foundLastElementInChain = false;

        Actor temp = firstActor;
        Actor prevActor = null;
        Actor nextActor = null;

        while(!foundLastElementInChain)
        {
            prevActor = temp;
            nextActor = prevActor.GetNextChainElement();
            
            if(prevActor.gameObject.GetInstanceID()==nextActor.gameObject.GetInstanceID())
            {
                foundLastElementInChain = true;
            }
            else
            {
                temp = nextActor;
            }
        }
        return nextActor;
    }

    public Actor GetTheLastActorInChain(Actor firstActor, FaceDirection directionToParseChain)
    {
        bool foundLastElementInChain = false;

        Actor temp = firstActor;
        Actor prevActor = null;
        Actor nextActor = null;
        while (!foundLastElementInChain)
        {
            prevActor = temp;
            nextActor = prevActor.GetNextPetrifiedActorInDirection(directionToParseChain);

            if (prevActor.gameObject.GetInstanceID() == nextActor.gameObject.GetInstanceID())
            {
                foundLastElementInChain = true;
            }
            else
            {
                temp = nextActor;
            }
        }

        return nextActor;
    }

    public bool HasTileAtCellPoint(Vector3Int cellPosToCheckFor, EnumData.TileType tileTypeTocheck)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && td.tileType == tileTypeTocheck)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsCellBlockedForPetrifiedUnitMotionAtPos(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();

            if (td != null && td.blockPetrifiedObjects)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsHeadCollision(Vector2 collidedObjectPosition, Vector3 headObjectPosition, FaceDirection facing)
    {
        Vector2 myPosition = headObjectPosition;
        Vector2 otherObjectPosition = collidedObjectPosition;

        Vector2 lineFacingDirectionOfMotion = GetFacingDirectionOffsetVector3(facing);
        Vector2 lineWithOtherObject = (otherObjectPosition - myPosition).normalized;

        float angle = Vector2.Angle(lineFacingDirectionOfMotion, lineWithOtherObject);
        Debug.DrawRay(myPosition, lineFacingDirectionOfMotion, Color.red);
        Debug.DrawRay(myPosition, lineWithOtherObject, Color.blue);
        if (angle <= 45)
        {
            return true;
        }
        return false;
    }


    public bool IsCellBlockedForUnitMotionAtPos(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && td.blockUnitMotion)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsCellBlockedForBoulderPlacementAtPos(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && td.blockBoulderPlacement)
            {
                return true;
            }
        }
        return false;
    }

    public void PlaceBoulderAnimation(Vector3Int cellBoulderDropped)
    {
        //if (AudioManager.instance != null)
        //{
        //    AudioManager.instance.PlayClip(AudioManager.instance.onPlaceBoulder);
        //}
        GameObject g = Instantiate(rockFormation, cellToworld(cellBoulderDropped), Quaternion.identity);
        FrameLooper fL = g.GetComponent<FrameLooper>();
        fL.PlayOneShotAnimation();
        fL.onPlayOneShotAnimation.RemoveAllListeners();
        fL.onPlayOneShotAnimation.AddListener(() =>
        {
            Destroy(g);
        });
    }

    public void RemoveBoulderAnimation(Vector3Int cellBoulderRemoved)
    {
        GameObject g = Instantiate(rockRemoval, cellToworld(cellBoulderRemoved), Quaternion.identity);
        FrameLooper fL = g.GetComponent<FrameLooper>();
        fL.PlayOneShotAnimation();
        fL.onPlayOneShotAnimation.RemoveAllListeners();
        fL.onPlayOneShotAnimation.AddListener(() => {
            Destroy(g);
        });
    }

    public void SetTile(Vector3Int cellPos, EnumData.TileType tType, bool HasTile,bool playAnimation)
    {
        Vector3 worldPos = Vector3.zero;
        Vector3Int fireCellPos = Vector3Int.zero;
        GameObject fire = null;

        if(playAnimation)
        {
            if (tType == EnumData.TileType.Boulder)
            {
                if (HasTile)
                {
                    PlaceBoulderAnimation(cellPos);
                }
                else
                {
                    RemoveBoulderAnimation(cellPos);
                }
            }
        }
        

        if (((int)tType - 1) > gameStateDependentTileArray.Length - 1)
        {
            Debug.LogError("index more thean range: " + tType);
            return;
        }
        if (((int)tType - 1) < 0)
        {
            Debug.LogError("index less thean zero: " + tType);
            return;
        }
        if (gameStateDependentTileArray[(int)tType - 1].tileMap == null)
        {
            Debug.LogError("tilemap is null");
            return;
        }
        if (!HasTile)
        {

            gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, null);
            if (gameStateDependentTileArray[(int)tType - 1].cereberustileToggle)
            {
                //switch (tType)
                //{
                //    case EnumData.TileType.LeftCereberusHead:
                //        worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Left);
                //        fireCellPos = grid.WorldToCell(worldPos);
                //        fire = GetFireObject(fireCellPos, FaceDirection.Left);
                //        if (fire != null)
                //        {
                //            Destroy(fire);
                //        }
                //        break;
                //    case EnumData.TileType.RightCereberusHead:
                //        worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Right);
                //        fireCellPos = grid.WorldToCell(worldPos);
                //        fire = GetFireObject(fireCellPos, FaceDirection.Right);
                //        if (fire != null)
                //        {
                //            Destroy(fire);
                //        }
                //        break;
                //    case EnumData.TileType.UpCereberusHead:
                //        worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Up);
                //        fireCellPos = grid.WorldToCell(worldPos);
                //        fire = GetFireObject(fireCellPos, FaceDirection.Up);
                //        if (fire != null)
                //        {
                //            Destroy(fire);
                //        }
                //        break;
                //    case EnumData.TileType.DownCereberusHead:
                //        worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Down);
                //        fireCellPos = grid.WorldToCell(worldPos);
                //        fire = GetFireObject(fireCellPos, FaceDirection.Down);
                //        if (fire != null)
                //        {
                //            Destroy(fire);
                //        }
                //        break;
                //}
            }

        }
        else
        {
            int xAbs = Mathf.Abs(cellPos.x);
            int yAbs = Mathf.Abs(cellPos.y);
            if (gameStateDependentTileArray[(int)tType - 1].cereberustileToggle)
            {
                switch (tType)
                {
                    //case EnumData.TileType.RightCereberusHead:
                    //    worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Right);
                    //    if (!IsCellBlockedForPetrifiedUnitMotionAtPos(grid.WorldToCell(worldPos)) || IsCellContainingMirrorAtPos(grid.WorldToCell(worldPos)))
                    //    {
                    //        Instantiate(rightFire, worldPos, Quaternion.identity);
                    //    }
                    //    break;
                    //case EnumData.TileType.LeftCereberusHead:
                    //    worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Left);
                    //    if (!IsCellBlockedForPetrifiedUnitMotionAtPos(grid.WorldToCell(worldPos)) || IsCellContainingMirrorAtPos(grid.WorldToCell(worldPos)))
                    //    {
                    //        Instantiate(leftFire, worldPos, Quaternion.identity);
                    //    }
                    //    break;
                    //case EnumData.TileType.DownCereberusHead:
                    //    worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Down);
                    //    if (!IsCellBlockedForPetrifiedUnitMotionAtPos(grid.WorldToCell(worldPos)) || IsCellContainingMirrorAtPos(grid.WorldToCell(worldPos)))
                    //    {
                    //        Instantiate(downFire, worldPos, Quaternion.identity);
                    //    }
                    //    break;
                    //case EnumData.TileType.UpCereberusHead:
                    //    worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Up);
                    //    if (!IsCellBlockedForPetrifiedUnitMotionAtPos(grid.WorldToCell(worldPos)) || IsCellContainingMirrorAtPos(grid.WorldToCell(worldPos)))
                    //    {
                    //        Instantiate(upFire, worldPos, Quaternion.identity);
                    //    }
                    //    break;
                }
            }
            if (gameStateDependentTileArray[(int)tType - 1].multipleTileGraphic)
            {
                if (gameStateDependentTileArray[(int)tType - 1].isDarkOnOdd)
                {
                    if ((xAbs + yAbs) % 2 == 1)
                    {
                        gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].darkTile);
                    }
                    if ((xAbs + yAbs) % 2 == 0)
                    {
                        gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tile);
                    }
                }
                else
                {
                    if ((xAbs + yAbs) % 2 == 1)
                    {
                        gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tile);
                    }
                    if ((xAbs + yAbs) % 2 == 0)
                    {
                        gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].darkTile);
                    }
                }
            }
            else
            {
                //if (gameStateDependentTileArray[(int)tType - 1].cereberustileToggle)
                //{
                //    if (!IsCellBlockedForPetrifiedUnitMotionAtPos(grid.WorldToCell(worldPos)) || IsCellContainingMirrorAtPos(grid.WorldToCell(worldPos)))
                //    {
                //        gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tile);
                //    }
                //    else
                //    {
                //        gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tileOff);
                //    }
                //}
                //else
                //{
                    gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tile);
                //}
            }
            if (gameStateDependentTileArray[(int)tType - 1].tileMap.GetComponent<TilemapCollider2D>() != null)
            {
                gameStateDependentTileArray[(int)tType - 1].tileMap.GetComponent<TilemapCollider2D>().enabled = false;
                gameStateDependentTileArray[(int)tType - 1].tileMap.GetComponent<TilemapCollider2D>().enabled = true;
            }
        }
    }

    public bool IsCellBlockedForFlyingUnitsAtPos(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();

            if (td != null && td.blockFlyingUnits)
            {
                return true;
            }
        }
        return false;
    }

    public List<Vector3Int> GetAllPositionForTileMap(EnumData.TileType tileType)
    {
        Tilemap tilemap = gameStateDependentTileArray[(int)tileType - 1].tileMap;
        List<Vector3Int> cellPositions = new List<Vector3Int>();
        if(tilemap!=null)
        {
            for (int n = tilemap.cellBounds.xMin; n < tilemap.cellBounds.xMax; n++)
            {
                for (int p = tilemap.cellBounds.yMin; p < tilemap.cellBounds.yMax; p++)
                {
                    Vector3Int localPlace = (new Vector3Int(n, p, (int)tilemap.transform.position.y));
                    if (tilemap.HasTile(localPlace))
                    {
                        cellPositions.Add(localPlace);
                    }
                    else
                    {
                        //No tile at "place"
                    }
                }
            }
        }
        
        return cellPositions;
    }

    public List<Vector3Int> GetPlusNeighbours(Vector3Int cell)
    {
        List<Vector3Int> neighbours = new List<Vector3Int>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 || y == 0)
                {
                    int checkX = cell.x + x;
                    int checkY = cell.y + y;

                    if (checkX >= xMin && checkX < xMax && checkY >= yMin && checkY < yMax)
                    {
                        neighbours.Add(new Vector3Int(checkX, checkY, 0));
                    }
                }
            }
        }

        if (neighbours.Contains(cell))
        {
            neighbours.Remove(cell);
        }
        return neighbours;
    }

    public List<Vector3Int> GetAdvancedPlusNeighbours(Vector3Int cell)
    {
        List<Vector3Int> neighbours = new List<Vector3Int>();
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                if (x == 0 || y == 0)
                {
                    int checkX = cell.x + x;
                    int checkY = cell.y + y;

                    if (checkX >= xMin && checkX < xMax && checkY >= yMin && checkY < yMax)
                    {
                        neighbours.Add(new Vector3Int(checkX, checkY, 0));
                    }
                }
            }
        }

        if (neighbours.Contains(cell))
        {
            neighbours.Remove(cell);
        }
        return neighbours;
    }

    public List<Vector3Int> GetCornerNeighbours(Vector3Int cell)
    {
        List<Vector3Int> neighbours = new List<Vector3Int>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != 0 && y != 0)
                {
                    int checkX = cell.x + x;
                    int checkY = cell.y + y;

                    if (checkX >= xMin && checkX < xMax && checkY >= yMin && checkY < yMax)
                    {
                        neighbours.Add(new Vector3Int(checkX, checkY, 0));
                    }
                }
            }
        }
        return neighbours;
    }

   

    public void EarthQuake(Hero hero,Vector3Int explodeCell)
    {
        List<Vector3Int> vList = GetAdvancedPlusNeighbours(explodeCell);
        List<Vector3Int> vList2 = GetCornerNeighbours(explodeCell);
        vList.AddRange(vList2);
        Explode(hero,vList);
    }

    void Explode(Hero hero,List<Vector3Int> plusCell)
    {
        hero.isInputFreezed = true;
        for (int i = 0; i < plusCell.Count; i++)
        {
            GameObject smallThunderGOPlus = Instantiate(smallExplosion, cellToworld(plusCell[i]), Quaternion.identity);
            StaticAnimatingTileUtil smallThunderGOPlusStaticTileAnimating = smallThunderGOPlus.GetComponent<StaticAnimatingTileUtil>();

            smallThunderGOPlusStaticTileAnimating.Initialise(plusCell[i]);

            smallThunderGOPlus.GetComponent<FrameLooper>().PlayOneShotAnimation();
            smallThunderGOPlus.GetComponent<FrameLooper>().onPlayOneShotAnimation.RemoveAllListeners();
            smallThunderGOPlus.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
            {
                smallThunderGOPlusStaticTileAnimating.DestroyObject();
                hero.isInputFreezed = false;
            });
        }
    
    }

    public void OnHitMedusa(Vector3 position)
    {
        Instantiate(GamePrefabCreator.instance.onHitMedusaVFX, position, Quaternion.identity);
    }
}
[Serializable]
public struct GameStateDependentTiles
{
    public EnumData.TileType tileAssetType;
    public Tile tile;
    public Tile darkTile;
    public Tile tileOff;
    public Tilemap tileMap;
    //public Tile[] allTileArr;
    public bool cereberustileToggle;
    public bool multipleTileGraphic;
    public bool isDarkOnOdd;

    //public int GetIndexOfTile(Sprite sp)
    //{
    //    for(int i=0;i<allTileArr.Length;i++)
    //    {
    //        if(allTileArr[i].sprite.name == sp.name)
    //        {
    //            return i;
    //        }
    //    }
    //    Debug.LogError("Could not find the sprite "+tileMap.name+"  "+sp.name);
    //    return -1;
    //}
}