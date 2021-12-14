using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace MedusaMultiplayer
{
    public class PlayerInfo : MonoBehaviour
    {
        public TextMeshProUGUI playerNameText;
        public PlayerInfoData player;

        public void Initialise(PlayerInfoData player)
        {
            this.player = player;
            playerNameText.text = this.player.Name;
        }
    }
}
