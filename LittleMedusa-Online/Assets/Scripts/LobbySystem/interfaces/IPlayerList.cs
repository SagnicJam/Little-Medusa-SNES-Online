using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public interface IPlayerList
    {
        void OnPlayerJoinedRoom(PlayerInfoData player);
        void OnPlayerLeftRoom(PlayerInfoData player);
    }
}