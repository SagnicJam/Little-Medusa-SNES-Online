using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public interface IRoomList
    {
        void OnNewRoomAdded(Room room);
        void OnRoomDeleted(Room room);
    }
}