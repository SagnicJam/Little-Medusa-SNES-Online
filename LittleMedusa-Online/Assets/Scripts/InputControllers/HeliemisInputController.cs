using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliemisInputController : MonoBehaviour
{
    public Actor localPlayer;
    public ClientMasterController clientMasterController;

    public bool up;
    public bool left;
    public bool down;
    public bool right;
    public bool shootMightyWind;
    public bool placeTornado;
    public bool respawnPlayer;

    private void Awake()
    {
        clientMasterController.getInputs = GetHeliemisInputs;
    }

    private void FixedUpdate()
    {
        if (localPlayer.isPushed || localPlayer.isPetrified||localPlayer.isPhysicsControlled)
        {
            up = false;
            left = false;
            down = false;
            right = false;
            shootMightyWind = false;
            placeTornado = false;
            respawnPlayer = false;
        }
        else
        {
            up = Input.GetKey(KeyCode.W);
            left = Input.GetKey(KeyCode.A);
            down = Input.GetKey(KeyCode.S);
            right = Input.GetKey(KeyCode.D);
            shootMightyWind = Input.GetKey(KeyCode.J);
            placeTornado = Input.GetKey(KeyCode.K);
            respawnPlayer = Input.GetKey(KeyCode.Return);
        }
    }

    public bool[] GetHeliemisInputs()
    {
        bool[] inputs = new bool[]
                {
                up,
                left,
                down,
                right,
                shootMightyWind,
                placeTornado,
                respawnPlayer
                };
        return inputs;
    }
}
