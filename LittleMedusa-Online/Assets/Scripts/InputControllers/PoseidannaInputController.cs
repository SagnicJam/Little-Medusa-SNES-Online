using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseidannaInputController : MonoBehaviour
{
    public Actor localPlayer;

    public bool up;
    public bool left;
    public bool down;
    public bool right;
    public bool waterWave;
    public bool bubbleShield;
    public bool respawnPlayer;

    private void FixedUpdate()
    {
        if (localPlayer.isPushed || localPlayer.isPetrified)
        {
            up = false;
            left = false;
            down = false;
            right = false;
            waterWave = false;
            bubbleShield = false;
            respawnPlayer = false;
        }
        else
        {
            up = Input.GetKey(KeyCode.W);
            left = Input.GetKey(KeyCode.A);
            down = Input.GetKey(KeyCode.S);
            right = Input.GetKey(KeyCode.D);
            waterWave = Input.GetKey(KeyCode.J);
            bubbleShield = Input.GetKey(KeyCode.K);
            respawnPlayer = Input.GetKey(KeyCode.Return);
        }
    }
}
