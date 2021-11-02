using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RoomDto
{
    public string RoomName;
    public int RoomSize;
    public RoomDto(string roomName,int roomSize)
    {
        RoomName = roomName;
        RoomSize = roomSize;
    }
}
