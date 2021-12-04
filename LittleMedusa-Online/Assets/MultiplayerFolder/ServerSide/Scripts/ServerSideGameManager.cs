using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSideGameManager : MonoBehaviour
{
    public static ServerSideGameManager instance;

    [Header("Tweak Params")]
    public List<Vector3> spawnPositions;
    public int worldDestructionTickRate;
    public int maxItemCount;
    public int itemSpawnTickRate;
    public int cereberausHeadSwitchTickRate;
    public int timeToStartMatch;
    public int stopWorldDestructionTimeCount;
    public List<EnumData.TileType> toNetworkTileType;
    public List<EnumData.TileType> itemTilesType;

    public int snapShotsInOnePacket;
    public int packetHistorySize;


    [Header("Live Data")]
    public bool worldDestructionStop;
    public int liveWorldDestructionTickCountTemp;
    public int liveStopWorldDestructionTimeCount;
    public int liveItemSpawnCountTemp;
    public int liveCereberausHeadSwitchTickRateTemp;
    public int serverWorldSequenceNumber=0;
    private List<WorldUpdate> worldUpdatesToBeSentFromServerToClient = new List<WorldUpdate>();
    private List<PreviousWorldUpdatePacks> previousHistoryForWorldUpdatesToBeSentToServerCollection = new List<PreviousWorldUpdatePacks>();
    public static Dictionary<int, ProjectileData> projectilesDic = new Dictionary<int, ProjectileData>();
    public static Dictionary<int, EnemyData> enemiesDic = new Dictionary<int, EnemyData>();
    public static Dictionary<int, AnimatingStaticTile> animatingStaticTileDic = new Dictionary<int, AnimatingStaticTile>();
    public EnumData.GameState currentGameState;
    public int xWorldDeathSize = 0;
    public int yWorldDeathSize = 0;

    [Header("Unit Template")]
    public GameObject serverInstancePlayer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object");
            Destroy(this);
        }
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        if(MultiplayerManager.instance.isDebug)
        {
            MultiplayerManager.instance.serverPort = 13;
            MultiplayerManager.instance.matchBeginDto = new MatchBeginDto
            {
                matchId = 13,
                matchConditionDto = new MatchConditionDto
                {
                    enemy = 0,
                    enemyCount = 0
                }
            };
            Server.Start(10, MultiplayerManager.instance.matchBeginDto);
        }
        else
        {
            string[] arguments = Environment.GetCommandLineArgs();

            MultiplayerManager.instance.matchBeginDto = JsonMapper.ToObject<MatchBeginDto>(arguments[1]);
            
            MultiplayerManager.instance.serverPort = MultiplayerManager.instance.matchBeginDto.matchId;

            spawnPositions = MultiplayerManager.instance.battleMaps[MultiplayerManager.instance.matchBeginDto.matchConditionDto.map].spawnPoints;
            MultiplayerManager.instance.battleMaps[MultiplayerManager.instance.matchBeginDto.matchConditionDto.map].battleMapsGO.SetActive(true);

            Server.Start(10, MultiplayerManager.instance.matchBeginDto);
        }
    }

    public ServerMasterController InstantiatePlayer(int hero)
    {
        ServerMasterController serverMasterController = Instantiate(serverInstancePlayer, Vector3.zero, Quaternion.identity).GetComponent<ServerMasterController>();
        Hero serverInstanceHero = Instantiate(Resources.Load("Characters/" + ((EnumData.Heroes)hero).ToString() + "/ServerInstance-" + ((EnumData.Heroes)hero).ToString()) as GameObject, serverMasterController.transform, false).GetComponentInChildren<Hero>();
        serverMasterController.serverInstanceHero = serverInstanceHero;
        serverMasterController.serverInstanceHero.hero = hero;
        serverMasterController.serverInstanceHero.inCharacterSelectionScreen = (currentGameState==EnumData.GameState.CharacterSelection);
        serverMasterController.serverInstanceHero.inGame = (currentGameState==EnumData.GameState.Gameplay);
        return serverMasterController;
    }

    void StartMatch()
    {
        currentGameState = EnumData.GameState.Gameplay;
        foreach (KeyValuePair<int, ServerSideClient> kvp in Server.clients)
        {
            if (kvp.Value.serverMasterController != null)
            {
                kvp.Value.serverMasterController.serverInstanceHero.inCharacterSelectionScreen = (currentGameState == EnumData.GameState.CharacterSelection);
                kvp.Value.serverMasterController.serverInstanceHero.inGame = (currentGameState == EnumData.GameState.Gameplay);
            }
        }
        GridManager.instance.enemySpawnner.InitialiseSpawnner(MultiplayerManager.instance.matchBeginDto.matchConditionDto.enemy, MultiplayerManager.instance.matchBeginDto.matchConditionDto.enemyCount);
    }

    private void FixedUpdate()
    {
        serverWorldSequenceNumber++;
        
        DealItemSpawn();
        DealCereberausHeadRotation();
        //DealWorldDestruction();
        DealMatchStartTime();
        //////Debug.Log("<color=blue>inputsequence </color>"+ playerMovingCommandSequenceNumber + "<color=blue>inputs </color> "+ inputs[0]+" "+inputs[1]+" "+inputs[2]+" "+inputs[3]);

        List<WorldGridItem> worldGridItemList = new List<WorldGridItem>();

        for(int i=0;i<toNetworkTileType.Count;i++)
        {
            List<Vector3Int> positionsOfTile = GridManager.instance.GetAllPositionForTileMap(toNetworkTileType[i]);
            //Debug.LogError("TileType: " + toNetworkTileType[i] + " positionsOfTile " + positionsOfTile.Count);
            WorldGridItem worldGridItem = new WorldGridItem((int)toNetworkTileType[i], positionsOfTile);
            worldGridItemList.Add(worldGridItem);
        }


        GameData gameData = new GameData((int)currentGameState, timeToStartMatch);
        worldUpdatesToBeSentFromServerToClient.Add(new WorldUpdate(serverWorldSequenceNumber, worldGridItemList.ToArray(), gameData, new Dictionary<int, ProjectileData>(projectilesDic), new Dictionary<int, EnemyData>(enemiesDic), new Dictionary<int, AnimatingStaticTile>(animatingStaticTileDic), GridManager.instance.portal.portalEntranceDic));

        //Local client sending data
        if (worldUpdatesToBeSentFromServerToClient.Count >= snapShotsInOnePacket)
        {
            if (previousHistoryForWorldUpdatesToBeSentToServerCollection.Count > packetHistorySize)
            {
                previousHistoryForWorldUpdatesToBeSentToServerCollection.RemoveAt(0);
            }

            ServerSend.WorldUpdate(worldUpdatesToBeSentFromServerToClient, previousHistoryForWorldUpdatesToBeSentToServerCollection);

            previousHistoryForWorldUpdatesToBeSentToServerCollection.Add(new PreviousWorldUpdatePacks(worldUpdatesToBeSentFromServerToClient.ToArray()));

            worldUpdatesToBeSentFromServerToClient.Clear();

            //Debug.Log("<color=red>--------------------------------------------------------------------</color>");
        }
    }

    void DealCereberausHeadRotation()
    {
        if (currentGameState == EnumData.GameState.Gameplay)
        {
            if (liveCereberausHeadSwitchTickRateTemp <= cereberausHeadSwitchTickRate)
            {
                liveCereberausHeadSwitchTickRateTemp++;
            }
            else
            {
                liveCereberausHeadSwitchTickRateTemp = 0;
                RotateCereberausHeads();
            }
        }
    }

    void DealItemSpawn()
    {
        if (currentGameState == EnumData.GameState.Gameplay)
        {
            if (liveItemSpawnCountTemp <= itemSpawnTickRate)
            {
                liveItemSpawnCountTemp++;
            }
            else
            {
                liveItemSpawnCountTemp = 0;
                SpawnItem();
            }
        }
    }

    void RotateCereberausHeads()
    {
        List<Vector3Int> upcereberuasHead = GridManager.instance.GetAllPositionForTileMap(EnumData.TileType.UpCereberusHead);
        List<Vector3Int> downcereberuasHead = GridManager.instance.GetAllPositionForTileMap(EnumData.TileType.DownCereberusHead);
        List<Vector3Int> leftcereberuasHead = GridManager.instance.GetAllPositionForTileMap(EnumData.TileType.LeftCereberusHead);
        List<Vector3Int> rightcereberuasHead = GridManager.instance.GetAllPositionForTileMap(EnumData.TileType.RightCereberusHead);

        foreach (Vector3Int item in upcereberuasHead)
        {
            GridManager.instance.ReplaceTileWith(item,EnumData.TileType.UpCereberusHead,EnumData.TileType.RightCereberusHead,false);
        }

        foreach (Vector3Int item in downcereberuasHead)
        {
            GridManager.instance.ReplaceTileWith(item, EnumData.TileType.DownCereberusHead, EnumData.TileType.LeftCereberusHead, false);
        }

        foreach (Vector3Int item in leftcereberuasHead)
        {
            GridManager.instance.ReplaceTileWith(item, EnumData.TileType.LeftCereberusHead, EnumData.TileType.UpCereberusHead, false);
        }

        foreach (Vector3Int item in rightcereberuasHead)
        {
            GridManager.instance.ReplaceTileWith(item, EnumData.TileType.RightCereberusHead, EnumData.TileType.DownCereberusHead, false);
        }
    }

    void SpawnItem()
    {
        List<Vector3Int> cellPosOfItemSpawners = GridManager.instance.GetAllPositionForTileMap(EnumData.TileType.ItemSpawner);

        List<Vector3Int> cellPosForItemTiles = new List<Vector3Int>(cellPosOfItemSpawners);

        foreach (Vector3Int item in cellPosOfItemSpawners)
        {
            if (GridManager.instance.HasItemOnTiles(item))
            {
                cellPosForItemTiles.Remove(item);
            }
        }

        if(cellPosForItemTiles.Count>0 && cellPosForItemTiles.Count <= maxItemCount)
        {
            //GridManager.instance.SetTile(
            //    cellPosForItemTiles[UnityEngine.Random.Range(0, cellPosForItemTiles.Count)]
            //, itemTilesType[UnityEngine.Random.Range(0, itemTilesType.Count)],
            //true,
            //false);

            GridManager.instance.SetTile(
               cellPosForItemTiles[UnityEngine.Random.Range(0, cellPosForItemTiles.Count)]
           , EnumData.TileType.PortalItem,
           true,
           false);
        }
    }

    public void StopWorldDestruction()
    {
        liveStopWorldDestructionTimeCount = stopWorldDestructionTimeCount;
        worldDestructionStop = true;
    }

    void DealWorldDestruction()
    {
        if(currentGameState==EnumData.GameState.Gameplay)
        {
            if(worldDestructionStop)
            {
                if (liveStopWorldDestructionTimeCount > 0)
                {
                    liveStopWorldDestructionTimeCount--;
                }
                else
                {
                    liveStopWorldDestructionTimeCount = 0;
                    worldDestructionStop = false;
                }
            }
            else
            {
                if (liveWorldDestructionTickCountTemp <= worldDestructionTickRate)
                {
                    liveWorldDestructionTickCountTemp++;
                }
                else
                {
                    liveWorldDestructionTickCountTemp = 0;
                    UpdateDeathTiles();
                }
            }
        }
    }

    void DealMatchStartTime()
    {
        if(timeToStartMatch>0)
        {
            timeToStartMatch--;
            MultiplayerManager.instance.matchStartTimeText.text = Mathf.RoundToInt(timeToStartMatch*Time.fixedDeltaTime).ToString();
            currentGameState = EnumData.GameState.CharacterSelection;
        }
        else
        {
            if(currentGameState==EnumData.GameState.CharacterSelection)
            {
                StartMatch();
            }
        }
    }
    
    

    void UpdateToDeathTilesAtPosition(Vector3Int cellPos)
    {
        List<TileData>tileDatas = GridManager.instance.GetAllTileDataInCellPos(cellPos);
        foreach (TileData item in tileDatas)
        {
            GridManager.instance.SetTile(cellPos,item.tileType,false,false);
        }

        GridManager.instance.SetTile(cellPos,EnumData.TileType.VoidDeathTiles,true,false);
    }

    void UpdateDeathTiles()
    {
        if (GridManager.instance.xMin + xWorldDeathSize < GridManager.instance.xMax - xWorldDeathSize && GridManager.instance.yMin + yWorldDeathSize < GridManager.instance.yMax - yWorldDeathSize)
        {
            for (int j = GridManager.instance.yMin + yWorldDeathSize; j <= GridManager.instance.yMax - yWorldDeathSize; j++)
            {
                UpdateToDeathTilesAtPosition(new Vector3Int(GridManager.instance.xMin + xWorldDeathSize, j, 0));
            }
            for (int j = GridManager.instance.yMin + yWorldDeathSize; j <= GridManager.instance.yMax - yWorldDeathSize; j++)
            {
                UpdateToDeathTilesAtPosition(new Vector3Int(GridManager.instance.xMax - xWorldDeathSize, j, 0));
            }
            for (int j = GridManager.instance.xMin + xWorldDeathSize; j <= GridManager.instance.xMax - xWorldDeathSize; j++)
            {
                UpdateToDeathTilesAtPosition(new Vector3Int(j, GridManager.instance.yMin + yWorldDeathSize, 0));
            }
            for (int j = GridManager.instance.xMin + xWorldDeathSize; j <= GridManager.instance.xMax - xWorldDeathSize; j++)
            {
                UpdateToDeathTilesAtPosition(new Vector3Int(j, GridManager.instance.yMax - yWorldDeathSize, 0));
            }
            if (GridManager.instance.xMin + xWorldDeathSize < GridManager.instance.xMax - xWorldDeathSize)
            {
                xWorldDeathSize++;
            }
            if (GridManager.instance.yMin + yWorldDeathSize < GridManager.instance.yMax - yWorldDeathSize)
            {
                yWorldDeathSize++;
            }
        }
        else if (GridManager.instance.xMin + xWorldDeathSize < GridManager.instance.xMax - xWorldDeathSize || GridManager.instance.yMin + yWorldDeathSize < GridManager.instance.yMax - yWorldDeathSize)
        {
            if (GridManager.instance.xMin + xWorldDeathSize < GridManager.instance.xMax - xWorldDeathSize)
            {
                UpdateToDeathTilesAtPosition(new Vector3Int(GridManager.instance.xMin + xWorldDeathSize, GridManager.instance.yMin + yWorldDeathSize, 0));
                UpdateToDeathTilesAtPosition(new Vector3Int(GridManager.instance.xMax - xWorldDeathSize, GridManager.instance.yMax - yWorldDeathSize, 0));
                xWorldDeathSize++;
            }

            if (GridManager.instance.yMin + yWorldDeathSize < GridManager.instance.yMax - yWorldDeathSize)
            {
                UpdateToDeathTilesAtPosition(new Vector3Int(GridManager.instance.xMin + xWorldDeathSize, GridManager.instance.yMin + yWorldDeathSize, 0));
                UpdateToDeathTilesAtPosition(new Vector3Int(GridManager.instance.xMax - xWorldDeathSize, GridManager.instance.yMax - yWorldDeathSize, 0));
                yWorldDeathSize++;
            }
        }
        else if ((GridManager.instance.xMax - GridManager.instance.xMin) > (GridManager.instance.yMax - GridManager.instance.yMin) && GridManager.instance.xMin + xWorldDeathSize == GridManager.instance.xMax - xWorldDeathSize)
        {
            if (GridManager.instance.xMin + xWorldDeathSize == GridManager.instance.xMax - xWorldDeathSize)
            {
                UpdateToDeathTilesAtPosition(new Vector3Int(GridManager.instance.xMin + xWorldDeathSize, GridManager.instance.yMin + yWorldDeathSize, 0));
                UpdateToDeathTilesAtPosition(new Vector3Int(GridManager.instance.xMax - xWorldDeathSize, GridManager.instance.yMax - yWorldDeathSize, 0));
                xWorldDeathSize++;
            }
        }
        else if ((GridManager.instance.xMax - GridManager.instance.xMin) < (GridManager.instance.yMax - GridManager.instance.yMin) && GridManager.instance.yMin + yWorldDeathSize == GridManager.instance.yMax - yWorldDeathSize)
        {
            if (GridManager.instance.xMin + xWorldDeathSize == GridManager.instance.xMax - xWorldDeathSize)
            {
                UpdateToDeathTilesAtPosition(new Vector3Int(GridManager.instance.xMin + xWorldDeathSize, GridManager.instance.yMin + yWorldDeathSize, 0));
                UpdateToDeathTilesAtPosition(new Vector3Int(GridManager.instance.xMax - xWorldDeathSize, GridManager.instance.yMax - yWorldDeathSize, 0));
                yWorldDeathSize++;
            }
        }
        else
        {
            //Debug.LogError("finish");
        }
    }
    private void OnApplicationQuit()
    {
        Debug.LogError("Server has been stopped................");
        Server.Stop();
    }
}
public struct WorldUpdate
{
    public int sequenceNumber;
    public WorldGridItem[] worldGridItems;
    public GameData gameData;
    public Dictionary<Vector3Int, PortalInfo> portalEntranceDic;
    public Dictionary<int,ProjectileData> projectileDatas;
    public Dictionary<int,EnemyData> enemyDatas;
    public Dictionary<int,AnimatingStaticTile> animatingTileDatas;

