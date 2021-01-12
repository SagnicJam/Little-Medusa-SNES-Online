using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerList
{
    void OnPlayerJoinedRoom(PlayerInfoData player);
    void OnPlayerLeftRoom(PlayerInfoData player);
}
