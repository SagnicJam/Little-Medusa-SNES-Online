using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MultiplayerManager : MonoBehaviour
{
    public Text matchStartTimeText;
    public Transform Canvas;

    public ServerSideGameManager serverSideGameManager;
    public ClientSideGameManager clientSideGameManager;
    public GameObject signalRGO;
    public GameObject lobbyGO;
    public GameObject roomListGO;
    public GameObject playerListGO;
    public GameObject enterNameGO;
    public Loader loader;


    public CharacterSelectionScreen characterSelectionScreen;

    public static MultiplayerManager instance;

    public bool isServer;

    public int serverPort;


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
            InitialiseEnterNameScreen();
            //Instantiate(clientSideGameManager);
            //Instantiate(characterSelectionScreen, Canvas, false);
        }
    }

    GameObject enterNameGORef;
    GameObject lobbyGORef;
    GameObject roomListRef;
    GameObject playerListRef;
    GameObject signalRGORef;

    public void OnMatchBegun(Match match)
    {
        Debug.Log("On Match begun on client");
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

    public void EstablishServerConnection(int port)
    {
        Debug.Log("Establishing server side signalR "+port);
        signalRGORef = Instantiate(signalRGO, Canvas, false);
        signalRGORef.GetComponent<SignalRCoreConnect>().ServerConnectSignalR("server-instance", port, OnServerSignalRConnectionEstablished);
    }

    void OnServerSignalRConnectionEstablished(int port)
    {
        SignalRCoreConnect.instance.StartMatch(port);
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

    public void DestroyLobbyScreen()
    {
        Destroy(lobbyGORef);
    }
}
