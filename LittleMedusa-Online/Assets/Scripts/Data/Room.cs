using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Room
{
    [field:SerializeField]
    public int RoomId { get; set; }
    [field: SerializeField]
    public int RoomSize { get; set; }
    [field: SerializeField]
    public string RoomName { get; set; }
    [field: SerializeField]
    public string roomOwnerConnectionID { get; set; }
    [field: SerializeField]
    public Dictionary<string,PlayerInfoData> playerList { get; set; }
}