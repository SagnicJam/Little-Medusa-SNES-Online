using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosidannaInputController : MonoBehaviour
{
    public Actor localPlayer;
    public ClientMasterController clientMasterController;

    public bool up;
    public bool left;
    public bool down;
    public bool right;
    public bool shootTidalWave;
    public bool castBubbleShield;
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
            shootTidalWave = false;
            castBubbleShield = false;
            respawnPlayer = false;
        }
        else
        {
            up = Input.GetKey(KeyCode.W);
            left = Input.GetKey(KeyCode.A);
            down = Input.GetKey(KeyCode.S);
            right = Input.GetKey(KeyCode.D);
            shootTidalWave = Input.GetKey(KeyCode.J);
            castBubbleShield = Input.GetKey(KeyCode.K);
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
                shootTidalWave,
                castBubbleShield,
                respawnPlayer
                };
        return inputs;
    }
}