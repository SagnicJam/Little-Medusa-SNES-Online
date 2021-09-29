using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MultiplayerManager : MonoBehaviour
{
    public Text matchStartTimeText;
    public Transform Canvas;

    public ServerSideGameManager serverSideGameManager;
    public ClientSideGameManager clientSideGameManager;

    public BattleMaps[] battleMaps;

    public GameObject signalRGO;
    public GameObject lobbyGO;
    public GameObject roomListGO;
    public GameObject playerListGO;
    public GameObject enterNameGO;
    public GameObject matchConditionManagerGO;

    [Header("Live Units")]
    public GameObject enterNameGORef;
    public GameObject lobbyGORef;
    public GameObject roomListRef;
    public GameObject playerListRef;
    public GameObject signalRGORef;
    public GameObject matchConditionManagerGORef;

    public Loader loader;


    public CharacterSelectionScreen characterSelectionScreen;

    public static MultiplayerManager instance;
    public bool isDebug;
    public bool isServer;

    public MatchBeginDto matchBeginDto;
    public int serverPort;

    public string matchOwnerConnectionId;
    public string localPlayerConnectionId;

    private void Awake()
    {
        instance = this;
        
    }

    private void Start()
    {
        if (isServer)
        {
            Instantiate(serverSideGameManager);
        }
        else
        {
            if (isDebug)
            {
                Match debugMatch = new Match();
                debugMatch.MatchID = 13;
                debugMatch.ProcessID = 9156;

                PlayerInfoData playerInfoData = new PlayerInfoData("cZE3aJKSS50lCuXkelhz7Q", "sagnic");

                Dictionary<string, PlayerInfoData> dic = new Dictionary<string, PlayerInfoData>();
                dic.Add("cZE3aJKSS50lCuXkelhz7Q", playerInfoData);

                debugMatch.playerList = dic;
                OnMatchBegun(debugMatch);
            }
            else
            {
                InitialiseEnterNameScreen();
                //Instantiate(clientSideGameManager);
                //Instantiate(characterSelectionScreen, Canvas, false);
            }
        }
    }

    public bool IsMatchOwner()
    {
        return !string.IsNullOrEmpty(matchOwnerConnectionId) && localPlayerConnectionId == matchOwnerConnectionId;
    }

    

    public void OnMatchBegun(Match match)
    {
        Debug.Log(match.MatchID);
        Debug.Log(match.ProcessID);
        foreach(KeyValuePair<string,PlayerInfoData>kvp in match.playerList)
        {
            Debug.Log(kvp.Key+"  "+kvp.Value.connectionId +" "+kvp.Value.Name);
        }

        battleMaps[match.MatchConditionDto.map].battleMapsGO.SetActive(true);

        Debug.Log("On Match begun on client "+JsonUtility.ToJson(match));
        DestroyPlayerList();
        DestroyLobbyScreen();
        serverPort = match.MatchID;

        Instantiate(clientSideGameManager);
        Instantiate(characterSelectionScreen, Canvas, false);
    }

    public void DestroyEnterName()
    {
        Destroy(enterNameGORef);
    }

    public void DestroyPlayerList()
    {
        Destroy(playerListRef);
    }

    public void DestroyRoomList()
    {
        Destroy(roomListRef);
    }

    public void InitialiseEnterNameScreen()
    {
        enterNameGORef = Instantiate(enterNameGO, Canvas, false);
    }

    public void EstablishConnection(string username)
    {
        signalRGORef = Instantiate(signalRGO, Canvas, false);
        signalRGORef.GetComponent<SignalRCoreConnect>().ClientConnectSignalR(username, OnClientConnectedToSignalR);
    }

    public void EstablishServerConnection(MatchBeginDto matchBeginDto)
    {
        Debug.Log("Establishing server side signalR "+ matchBeginDto.matchId);
        signalRGORef = Instantiate(signalRGO, Canvas, false);
        signalRGORef.GetComponent<SignalRCoreConnect>().ServerConnectSignalR("server-instance", matchBeginDto, OnServerSignalRConnectionEstablished);
    }

    void OnServerSignalRConnectionEstablished(MatchBeginDto matchBeginDto)
    {
        SignalRCoreConnect.instance.StartMatch(matchBeginDto);
    }

    void OnClientConnectedToSignalR()
    {
        DestroyEnterName();
        InitialiseLobbyScreen();
    }

    public void InitialisePlayerList(Room room)
    {
        playerListRef = Instantiate(playerListGO, Canvas, false);
        playerListRef.GetComponent<PlayerList>().DisplayAllPlayerList(room);
    }

    public void InitialiseLobbyScreen()
    {
        lobbyGORef = Instantiate(lobbyGO, Canvas, false);
    }

    public void InitialiseRoomList(Lobby lobby)
    {
        roomListRef = Instantiate(roomListGO, Canvas, false);
        roomListRef.GetComponent<RoomList>().DisplayRoomList(lobby);
    }

    public void InstantiateMatchOptions(int roomId)
    {
        matchConditionManagerGORef=Instantiate(matchConditionManagerGO, Canvas, false);
        matchConditionManagerGORef.GetComponent<MatchConditionManager>().Initialise(roomId);
    }

    public void DestroyMatchOptions()
    {
        Destroy(matchConditionManagerGORef);
    }

    public void DestroyLobbyScreen()
    {
        Destroy(lobbyGORef);
    }
}
[Serializable]
public struct BattleMaps
{
    public EnumData.BattleRoyaleMaps battleRoyaleMapType;
    public GameObject battleMapsGO;
    public List<Vector3> spawnPoints;
}