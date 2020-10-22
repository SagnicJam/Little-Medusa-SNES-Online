using System.Collections.Generic;
using UnityEngine;

public class ServerMasterController : MonoBehaviour
{
    [Header("Scene references")]
    public Hero serverInstanceHero;

    [Header("Tweak params")]
    public int packetHistorySize;
    public int snapShotsInOnePacket;
    public int idealSnapshotBufferCount;
    public int maxedOutSnapshotBufferCount;
    public int reliabilityCheckBufferCount;
    //public int playerStateSaveBufferCount;
    
    //private float moveSpeed = 5f / Constants.TICKS_PER_SECONDS;

    private Dictionary<int, InputCommands> inputUpdatesFromClientToServerDic = new Dictionary<int, InputCommands>();
    private Dictionary<int, PushCommand> pushCommandFromClientToServerDic = new Dictionary<int, PushCommand>();
    private Dictionary<int, PlaceBoulderCommand> placeBoulderCommandFromClientToServerDic = new Dictionary<int, PlaceBoulderCommand>();
    private Dictionary<int, RemoveBoulderCommand> removeBoulderCommandFromClientToServerDic = new Dictionary<int, RemoveBoulderCommand>();
    private Dictionary<int, PetrificationCommand> petrificationRequestReceivedFromServerDic = new Dictionary<int, PetrificationCommand>();
    private List<PlayerStateServerUpdates> playerStateListOnServer = new List<PlayerStateServerUpdates>();
    private List<PreviousPlayerUpdatedStatePacks> previousPlayerUpdatedStatePacks = new List<PreviousPlayerUpdatedStatePacks>();

    [Header("Live Units")]
    public int id;
    public string username;
    public ProcessMode currentInputProcessingModeOnServer;
    public int snapShotBufferSize;
    public int serverLocalSequenceNumber = 0;
    public int playerSequenceNumberProcessed = 0;
    public int lastSequenceNumberProcessed;
    public InputCommands latestPlayerInputPackage;

    

    public void Initialise(int id, string username,Vector3 position)
    {
        this.id = id;
        this.username = username;
        serverInstanceHero.actorTransform.position = position;
        serverInstanceHero.movePoint.position = position;
        serverInstanceHero.InitialiseHP();
        serverInstanceHero.InitialiseStockLives();
        serverInstanceHero.InitialiseServerActor(this,id);
    }

