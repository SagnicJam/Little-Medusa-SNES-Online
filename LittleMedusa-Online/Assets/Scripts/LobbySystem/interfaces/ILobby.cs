using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace MedusaMultiplayer
{
    public interface ILobby
    {
        Task CreateRoom(RoomDto roomDto);
        Task JoinRoom(Room room);
        Task JoinRandomRoom();
        Task LeaveRoom(Room room);
        Task GetLobby();
    }
}