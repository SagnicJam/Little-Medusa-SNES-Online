using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientMasterController : MonoBehaviour
{
    [Header("Scene references")]
    public Hero localPlayer;
    public Hero serverPlayer;
    public Hero clientPlayer;
    public delegate bool[] GetInputs();
    public GetInputs getInputs;

    [Header("Tweak Params")]
    public int packetHistorySize;
    public int snapShotsInOnePacket;
    public int idealSnapshotBufferCount;
    public int maxedOutSnapshotBufferCount;
    public float positionThreshold;

    //private float moveSpeed = 5f / Constants.TICKS_PER_SECONDS;
    
    private Dictionary<int, InputCommands> localClientActions = new Dictionary<int, InputCommands>();
    private Dictionary<int, PlayerStateUpdates> playerStateUpdatesDic = new Dictionary<int, PlayerStateUpdates>();
    private List<InputCommands> inputCommandsToBeSentToServerCollection = new List<InputCommands>();
    private List<PreviousInputPacks> previousHistoryForInputCommandsToBeSentToServerCollection = new List<PreviousInputPacks>();

    [Header("Live Units")]
    public int id;
    public string connectionId;
    public ProcessMode currentStateProcessingModeOnClient;
    public int localSequenceNumber = 0;
    public int serverSequenceNumberToBeProcessed = 0;
    public int playerSequenceNumberProcessed = 0;
    public int latestPacketProcessedLocally;
    public int snapShotBufferSize;
    public bool isInitialised;
    public bool hasAuthority;
    public PlayerStateUpdates latestPlayerStateUpdate;

    public int dicSize;

    private void Update()
    {
        dicSize = inputCommandsToBeSentToServerCollection.Count;
    }

    private void Awake()
    {
        previousInputs = new bool[Enum.GetNames(typeof(EnumData.MedusaInputs)).Length];
    }

    bool[] previousInputs;

    public void SetCharacter(int hero,PlayerFlyData playerFlyData,PositionUpdates positionUpdates)
    {
        if (id == Client.instance.myID)
        {
            Hero localHero = (Instantiate(Resources.Load("Characters/" + ((EnumData.Heroes)hero).ToString() + "/LocalPlayer-" + ((EnumData.Heroes)hero).ToString()), transform, false) as GameObject).GetComponentInChildren<Hero>();
            Hero serverPredictedHero = (Instantiate(Resources.Load("Characters/" + ((EnumData.Heroes)hero).ToString() + "/ServerPredicted-" + ((EnumData.Heroes)hero).ToString()), transform, false) as GameObject).GetComponentInChildren<Hero>();
            Hero remoteClientHero = (Instantiate(Resources.Load("Characters/" + ((EnumData.Heroes)hero).ToString() + "/RemoteClient-" + ((EnumData.Heroes)hero).ToString()), transform, false) as GameObject).GetComponentInChildren<Hero>();

            GetComponent<InputController>().localPlayer = localHero;

            localPlayer = localHero;
            serverPlayer = serverPredictedHero;
            clientPlayer = remoteClientHero;

            localPlayer.SetActorPositionalState(positionUpdates);
            serverPlayer.SetActorPositionalState(positionUpdates);
            clientPlayer.SetActorPositionalState(positionUpdates);
            
            localPlayer.SetFlyingTickCount(playerFlyData);
            serverPlayer.SetFlyingTickCount(playerFlyData);
            clientPlayer.SetFlyingTickCount(playerFlyData);


            localPlayer.InitialiseClientActor(this,connectionId, id);
            serverPlayer.InitialiseClientActor(this, connectionId, id);
            clientPlayer.InitialiseClientActor(this, connectionId, id);

            getInputs = localPlayer.GetHeroInputs;
            CharacterSelectionScreen.instance.clientlocalActor = localPlayer;
        }
        else
        {
            Hero remoteOtherClient = (Instantiate(Resources.Load("Characters/" + ((EnumData.Heroes)hero).ToString() + "/RemoteClientOther-" + ((EnumData.Heroes)hero).ToString()), transform, false) as GameObject).GetComponentInChildren<Hero>();
            clientPlayer = remoteOtherClient;
            clientPlayer.SetActorPositionalState(positionUpdates);
            clientPlayer.SetFlyingTickCount(playerFlyData);
            clientPlayer.InitialiseClientActor(this, connectionId, id);
        }
    }

    private void FixedUpdate()
    {
        if(isInitialised)
        {
            //Remote clients
            for (int i = 0; i < (int)currentStateProcessingModeOnClient; i++)
            {
                PlayerStateUpdates updateCorrespondingToSeq;
                if (playerStateUpdatesDic.TryGetValue(serverSequenceNumberToBeProcessed + 1, out updateCorrespondingToSeq))
                {
                    //if(clientPlayer.ownerId==2)
                    //{
                    //    Debug.LogError("Processing : "+ updateCorrespondingToSeq.playerServerSequenceNumber);
                    //}
                    //Debug.Log("<color=yellow>Remote Client of id " + id + " is Processing seqence no </color>" + updateCorrespondingToSeq.playerServerSequenceNumber + " and the processed sequence no: " + updateCorrespondingToSeq.playerProcessedSequenceNumber);
                    playerStateUpdatesDic.Remove(updateCorrespondingToSeq.playerServerSequenceNumber);
                    serverSequenceNumberToBeProcessed = updateCorrespondingToSeq.playerServerSequenceNumber;
                    playerSequenceNumberProcessed = updateCorrespondingToSeq.playerProcessedSequenceNumber;
                    latestPlayerStateUpdate = updateCorrespondingToSeq;


                    PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(serverSequenceNumberToBeProcessed
                        , playerSequenceNumberProcessed
                        , updateCorrespondingToSeq.playerAuthoratativeStates
                        , updateCorrespondingToSeq.positionUpdates
                        , updateCorrespondingToSeq.playerEvents
                        , updateCorrespondingToSeq.playerAnimationEvents
                        , updateCorrespondingToSeq.playerFlyData);

                    SetPlayerStateUpdatesReceivedFromServer(playerStateUpdates);
                }
                else
                {
                    Debug.LogError(id + " latestPlayerStateUpdate.playerServerSequenceNumber " + latestPlayerStateUpdate.playerServerSequenceNumber);
                    Debug.LogError(id + " Could not find any posudates for  seq: " + (serverSequenceNumberToBeProcessed + 1));
                    if (latestPlayerStateUpdate.playerServerSequenceNumber != 0)
                    {
                        serverSequenceNumberToBeProcessed = serverSequenceNumberToBeProcessed + 1;
                        PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(serverSequenceNumberToBeProcessed
                            , latestPlayerStateUpdate.playerProcessedSequenceNumber
                            , latestPlayerStateUpdate.playerAuthoratativeStates
                            , latestPlayerStateUpdate.positionUpdates
                            , latestPlayerStateUpdate.playerEvents
                            , latestPlayerStateUpdate.playerAnimationEvents
                            ,latestPlayerStateUpdate.playerFlyData);
                        SetPlayerStateUpdatesReceivedFromServer(playerStateUpdates);
                    }
                }
            }

            if (hasAuthority)
            {
                bool[] inputs = getInputs();
                localSequenceNumber++;

                ProcessInputsLocally(inputs, previousInputs);
                RecordLocalClientActions(localSequenceNumber, inputs, previousInputs);

                //////Debug.Log("<color=blue>inputsequence </color>"+ playerMovingCommandSequenceNumber + "<color=blue>inputs </color> "+ inputs[0]+" "+inputs[1]+" "+inputs[2]+" "+inputs[3]);

                inputCommandsToBeSentToServerCollection.Add(new InputCommands(inputs, previousInputs, localSequenceNumber));
                previousInputs = inputs;

                //Local client sending data
                if (inputCommandsToBeSentToServerCollection.Count >= snapShotsInOnePacket)
                {
                    if (previousHistoryForInputCommandsToBeSentToServerCollection.Count > packetHistorySize)
                    {
                        previousHistoryForInputCommandsToBeSentToServerCollection.RemoveAt(0);
                    }
                    ClientSend.PlayerInput(inputCommandsToBeSentToServerCollection, previousHistoryForInputCommandsToBeSentToServerCollection);

                    previousHistoryForInputCommandsToBeSentToServerCollection.Add(new PreviousInputPacks(inputCommandsToBeSentToServerCollection.ToArray()));

                    inputCommandsToBeSentToServerCollection.Clear();

                    //Debug.Log("<color=red>--------------------------------------------------------------------</color>");

                }

            }

            UpdateProcessMode();
            snapShotBufferSize = GetTheLastestSequenceNoInDic() - serverSequenceNumberToBeProcessed;
        }
        
        //Debug.Log("<color=cyan>Dic Count </color>" + positionUpdates.Count);
    }

    void UpdateProcessMode()
    {

        if (playerStateUpdatesDic.Count == 0)
        {
            currentStateProcessingModeOnClient = ProcessMode.Lazy;
        }
        else if (currentStateProcessingModeOnClient == ProcessMode.Lazy && playerStateUpdatesDic.Count >= idealSnapshotBufferCount)
        {
            currentStateProcessingModeOnClient = ProcessMode.Ideal;
        }
        else if (currentStateProcessingModeOnClient == ProcessMode.Ideal && playerStateUpdatesDic.Count > maxedOutSnapshotBufferCount)
        {
            currentStateProcessingModeOnClient = ProcessMode.Hyper;
        }
        else if (currentStateProcessingModeOnClient == ProcessMode.Hyper && playerStateUpdatesDic.Count <= idealSnapshotBufferCount)
        {
            currentStateProcessingModeOnClient = ProcessMode.Ideal;
        }
        //if (!hasAuthority)
        //{
        //    Debug.LogError("count " + positionUpdates.Count + "  mode " + currentProcessingMode);
        //}
    }

    int GetTheLastestSequenceNoInDic()
    {
        int largestInt=0;
        foreach(KeyValuePair<int,PlayerStateUpdates>kvp in playerStateUpdatesDic)
        {
            if(largestInt<kvp.Key)
            {
                largestInt = kvp.Key;
            }
        }
        return largestInt;
    }

    public void RecordLocalClientActions(int sequenceNumber,bool[] inputs,bool[] previousInputs)
    {
        //Debug.Log("After wards player position locally "+localPlayer.actorTransform.position+"<color=red>Recording here </color>"+"-->"+ sequenceNumber+" "+ inputs[0]+" "+inputs[1]+" "+inputs[2]+" "+inputs[3]+"<color=blue>Previous Inputs</color>"+previousInputs[0]+" "+previousInputs[1]+" "+previousInputs[2]+" "+previousInputs[3]);
        localClientActions.Add(sequenceNumber, new InputCommands(inputs, previousInputs, sequenceNumber));
        sequenceNoList.Add(sequenceNumber);
        latestPacketProcessedLocally = sequenceNumber;
        //Debug.Log("<color=pink>sequence no recorded </color>" + sequenceNo + "<color=pink> last run is  </color>"+localClientInputCommands.Count);
    }

    private void ProcessInputsLocally(bool[]inputs,bool[] previousInputs)
    {
        localPlayer.ProcessMovementInputs(inputs, previousInputs);
        localPlayer.ProcessEventsInputs(inputs, previousInputs);
        localPlayer.ProcessAnimationsInputs(inputs, previousInputs);

        localPlayer.ProcessInputMovementsControl();
        localPlayer.ProcessFlyingControl();
        localPlayer.ProcessInputEventControl();
        localPlayer.ProcessInputAnimationControl();
    }

    public List<int> sequenceNoList=new List<int>();

    private void UpdatePredictedGhost(PlayerStateUpdates playerStateUpdates)
    {
        //Discard all previous sequence number records
        List<int> toDiscardSequencesInputs = new List<int>();
        foreach (KeyValuePair<int, InputCommands> kvp in localClientActions)
        {
            if (kvp.Key <= playerStateUpdates.playerProcessedSequenceNumber)
            {
                toDiscardSequencesInputs.Add(kvp.Key);
                //Debug.Log("<color=red>Need to discard</color>"+kvp.Key);
            }
        }

        foreach (int i in toDiscardSequencesInputs)
        {
            if (localClientActions.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                localClientActions.Remove(i);

                if (sequenceNoList.Contains(i))
                {
                    sequenceNoList.Remove(i);
                }
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }


        //Accept the current position and reapply input since sequencee number
        //Debug.Log("<color=green>Before localClientCommands to iterate across blockposition </color>" + playerStateUpdates.positionUpdates.updatedBlockActorPosition + "player position" + playerStateUpdates.positionUpdates.updatedActorPosition + "<color=pink> first sequence is  </color>" + (playerStateUpdates.playerProcessedSequenceNumber + 1) + "<color=pink> last sequence no </color>" + latestPacketProcessedLocally);
        //Debug.Break();
        for (int i = playerStateUpdates.playerProcessedSequenceNumber + 1; i <= latestPacketProcessedLocally; i++)
        {
            InputCommands la;
            if (localClientActions.TryGetValue(i, out la))
            {
                //Debug.Log("<color=yellow>prediction done using sequnce no: </color>" + localClientInputCommands[i].sequenceNumber + "<color=green> inputs: </color>" + localClientInputCommands[i].commands[0] + localClientInputCommands[i].commands[1] + localClientInputCommands[i].commands[2] + localClientInputCommands[i].commands[3] + " Previous commands " + localClientInputCommands[i].previousCommands[0] + localClientInputCommands[i].previousCommands[1] + localClientInputCommands[i].previousCommands[2] + localClientInputCommands[i].previousCommands[3]);
                //Debug.Log(serverPlayer.movePoint.position + "--" + serverPlayer.currentMovePointCellPosition + "Before" + serverPlayer.actorTransform.position + "<color=yellow>Before prediction done using sequnce no: </color>" + localClientInputCommands[i].sequenceNumber);
                serverPlayer.ProcessMovementInputs(la.commands, la.previousCommands);
                
                serverPlayer.ProcessInputMovementsControl();
                serverPlayer.ProcessCollisionEnter();
                serverPlayer.ProcessFlyingControl();
                //Debug.Log(serverPlayer.movePoint.position + "--" + serverPlayer.currentMovePointCellPosition + "After wards" + serverPlayer.actorTransform.position + "<color=yellow>After prediction done using sequnce no: </color>" + localClientInputCommands[i].sequenceNumber + "<color=green> inputs: </color>" + localClientInputCommands[i].commands[0] + localClientInputCommands[i].commands[1] + localClientInputCommands[i].commands[2] + localClientInputCommands[i].commands[3] + " Previous commands " + localClientInputCommands[i].previousCommands[0] + localClientInputCommands[i].previousCommands[1] + localClientInputCommands[i].previousCommands[2] + localClientInputCommands[i].previousCommands[3]);
            }
            else
            {
                Debug.LogError("Could not find input commands for sequence no. " + i);
            }
        }
        //Debug.Log("<color=green>After localClientCommands to iterate across </color>" + predictedPositionOfPlayerOnServer);
        //Debug.Log("changing position here");
    }

    public void SetPlayerStateUpdatesReceivedFromServer(PlayerStateUpdates playerStateUpdates)
    {
        if (hasAuthority)
        {
            if (localPlayer.hero != playerStateUpdates.playerAuthoratativeStates.hero)
            {
                Hero previousServerHero = serverPlayer;
                Hero previousClientHero = clientPlayer;
                Hero previousLocalHero = localPlayer;
                SetCharacter(playerStateUpdates.playerAuthoratativeStates.hero, playerStateUpdates.playerFlyData, playerStateUpdates.positionUpdates);

                CharacterSelectionScreen.instance.AssignCharacterToId(playerStateUpdates.playerAuthoratativeStates.hero,id);

                Destroy(previousServerHero.transform.parent.gameObject);
                Destroy(previousClientHero.transform.parent.gameObject);
                Destroy(previousLocalHero.transform.parent.gameObject);
            }
        }
        else
        {
            if(clientPlayer.hero!= playerStateUpdates.playerAuthoratativeStates.hero)
            {
                Hero previousClientHero = clientPlayer;

                SetCharacter(playerStateUpdates.playerAuthoratativeStates.hero, playerStateUpdates.playerFlyData, playerStateUpdates.positionUpdates);

                CharacterSelectionScreen.instance.AssignCharacterToId(playerStateUpdates.playerAuthoratativeStates.hero, id);

                Destroy(previousClientHero.transform.parent.gameObject);
            }
        }

        clientPlayer.SetActorPositionalState(playerStateUpdates.positionUpdates);
        clientPlayer.SetFlyingTickCount(playerStateUpdates.playerFlyData);
        clientPlayer.SetActorEventActionState(playerStateUpdates.playerEvents);
        clientPlayer.SetActorAnimationState(playerStateUpdates.playerAnimationEvents);
        clientPlayer.SetAuthoratativeStates(playerStateUpdates.playerAuthoratativeStates);
        if (hasAuthority)
        {
            if (playerStateUpdates.playerAuthoratativeStates.isRespawnningPlayer && localPlayer.isRespawnningPlayer != playerStateUpdates.playerAuthoratativeStates.isRespawnningPlayer)
            {
                localPlayer.SetActorPositionalState(playerStateUpdates.positionUpdates);
                localPlayer.SetFlyingTickCount(playerStateUpdates.playerFlyData);
            }
            localPlayer.SetAuthoratativeStates(playerStateUpdates.playerAuthoratativeStates);

            if (localPlayer.isPetrified || localPlayer.isPushed||localPlayer.isPhysicsControlled)
            {
                localPlayer.SetActorPositionalState(playerStateUpdates.positionUpdates);
                localPlayer.SetFlyingTickCount(playerStateUpdates.playerFlyData);
            }

            serverPlayer.SetActorPositionalState(playerStateUpdates.positionUpdates);
            serverPlayer.SetFlyingTickCount(playerStateUpdates.playerFlyData);

            serverPlayer.SetActorEventActionState(playerStateUpdates.playerEvents);
            serverPlayer.SetActorAnimationState(playerStateUpdates.playerAnimationEvents);
            serverPlayer.SetAuthoratativeStates(playerStateUpdates.playerAuthoratativeStates);

            UpdatePredictedGhost(playerStateUpdates);
            if (Vector3.Distance(serverPlayer.actorTransform.position, localPlayer.actorTransform.position) >= positionThreshold)
            {
                Debug.Log("Correction regarding position difference " + serverPlayer.actorTransform.position + "local---server" + localPlayer.actorTransform.position + "<color=red>Corrected player position</color>" + playerStateUpdates.playerProcessedSequenceNumber);
                PositionUpdates positionUpdates = new PositionUpdates(serverPlayer.actorTransform.position, serverPlayer.currentMovePointCellPosition, serverPlayer.previousMovePointCellPosition,
                    (int)serverPlayer.Facing, (int)serverPlayer.PreviousFacingDirection);
                localPlayer.SetActorPositionalState(positionUpdates);
                localPlayer.SetFlyingTickCount(new PlayerFlyData(serverPlayer.flyingTickCountTemp));
            }
        }
        else
        {
            clientPlayer.ProcessInputEventControl();
            clientPlayer.ProcessInputAnimationControl();
        }
    }
    

    public void AccumulateDataToBePlayedOnClientFromServer(PlayerStateUpdates playerStateUpdates)
    {
        if(!isInitialised)
        {
            return;
        }
        //if(!hasAuthority)
        //{
        //    Debug.Log("<color=blue>sequence number accumulated on client </color>" + sequenceNumberProcessedOnServer);
        //}
        if (playerStateUpdates.playerServerSequenceNumber > serverSequenceNumberToBeProcessed)
        {
            PlayerStateUpdates dataPackage;
            if (playerStateUpdatesDic.TryGetValue(playerStateUpdates.playerServerSequenceNumber, out dataPackage))
            {
                //if (!hasAuthority)
                //    Debug.Log("<color=orange>dataPackage already exists for sequence no. </color>" + positionUpdate.sequenceNumber);
            }
            else
            {
                //if (!hasAuthority)
                //    Debug.Log("<color=green>Added successfully to processing buffer dic </color>" + sequenceNumberProcessedOnServer);
                playerStateUpdatesDic.Add(playerStateUpdates.playerServerSequenceNumber, playerStateUpdates);
            }
        }
        else
        {
            //if (!hasAuthority)
            //    Debug.Log("<color=red>Already processed this sequence no </color>" + sequenceNumberProcessedOnServer);
        }
    }
}

public struct PlayerStateUpdates
{
    public int playerServerSequenceNumber;
    public int playerProcessedSequenceNumber;
    public PositionUpdates positionUpdates;
    public PlayerAuthoratativeStates playerAuthoratativeStates;
    public PlayerEvents playerEvents;
    public PlayerAnimationEvents playerAnimationEvents;
    public PlayerFlyData playerFlyData;

    public PlayerStateUpdates(int playerServerSequenceNumber, int playerProcessedSequenceNumber, PlayerAuthoratativeStates playerAuthoratativeStates, PositionUpdates positionUpdates,PlayerEvents playerEvents,PlayerAnimationEvents playerAnimationEvents, PlayerFlyData playerFlyData)
    {
        this.playerServerSequenceNumber = playerServerSequenceNumber;
        this.playerProcessedSequenceNumber = playerProcessedSequenceNumber;
        this.positionUpdates = positionUpdates;
        this.playerEvents = playerEvents;
        this.playerAuthoratativeStates = playerAuthoratativeStates;
        this.playerAnimationEvents = playerAnimationEvents;
        this.playerFlyData = playerFlyData;
    }
}

public struct PlayerFlyData
{
    public int flyingTickCount;

    public PlayerFlyData(int flyingTickCount)
    {
        this.flyingTickCount = flyingTickCount;
    }
}

public struct PlayerAuthoratativeStates
{
    public bool isPetrified;
    public bool isPushed;
    public bool isInvincible;
    public bool isPhysicsControlled;
    public bool inputFreezed;
    public bool isRespawnningPlayer;
    public bool inCharacterSelectionScreen;
    public bool inGame;
    public int currentHP;
    public int currentStockLives;
    public int hero;
    public ItemToCast itemToCast;
    public PlayerAuthoratativeStates(bool isPetrified, bool isPushed,bool isPhysicsControlled, bool inputFreezed, bool isInvincible,bool isRespawnningPlayer,bool inCharacterSelectionScreen,bool inGame, int currentHP,int currentStockLives,int hero, ItemToCast itemToCast)
    {
        this.isPetrified = isPetrified;
        this.isPushed = isPushed;
        this.isPhysicsControlled = isPhysicsControlled;
        this.inputFreezed = inputFreezed;
        this.isInvincible = isInvincible;
        this.isRespawnningPlayer = isRespawnningPlayer;
        this.inCharacterSelectionScreen = inCharacterSelectionScreen;
        this.inGame = inGame;
        this.currentHP = currentHP;
        this.currentStockLives = currentStockLives;
        this.hero = hero;
        this.itemToCast = itemToCast;
    }
}

public class ItemToCast
{
    public int castItemType;
    public int usableItemType;
    public int itemCount;

    public ItemToCast(int castItemType, int usableItemType, int itemCount)
    {
        this.castItemType = castItemType;
        this.usableItemType = usableItemType;
        this.itemCount = itemCount;
    }
}

public struct PositionUpdates
{
    public Vector3 updatedActorPosition;
    public Vector3Int updatedBlockActorPosition;
    public Vector3Int updatedPreviousBlockActorPosition;
    public int Facing;
    public int previousFacing;

    public PositionUpdates(Vector3 updatedActorPosition, Vector3Int updatedBlockActorPosition, Vector3Int updatedPreviousBlockActorPosition,int Facing,int previousFacing)
    {
        this.updatedActorPosition = updatedActorPosition;
        this.updatedBlockActorPosition = updatedBlockActorPosition;
        this.updatedPreviousBlockActorPosition = updatedPreviousBlockActorPosition;
        this.Facing = Facing;
        this.previousFacing = previousFacing;
    }
}

public struct PlayerAnimationEvents
{
    public bool isWalking;
    public bool isFlying;
    public bool isPrimaryMoveAnimationBeingPlayed;

    public PlayerAnimationEvents(bool isWalking,bool isFlying,bool isPrimaryMoveAnimationBeingPlayed)
    {
        this.isWalking = isWalking;
        this.isFlying = isFlying;
        this.isPrimaryMoveAnimationBeingPlayed = isPrimaryMoveAnimationBeingPlayed;
    }
}

public struct PlayerEvents
{
    public bool firedPrimaryMoveProjectile;
    public bool firedItemEyeLaserMoveProjectile;
    public bool firedItemFireballMoveProjectile;

    public PlayerEvents(bool firedPrimaryMoveProjectile,bool firedItemEyeLaserMoveProjectile,bool firedItemFireballMoveProjectile)
    {
        this.firedPrimaryMoveProjectile = firedPrimaryMoveProjectile;
        this.firedItemEyeLaserMoveProjectile = firedItemEyeLaserMoveProjectile;
        this.firedItemFireballMoveProjectile = firedItemFireballMoveProjectile;
    }
}

public struct PetrificationCommand
{
    public int sequenceNoForPetrificationCommand;
    public int playerIdPetrified;

    public PetrificationCommand(int sequenceNoForPetrificationCommand, int playerIdPetrified)
    {
        this.sequenceNoForPetrificationCommand = sequenceNoForPetrificationCommand;
        this.playerIdPetrified = playerIdPetrified;
    }
}



public struct FireTidalWaveCommand
{
    public int sequenceNoForFiringTidalWaveCommand;
    public int direction;
    public Vector3Int predictedCell;

    public FireTidalWaveCommand(int sequenceNoForFiringTidalWaveCommand,int direction,Vector3Int predictedCell)
    {
        this.sequenceNoForFiringTidalWaveCommand = sequenceNoForFiringTidalWaveCommand;
        this.direction = direction;
        this.predictedCell = predictedCell;
    }
}

public struct CastEarthQuakeCommand
{
    public int sequenceNoForCastingEarthQuakeCommand;
    
    public CastEarthQuakeCommand(int sequenceNoForCastingEarthQuakeCommand)
    {
        this.sequenceNoForCastingEarthQuakeCommand = sequenceNoForCastingEarthQuakeCommand;
    }
}

public struct CastPitfallCommand
{
    public int sequenceNoForCastingPitfallCommand;
    public int direction;

    public CastPitfallCommand(int sequenceNoForCastingPitfallCommand, int direction)
    {
        this.sequenceNoForCastingPitfallCommand = sequenceNoForCastingPitfallCommand;
        this.direction = direction;
    }
}

public struct CastBubbleShieldCommand
{
    public int sequenceNoForCastingBubbleShield;
    public Vector3Int predictedCell;

    public CastBubbleShieldCommand(int sequenceNoForCastingBubbleShield,Vector3Int predictedCell)
    {
        this.sequenceNoForCastingBubbleShield = sequenceNoForCastingBubbleShield;
        this.predictedCell = predictedCell;
    }
}

public struct FireMightyWindCommand
{
    public int sequenceNoForFiringMightyWindCommand;
    public int direction;
    public Vector3Int cellPredicted;

    public FireMightyWindCommand(int sequenceNoForFiringMightyWindCommand,int direction,Vector3Int cellPredicted)
    {
        this.sequenceNoForFiringMightyWindCommand = sequenceNoForFiringMightyWindCommand;
        this.direction = direction;
        this.cellPredicted = cellPredicted;
    }
}

public struct PlaceTornadoCommand
{
    public int sequenceForPlaceTornadoCommand;
    public int direction;

    public PlaceTornadoCommand(int sequenceForPlaceTornadoCommand,int direction)
    {
        this.sequenceForPlaceTornadoCommand = sequenceForPlaceTornadoCommand;
        this.direction = direction;
    }
}

public struct CastFlamePillar
{
    public int sequenceNoCastingFlamePillarCommand;
    public int direction;
    public Vector3Int predictedCell;

    public CastFlamePillar(int sequenceNoCastingFlamePillarCommand, int direction, Vector3Int predictedCell)
    {
        this.sequenceNoCastingFlamePillarCommand = sequenceNoCastingFlamePillarCommand;
        this.direction = direction;
        this.predictedCell = predictedCell;
    }
}

public struct CharacterChangeCommand
{
    public int sequenceNoCharacterChangeCommand;
    public int characterHero;

    public CharacterChangeCommand(int sequenceNoCharacterChangeCommand, int characterHero)
    {
        this.sequenceNoCharacterChangeCommand = sequenceNoCharacterChangeCommand;
        this.characterHero = characterHero;
    }
}

public struct PushCommand
{
    public int sequenceNumber;
    public int directionOfPush;
    public int playerIdToPush;

    public PushCommand(int sequenceNumber, int directionOfPush, int playerIdToPush)
    {
        this.sequenceNumber = sequenceNumber;
        this.directionOfPush = directionOfPush;
        this.playerIdToPush = playerIdToPush;
    }
}

public struct PlaceBoulderCommand
{
    public int sequenceNumber;
    public Vector3Int boulderCellPos;

    public PlaceBoulderCommand(int sequenceNumber, Vector3Int boulderCellPos)
    {
        this.sequenceNumber = sequenceNumber;
        this.boulderCellPos = boulderCellPos;
    }
}

public struct SpawnItemCommand
{
    public int sequenceNumber;
    public int direction;
    public int spawnItemType;
    public Vector3Int spawnCell;

    public SpawnItemCommand(int sequenceNumber, int direction,int spawnItemType, Vector3Int spawnCell)
    {
        this.sequenceNumber = sequenceNumber;
        this.direction = direction;
        this.spawnItemType = spawnItemType;
        this.spawnCell = spawnCell;
    }
}

public struct RemoveBoulderCommand
{
    public int sequenceNumber;
    public Vector3Int removalCellPos;

    public RemoveBoulderCommand(int sequenceNumber, Vector3Int removalCellPos)
    {
        this.sequenceNumber = sequenceNumber;
        this.removalCellPos = removalCellPos;
    }
}

public struct LandPlayerCommand
{
    public int sequenceNumber;

    public LandPlayerCommand(int sequenceNumber)
    {
        this.sequenceNumber = sequenceNumber;
    }
}

public struct RespawnPlayerCommand
{
    public int sequenceNumber;

    public RespawnPlayerCommand(int sequenceNumber)
    {
        this.sequenceNumber = sequenceNumber;
    }
}

public struct InputCommands
{
    public int sequenceNumber;
    public bool[] commands;
    public bool[] previousCommands;

    public InputCommands(bool[] commands,bool[] previousCommands, int sequenceNumber)
    {
        this.commands = commands;
        this.previousCommands = previousCommands;
        this.sequenceNumber = sequenceNumber;
    }
}

public struct PreviousInputPacks
{
    public InputCommands[] previousInputCommands;

    public PreviousInputPacks(InputCommands[] previousInputCommands)
    {
        this.previousInputCommands = previousInputCommands;
    }
}