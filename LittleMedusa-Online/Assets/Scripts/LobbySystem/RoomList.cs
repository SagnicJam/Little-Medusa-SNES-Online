using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class RoomList : MonoBehaviour, IRoomList
    {
        public GameObject roomGO;
        public Transform roomGOParent;
        public Dictionary<int, JoinRoom> roomInfoDic = new Dictionary<int, JoinRoom>();

        void OnEnable()
        {
            SignalRCoreConnect.instance._connection.On<Room>(nameof(OnNewRoomAdded), OnNewRoomAdded);
            SignalRCoreConnect.instance._connection.On<Room>(nameof(OnRoomDeleted), OnRoomDeleted);
        }

        void OnDisable()
        {
            SignalRCoreConnect.instance._connection.Remove(nameof(OnNewRoomAdded));
            SignalRCoreConnect.instance._connection.Remove(nameof(OnRoomDeleted));
        }
        public void Close()
        {
            Destroy(gameObject);
        }

        public void OnNewRoomAdded(Room room)
        {
            Debug.Log("OnNewRoomAdded " + room.RoomName + " with min Room Size: " + room.MinRoomSize + " and max room size " + room.MaxRoomSize + " room Id: " + room.RoomId);
            AddNewRoom(room);
        }

        public void OnRoomDeleted(Room room)
        {
            Debug.Log("OnRoomDeleted " + room.RoomName + " with min Room Size: " + room.MinRoomSize + " and max room size " + room.MaxRoomSize);
            RemoveRoom(room);
        }

        public void DisplayRoomList(Lobby lobby)
        {
            for (int i = 0; i < lobby.LobbyData.Count; i++)
            {
                GameObject roomGORef = Instantiate(roomGO, roomGOParent, false);
                JoinRoom joinRoom = roomGORef.GetComponent<JoinRoom>();
                joinRoom.Initialise(lobby.LobbyData[i]);
                roomInfoDic.Add(lobby.LobbyData[i].RoomId, joinRoom);
            }
        }

        void AddNewRoom(Room room)
        {
            if (roomInfoDic.ContainsKey(room.RoomId))
            {
                Debug.LogError("Already containing room id for room: " + room.RoomName + " with key: " + room.RoomId);
            }
            else
            {
                GameObject roomGORef = Instantiate(roomGO, roomGOParent, false);
                JoinRoom joinRoom = roomGORef.GetComponent<JoinRoom>();
                joinRoom.Initialise(room);
                roomInfoDic.Add(room.RoomId, joinRoom);
            }
        }

        void RemoveRoom(Room room)
        {
            if (roomInfoDic.ContainsKey(room.RoomId))
            {
                JoinRoom joinRoom = roomInfoDic[room.RoomId];
                Destroy(joinRoom.gameObject);
                roomInfoDic.Remove(room.RoomId);
            }
            else
            {
                Debug.LogError("No rooms found to remove with name " + room.RoomName + " with key: " + room.RoomId);
            }
        }


    }
}