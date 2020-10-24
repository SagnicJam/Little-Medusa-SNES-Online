using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ClientSideGameManager : MonoBehaviour
{
    public static ClientSideGameManager instance;

    [Header("Tweak paramters")]
    public List<EnumData.TileType> toNetworkTileType;
    public int idealSnapshotBufferCount;
    public int maxedOutSnapshotBufferCount;

    [Header("Unit Templates")]
    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    [Header("Live Data")]
    public ProcessMode currentWorldStateUpdateProcessingMode;
    public int serverWorldSequenceNumberProcessed=0;
    public int snapShotBufferSize = 0;
    public int lastSequenceNumberReceivedViaUDPOfWorldUpdate = 0;
    private WorldUpdate latestWorldUpdate;
    private Dictionary<int, WorldUpdate> worldUpdatesFromServerToClientDic = new Dictionary<int, WorldUpdate>();

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public int dicCount;

    private void Update()
    {
        dicCount = worldUpdatesFromServerToClientDic.Count;
    }

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

    public void SpawnPlayer(int id,string username,PlayerStateUpdates playerStateUpdates)
    {
        GameObject player;
        if(id == Client.instance.myID)
        {
            player = Instantiate(localPlayerPrefab,Vector3.zero,Quaternion.identity);
            player.GetComponent<PlayerManager>().Initialise(id,username, playerStateUpdates, true);
        }
        else
        {
            player = Instantiate(playerPrefab,Vector3.zero, Quaternion.identity);
            player.GetComponent<PlayerManager>().Initialise(id, username, playerStateUpdates, false);
        }
        players.Add(id,player.GetComponent<PlayerManager>());
    }

    public bool isWorldInitialised;

    public void SpawnWorldGridElements(WorldUpdate worldUpdates)
    {
        UpdateWorldStart(worldUpdates);
        serverWorldSequenceNumberProcessed = worldUpdates.sequenceNumber;
        isWorldInitialised = true;
        Debug.Log("<color=blue>spawned grid world serverWorldSequenceNumberProcessed. </color>"+ serverWorldSequenceNumberProcessed);
    }

    public void AccumulateWorldUpdatesToBePlayedOnClientFromServer(WorldUpdate worldUpdates)
    {
        if(!isWorldInitialised)
        {
            return;
        }
        if (worldUpdates.sequenceNumber > serverWorldSequenceNumberProcessed)
        {
            WorldUpdate dataPackage;
            if (worldUpdatesFromServerToClientDic.TryGetValue(worldUpdates.sequenceNumber, out dataPackage))
            {
                //Debug.Log("<color=orange>AccumulateWorldUpdatesToBePlayedOnClientFromServer dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNumber);
            }
            else
            {
                //Debug.Log("<color=green>AccumulateWorldUpdatesToBePlayedOnClientFromServer Added successfully to processing buffer dic </color>" + worldUpdates.sequenceNumber + "  processed sequence number: " + worldUpdates.sequenceNumber);
                worldUpdatesFromServerToClientDic.Add(worldUpdates.sequenceNumber, worldUpdates);
            }
        }
        else
        {
            //Debug.Log("<color=red>AccumulateWorldUpdatesToBePlayedOnClientFromServer Already processed this sequence no </color>" + serverWorldSequenceNumberProcessed);
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < (int)currentWorldStateUpdateProcessingMode; i++)
        {
            WorldUpdate worldUpdatePackageCorrespondingToSeq;


            if (worldUpdatesFromServerToClientDic.TryGetValue(serverWorldSequenceNumberProcessed + 1, out worldUpdatePackageCorrespondingToSeq))
            {
                worldUpdatesFromServerToClientDic.Remove(worldUpdatePackageCorrespondingToSeq.sequenceNumber);

                UpdateWorld(worldUpdatePackageCorrespondingToSeq);

                serverWorldSequenceNumberProcessed = worldUpdatePackageCorrespondingToSeq.sequenceNumber;
                
            }
            else
            {
                if (latestWorldUpdate.sequenceNumber != 0)
                {
                    //Debug.LogError("Could not find any inputToProcess for  seq: " + (sequenceNumberProcessed + 1));
                    serverWorldSequenceNumberProcessed = serverWorldSequenceNumberProcessed + 1;

                    UpdateWorld(latestWorldUpdate);
                }
            }
        }
        lastSequenceNumberReceivedViaUDPOfWorldUpdate = GetTheLastestSequenceNoInDic();
        snapShotBufferSize = lastSequenceNumberReceivedViaUDPOfWorldUpdate - serverWorldSequenceNumberProcessed;
        UpdateWorldStateUpdatesProcessMode();
    }
    int GetTheLastestSequenceNoInDic()
    {
        int largestInt = 0;
        foreach (KeyValuePair<int, WorldUpdate> kvp in worldUpdatesFromServerToClientDic)
        {
            if (largestInt < kvp.Key)
            {
                largestInt = kvp.Key;
            }
        }
        return largestInt;
    }
    void UpdateWorldStateUpdatesProcessMode()
    {
        if (worldUpdatesFromServerToClientDic.Count == 0)
        {
            currentWorldStateUpdateProcessingMode = ProcessMode.Lazy;
        }
        else if (currentWorldStateUpdateProcessingMode == ProcessMode.Lazy && worldUpdatesFromServerToClientDic.Count >= idealSnapshotBufferCount)
        {
            currentWorldStateUpdateProcessingMode = ProcessMode.Ideal;
        }
        else if (currentWorldStateUpdateProcessingMode == ProcessMode.Ideal && worldUpdatesFromServerToClientDic.Count > maxedOutSnapshotBufferCount)
        {
            currentWorldStateUpdateProcessingMode = ProcessMode.Hyper;
        }
        else if (currentWorldStateUpdateProcessingMode == ProcessMode.Hyper && worldUpdatesFromServerToClientDic.Count <= idealSnapshotBufferCount)
        {
            currentWorldStateUpdateProcessingMode = ProcessMode.Ideal;
        }
    }

    void UpdateWorldStart(WorldUpdate newWorldUpdate)
    {
        for (int i = 0; i < newWorldUpdate.worldGridItems.Length; i++)
        {
            for (int j = 0; j < newWorldUpdate.worldGridItems[i].cellGridWorldPositionList.Count; j++)
            {
                GridManager.instance.SetTile(newWorldUpdate.worldGridItems[i].cellGridWorldPositionList[j], (EnumData.TileType)newWorldUpdate.worldGridItems[i].tileType, true,false);
            }
        }
        latestWorldUpdate = newWorldUpdate;
    }

    void UpdateWorld(WorldUpdate newWorldUpdate)
    {
        if(latestWorldUpdate.worldGridItems!=null)
        {
            for (int i = 0; i < latestWorldUpdate.worldGridItems.Length; i++)
            {
                for (int j = 0; j < latestWorldUpdate.worldGridItems[i].cellGridWorldPositionList.Count; j++)
                {
                    if(!newWorldUpdate.worldGridItems[i].cellGridWorldPositionList.Contains(latestWorldUpdate.worldGridItems[i].cellGridWorldPositionList[j]))
                    {
                        //delete old
                        GridManager.instance.SetTile(latestWorldUpdate.worldGridItems[i].cellGridWorldPositionList[j], (EnumData.TileType)latestWorldUpdate.worldGridItems[i].tileType, false,true);
                    }
                }
            }
            for (int i = 0; i < newWorldUpdate.worldGridItems.Length; i++)
            {
                for (int j = 0; j < newWorldUpdate.worldGridItems[i].cellGridWorldPositionList.Count; j++)
                {
                    if (!latestWorldUpdate.worldGridItems[i].cellGridWorldPositionList.Contains(newWorldUpdate.worldGridItems[i].cellGridWorldPositionList[j]))
                    {
                        //add new
                        GridManager.instance.SetTile(newWorldUpdate.worldGridItems[i].cellGridWorldPositionList[j], (EnumData.TileType)newWorldUpdate.worldGridItems[i].tileType, true,true);
                    }
                }
            }
        }

        

        latestWorldUpdate = newWorldUpdate;
    }
}


