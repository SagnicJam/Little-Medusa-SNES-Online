using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class AvernaInputController : MonoBehaviour
    {
        public Actor localPlayer;
        public ClientMasterController clientMasterController;

        public bool up;
        public bool left;
        public bool down;
        public bool right;
        public bool shootFireBall;
        public bool castFlamePillar;
        public bool respawnPlayer;

        private void Awake()
        {
            clientMasterController.getInputs = GetAvernaInputs;
        }

        private void FixedUpdate()
        {
            if (localPlayer.isPushed || localPlayer.isPetrified)
            {
                up = false;
                left = false;
                down = false;
                right = false;
                shootFireBall = false;
                castFlamePillar = false;
                respawnPlayer = false;
            }
            else
            {
                up = Input.GetKey(KeyCode.W);
                left = Input.GetKey(KeyCode.A);
                down = Input.GetKey(KeyCode.S);
                right = Input.GetKey(KeyCode.D);
                shootFireBall = Input.GetKey(KeyCode.J);
                castFlamePillar = Input.GetKey(KeyCode.K);
                respawnPlayer = Input.GetKey(KeyCode.Return);
            }
        }

        public bool[] GetAvernaInputs()
        {
            bool[] inputs = new bool[]
                    {
                up,
                left,
                down,
                right,
                shootFireBall,
                castFlamePillar,
                respawnPlayer
                    };
            return inputs;
        }
    }
}
