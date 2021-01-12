using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
public class LobbyScreen : MonoBehaviour,ILobby
{
    public TMP_InputField roomName;

    public static LobbyScreen instance; 

    void Awake()
    {
        instance = this;
        
    }

    public void OpenRoomList()
    {
        GetLobby();
    }

    public void CreateRoomWithName()
    {
        RoomDto roomDto = new RoomDto(roomName.text);
        CreateRoom(roomDto);
    }
     
    public void JoinSpecificRoom(Room room)
    {
        JoinRoom(room);
    }

    public void JoinRandom()
    {
        JoinRandomRoom();
    }

    public async Task CreateRoom(RoomDto roomDto)
    {
        OnWorkDone<Room> onRoomCreated = OnRoomCreated;
        await SignalRCoreConnect.instance.SendAsyncData("CreateRoom",roomDto, onRoomCreated);
    }

    void OnRoomCreated(Room room)
    {
        Debug.Log("OnRoom created : "+room.RoomName);
        MultiplayerManager.instance.InitialisePlayerList(room);
    }

    void OnRoomLeft(Room room)
    {
        Debug.Log("OnRoom left : "+room.RoomName);
        MultiplayerManager.instance.DestroyPlayerList();
    }

    void OnGetLobby(Lobby lobby)
    {
        Debug.Log("On get lobby");
        MultiplayerManager.instance.InitialiseRoomList(lobby);
    }

    void OnJoinedRoom(Room room)
    {
        Debug.Log("OnJoinedRoom");
        MultiplayerManager.instance.InitialisePlayerList(room);
        MultiplayerManager.instance.DestroyRoomList();
    }

    public async Task JoinRandomRoom()
    {
        throw new System.NotImplementedException();
    }

    public async Task JoinRoom(Room room)
    {
        OnWorkDone<Room> OnJoinedRoomSuccess = OnJoinedRoom;
        await SignalRCoreConnect.instance.SendAsyncData("JoinRoom",room, OnJoinedRoomSuccess);
    }

    public async Task GetLobby()
    {
        OnWorkDone<Lobby> OnGetLobbySuccess = OnGetLobby;
        await SignalRCoreConnect.instance.SendAsyncData("GetLobby", OnGetLobbySuccess);
    }

    public async Task LeaveRoom(Room room)
    {
        OnWorkDone<Room> OnRoomLeave = OnRoomLeft;
        await SignalRCoreConnect.instance.SendAsyncData("LeaveRoom", room, OnRoomLeave);
    }
}
