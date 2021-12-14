using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace MedusaMultiplayer
{
    public class JoinRoom : MonoBehaviour
    {
        public Room room;
        public TextMeshProUGUI roomText;

        public void Initialise(Room room)
        {
            this.room = room;
            roomText.text = room.RoomName;
        }

        public void JoinRoomButton()
        {
            LobbyScreen.instance.JoinSpecificRoom(room);
        }
    }
}