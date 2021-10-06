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
    public int timeToStartMatch;
    public List<EnumData.TileType> toNetworkTileType;

    public int snapShotsInOnePacket;
    public int packetHistorySize;

    [Header("Live Data")]
    public int serverWorldSequenceNumber=0;
    private List<WorldUpdate> worldUpdatesToBeSentFromServerToClient = new List<WorldUpdate>();
    private List<PreviousWorldUpdatePacks> previousHistoryForWorldUpdatesToBeSentToServerCollection = new List<PreviousWorldUpdatePacks>();
    public static Dictionary<int, ProjectileData> projectilesDic = new Dictionary<int, ProjectileData>();
    public static Dictionary<int, EnemyData> enemiesDic = new Dictionary<int, EnemyData>();
    public static Dictionary<int, AnimatingStaticTile> animatingStaticTileDic = new Dictionary<int, AnimatingStaticTile>();
    public EnumData.GameState currentGameState;

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
        worldUpdatesToBeSentFromServerToClient.Add(new WorldUpdate(serverWorldSequenceNumber, worldGridItemList.ToArray(), gameData, new Dictionary<int, ProjectileData>(projectilesDic), new Dictionary<int, EnemyData>(enemiesDic), new Dictionary<int, AnimatingStaticTile>(animatingStaticTileDic)));

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
    public Dictionary<int,ProjectileData> projectileDatas;
    public Dictionary<int,EnemyData> enemyDatas;
    public Dictionary<int,AnimatingStaticTile> animatingTileDatas;

    public WorldUpdate(int sequenceNumber, WorldGridItem[] worldGridItems,GameData gameData, Dictionary<int, ProjectileData> projectileDatas,Dictionary<int,EnemyData> enemyDatas, Dictionary<int, AnimatingStaticTile> animatingTileDatas)
    {
        this.sequenceNumber = sequenceNumber;
        this.worldGridItems = worldGridItems;
        this.gameData = gameData;
        this.projectileDatas = projectileDatas;
        this.enemyDatas = enemyDatas;
        this.animatingTileDatas = animatingTileDatas;
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

    public WorldGridItem(int tileType, List<Vector3Int> updatedCellPositionList)
    {
        this.tileType = tileType;
        this.updatedCellGridWorldPositionList = updatedCellPositionList;
    }
}
