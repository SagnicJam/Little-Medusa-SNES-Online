using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;

    public ClientMasterController masterController;

    public void Initialise(int id,string username,int facing,int previousFacing, PlayerStateUpdates playerStateUpdates,bool hasAuthority)
    {
        this.id = id;
        this.username = username;

        if(hasAuthority)
        {
            masterController.clientPlayer.InitialiseClientActor(masterController,id);
            masterController.localPlayer.InitialiseClientActor(masterController, id);
            masterController.serverPlayer.InitialiseClientActor(masterController, id);

            masterController.clientPlayer.InitialiseActor(playerStateUpdates);
            masterController.localPlayer.InitialiseActor(playerStateUpdates);
            masterController.serverPlayer.InitialiseActor(playerStateUpdates);

            masterController.clientPlayer.Facing = (FaceDirection)facing;
            masterController.localPlayer.Facing = (FaceDirection)facing;
            masterController.serverPlayer.Facing = (FaceDirection)facing;

            masterController.clientPlayer.PreviousFacingDirection = (FaceDirection)previousFacing;
            masterController.localPlayer.PreviousFacingDirection = (FaceDirection)previousFacing;
            masterController.serverPlayer.PreviousFacingDirection = (FaceDirection)previousFacing;


        }
        else
        {

            masterController.clientPlayer.InitialiseClientActor(masterController,id);

            masterController.clientPlayer.InitialiseActor(playerStateUpdates);

            masterController.clientPlayer.Facing = (FaceDirection)facing;

            masterController.clientPlayer.PreviousFacingDirection = (FaceDirection)previousFacing;
        }
        masterController.latestPlayerStateUpdate = playerStateUpdates;
        masterController.hasAuthority = hasAuthority;

        masterController.serverSequenceNumberToBeProcessed = playerStateUpdates.playerProcessedSequenceNumber;
    }
}
