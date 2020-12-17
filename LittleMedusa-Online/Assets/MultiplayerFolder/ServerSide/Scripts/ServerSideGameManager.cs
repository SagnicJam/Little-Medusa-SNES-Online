using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSideGameManager : MonoBehaviour
{
    public static ServerSideGameManager instance;

    [Header("Tweak Params")]
    public List<EnumData.TileType> toNetworkTileType;

    public int snapShotsInOnePacket;
    public int packetHistorySize;

    [Header("Live Data")]
    public int serverWorldSequenceNumber=0;
    private List<WorldUpdate> worldUpdatesToBeSentFromServerToClient = new List<WorldUpdate>();
    private List<PreviousWorldUpdatePacks> previousHistoryForWorldUpdatesToBeSentToServerCollection = new List<PreviousWorldUpdatePacks>();
    public static Dictionary<int, ProjectileData> projectilesDic = new Dictionary<int, ProjectileData>();
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
        Server.Start(10, 23456);
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            currentGameState = EnumData.GameState.Gameplay;
            foreach(KeyValuePair<int,ServerSideClient> kvp in Server.clients)
            {
                if(kvp.Value.serverMasterController!=null)
                {
                    kvp.Value.serverMasterController.serverInstanceHero.inCharacterSelectionScreen = (currentGameState == EnumData.GameState.CharacterSelection);
                    kvp.Value.serverMasterController.serverInstanceHero.inGame = (currentGameState == EnumData.GameState.Gameplay);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        serverWorldSequenceNumber++;

        //////Debug.Log("<color=blue>inputsequence </color>"+ playerMovingCommandSequenceNumber + "<color=blue>inputs </color> "+ inputs[0]+" "+inputs[1]+" "+inputs[2]+" "+inputs[3]);

        List<WorldGridItem> worldGridItemList = new List<WorldGridItem>();

        for(int i=0;i<toNetworkTileType.Count;i++)
        {
            List<Vector3Int> positionsOfTile = GridManager.instance.GetAllPositionForTileMap(toNetworkTileType[i]);
            WorldGridItem worldGridItem = new WorldGridItem((int)toNetworkTileType[i], positionsOfTile);
            worldGridItemList.Add(worldGridItem);
        }
        worldUpdatesToBeSentFromServerToClient.Add(new WorldUpdate(serverWorldSequenceNumber, worldGridItemList.ToArray(), new Dictionary<int, ProjectileData>(projectilesDic)));

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
    public Dictionary<int,ProjectileData> projectileDatas;

    public WorldUpdate(int sequenceNumber, WorldGridItem[] worldItems, Dictionary<int,ProjectileData> projectileDatas)
    {
        this.sequenceNumber = sequenceNumber;
        this.worldGridItems = worldItems;
        this.projectileDatas = projectileDatas;
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

public struct ProjectileData
{
    public int uid;
    public int projectileType;
    public Vector3 projectilePosition;

    public ProjectileData(int uid, int projectileType, Vector3 projectilePosition)
    {
        this.uid = uid;
        this.projectileType = projectileType;
        this.projectilePosition = projectilePosition;
    }
}

public struct WorldGridItem
{
    public int tileType;
    public List<Vector3Int> cellGridWorldPositionList;

    public WorldGridItem(int tileType, List<Vector3Int> cellPositionList)
    {
        this.tileType = tileType;
        this.cellGridWorldPositionList = cellPositionList;
    }
}