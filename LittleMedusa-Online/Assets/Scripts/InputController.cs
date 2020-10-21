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
        }
        else if (localPlayer.completedMotionToMovePoint && !localPlayer.IsActorInputMovableInDirection(FaceDirection.Up))
        {
            up = false;
            left = Input.GetKey(KeyCode.A);
            down = Input.GetKey(KeyCode.S);
            right = Input.GetKey(KeyCode.D);
            shoot = Input.GetKey(KeyCode.J);
            push = Input.GetKey(KeyCode.J);
            placeORRemovalBoulder = Input.GetKey(KeyCode.K);
        }
        else if (localPlayer.completedMotionToMovePoint && !localPlayer.IsActorInputMovableInDirection(FaceDirection.Left))
        {
            up = Input.GetKey(KeyCode.W);
            left = false;
            down = Input.GetKey(KeyCode.S);
            right = Input.GetKey(KeyCode.D);
            shoot = Input.GetKey(KeyCode.J);
            push = Input.GetKey(KeyCode.J);
            placeORRemovalBoulder = Input.GetKey(KeyCode.K);
        }
        else if (localPlayer.completedMotionToMovePoint && !localPlayer.IsActorInputMovableInDirection(FaceDirection.Down))
        {
            up = Input.GetKey(KeyCode.W);
            left = Input.GetKey(KeyCode.A);
            down = false;
            right = Input.GetKey(KeyCode.D);
            shoot = Input.GetKey(KeyCode.J);
            push = Input.GetKey(KeyCode.J);
            placeORRemovalBoulder = Input.GetKey(KeyCode.K);
        }
        else if (localPlayer.completedMotionToMovePoint && !localPlayer.IsActorInputMovableInDirection(FaceDirection.Right))
        {
            up = Input.GetKey(KeyCode.W);
            left = Input.GetKey(KeyCode.A);
            down = Input.GetKey(KeyCode.S);
            right = false;
            shoot = Input.GetKey(KeyCode.J);
            push = Input.GetKey(KeyCode.J);
            placeORRemovalBoulder = Input.GetKey(KeyCode.K);
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
        }
    }
}