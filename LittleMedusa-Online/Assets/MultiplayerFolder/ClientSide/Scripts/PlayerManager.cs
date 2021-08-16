using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string connectionId;
    public string username;

    public ClientMasterController masterController;

    public void Initialise(int id,string connectionId,string username, PlayerStateUpdates playerStateUpdates,bool hasAuthority)
    {
        this.id = id;
        this.connectionId = connectionId;
        this.username = username;

        if(hasAuthority)
        {
            masterController.clientPlayer.InitialiseClientActor(masterController, connectionId, id);
            masterController.localPlayer.InitialiseClientActor(masterController, connectionId, id);
            masterController.getInputs = masterController.localPlayer.GetHeroInputs;
            CharacterSelectionScreen.instance.clientlocalActor = masterController.localPlayer;

            masterController.serverPlayer.InitialiseClientActor(masterController, connectionId, id);

            masterController.clientPlayer.InitialiseActor(playerStateUpdates);
            masterController.localPlayer.InitialiseActor(playerStateUpdates);
            masterController.serverPlayer.InitialiseActor(playerStateUpdates);

        }
        else
        {
            masterController.clientPlayer.InitialiseClientActor(masterController, connectionId, id);
            masterController.clientPlayer.InitialiseActor(playerStateUpdates);
        }
        masterController.latestPlayerStateUpdate = playerStateUpdates;
        masterController.hasAuthority = hasAuthority;
        masterController.serverSequenceNumberToBeProcessed = playerStateUpdates.playerServerSequenceNumber;
        masterController.isInitialised = true;
    }
}
