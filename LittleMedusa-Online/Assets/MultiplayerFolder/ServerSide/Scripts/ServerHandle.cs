﻿using UnityEngine;
using System.Collections.Generic;
public class ServerHandle
{
    public static void WelcomeReceived(int fromClient, Packet packet)
    {
        int clientIDToCheck = packet.ReadInt();
        string username = packet.ReadString();

        Debug.Log($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player: {fromClient}.");

        if (fromClient != clientIDToCheck)
        {
            Debug.Log($"Player {username} ID {fromClient} has assumed the wrong client id: {clientIDToCheck}");
        }

        Server.clients[fromClient].SendIntoGame(username);
    }
    public static void PlayerCastingTornadoCommandReceived(int fromClient, Packet packet)
    {
        int direction = packet.ReadInt();
        int sequenceNumber = packet.ReadInt();
        PlaceTornadoCommand placeTornadoCommand = new PlaceTornadoCommand(sequenceNumber, direction);
        Server.clients[fromClient].serverMasterController.AccumulateCastingTornadoRequestToBePlayedOnServerFromClient(placeTornadoCommand);
    }

    public static void PlayerCastingPitfallCommandReceived(int fromClient, Packet packet)
    {
        int direction = packet.ReadInt();
        int sequenceNumber = packet.ReadInt();
        CastPitfallCommand castPitfallCommand = new CastPitfallCommand(sequenceNumber, direction);
        Server.clients[fromClient].serverMasterController.AccumulateCastingPitfallRequestToBePlayedOnServerFromClient(castPitfallCommand);
    }

    public static void PlayerCastingFlamePillarCommandReceived(int fromClient, Packet packet)
    {
        int direction = packet.ReadInt();
        int sequenceNumber = packet.ReadInt();
        CastFlamePillar castFlamePillar = new CastFlamePillar(sequenceNumber, direction);
        Server.clients[fromClient].serverMasterController.AccumulateCastingFlamePillarRequestToBePlayedOnServerFromClient(castFlamePillar);
    }

    public static void PlayerCastingBubbleShieldCommandReceived(int fromClient,Packet packet)
    {
        int sequenceNumber = packet.ReadInt();
        CastBubbleShieldCommand castBubbleShieldCommand = new CastBubbleShieldCommand(sequenceNumber);
        Server.clients[fromClient].serverMasterController.AccumulateCastingBubbleShieldRequestToBePlayedOnServerFromClient(castBubbleShieldCommand);
    }

    public static void PlayerFiringMightyWindCommandReceived(int fromClient, Packet packet)
    {
        int direction = packet.ReadInt();
        int sequenceNumber = packet.ReadInt();

        FireMightyWindCommand fireMightyWindCommand = new FireMightyWindCommand(sequenceNumber, direction);
        Server.clients[fromClient].serverMasterController.AccumulateFiringMightyWindRequestToBePlayedOnServerFromClient(fireMightyWindCommand);
    }

    public static void PlayerFiringTidalWaveCommandReceived(int fromClient, Packet packet)
    {
        int direction = packet.ReadInt();
        int sequenceNumber = packet.ReadInt();

        FireTidalWaveCommand fireTidalWaveCommand = new FireTidalWaveCommand(sequenceNumber, direction);
        Server.clients[fromClient].serverMasterController.AccumulateFiringTidalWaveRequestToBePlayedOnServerFromClient(fireTidalWaveCommand);
    }

    public static void PlayerOnGettingHitByDispersedFireBallCommandReceived(int fromClient, Packet packet)
    {
        int playerIdHit = packet.ReadInt();
        int damage = packet.ReadInt();
        int sequenceNumber = packet.ReadInt();

        OnHitByDispersedFireBall onHitByDispersedFireBall = new OnHitByDispersedFireBall(sequenceNumber, playerIdHit, damage);
        Server.clients[fromClient].serverMasterController.AccumulateOnGettingHitByDispersedFireballRequestToBePlayedOnServerFromClient(onHitByDispersedFireBall);
    }

    public static void PlayerPetrificationCommandReceived(int fromClient, Packet packet)
    {
        int playerIdPetrified = packet.ReadInt();
        int sequenceNumber = packet.ReadInt();

        PetrificationCommand petrificationCommand = new PetrificationCommand(sequenceNumber,playerIdPetrified);
        Server.clients[fromClient].serverMasterController.AccumulatePetrificationRequestToBePlayedOnServerFromClient(petrificationCommand);
    }

    public static void PlayerRespawnCommandReceived(int fromClient,Packet packet)
    {
        Vector3Int cellPostionToRespawnPlayerOn = packet.ReadVector3Int();
        int sequenceNumber = packet.ReadInt();

        RespawnPlayerCommand respawnCommand = new RespawnPlayerCommand(sequenceNumber,cellPostionToRespawnPlayerOn);
        Server.clients[fromClient].serverMasterController.AccumulateRespawnningRequestFromClientToServer(respawnCommand);
    }

    public static void PlayerPushCommandReceived(int fromClient,Packet packet)
    {
        int playerIdToPush = packet.ReadInt();
        int directionOfPush = packet.ReadInt();
        int sequenceNumber = packet.ReadInt();

        PushCommand pushCommand = new PushCommand(sequenceNumber,directionOfPush, playerIdToPush);
        Server.clients[fromClient].serverMasterController.AccumulatePushCommandsToBePlayedOnServerFromClient(pushCommand);
    }

    public static void PlayerPlaceBoulderBoulderCommandReceived(int fromClient, Packet packet)
    {
        Vector3Int cellPosToSpawnBoulder = packet.ReadVector3Int();
        int sequenceNumber = packet.ReadInt();

        PlaceBoulderCommand placeBoulderCommand = new PlaceBoulderCommand(sequenceNumber, cellPosToSpawnBoulder);
        Server.clients[fromClient].serverMasterController.AccumulatePlaceBoulderCommandsToBePlayedOnServerFromClient(placeBoulderCommand);
    }

    public static void PlayerRemoveBoulderBoulderCommandReceived(int fromClient, Packet packet)
    {
        Vector3Int cellToRemoveBoulderFrom = packet.ReadVector3Int();
        int sequenceNumber = packet.ReadInt();

        RemoveBoulderCommand removeBoulderCommand = new RemoveBoulderCommand(sequenceNumber, cellToRemoveBoulderFrom);
        Server.clients[fromClient].serverMasterController.AccumulateRemoveBoulderCommandsToBePlayedOnServerFromClient(removeBoulderCommand);
    }

    public static void PlayerInputs(int fromClient, Packet packet)
    {
        int dataCount = packet.ReadInt();
        for (int j = 0; j < dataCount; j++)
        {
            bool[] inputs = new bool[packet.ReadInt()];
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = packet.ReadBool();
            }

            bool[] previousInputs = new bool[packet.ReadInt()];
            for (int i = 0; i < previousInputs.Length; i++)
            {
                previousInputs[i] = packet.ReadBool();
            }
            int inputSequenceNumber = packet.ReadInt();
            //Debug.LogWarning("<color=green>receiving inputs packet to server </color>playerMovingCommandSequenceNumber : " + inputSequenceNumber + " w " + inputs[0] + " a " + inputs[1] + " s " + inputs[2] + " d " + inputs[3]);
            Server.clients[fromClient].serverMasterController.AccumulateInputsToBePlayedOnServerFromClient(new InputCommands(inputs, previousInputs, inputSequenceNumber));            
        }
        int previousInputPacks = packet.ReadInt();
        for (int i = 0; i < previousInputPacks; i++)
        {
            int previousInputCommandsInPacks=packet.ReadInt();
            for (int j = 0; j < previousInputCommandsInPacks; j++)
            {
                bool[] previousDataInputCommands = new bool[packet.ReadInt()];
                for (int k = 0; k < previousDataInputCommands.Length; k++)
                {
                    previousDataInputCommands[k] = packet.ReadBool();
                }

                bool[] previousDataPreviousInputCommands = new bool[packet.ReadInt()];
                for (int k = 0; k < previousDataPreviousInputCommands.Length; k++)
                {
                    previousDataPreviousInputCommands[k] = packet.ReadBool();
                }
                int previousSeqNo = packet.ReadInt();
                Server.clients[fromClient].serverMasterController.AccumulateInputsToBePlayedOnServerFromClient(new InputCommands(previousDataInputCommands, previousDataPreviousInputCommands, previousSeqNo));
            }
        }
    }
}

