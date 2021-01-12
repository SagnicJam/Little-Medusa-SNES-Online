using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Room
{
    public int RoomId { get; set; }
    public string RoomName { get; set; }
    public Dictionary<string,PlayerInfoData> playerList { get; set; }
}
