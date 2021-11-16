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
    public AStar aStar;
    public Grid grid;
    public Tornado tornado;
    public Portal portal;
    public EnemySpawnner enemySpawnner;

    [Header("Tweak Params")]
    public GameStateDependentTiles[] gameStateDependentTileArray;

    [Header("Unit Templates")]
    public GameObject rockFormation;
    public GameObject rockRemoval;
    public GameObject bigExplosion;
    public GameObject smallExplosion;
    public GameObject lightning;
    public GameObject earthquake;
    public GameObject tornadoAnimation;
    public GameObject leftFire;
    public GameObject rightFire;
    public GameObject upFire;
    public GameObject downFire;

    [Header("Live Units")]
    public Dictionary<Vector3Int,GameObject> liveTornadoAnimationsDic = new Dictionary<Vector3Int, GameObject>();

    [Header("Live Data")]
    public int xMin;
    public int yMin;
    public int xMax;
    public int yMax;

    private void Awake()
    {
        instance = this;
        xMin = int.MaxValue;
        yMin = int.MaxValue;
        xMax = -int.MaxValue;
        yMax = -int.MaxValue;

        for (int i = 0; i < gameStateDependentTileArray.Length; i++)
        {
            if (gameStateDependentTileArray[i].tileMap != null)
            {
                List<Vector3Int> allPositionList = GetAllPositionForTileMap(gameStateDependentTileArray[i].tileAssetType);
                for (int j = 0; j < allPositionList.Count; j++)
                {
                    if (xMin > allPositionList[j].x)
                    {
                        xMin = allPositionList[j].x;
                    }
                    if (yMin > allPositionList[j].y)
                    {
                        yMin = allPositionList[j].y;
                    }
                    if (xMax < allPositionList[j].x)
                    {
                        xMax = allPositionList[j].x;
                    }
                    if (yMax < allPositionList[j].y)
                    {
                        yMax = allPositionList[j].y;
                    }
                }
                gameStateDependentTileArray[i].initialVectorToTileMapper = new Dictionary<Vector3Int, Tile>();
                gameStateDependentTileArray[i].liveAnimatedGORef = new Dictionary<Vector3Int, GameObject>();
                foreach (Vector3Int item in allPositionList)
                {
                    gameStateDependentTileArray[i].initialVectorToTileMapper.Add(item,gameStateDependentTileArray[i].tileMap.GetTile(item)as Tile);
                }
            }
        }
        if(aStar!=null)
        {
            aStar.Initialise();
        }
    }

    public void CastQuickSand(Actor serverActor)
    {
        List<Vector3Int> vList = GetSizeCells(GameConfig.quickSandSize / 2, grid.WorldToCell(serverActor.actorTransform.position));
        List<Actor> actorseffected = new List<Actor>();

        foreach (Vector3Int v in vList)
        {
            Actor actor = GetActorOnPos(v);
            if (actor != null && serverActor.ownerId != actor.ownerId)
            {
                actor.isMovementFreezed = true;
                actorseffected.Add(actor);
            }
        }
        IEnumerator ie = QuickSandTimer(actorseffected);
        StopCoroutine(ie);
        StartCoroutine(ie);
    }

    IEnumerator QuickSandTimer(List<Actor> actorsEffected)
    {
        int temp = 0;
        while (temp < GameConfig.quickSandTimerTickCount)
        {
            yield return new WaitForFixedUpdate();
            temp++;
        }
        foreach (Actor actor in actorsEffected)
        {
            actor.isMovementFreezed = false;
        }
        yield break;
    }

    public void CastAeloianMight(int ownerCasting)
    {
        List<Actor> actorsBeingPushed = new List<Actor>();
        foreach (KeyValuePair<int, ServerSideClient> kvp in Server.clients)
        {
            if (kvp.Value.serverMasterController != null)
            {
                Actor actor = kvp.Value.serverMasterController.serverInstanceHero;
                if (actor.ownerId != ownerCasting)
                {
                    actor.StartGettingPushedByWind(FaceDirection.Up);
                    actorsBeingPushed.Add(actor);
                }
            }
        }

        foreach (KeyValuePair<int,Enemy> kvp in Enemy.enemies)
        {
            kvp.Value.StartGettingPushedByWind(FaceDirection.Up);
            actorsBeingPushed.Add(kvp.Value);
        }
        IEnumerator ie = AeloianMightTimer(actorsBeingPushed);
        StopCoroutine(ie);
        StartCoroutine(ie);
    }

    IEnumerator AeloianMightTimer(List<Actor> actorsBeingPushed)
    {
        int temp = 0;
        while (temp < GameConfig.aeloianMightTickCount)
        {
            yield return new WaitForFixedUpdate();
            temp++;
        }
        foreach (Actor actor in actorsBeingPushed)
        {
            actor.StopPushWithoutDamage(actor);
        }
        yield break;
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
                actor.actorTransform.position = Vector3.MoveTowards(actor.actorTransform.position, pos, Time.fixedDeltaTime * GameConfig.pullforce);
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
        yield return new WaitForSeconds(GameConfig.timeOfForce);
        if(onUsed!=null)
        {
            onUsed.Invoke(actor);
        }
        yield break;
    }

    public void SwitchTileAfter(Vector3Int cellPos, EnumData.TileType fromTile,EnumData.TileType toTile)
    {
        IEnumerator ie = SwitchTileCor(cellPos,fromTile, toTile);
        StopCoroutine(ie);
        StartCoroutine(ie);
    }

    public IEnumerator SwitchTileCor(Vector3Int cellPos,EnumData.TileType fromTile,EnumData.TileType toTile)
    {
        yield return new WaitForSeconds(GameConfig.switchHoleToNormalTileAfterDuration);
        SetTile(cellPos, fromTile,false,false);
        SetTile(cellPos, toTile, true,false);
        yield break;
    }

    public void Disperse(GameObject dispersedGO,float dispersionRadius,float dispersionSpeed,int ownerId, Vector3Int dispersePoint)
    {
        //List<Vector3Int> vList = new List<Vector3Int>();
        Vector3 dispersedPoint = cellToworld(dispersePoint);

        float perAngleJump = 360 / 8;
        for (float angle = 0; angle <= 360; angle += perAngleJump)
        {
            Vector3 finalPoint = dispersedPoint + GetVectorAtAngle(angle, Vector3.right, Vector3.forward) * dispersionRadius;
            GameObject g = Instantiate(dispersedGO, dispersedPoint, Quaternion.identity);

            g.GetComponent<DispersionCollider>().Initialise(ownerId);

            IEnumerator tt = TravelToPos(g, finalPoint, dispersionSpeed, (x) =>
            {
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

    public bool IsPureHeadOn(Vector2 collidedObjectPosition, ClientEnemyManager cE)
    {
        Vector2 myPosition = cE.transform.position;
        Vector2 otherObjectPosition = collidedObjectPosition;

        Vector2 lineFacingDirectionOfMotion = GetFacingDirectionOffsetVector3(cE.currentFaceDirection);
        Vector2 lineWithOtherObject = (otherObjectPosition - myPosition).normalized;

        float angle = Vector2.Angle(lineFacingDirectionOfMotion, lineWithOtherObject);
        Debug.DrawRay(myPosition, lineFacingDirectionOfMotion, Color.red);
        Debug.DrawRay(myPosition, lineWithOtherObject, Color.blue);
        if (angle == 0)
        {
            //Debug.Log("true for : " + transform.parent.name);
            return true;
        }
        //Debug.Log("Faslse for : "+transform.parent.name);
        return false;
    }

    public bool IsPureHeadOn(Vector2 collidedObjectPosition, Actor headActor)
    {
        Vector2 myPosition = headActor.transform.position;
        Vector2 otherObjectPosition = collidedObjectPosition;

        Vector2 lineFacingDirectionOfMotion = GetFacingDirectionOffsetVector3(headActor.Facing);
        Vector2 lineWithOtherObject = (otherObjectPosition - myPosition).normalized;

        float angle = Vector2.Angle(lineFacingDirectionOfMotion, lineWithOtherObject);
        Debug.DrawRay(myPosition, lineFacingDirectionOfMotion, Color.red);
        Debug.DrawRay(myPosition, lineWithOtherObject, Color.blue);
        if (angle == 0)
        {
            //Debug.Log("true for : " + transform.parent.name);
            return true;
        }
        //Debug.Log("Faslse for : "+transform.parent.name);
        return false;
    }

    public bool IsPureBackOrSideStab(Vector2 collidedObjectPosition, Actor headActor)
    {
        Vector2 myPosition = headActor.transform.position;
        Vector2 otherObjectPosition = collidedObjectPosition;

        Vector2 lineFacingDirectionOfMotion = GetFacingDirectionOffsetVector3(headActor.Facing);
        Vector2 lineWithOtherObject = (otherObjectPosition - myPosition).normalized;

        float angle = Vector2.Angle(lineFacingDirectionOfMotion, lineWithOtherObject);
        Debug.DrawRay(myPosition, lineFacingDirectionOfMotion, Color.red);
        Debug.DrawRay(myPosition, lineWithOtherObject, Color.blue);

        if (angle > 0f)
        {
            //Debug.Log("true for : " + transform.parent.name);
            return true;
        }
        return false;
    }

    public bool IsPureBackOrSideStab(Vector2 collidedObjectPosition, ClientEnemyManager cE)
    {
        Vector2 myPosition = cE.transform.position;
        Vector2 otherObjectPosition = collidedObjectPosition;

        Vector2 lineFacingDirectionOfMotion = GetFacingDirectionOffsetVector3(cE.currentFaceDirection);
        Vector2 lineWithOtherObject = (otherObjectPosition - myPosition).normalized;

        float angle = Vector2.Angle(lineFacingDirectionOfMotion, lineWithOtherObject);
        Debug.DrawRay(myPosition, lineFacingDirectionOfMotion, Color.red);
        Debug.DrawRay(myPosition, lineWithOtherObject, Color.blue);

        if (angle > 0f)
        {
            //Debug.Log("true for : " + transform.parent.name);
            return true;
        }
        return false;
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

    public bool IsClientEnemyOnPositionPushable(Vector3 objectPosition)
    {
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            ClientEnemyManager cE = hit2DArr[i].collider.gameObject.GetComponent<ClientEnemyManager>();
            if (cE != null && cE.currentEnemyState == EnumData.EnemyState.Petrified)
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

    public ClientEnemyManager GetClientEnemyOnPos(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            ClientEnemyManager clientEnemy = hit2DArr[i].collider.gameObject.GetComponent<ClientEnemyManager>();
            if (clientEnemy != null)
            {
                return clientEnemy;
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

    public bool HasTileAtCellPoint(Vector3Int cellPosToCheckFor, EnumData.GameObjectEnums tileTypeTocheck_1, EnumData.GameObjectEnums tileTypeTocheck_2)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && (td.gameObjectEnums == tileTypeTocheck_1 || td.gameObjectEnums == tileTypeTocheck_2))
            {
                return true;
            }
        }
        return false;
    }

    public TileData GetTileAtCellPoint(Vector3Int cellPosToCheckFor, EnumData.TileType tileTypeTocheck)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && td.tileType == tileTypeTocheck)
            {
                return td;
            }
        }
        return null;
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

    public bool HasTileAtCellPoint(Vector3Int cellPosToCheckFor, EnumData.GameObjectEnums tileTypeTocheck)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && td.gameObjectEnums == tileTypeTocheck)
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

    public bool IsCellKillableForSpawnAtPos(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && (td.killUnitsInstantlyIfInTheirRegion||td is Enemy))
            {
                return true;
            }
        }
        return false;
    }

    public EnumData.TileType GetCereberausHeadTypeFromDirection(FaceDirection direction)
    {
        switch (direction)
        {
            case FaceDirection.Down:
                return EnumData.TileType.DownCereberusHead;
            case FaceDirection.Up:
                return EnumData.TileType.UpCereberusHead;
            case FaceDirection.Left:
                return EnumData.TileType.LeftCereberusHead;
            case FaceDirection.Right:
                return EnumData.TileType.RightCereberusHead;
        }
        Debug.LogError("wrong input for direction");
        return EnumData.TileType.None;
    }

    public bool IsCellBlockedForSpawnObjectPlacementAtPos(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && td.blockToSpawnObjectsPlacement)
            {
                return true;
            }
        }
        return false;
    }

    public void PlaceTornadoAnimation(Vector3Int cellTornadoDropped)
    {
        liveTornadoAnimationsDic.Add(cellTornadoDropped, Instantiate(tornadoAnimation, cellToworld(cellTornadoDropped), Quaternion.identity));
    }

    public void RemoveTornadoAnimation(Vector3Int cellTornadoDropped)
    {
        if(liveTornadoAnimationsDic.ContainsKey(cellTornadoDropped))
        {
            Destroy(liveTornadoAnimationsDic[cellTornadoDropped]);
            liveTornadoAnimationsDic.Remove(cellTornadoDropped);
        }
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
        fL.onPlayOneShotAnimation.AddListener(() => 
        {
            Destroy(g);
        });
    }

    public GameObject GetFireObject(Vector3Int cellPosToCheckFor, FaceDirection facing)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);

        EnumData.GameObjectEnums toFindEnum = EnumData.GameObjectEnums.None;
        switch (facing)
        {
            case FaceDirection.Up:
                toFindEnum = EnumData.GameObjectEnums.UpCereberusFire;
                break;
            case FaceDirection.Down:
                toFindEnum = EnumData.GameObjectEnums.DownCereberusFire;
                break;
            case FaceDirection.Left:
                toFindEnum = EnumData.GameObjectEnums.LeftCereberusFire;
                break;
            case FaceDirection.Right:
                toFindEnum = EnumData.GameObjectEnums.RightCereberusFire;
                break;
        }

        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && td.gameObjectEnums == toFindEnum)
            {
                return td.gameObject;
            }
        }
        return null;
    }

    public void ReplaceTileWith(Vector3Int cellPos, EnumData.TileType toReplaceTile, EnumData.TileType replaceWith,bool playAnimation)
    {
        SetTile(cellPos, toReplaceTile, false, playAnimation);
        SetTile(cellPos, replaceWith, true, playAnimation);
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
            else if(tType == EnumData.TileType.Tornado)
            {
                if (HasTile)
                {
                    PlaceTornadoAnimation(cellPos);
                }
                else
                {
                    RemoveTornadoAnimation(cellPos);
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
            if(gameStateDependentTileArray[(int)tType - 1].hasGameObjectAnimation)
            {
                if(gameStateDependentTileArray[(int)tType - 1].liveAnimatedGORef.ContainsKey(cellPos))
                {
                    GameObject go = gameStateDependentTileArray[(int)tType - 1].liveAnimatedGORef[cellPos];
                    Destroy(go);
                    gameStateDependentTileArray[(int)tType - 1].liveAnimatedGORef.Remove(cellPos);
                }
            }
            if(gameStateDependentTileArray[(int)tType - 1].hasGhost)
            {
                gameStateDependentTileArray[(int)tType - 1].ghosttileMap.SetTile(cellPos, null);
            }
            if (gameStateDependentTileArray[(int)tType - 1].cereberustileToggle)
            {
                switch (tType)
                {
                    case EnumData.TileType.LeftCereberusHead:
                        worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Left);
                        fireCellPos = grid.WorldToCell(worldPos);
                        fire = GetFireObject(fireCellPos, FaceDirection.Left);
                        if (fire != null)
                        {
                            Destroy(fire);
                        }
                        break;
                    case EnumData.TileType.RightCereberusHead:
                        worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Right);
                        fireCellPos = grid.WorldToCell(worldPos);
                        fire = GetFireObject(fireCellPos, FaceDirection.Right);
                        if (fire != null)
                        {
                            Destroy(fire);
                        }
                        break;
                    case EnumData.TileType.UpCereberusHead:
                        worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Up);
                        fireCellPos = grid.WorldToCell(worldPos);
                        fire = GetFireObject(fireCellPos, FaceDirection.Up);
                        if (fire != null)
                        {
                            Destroy(fire);
                        }
                        break;
                    case EnumData.TileType.DownCereberusHead:
                        worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Down);
                        fireCellPos = grid.WorldToCell(worldPos);
                        fire = GetFireObject(fireCellPos, FaceDirection.Down);
                        if (fire != null)
                        {
                            Destroy(fire);
                        }
                        break;
                }
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
                    case EnumData.TileType.RightCereberusHead:
                        worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Right);
                        if (!IsCellBlockedForPetrifiedUnitMotionAtPos(grid.WorldToCell(worldPos)) || IsCellContainingMirrorAtPos(grid.WorldToCell(worldPos)))
                        {
                            Instantiate(rightFire, worldPos, Quaternion.identity);
                            gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tile);
                        }
                        else
                        {
                            gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tileOff);
                        }
                        break;
                    case EnumData.TileType.LeftCereberusHead:
                        worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Left);
                        if (!IsCellBlockedForPetrifiedUnitMotionAtPos(grid.WorldToCell(worldPos)) || IsCellContainingMirrorAtPos(grid.WorldToCell(worldPos)))
                        {
                            Instantiate(leftFire, worldPos, Quaternion.identity);
                            gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tile);
                        }
                        else
                        {
                            gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tileOff);
                        }
                        break;
                    case EnumData.TileType.DownCereberusHead:
                        worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Down);
                        if (!IsCellBlockedForPetrifiedUnitMotionAtPos(grid.WorldToCell(worldPos)) || IsCellContainingMirrorAtPos(grid.WorldToCell(worldPos)))
                        {
                            Instantiate(downFire, worldPos, Quaternion.identity);
                            gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tile);
                        }
                        else
                        {
                            gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tileOff);
                        }
                        break;
                    case EnumData.TileType.UpCereberusHead:
                        worldPos = cellToworld(cellPos) + GetFacingDirectionOffsetVector3(FaceDirection.Up);
                        if (!IsCellBlockedForPetrifiedUnitMotionAtPos(grid.WorldToCell(worldPos)) || IsCellContainingMirrorAtPos(grid.WorldToCell(worldPos)))
                        {
                            Instantiate(upFire, worldPos, Quaternion.identity);
                            gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tile);
                        }
                        else
                        {
                            gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tileOff);
                        }
                        break;
                }
            }
            else if (gameStateDependentTileArray[(int)tType - 1].initialVectorToTileMapper.ContainsKey(cellPos))
            {
                gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].initialVectorToTileMapper[cellPos]);
            }
            else if (gameStateDependentTileArray[(int)tType - 1].multipleTileGraphic)
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
                gameStateDependentTileArray[(int)tType - 1].tileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tile);
                if(gameStateDependentTileArray[(int)tType - 1].hasGameObjectAnimation)
                {
                    if(!gameStateDependentTileArray[(int)tType - 1].liveAnimatedGORef.ContainsKey(cellPos))
                    {
                        GameObject go = Instantiate(gameStateDependentTileArray[(int)tType - 1].animatedGO);
                        go.transform.position = cellToworld(cellPos);
                        gameStateDependentTileArray[(int)tType - 1].liveAnimatedGORef.Add(cellPos, go);
                    }
                    else
                    {
                        Debug.LogError("Already contains animation on the cell");
                    }
                }
                if (gameStateDependentTileArray[(int)tType - 1].hasGhost)
                {
                    gameStateDependentTileArray[(int)tType - 1].ghosttileMap.SetTile(cellPos, gameStateDependentTileArray[(int)tType - 1].tile);
                }
            }
            if (gameStateDependentTileArray[(int)tType - 1].tilemapCollider2D != null)
            {
                if (!gameStateDependentTileArray[(int)tType - 1].tilemapCollider2D.enabled)
                {
                    gameStateDependentTileArray[(int)tType - 1].tilemapCollider2D.enabled = true;
                }
            }

            if(gameStateDependentTileArray[(int)tType - 1].hasGhost)
            {
                if (gameStateDependentTileArray[(int)tType - 1].ghosttilemapCollider2D != null)
                {
                    if (!gameStateDependentTileArray[(int)tType - 1].ghosttilemapCollider2D.enabled)
                    {
                        gameStateDependentTileArray[(int)tType - 1].ghosttilemapCollider2D.enabled = true;
                    }
                }
            }
        }
    }
    public bool IsCellContainingMirrorAtPos(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();

            if (td != null && td.tileType == EnumData.TileType.Mirror)
            {
                return true;
            }
        }
        return false;
    }
    public bool HasPetrifiedObject(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
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

    public bool HasLeader(Vector3Int cellPosToCheckFor,int leaderId)
    {
        if(leaderId==0)
        {
            return false;
        }
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            Actor actor = hit2DArr[i].collider.gameObject.GetComponent<Actor>();
            if (actor != null && actor.ownerId == leaderId)
            {
                return true;
            }
        }
        return false;
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
    public bool IsCellBlockedBySpawnJar(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && td.tileType == EnumData.TileType.SpawnJar)
            {
                return true;
            }
        }
        return false;
    }

    public List<TileData>GetAllTileDataInCellPos(Vector3Int cellPos)
    {
        List<TileData> tileDatas = new List<TileData>();
        Vector3 objectPosition = cellToworld(cellPos);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if(td!=null)
            {
                tileDatas.Add(td);
            }
        }
        return tileDatas;
    }

    public void SolidifyTiles(List<Vector3Int>cellList)
    {
        foreach (Vector3Int cell in cellList)
        {
            List<TileData> tileDatas = GetAllTileDataInCellPos(cell);
            
            foreach (TileData item in tileDatas)
            {
                if(item.solidifyTile)
                {
                    SetTile(cell, EnumData.TileType.Solid,true,false);
                    break;
                }
            }
        }
    }

    public void NormaliseTiles(List<Vector3Int> cellList)
    {
        foreach (Vector3Int cell in cellList)
        {
            List<TileData> tileDatas = GetAllTileDataInCellPos(cell);
            foreach (TileData item in tileDatas)
            {
                if (item.solidifyTile)
                {
                    SetTile(cell, EnumData.TileType.Solid, false, false);
                    break;
                }
            }
        }
    }

    public bool HasItemOnTiles(Vector3Int cellPosToCheckFor)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheckFor);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            TileData td = hit2DArr[i].collider.gameObject.GetComponent<TileData>();
            if (td != null && td.isItem)
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

    public List<Vector3Int> GetSizeCells(int size,Vector3Int cell)
    {
        if(size<=0)
        {
            return null;
        }
        List<Vector3Int> neighbours = new List<Vector3Int>();
        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                int checkX = cell.x + x;
                int checkY = cell.y + y;

                if (checkX >= xMin && checkX < xMax && checkY >= yMin && checkY < yMax)
                {
                    neighbours.Add(new Vector3Int(checkX, checkY, 0));
                }
            }
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
        //List<Vector3Int> vList = GetAdvancedPlusNeighbours(explodeCell);
        //List<Vector3Int> vList2 = GetCornerNeighbours(explodeCell);
        //vList.AddRange(vList2);
        List<Vector3Int> vList = GetPlusNeighbours(explodeCell);
        Earthquake(hero,vList);
    }

    public void ExplodeCells(Vector3Int cellLightningBoltDropped)
    {
        Explode(cellLightningBoltDropped, GetCornerNeighbours(cellLightningBoltDropped), GetPlusNeighbours(cellLightningBoltDropped));
    }

    void Explode(Vector3Int explodeCell, List<Vector3Int> cornerCell, List<Vector3Int> plusCell)
    {
        GameObject lightningGO = Instantiate(lightning, cellToworld(explodeCell), Quaternion.identity);
        StaticAnimatingTileUtil lightningGOPlusStaticTileAnimating = lightningGO.GetComponent<StaticAnimatingTileUtil>();
        lightningGOPlusStaticTileAnimating.Initialise(explodeCell);

        lightningGO.GetComponent<FrameLooper>().PlayOneShotAnimation();
        lightningGO.GetComponent<FrameLooper>().onPlayOneShotAnimation.RemoveAllListeners();
        lightningGO.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
        {
            lightningGOPlusStaticTileAnimating.DestroyObject();

            GameObject bigThunderGO = Instantiate(bigExplosion, cellToworld(explodeCell), Quaternion.identity);
            StaticAnimatingTileUtil bigThunderGOPlusStaticTileAnimating = bigThunderGO.GetComponent<StaticAnimatingTileUtil>();
            bigThunderGOPlusStaticTileAnimating.Initialise(explodeCell);

            bigThunderGO.GetComponent<FrameLooper>().PlayOneShotAnimation();
            bigThunderGO.GetComponent<FrameLooper>().onPlayOneShotAnimation.RemoveAllListeners();
            bigThunderGO.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
            {
                bigThunderGOPlusStaticTileAnimating.DestroyObject();
                for (int i = 0; i < plusCell.Count; i++)
                {
                    GameObject smallThunderGOPlus = Instantiate(smallExplosion, cellToworld(plusCell[i]), Quaternion.identity);
                    StaticAnimatingTileUtil smallThunderGOPlusStaticTileAnimating = smallThunderGOPlus.GetComponent<StaticAnimatingTileUtil>();
                    smallThunderGOPlusStaticTileAnimating.Initialise(plusCell[i]);

                    smallThunderGOPlus.GetComponent<FrameLooper>().PlayOneShotAnimation();
                    smallThunderGOPlus.GetComponent<FrameLooper>().onPlayOneShotAnimation.RemoveAllListeners();
                    if (i == plusCell.Count - 1)
                    {
                        smallThunderGOPlus.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                        {
                            smallThunderGOPlusStaticTileAnimating.DestroyObject();
                            for (int j = 0; j < cornerCell.Count; j++)
                            {
                                GameObject smallThunderGOCorner = Instantiate(smallExplosion, cellToworld(cornerCell[j]), Quaternion.identity);
                                StaticAnimatingTileUtil smallThunderGOCornerStaticTileAnimating = smallThunderGOCorner.GetComponent<StaticAnimatingTileUtil>();
                                smallThunderGOCornerStaticTileAnimating.Initialise(cornerCell[j]);

                                smallThunderGOCorner.GetComponent<FrameLooper>().PlayOneShotAnimation();
                                smallThunderGOCorner.GetComponent<FrameLooper>().onPlayOneShotAnimation.RemoveAllListeners();
                                if (j == cornerCell.Count - 1)
                                {
                                    smallThunderGOCorner.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                                    {
                                        //Debug.Log("Plus generation");
                                        for (int k = 0; k < plusCell.Count; k++)
                                        {
                                            GameObject smallThunderGOPlusFast = Instantiate(smallExplosion, cellToworld(plusCell[k]), Quaternion.identity);
                                            StaticAnimatingTileUtil smallThunderGOPlusFastStaticTileAnimating = smallThunderGOPlusFast.GetComponent<StaticAnimatingTileUtil>();
                                            smallThunderGOPlusFastStaticTileAnimating.Initialise(plusCell[k]);

                                            smallThunderGOPlusFast.GetComponent<FrameLooper>().animationDuration /= 2f;
                                            smallThunderGOPlusFast.GetComponent<FrameLooper>().PlayOneShotAnimation();
                                            smallThunderGOPlusFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.RemoveAllListeners();

                                            if (k == plusCell.Count - 1)
                                            {
                                                smallThunderGOPlusFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                                                {
                                                    for (int l = 0; l < cornerCell.Count; l++)
                                                    {
                                                        GameObject smallThunderGOCornerFast = Instantiate(smallExplosion, cellToworld(cornerCell[l]), Quaternion.identity);
                                                        StaticAnimatingTileUtil smallThunderGOCornerFastStaticTileAnimating = smallThunderGOCornerFast.GetComponent<StaticAnimatingTileUtil>();
                                                        smallThunderGOCornerFastStaticTileAnimating.Initialise(cornerCell[l]);

                                                        smallThunderGOCornerFast.GetComponent<FrameLooper>().animationDuration /= 2f;
                                                        smallThunderGOCornerFast.GetComponent<FrameLooper>().PlayOneShotAnimation();
                                                        smallThunderGOCornerFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.RemoveAllListeners();
                                                        if (l == cornerCell.Count - 1)
                                                        {
                                                            smallThunderGOCornerFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                                                            {
                                                                for (int m = 0; m < plusCell.Count; m++)
                                                                {
                                                                    GameObject smallThunderGOPlusDoubleFast = Instantiate(smallExplosion, cellToworld(plusCell[m]), Quaternion.identity);
                                                                    StaticAnimatingTileUtil smallThunderGOPlusDoubleFastStaticTileAnimating = smallThunderGOPlusDoubleFast.GetComponent<StaticAnimatingTileUtil>();
                                                                    smallThunderGOPlusDoubleFastStaticTileAnimating.Initialise(plusCell[m]);

                                                                    smallThunderGOPlusDoubleFast.GetComponent<FrameLooper>().animationDuration /= 4f;
                                                                    smallThunderGOPlusDoubleFast.GetComponent<FrameLooper>().PlayOneShotAnimation();
                                                                    smallThunderGOPlusDoubleFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.RemoveAllListeners();
                                                                    if (m == plusCell.Count - 1)
                                                                    {
                                                                        smallThunderGOPlusDoubleFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                                                                        {
                                                                            for (int n = 0; n < cornerCell.Count; n++)
                                                                            {
                                                                                GameObject smallThunderGOCornerDoubleFast = Instantiate(smallExplosion, cellToworld(cornerCell[n]), Quaternion.identity);
                                                                                StaticAnimatingTileUtil smallThunderGOCornerDoubleFastStaticTileAnimating = smallThunderGOCornerDoubleFast.GetComponent<StaticAnimatingTileUtil>();
                                                                                smallThunderGOCornerDoubleFastStaticTileAnimating.Initialise(cornerCell[n]);

                                                                                smallThunderGOCornerDoubleFast.GetComponent<FrameLooper>().animationDuration /= 4f;
                                                                                smallThunderGOCornerDoubleFast.GetComponent<FrameLooper>().PlayOneShotAnimation();
                                                                                smallThunderGOCornerDoubleFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.RemoveAllListeners();
                                                                                if (n == cornerCell.Count - 1)
                                                                                {
                                                                                    smallThunderGOCornerDoubleFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                                                                                    {
                                                                                        for (int o = 0; o < plusCell.Count; o++)
                                                                                        {
                                                                                            GameObject smallThunderGOPlusFourXFast = Instantiate(smallExplosion, cellToworld(plusCell[o]), Quaternion.identity);
                                                                                            StaticAnimatingTileUtil smallThunderGOPlusFourXFastStaticTileAnimating = smallThunderGOPlusFourXFast.GetComponent<StaticAnimatingTileUtil>();
                                                                                            smallThunderGOPlusFourXFastStaticTileAnimating.Initialise(plusCell[o]);

                                                                                            smallThunderGOPlusFourXFast.GetComponent<FrameLooper>().animationDuration /= 8f;
                                                                                            smallThunderGOPlusFourXFast.GetComponent<FrameLooper>().PlayOneShotAnimation();
                                                                                            smallThunderGOPlusFourXFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.RemoveAllListeners();
                                                                                            smallThunderGOPlusFourXFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                                                                                            {
                                                                                                smallThunderGOPlusFourXFastStaticTileAnimating.DestroyObject();
                                                                                            });
                                                                                        }
                                                                                        smallThunderGOCornerDoubleFastStaticTileAnimating.DestroyObject();
                                                                                    });
                                                                                }
                                                                                else
                                                                                {
                                                                                    smallThunderGOCornerDoubleFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                                                                                    {
                                                                                        smallThunderGOCornerDoubleFastStaticTileAnimating.DestroyObject();
                                                                                    });
                                                                                }
                                                                            }
                                                                            smallThunderGOPlusDoubleFastStaticTileAnimating.DestroyObject();
                                                                        });
                                                                    }
                                                                    else
                                                                    {
                                                                        smallThunderGOPlusDoubleFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                                                                        {
                                                                            smallThunderGOPlusDoubleFastStaticTileAnimating.DestroyObject();
                                                                        });
                                                                    }
                                                                }
                                                                smallThunderGOCornerFastStaticTileAnimating.DestroyObject();
                                                            });
                                                        }
                                                        else
                                                        {
                                                            smallThunderGOCornerFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                                                            {
                                                                smallThunderGOCornerFastStaticTileAnimating.DestroyObject();
                                                            });
                                                        }
                                                    }
                                                    smallThunderGOPlusFastStaticTileAnimating.DestroyObject();
                                                });
                                            }
                                            else
                                            {
                                                smallThunderGOPlusFast.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                                                {
                                                    smallThunderGOPlusFastStaticTileAnimating.DestroyObject();
                                                });
                                            }

                                        }
                                        smallThunderGOCornerStaticTileAnimating.DestroyObject();
                                    });
                                }
                                else
                                {
                                    smallThunderGOCorner.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                                    {
                                        smallThunderGOCornerStaticTileAnimating.DestroyObject();
                                    });
                                }
                            }
                        });

                    }
                    else
                    {
                        smallThunderGOPlus.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
                        {
                            smallThunderGOPlusStaticTileAnimating.DestroyObject();
                        });
                    }
                }
            });
        });
    }


    void Earthquake(Hero hero,List<Vector3Int> plusCell)
    {
        hero.isInputFreezed = true;
        for (int i = 0; i < plusCell.Count; i++)
        {
            EarthQuake earthQuake = Instantiate(earthquake, cellToworld(plusCell[i]), Quaternion.identity).GetComponent<EarthQuake>();
            earthQuake.InitialiseEarthquake(hero.ownerId);

            StaticAnimatingTileUtil smallThunderGOPlusStaticTileAnimating = earthQuake.GetComponent<StaticAnimatingTileUtil>();

            smallThunderGOPlusStaticTileAnimating.Initialise(plusCell[i]);

            earthQuake.GetComponent<FrameLooper>().PlayOneShotAnimation();
            earthQuake.GetComponent<FrameLooper>().onPlayOneShotAnimation.RemoveAllListeners();
            earthQuake.GetComponent<FrameLooper>().onPlayOneShotAnimation.AddListener(() =>
            {
                smallThunderGOPlusStaticTileAnimating.DestroyObject();
                hero.isInputFreezed = false;
            });
        }
    
    }

    public bool IsCellBlockedForMonsterWithAnotherMonster(Vector3Int cellPosToCheck, Enemy monster)
    {
        Vector3 objectPosition = cellToworld(cellPosToCheck);
        RaycastHit2D[] hit2DArr = Physics2D.BoxCastAll(objectPosition, grid.cellSize * GameConfig.boxCastCellSizePercent, 0, objectPosition, 0);
        for (int i = 0; i < hit2DArr.Length; i++)
        {
            Enemy monsterCollidedWith = hit2DArr[i].collider.gameObject.GetComponent<Enemy>();
            if (monsterCollidedWith != null && monsterCollidedWith.gameObject.GetInstanceID() != monster.gameObject.GetInstanceID())
            {
                return true;
            }
        }
        return false;
    }

    public bool IsCellPointTheNextPointToMoveInForPathFindingAnyMonster(Vector3Int cellPos)
    {
        foreach(KeyValuePair<int,Enemy>kvp in Enemy.enemies)
        {
            if (kvp.Value.followingTarget)
            {
                if (cellPos == kvp.Value.currentMovePointCellPosition)
                {
                    return true;
                }
            }
        }
        return false;
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
    public TileData tileData;
    public TilemapCollider2D tilemapCollider2D;
    public Tilemap ghosttileMap;
    public TileData ghosttileData;
    public TilemapCollider2D ghosttilemapCollider2D;
    public GameObject animatedGO;
    public Dictionary<Vector3Int, GameObject> liveAnimatedGORef;
    public bool hasGhost;
    public bool hasGameObjectAnimation;
    public bool cereberustileToggle;
    public bool multipleTileGraphic;
    public bool isDarkOnOdd;

    public Dictionary<Vector3Int, Tile> initialVectorToTileMapper;
}