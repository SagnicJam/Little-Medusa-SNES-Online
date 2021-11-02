using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
public class LobbyScreen : MonoBehaviour,ILobby
{
    public TMP_InputField roomNameText;
    public TMP_InputField roomSizeText;

    public static LobbyScreen instance;

    public int roomSize;

    void Awake()
    {
        instance = this;
        
    }

    public void OpenRoomList()
    {
        GetLobby();
    }

    public void SetCount(TMP_InputField tMP_InputField)
    {
        string inputString = tMP_InputField.text;
        int amount = 0;
        if (int.TryParse(inputString, out amount))
        {
            if (amount <= 4&&amount>0)
            {
                roomSize = amount;
            }
            else if (amount < 1)
            {
                roomSize = 1;
                tMP_InputField.text = 1.ToString();
                Debug.LogError("Cant be smaller than 1");
            }
            else
            {
                roomSize = 4;
                tMP_InputField.text = 4.ToString();
                Debug.LogError("Cant be larger than 4");
            }
        }
        else
        {
            Debug.LogError("Could not parse string");
        }
    }

    public void CreateRoomWithName()
    {
        roomSize=2;
        RoomDto roomDto;
        if (int.TryParse(roomSizeText.text,out roomSize))
        {
            roomDto = new RoomDto(roomNameText.text, roomSize);
        }
        else
        {
            Debug.LogError("default room size set to 2");
            roomDto = new RoomDto(roomNameText.text, 2);
        }
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
        Debug.Log("OnRoom created : "+JsonUtility.ToJson(room));
        MultiplayerManager.instance.InitialisePlayerList(room);
        MultiplayerManager.instance.InstantiateMatchOptions(room.RoomId);
        MultiplayerManager.instance.matchOwnerConnectionId = room.roomOwnerConnectionID;
    }

    void OnRoomLeft(Room room)
    {
        Debug.Log("OnRoom left : "+room.RoomName);
        MultiplayerManager.instance.DestroyPlayerList();
        MultiplayerManager.instance.DestroyMatchOptions();
        MultiplayerManager.instance.matchOwnerConnectionId = null;
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
        MultiplayerManager.instance.matchOwnerConnectionId = room.roomOwnerConnectionID;
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
