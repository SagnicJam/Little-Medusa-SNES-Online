using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public Actor localPlayer;

    public bool up;
    public bool left;
    public bool down;
    public bool right;
    public bool shoot;
    public bool push;
    public bool placeORRemovalBoulder;
    public bool respawnPlayer;

    private void FixedUpdate()
    {
        if (localPlayer.isPushed || localPlayer.isPetrified)
        {
            up = false;
            left = false;
            down = false;
            right = false;
            shoot = false;
            push = false;
            placeORRemovalBoulder = false;
            respawnPlayer = false;
        }
        else
        {
            up = Input.GetKey(KeyCode.W);
            left = Input.GetKey(KeyCode.A);
            down = Input.GetKey(KeyCode.S);
            right = Input.GetKey(KeyCode.D);
            shoot = Input.GetKey(KeyCode.J);
            push = Input.GetKey(KeyCode.J);
            placeORRemovalBoulder = Input.GetKey(KeyCode.K);
            respawnPlayer = Input.GetKey(KeyCode.KeypadEnter);
        }
    }
}