using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class InputController : MonoBehaviour
    {
        public Hero localPlayer;
        public ClientMasterController clientMasterController;
        private void FixedUpdate()
        {
            localPlayer.DealInput();
        }
    }
}