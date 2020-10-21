﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet packet)
    {
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        Debug.Log("Message from server Connecting the endpioing for udp path......................................"+msg);
        Client.instance.myID = myId;

        //Send welcome packet receive ack
        ClientSend.WelcomeReceived();


        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnedPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Vector3Int blockposition = packet.ReadVector3Int();
        Vector3Int previousBlockposition = packet.ReadVector3Int();
        bool isFiringPrimaryProjectile = packet.ReadBool();
        bool isPrimaryMoveAnimationBeingPlayed = packet.ReadBool();
        bool isPlacingBoulderAnimationPlayed = packet.ReadBool();
        bool isPetrified = packet.ReadBool();
        bool isPushed = packet.ReadBool();
        int faceDirection = packet.ReadInt();
        int previousfaceDirection = packet.ReadInt();
        int playerProcessingSequenceNumber = packet.ReadInt();
        int playerServerSequenceNumber = packet.ReadInt();
        Debug.Log("<color=red>Sequence no spawned on: </color>"+playerProcessingSequenceNumber);

        PositionUpdates positionUpdates = new PositionUpdates(position, blockposition, previousBlockposition);
        PlayerEvents playerEvents = new PlayerEvents(isFiringPrimaryProjectile);
        PlayerAnimationEvents playerAnimtaionEvents = new PlayerAnimationEvents(isPrimaryMoveAnimationBeingPlayed, isPlacingBoulderAnimationPlayed);
        PlayerAuthoratativeStates playerAuthoratativeStates = new PlayerAuthoratativeStates(isPetrified, isPushed);

        PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(playerServerSequenceNumber, playerProcessingSequenceNumber, playerAuthoratativeStates, positionUpdates, playerEvents, playerAnimtaionEvents);
        ClientSideGameManager.instance.SpawnPlayer(id,username, faceDirection, previousfaceDirection, playerStateUpdates);
    }

    public static void SpawnGridWorld(Packet packet)
    {
        WorldGridItem[] worldItems = new WorldGridItem[packet.ReadInt()];
        for (int i = 0; i < worldItems.Length; i++)
        {
            int tileType = packet.ReadInt();

            int cellPositionCount = packet.ReadInt();

            List<Vector3Int> cellPositionList = new List<Vector3Int>();
            for (int k = 0; k < cellPositionCount; k++)
            {
                Vector3Int cell = packet.ReadVector3Int();
                cellPositionList.Add(cell);
            }

            worldItems[i] = new WorldGridItem(tileType, cellPositionList);
        }

        int worldUpdateSequenceNumber = packet.ReadInt();

        WorldUpdate worldUpdate = new WorldUpdate(worldUpdateSequenceNumber, worldItems);
        ClientSideGameManager.instance.SpawnWorldGridElements(worldUpdate);
    }

    public static void WorldStateUpdated(Packet packet)
    {
        int dataCount = packet.ReadInt();
        for (int j = 0; j < dataCount; j++)
        {
            WorldGridItem[] worldItems = new WorldGridItem[packet.ReadInt()];
            for (int i = 0; i < worldItems.Length; i++)
            {
                int tileType = packet.ReadInt();

                int cellPositionCount = packet.ReadInt();

                List<Vector3Int> cellPositionList = new List<Vector3Int>();
                for (int k = 0; k < cellPositionCount; k++)
                {
                    Vector3Int cell = packet.ReadVector3Int();
                    cellPositionList.Add(cell);
                }

                worldItems[i] = new WorldGridItem(tileType, cellPositionList);
            }

            int worldUpdateSequenceNumber = packet.ReadInt();
            //Debug.LogWarning("<color=green>receiving inputs packet to server </color>playerMovingCommandSequenceNumber : " + worldUpdateSequenceNumber + " w " + inputs[0] + " a " + inputs[1] + " s " + inputs[2] + " d " + inputs[3]);
             ClientSideGameManager.instance.AccumulateWorldUpdatesToBePlayedOnClientFromServer(new WorldUpdate(worldUpdateSequenceNumber, worldItems));
        }

        int previousWorldUpdatePacks = packet.ReadInt();
        for (int i = 0; i < previousWorldUpdatePacks; i++)
        {
            int previousWorldUpdateWorldItemsInPacks = packet.ReadInt();
            for (int j = 0; j < previousWorldUpdateWorldItemsInPacks; j++)
            {
                WorldGridItem[] previousDataWorldItems = new WorldGridItem[packet.ReadInt()];
                for (int k = 0; k < previousDataWorldItems.Length; k++)
                {
                    int tileType = packet.ReadInt();
                    int cellPositionCount = packet.ReadInt();

                    List<Vector3Int> cellPositionList = new List<Vector3Int>();
                    for (int l = 0; l < cellPositionCount; l++)
                    {
                        Vector3Int cell = packet.ReadVector3Int();
                        cellPositionList.Add(cell);
                    }
                    previousDataWorldItems[k] = new WorldGridItem(tileType, cellPositionList);
                }

                int previousSeqNo = packet.ReadInt();
                ClientSideGameManager.instance.AccumulateWorldUpdatesToBePlayedOnClientFromServer(new WorldUpdate(previousSeqNo, previousDataWorldItems));
            }
        }
    }

    public static void PlayerStateUpdated(Packet packet)
    {
        int dataCount = packet.ReadInt();
        
        for(int i=0;i<dataCount;i++)
        {
            int id = packet.ReadInt();
            Vector3 position = packet.ReadVector3();
            Vector3Int blockposition = packet.ReadVector3Int();
            Vector3Int previousBlockposition = packet.ReadVector3Int();
            bool isFiringPrimaryProjectile = packet.ReadBool();
            bool isPrimaryMoveAnimationBeingPlayed = packet.ReadBool();
            bool isPlacingBoulderAnimationPlayed = packet.ReadBool();
            bool isPetrified = packet.ReadBool();
            bool isPushed = packet.ReadBool();
            int playerProcessedsequenceNumberReceived = packet.ReadInt();
            int playerServerSequenceNumberReceived = packet.ReadInt();

            PositionUpdates positionUpdates = new PositionUpdates(position, blockposition, previousBlockposition);
            PlayerEvents playerEvents = new PlayerEvents(isFiringPrimaryProjectile);
            PlayerAnimationEvents playerAnimtaionEvents = new PlayerAnimationEvents(isPrimaryMoveAnimationBeingPlayed, isPlacingBoulderAnimationPlayed);
            PlayerAuthoratativeStates playerAuthoratativeStates = new PlayerAuthoratativeStates(isPetrified, isPushed);

            PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(playerServerSequenceNumberReceived, playerProcessedsequenceNumberReceived, playerAuthoratativeStates, positionUpdates, playerEvents, playerAnimtaionEvents);
            //Debug.LogError("<color=blue>Receiving updated position for movement </color>playerUpdatedPositionSequenceNumber: " + sequenceNumberReceived + " position: " + position);
            if(ClientSideGameManager.players.ContainsKey(id))
            {
                ClientSideGameManager.players[id].masterController.AccumulateDataToBePlayedOnClientFromServer(playerStateUpdates);
            }
            else
            {
                Debug.LogError("Player of id doesnot exists: "+id);
            }
            //if (!ClientSideGameManager.players[id].masterController.hasAuthority)
            //{
            //    Debug.LogWarning("<color=red>Receiving remote player data</color>"+sequenceNumberReceived);
            //}
            //else
            //{
            //    Debug.LogWarning("<color=green>Receiving my player data</color>"+sequenceNumberReceived);
            //}
        }

        int previousPlayerUpdatedPositionPacks = packet.ReadInt();

        for (int i = 0; i < previousPlayerUpdatedPositionPacks; i++)
        {
            int previousPlayerUpdatedPositionPacksCount = packet.ReadInt();

            for (int j = 0; j < previousPlayerUpdatedPositionPacksCount; j++)
            {
                int previousHistoryPlayerId = packet.ReadInt();
                Vector3 previousHistoryPositionUpdate = packet.ReadVector3();
                Vector3Int previousHistoryBlockPositionUpdate = packet.ReadVector3Int();
                Vector3Int previousHistoryPreviousBlockPositionUpdate = packet.ReadVector3Int();
                bool isFiringPrimaryProjectile = packet.ReadBool();
                bool isPrimaryMoveAnimationBeingPlayed = packet.ReadBool();
                bool isPlacingBoulderAnimationPlayed = packet.ReadBool();
                bool isPetrified = packet.ReadBool();
                bool isPushed = packet.ReadBool();
                int previousHistoryPlayerProcessingSequenceNo = packet.ReadInt();
                int previousHistoryServerSequenceNo = packet.ReadInt();

                PositionUpdates positionUpdates = new PositionUpdates(previousHistoryPositionUpdate, previousHistoryBlockPositionUpdate, previousHistoryPreviousBlockPositionUpdate);
                PlayerEvents playerEvents = new PlayerEvents(isFiringPrimaryProjectile);
                PlayerAnimationEvents playerAnimtaionEvents = new PlayerAnimationEvents(isPrimaryMoveAnimationBeingPlayed, isPlacingBoulderAnimationPlayed);
                PlayerAuthoratativeStates playerAuthoratativeStates = new PlayerAuthoratativeStates(isPetrified, isPushed);

                PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(previousHistoryServerSequenceNo, previousHistoryPlayerProcessingSequenceNo, playerAuthoratativeStates, positionUpdates, playerEvents, playerAnimtaionEvents);
                if (ClientSideGameManager.players.ContainsKey(previousHistoryPlayerId))
                {
                    ClientSideGameManager.players[previousHistoryPlayerId].masterController.AccumulateDataToBePlayedOnClientFromServer(playerStateUpdates);
                }
                else
                {
                    Debug.LogError("Player of id doesnot exists: " + previousHistoryPlayerId);
                }
            }
            
        }

    }

    public static void PlayerDisconnected(Packet packet)
    {
        int id = packet.ReadInt();
        Destroy(ClientSideGameManager.players[id].gameObject);
        ClientSideGameManager.players.Remove(id);
    }
}
