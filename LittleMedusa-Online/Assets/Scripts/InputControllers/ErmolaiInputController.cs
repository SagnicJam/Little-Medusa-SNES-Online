using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class ErmolaiInputController : MonoBehaviour
    {
        public Actor localPlayer;
        public ClientMasterController clientMasterController;

        public bool up;
        public bool left;
        public bool down;
        public bool right;
        public bool castPitfall;
        public bool castEarthQuake;
        public bool respawnPlayer;

        private void Awake()
        {
            clientMasterController.getInputs = GetPosidannaInputs;
        }

        private void FixedUpdate()
        {
            if (localPlayer.isPushed || localPlayer.isPetrified)
            {
                up = false;
                left = false;
                down = false;
                right = false;
                castPitfall = false;
                castEarthQuake = false;
                respawnPlayer = false;
            }
            else
            {
                up = Input.GetKey(KeyCode.W);
                left = Input.GetKey(KeyCode.A);
                down = Input.GetKey(KeyCode.S);
                right = Input.GetKey(KeyCode.D);
                castPitfall = Input.GetKey(KeyCode.J);
                castEarthQuake = Input.GetKey(KeyCode.K);
                respawnPlayer = Input.GetKey(KeyCode.Return);
            }
        }

        public bool[] GetPosidannaInputs()
        {
            bool[] inputs = new bool[]
                    {
                up,
                left,
                down,
                right,
                castPitfall,
                castEarthQuake,
                respawnPlayer
                    };
            return inputs;
        }
    }
}