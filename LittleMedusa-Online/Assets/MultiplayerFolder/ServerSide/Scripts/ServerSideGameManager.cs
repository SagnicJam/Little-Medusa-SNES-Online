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

    public ServerMasterController InstantiatePlayer()
    {
        return Instantiate(serverInstancePlayer, Vector3.zero, Quaternion.identity).GetComponent<ServerMasterController>();
    }

    private void FixedUpdate()
    {
        serverWorldSequenceNumber++;

        //////Debug.Log("<color=blue>inputsequence </color>"+ playerMovingCommandSequenceNumber + "<color=blue>inputs </color> "+ inputs[0]+" "+inputs[1]+" "+inputs[2]+" "+inputs[3]);

        List<WorldGridItem> worldGridItemList = new List<WorldGridItem>();

        for(int i=0;i<toNetworkTileType.Count;i++)
        {
            List<Vector3Int> positionsOfTile = GridManager.instance.GetAllPositionForTileMap(GridManager.instance.gameStateDependentTileArray[(int)toNetworkTileType[i]-1].tileMap);
            WorldGridItem worldGridItem = new WorldGridItem((int)toNetworkTileType[i], positionsOfTile);
            worldGridItemList.Add(worldGridItem);
        }

        worldUpdatesToBeSentFromServerToClient.Add(new WorldUpdate(serverWorldSequenceNumber, worldGridItemList.ToArray()));

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

    public WorldUpdate(int sequenceNumber, WorldGridItem[] worldItems)
    {
        this.sequenceNumber = sequenceNumber;
        this.worldGridItems = worldItems;
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