    public WorldUpdate(int sequenceNumber, WorldGridItem[] worldGridItems,GameData gameData, Dictionary<int, ProjectileData> projectileDatas,Dictionary<int,EnemyData> enemyDatas, Dictionary<int, AnimatingStaticTile> animatingTileDatas, Dictionary<Vector3Int, PortalInfo> portalEntranceDic)
    {
        this.sequenceNumber = sequenceNumber;
        this.worldGridItems = worldGridItems;
        this.gameData = gameData;
        this.projectileDatas = projectileDatas;
        this.enemyDatas = enemyDatas;
        this.animatingTileDatas = animatingTileDatas;
        this.portalEntranceDic = portalEntranceDic;
    }
}

public struct PreviousWorldUpdatePacks
{
    public WorldUpdate[] previousWorldUpdates;

    public PreviousWorldUpdatePacks(WorldUpdate[] previousWorldUpdates)
    {
        this.previousWorldUpdates = previousWorldUpdates;
    }
}

public struct AnimatingStaticTile
{
    public int uid;
    public int tileType;
    public int animationSpriteIndex;
    public Vector3Int pos;

    public AnimatingStaticTile(int uid, int tileType, int animationSpriteIndex, Vector3Int pos)
    {
        this.uid = uid;
        this.tileType = tileType;
        this.animationSpriteIndex = animationSpriteIndex;
        this.pos = pos;
    }
}

public struct GameData
{
    public int gameState;
    public int matchStartTime;

    public GameData(int gameState, int matchStartTime)
    {
        this.gameState = gameState;
        this.matchStartTime = matchStartTime;
    }
}

public struct ProjectileData
{
    public int uid;
    public int projectileType;
    public Vector3 projectilePosition;
    public int faceDirection;

    public ProjectileData(int uid, int projectileType, Vector3 projectilePosition, int faceDirection)
    {
        this.uid = uid;
        this.projectileType = projectileType;
        this.projectilePosition = projectilePosition;
        this.faceDirection = faceDirection;
    }
}


public struct WorldGridItem
{
    public int tileType;
    public List<Vector3Int> updatedCellGridWorldPositionList;

    public WorldGridItem(int tileType, List<Vector3Int> updatedCellGridWorldPositionList)
    {
        this.tileType = tileType;
        this.updatedCellGridWorldPositionList = updatedCellGridWorldPositionList;
    }
}
