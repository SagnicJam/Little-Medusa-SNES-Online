using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerInfoData
{
    public PlayerInfoData(string connectionId, string name)
    {
        this.connectionId = connectionId;
        Name = name;
    }

    public string connectionId { get; set; }
    public string Name { get; set; }
}