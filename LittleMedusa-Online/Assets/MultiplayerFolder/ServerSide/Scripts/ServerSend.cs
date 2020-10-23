using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{

    private static void SendTCPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].tcp.SendData(packet);
    }

    private static void SendUDPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    private static void SendTCPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendTCPDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }
    }

    private static void SendUDPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }

    private static void SendUDPDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }
    }

    #region Packets

    public static void Welcome(int toClient, string msg)
    {
        using (Packet packet = new Packet((int)ServerPackets.welcome))
        {
            packet.Write(msg);
            packet.Write(toClient);

            Debug.Log("Welcome packet send from server so that client can initialise its udp path...........");

            SendTCPData(toClient, packet);
        }
    }

    

    public static void SpawnPlayer(int toClient, ServerMasterController serverMasterController)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            packet.Write(serverMasterController.id);
            packet.Write(serverMasterController.username);
            packet.Write(serverMasterController.serverInstanceHero.actorTransform.position);
            packet.Write(serverMasterController.serverInstanceHero.currentMovePointCellPosition);
            packet.Write(serverMasterController.serverInstanceHero.previousMovePointCellPosition);
            packet.Write((int)serverMasterController.serverInstanceHero.Facing);
            packet.Write((int)serverMasterController.serverInstanceHero.PreviousFacingDirection);
            packet.Write(serverMasterController.serverInstanceHero.isFiringPrimaryProjectile);
            packet.Write(serverMasterController.serverInstanceHero.primaryMoveUseAnimationAction.isBeingUsed);
            packet.Write(serverMasterController.serverInstanceHero.isPetrified);
            packet.Write(serverMasterController.serverInstanceHero.isPushed);
            packet.Write(serverMasterController.serverInstanceHero.isInvincible);
            packet.Write(serverMasterController.serverInstanceHero.isRespawnningPlayer);
            packet.Write(serverMasterController.serverInstanceHero.currentHP);
            packet.Write(serverMasterController.serverInstanceHero.currentStockLives);
            packet.Write(serverMasterController.playerSequenceNumberProcessed);
            packet.Write(serverMasterController.serverLocalSequenceNumber);

            SendTCPData(toClient, packet);
        }
    }

    public static void SpawnGridWorld(int toClient,WorldUpdate worldUpdate)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnGridWorld))
        {
            packet.Write(worldUpdate.worldGridItems.Length);
            foreach (WorldGridItem worldGridItem in worldUpdate.worldGridItems)
            {
                packet.Write(worldGridItem.tileType);

                packet.Write(worldGridItem.cellGridWorldPositionList.Count);

                foreach (Vector3Int itemCellPosition in worldGridItem.cellGridWorldPositionList)
                {
                    packet.Write(itemCellPosition);
                }
            }
            packet.Write(worldUpdate.sequenceNumber);

            SendTCPData(toClient, packet);
        }
    }

    public static void WorldUpdate(List<WorldUpdate> worldUpdates, List<PreviousWorldUpdatePacks> previousWorldUpdatePacks)
    {
        using (Packet packet = new Packet((int)ServerPackets.worldUpdates))
        {
            packet.Write(worldUpdates.Count);

            for (int i = 0; i < worldUpdates.Count; i++)
            {
                packet.Write(worldUpdates[i].worldGridItems.Length);
                foreach (WorldGridItem worldGridItem in worldUpdates[i].worldGridItems)
                {
                    packet.Write(worldGridItem.tileType);

                    packet.Write(worldGridItem.cellGridWorldPositionList.Count);

                    foreach(Vector3Int itemCellPosition in worldGridItem.cellGridWorldPositionList)
                    {
                        packet.Write(itemCellPosition);
                    }
                }
                packet.Write(worldUpdates[i].sequenceNumber);
                //Debug.LogWarning("<color=green>Sending inputs packet to server </color>playerMovingCommandSequenceNumber : " + inputCommands[i].sequenceNumber + " w " + inputCommands[i].commands[0] + " a " + inputCommands[i].commands[1] + " s " + inputCommands[i].commands[2] + " d " + inputCommands[i].commands[3] + "<color=green> adding previous : </color>");
            }

            packet.Write(previousWorldUpdatePacks.Count);
            for (int i = 0; i < previousWorldUpdatePacks.Count; i++)
            {
                packet.Write(previousWorldUpdatePacks[i].previousWorldUpdates.Length);
                for (int j = 0; j < previousWorldUpdatePacks[i].previousWorldUpdates.Length; j++)
                {
                    packet.Write(previousWorldUpdatePacks[i].previousWorldUpdates[j].worldGridItems.Length);

                    foreach (WorldGridItem previousWorldGridItem in previousWorldUpdatePacks[i].previousWorldUpdates[j].worldGridItems)
                    {
                        packet.Write(previousWorldGridItem.tileType);

                        packet.Write(previousWorldGridItem.cellGridWorldPositionList.Count);

                        foreach (Vector3Int itemCellPosition in previousWorldGridItem.cellGridWorldPositionList)
                        {
                            packet.Write(itemCellPosition);
                        }
                    }
                    packet.Write(previousWorldUpdatePacks[i].previousWorldUpdates[j].sequenceNumber);
                }

            }
            SendUDPDataToAll(packet);
        }
    }

    public static void PlayerStateSend(List<PlayerStateServerUpdates> playerUpdatedPosition,List<PreviousPlayerUpdatedStatePacks>previousPlayerUpdatedPositionPacks)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerStateUpdated))
        {
            packet.Write(playerUpdatedPosition.Count);
            for (int i=0;i<playerUpdatedPosition.Count;i++)
            {
                packet.Write(playerUpdatedPosition[i].playerId);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.positionUpdates.updatedActorPosition);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.positionUpdates.updatedBlockActorPosition);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.positionUpdates.updatedPreviousBlockActorPosition);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.positionUpdates.Facing);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.positionUpdates.previousFacing);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerEvents.firedPrimaryMoveProjectile);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAnimationEvents.isPrimaryMoveAnimationBeingPlayed);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.isPetrified);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.isPushed);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.isInvincible);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.isRespawnningPlayer);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.currentHP);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.currentStockLives);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerProcessedSequenceNumber);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerServerSequenceNumber);
                //Debug.LogError("<color=green>#######Processing and sending packet player.processedInputSequenceNumber: </color>" + playerUpdatedPosition[i].sequenceNumber + "position " + playerUpdatedPosition[i].playerUpdatedPosition + " playerid: " + playerUpdatedPosition[i].playerId);
            }

            packet.Write(previousPlayerUpdatedPositionPacks.Count);
            for (int i = 0; i < previousPlayerUpdatedPositionPacks.Count; i++)
            {
                packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates.Length);

                for (int j = 0; j < previousPlayerUpdatedPositionPacks[i].previousUpdatedStates.Length; j++)
                {
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerId);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.positionUpdates.updatedActorPosition);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.positionUpdates.updatedBlockActorPosition);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.positionUpdates.updatedPreviousBlockActorPosition);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.positionUpdates.Facing);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.positionUpdates.previousFacing);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerEvents.firedPrimaryMoveProjectile);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAnimationEvents.isPrimaryMoveAnimationBeingPlayed);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.isPetrified);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.isPushed);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.isInvincible);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.isRespawnningPlayer);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.currentHP);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.currentStockLives);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerProcessedSequenceNumber);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerServerSequenceNumber);
                }
            }
            SendUDPDataToAll(packet);
            //SendTCPDataToAll(packet);
        }
    }
    
    public static void PlayerDisconnected(int playerId)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            packet.Write(playerId);
            SendTCPDataToAll(packet);
        }
    }
    #endregion
}
