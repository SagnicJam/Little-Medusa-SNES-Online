using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public struct RoomDto
    {
        public string RoomName;
        public int MinRoomSize;
        public int MaxRoomSize;
        public RoomDto(string roomName, int minRoomSize, int maxRoomSize)
        {
            RoomName = roomName;
            MinRoomSize = minRoomSize;
            MaxRoomSize = maxRoomSize;
        }
    }
}