    public void CheckForPetrificationRequestOnPlayer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, PetrificationCommand> kvp in petrificationRequestReceivedFromServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, PetrificationCommand> kvp in petrificationRequestReceivedFromServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (petrificationRequestReceivedFromServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                petrificationRequestReceivedFromServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            PetrificationCommand petrificationCommand;
            if (petrificationRequestReceivedFromServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out petrificationCommand))
            {
                petrificationRequestReceivedFromServerDic.Remove(petrificationCommand.sequenceNoForPetrificationCommand);
                //do server rollback here to check to check if damage actually occured on server
                int playerIdPetrified = petrificationCommand.playerIdPetrified;
                Server.clients[playerIdPetrified].serverMasterController.serverInstanceHero.Petrify();
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            PetrificationCommand petrificationCommand;
            if (petrificationRequestReceivedFromServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out petrificationCommand))
            {
                petrificationRequestReceivedFromServerDic.Remove(petrificationCommand.sequenceNoForPetrificationCommand);
                //do server rollback here to check to check if damage actually occured on server
                int playerIdPetrified = petrificationCommand.playerIdPetrified;
                Server.clients[playerIdPetrified].serverMasterController.serverInstanceHero.Petrify();
            }
        }
    }

    public void AccumulatePetrificationRequestFromServer(PetrificationCommand petrificationCommand)
    {
        if (petrificationCommand.sequenceNoForPetrificationCommand > playerSequenceNumberProcessed)
        {
            PetrificationCommand dataPackage;
            if (petrificationRequestReceivedFromServerDic.TryGetValue(petrificationCommand.sequenceNoForPetrificationCommand, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulatePetrificationRequestFromServer dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNoForPetrificationCommand);
            }
            else
            {
                Debug.Log("<color=green>AccumulatePetrificationRequestFromServer Added successfully to processing buffer dic </color>" + playerSequenceNumberProcessed);
                petrificationRequestReceivedFromServerDic.Add(petrificationCommand.sequenceNoForPetrificationCommand, petrificationCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulatePetrificationRequestFromServer Already processed this sequence no </color>" + playerSequenceNumberProcessed + " got the sequence for : " + petrificationCommand.sequenceNoForPetrificationCommand);
        }
    }

    public void CheckForPushRequestOnServer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, PushCommand> kvp in pushCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, PushCommand> kvp in pushCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (pushCommandFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                pushCommandFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            PushCommand pushCommand;
            if (pushCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out pushCommand))
            {
                pushCommandFromClientToServerDic.Remove(pushCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                int playerIdToPush = pushCommand.playerIdToPush;
                int directionOfPush = pushCommand.directionOfPush;

                serverInstanceHero.InitialisePush(playerIdToPush, directionOfPush);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            PushCommand pushCommand;
            if (pushCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out pushCommand))
            {
                pushCommandFromClientToServerDic.Remove(pushCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                int playerIdToPush = pushCommand.playerIdToPush;
                int directionOfPush = pushCommand.directionOfPush;

                serverInstanceHero.InitialisePush(playerIdToPush, directionOfPush);
            }
        }
    }

    public void CheckForPlaceBoulderRequestOnServer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, PlaceBoulderCommand> kvp in placeBoulderCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, PlaceBoulderCommand> kvp in placeBoulderCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (placeBoulderCommandFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                placeBoulderCommandFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            PlaceBoulderCommand placeBoulderCommand;
            if (placeBoulderCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out placeBoulderCommand))
            {
                placeBoulderCommandFromClientToServerDic.Remove(placeBoulderCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                Vector3Int cellPointToPlaceBoulder = placeBoulderCommand.boulderCellPos;

                GridManager.instance.SetTile(cellPointToPlaceBoulder, EnumData.TileType.Boulder, true,false);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            PlaceBoulderCommand placeBoulderCommand;
            if (placeBoulderCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out placeBoulderCommand))
            {
                placeBoulderCommandFromClientToServerDic.Remove(placeBoulderCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                Vector3Int cellPointToPlaceBoulder = placeBoulderCommand.boulderCellPos;
                GridManager.instance.SetTile(cellPointToPlaceBoulder, EnumData.TileType.Boulder, true,false);
                //Send Command for spawning on clients world
            }
        }
    }

    public void CheckForRemovingBoulderRequestOnServer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, RemoveBoulderCommand> kvp in removeBoulderCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, RemoveBoulderCommand> kvp in removeBoulderCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (removeBoulderCommandFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                removeBoulderCommandFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            RemoveBoulderCommand removeBoulderCommand;
            if (removeBoulderCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out removeBoulderCommand))
            {
                removeBoulderCommandFromClientToServerDic.Remove(removeBoulderCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                Vector3Int cellPointToRemoveBoulder = removeBoulderCommand.removalCellPos;

                GridManager.instance.SetTile(cellPointToRemoveBoulder, EnumData.TileType.Boulder, false,false);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            RemoveBoulderCommand removeBoulderCommand;
            if (removeBoulderCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out removeBoulderCommand))
            {
                removeBoulderCommandFromClientToServerDic.Remove(removeBoulderCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                Vector3Int cellPointToRemoveBoulder = removeBoulderCommand.removalCellPos;
                GridManager.instance.SetTile(cellPointToRemoveBoulder, EnumData.TileType.Boulder, false,false);
                //Send Command for spawning on clients world
            }
        }
    }

    public void AccumulateRemoveBoulderCommandsToBePlayedOnServerFromClient(RemoveBoulderCommand removeBoulderCommand)
    {
        if (removeBoulderCommand.sequenceNumber > playerSequenceNumberProcessed)
        {
            RemoveBoulderCommand dataPackage;
            if (removeBoulderCommandFromClientToServerDic.TryGetValue(removeBoulderCommand.sequenceNumber, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateRemoveBoulderCommandsToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + removeBoulderCommand.sequenceNumber);
            }
            else
            {
                Debug.Log("<color=green>AccumulateRemoveBoulderCommandsToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + removeBoulderCommand.sequenceNumber);
                removeBoulderCommandFromClientToServerDic.Add(removeBoulderCommand.sequenceNumber, removeBoulderCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateRemoveBoulderCommandsToBePlayedOnServerFromClient Already processed this sequence no </color>" + removeBoulderCommand.sequenceNumber);
        }
    }

    public void AccumulatePlaceBoulderCommandsToBePlayedOnServerFromClient(PlaceBoulderCommand placeBoulderCommand)
    {
        if (placeBoulderCommand.sequenceNumber > playerSequenceNumberProcessed)
        {
            PlaceBoulderCommand dataPackage;
            if (placeBoulderCommandFromClientToServerDic.TryGetValue(placeBoulderCommand.sequenceNumber, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulatePlaceBoulderCommandsToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + placeBoulderCommand.sequenceNumber);
            }
            else
            {
                Debug.Log("<color=green>AccumulatePlaceBoulderCommandsToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + placeBoulderCommand.sequenceNumber);
                placeBoulderCommandFromClientToServerDic.Add(placeBoulderCommand.sequenceNumber, placeBoulderCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulatePlaceBoulderCommandsToBePlayedOnServerFromClient Already processed this sequence no </color>" + placeBoulderCommand.sequenceNumber);
        }
    }

    public void AccumulatePushCommandsToBePlayedOnServerFromClient(PushCommand pushCommand)
    {
        if (pushCommand.sequenceNumber > playerSequenceNumberProcessed)
        {
            PushCommand dataPackage;
            if (pushCommandFromClientToServerDic.TryGetValue(pushCommand.sequenceNumber, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulatePushCommandsToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNumber);
            }
            else
            {
                Debug.Log("<color=green>AccumulatePushCommandsToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + pushCommand.sequenceNumber);
                pushCommandFromClientToServerDic.Add(pushCommand.sequenceNumber, pushCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>Already processed this sequence no </color>" + pushCommand.sequenceNumber);
        }
    }


    public void AccumulateInputsToBePlayedOnServerFromClient(InputCommands inputCommands)
    {
        if (inputCommands.sequenceNumber > playerSequenceNumberProcessed)
        {
            InputCommands dataPackage;
            if (inputUpdatesFromClientToServerDic.TryGetValue(inputCommands.sequenceNumber, out dataPackage))
            {
                //Debug.Log("<color=orange>dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNumber);
            }
            else
            {
                //Debug.Log("<color=green>Added successfully to processing buffer dic </color>" + inputCommands.sequenceNumber+"  processed sequence number: "+playerSequenceNumberProcessed);
                inputUpdatesFromClientToServerDic.Add(inputCommands.sequenceNumber, inputCommands);
            }
        }
        else
        {
            //Debug.Log("<color=red>Already processed this sequence no </color>" + playerSequenceNumberProcessed);
        }
    }

    private void FixedUpdate()
    {
        serverInstanceHero.ProcessAuthoratativeEvents();

        for (int i = 0; i < (int)currentInputProcessingModeOnServer; i++)
        {
            

            InputCommands inputPackageCorrespondingToSeq;

            if (inputUpdatesFromClientToServerDic.TryGetValue(playerSequenceNumberProcessed + 1, out inputPackageCorrespondingToSeq))
            {
                inputUpdatesFromClientToServerDic.Remove(inputPackageCorrespondingToSeq.sequenceNumber);
                latestPlayerInputPackage = inputPackageCorrespondingToSeq;
                playerSequenceNumberProcessed = inputPackageCorrespondingToSeq.sequenceNumber;

                serverInstanceHero.ProcessMovementInputs(inputPackageCorrespondingToSeq.commands
                    , inputPackageCorrespondingToSeq.previousCommands);
                serverInstanceHero.ProcessEventsInputs(inputPackageCorrespondingToSeq.commands, inputPackageCorrespondingToSeq.previousCommands);
                serverInstanceHero.ProcessAnimationsInputs(inputPackageCorrespondingToSeq.commands, inputPackageCorrespondingToSeq.previousCommands);

                serverInstanceHero.ProcessInputMovementsControl();
                serverInstanceHero.ProcessInputEventControl();
                serverInstanceHero.ProcessInputAnimationControl();
                //Debug.Log(serverInstanceHero.movePoint.position + "Block pos "+serverInstanceHero.GetBlockPosition()+"<color=yellow>Processing seqence no </color>" + inputPackageCorrespondingToSeq.sequenceNumber + "<color=green>position </color>" + serverInstanceHero.actorTransform.position + "<color=green>inputs </color>" + inputPackageCorrespondingToSeq.commands[0] + inputPackageCorrespondingToSeq.commands[1] + inputPackageCorrespondingToSeq.commands[2] + inputPackageCorrespondingToSeq.commands[3]+" Previous Commands "+ inputPackageCorrespondingToSeq.previousCommands[0] + inputPackageCorrespondingToSeq.previousCommands[1] + inputPackageCorrespondingToSeq.previousCommands[2] + inputPackageCorrespondingToSeq.previousCommands[3]);
            }
            else
            {
                if (latestPlayerInputPackage.sequenceNumber != 0)
                {
                    //Debug.LogError("Could not find any inputToProcess for  seq: " + (sequenceNumberProcessed + 1));
                    playerSequenceNumberProcessed = playerSequenceNumberProcessed + 1;

                    serverInstanceHero.ProcessMovementInputs(latestPlayerInputPackage.commands
                   , latestPlayerInputPackage.previousCommands);
                    serverInstanceHero.ProcessEventsInputs(latestPlayerInputPackage.commands, latestPlayerInputPackage.previousCommands);
                    serverInstanceHero.ProcessAnimationsInputs(latestPlayerInputPackage.commands, latestPlayerInputPackage.previousCommands);

                    serverInstanceHero.ProcessInputMovementsControl();
                    serverInstanceHero.ProcessInputEventControl();
                    serverInstanceHero.ProcessInputAnimationControl();
                }
            }

            CheckForPushRequestOnServer(playerSequenceNumberProcessed);
            CheckForPlaceBoulderRequestOnServer(playerSequenceNumberProcessed);
            CheckForRemovingBoulderRequestOnServer(playerSequenceNumberProcessed);
            CheckForPetrificationRequestOnPlayer(playerSequenceNumberProcessed);
        }

        serverLocalSequenceNumber++;

        //////Debug.Log("<color=blue>inputsequence </color>"+ playerMovingCommandSequenceNumber + "<color=blue>inputs </color> "+ inputs[0]+" "+inputs[1]+" "+inputs[2]+" "+inputs[3]);
        PlayerAuthoratativeStates playerAuthoratativeStates = new PlayerAuthoratativeStates(serverInstanceHero.isPetrified
            , serverInstanceHero.isPushed
            ,serverInstanceHero.isInvincible
            ,serverInstanceHero.currentHP
            ,serverInstanceHero.currentStockLives);

        PositionUpdates positionUpdates = new PositionUpdates(serverInstanceHero.actorTransform.position, serverInstanceHero.currentMovePointCellPosition
            , serverInstanceHero.previousMovePointCellPosition,(int)serverInstanceHero.Facing,(int)serverInstanceHero.PreviousFacingDirection);
        PlayerEvents playerEvents = new PlayerEvents(serverInstanceHero.isFiringPrimaryProjectile);
        PlayerAnimationEvents playerAnimationEvents = new PlayerAnimationEvents(serverInstanceHero.primaryMoveUseAnimationAction.isBeingUsed);

        PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(serverLocalSequenceNumber,playerSequenceNumberProcessed, playerAuthoratativeStates, positionUpdates, playerEvents, playerAnimationEvents);
        PlayerStateServerUpdates playerStateServerUpdates = new PlayerStateServerUpdates(id, playerStateUpdates);
        //Debug.LogError("Server sequence number : "+ serverLocalSequenceNumber+" player sequence number processeed: "+ playerSequenceNumberProcessed+" player position : "+serverInstanceHero.actorTransform.position);

        playerStateListOnServer.Add(playerStateServerUpdates);

        if (playerStateListOnServer.Count >= snapShotsInOnePacket)
        {
            if (previousPlayerUpdatedStatePacks.Count > packetHistorySize)
            {
                previousPlayerUpdatedStatePacks.RemoveAt(0);
            }
            ServerSend.PlayerStateSend(playerStateListOnServer, previousPlayerUpdatedStatePacks);

            previousPlayerUpdatedStatePacks.Add(new PreviousPlayerUpdatedStatePacks(playerStateListOnServer.ToArray()));

            playerStateListOnServer.Clear();

            //Debug.Log("<color=red>--------------------------------------------------------------------</color>");
        }

        UpdateClientInputsProcessMode();
        snapShotBufferSize = GetTheLastestSequenceNoInDic() - playerSequenceNumberProcessed;
        //Debug.Log("<color=cyan>Dic Count </color>"+inputUpdatesFromClientToServerDic.Count);
    }


    int GetTheLastestSequenceNoInDic()
    {
        int largestInt = 0;
        foreach (KeyValuePair<int, InputCommands> kvp in inputUpdatesFromClientToServerDic)
        {
            if (largestInt < kvp.Key)
            {
                largestInt = kvp.Key;
            }
        }
        return largestInt;
    }

    void UpdateClientInputsProcessMode()
    {
        if (inputUpdatesFromClientToServerDic.Count == 0)
        {
            currentInputProcessingModeOnServer = ProcessMode.Lazy;
        }
        else if (currentInputProcessingModeOnServer == ProcessMode.Lazy && inputUpdatesFromClientToServerDic.Count >= idealSnapshotBufferCount)
        {
            currentInputProcessingModeOnServer = ProcessMode.Ideal;
        }
        else if (currentInputProcessingModeOnServer == ProcessMode.Ideal && inputUpdatesFromClientToServerDic.Count > maxedOutSnapshotBufferCount)
        {
            currentInputProcessingModeOnServer = ProcessMode.Hyper;
        }
        else if (currentInputProcessingModeOnServer == ProcessMode.Hyper && inputUpdatesFromClientToServerDic.Count <= idealSnapshotBufferCount)
        {
            currentInputProcessingModeOnServer = ProcessMode.Ideal;
        }
    }
}


public struct PlayerStateServerUpdates
{
    public int playerId;
    public PlayerStateUpdates playerStateUpdates;

    public PlayerStateServerUpdates(int playerId,PlayerStateUpdates playerStateUpdates)
    {
        this.playerId = playerId;
        this.playerStateUpdates = playerStateUpdates;
    }
}

public struct PreviousPlayerUpdatedStatePacks
{
    public PlayerStateServerUpdates[] previousUpdatedStates;

    public PreviousPlayerUpdatedStatePacks(PlayerStateServerUpdates[] previousUpdatedPositions)
    {
        this.previousUpdatedStates = previousUpdatedPositions;
    }
}

public enum ProcessMode
{
    Lazy=0,
    Ideal,
    Hyper
}